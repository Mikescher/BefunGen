using BefunGen.AST.CodeGen;
using BefunGen.AST.CodeGen.Tags;
using BefunGen.AST.Exceptions;
using BefunGen.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BefunGen.AST
{
	public class Program : ASTObject
	{
		//TODO Possible Optimizations [LOW PRIO]
		//Optimize -> ArrayValuePointer/DisplayArrayPointer when Indizies Constant -> Direct Link
		//Optimize -> Remove unused global/local variables (not params)
		//Optimize -> Remove NOP - Switch Cases

		public string Identifier;

		public readonly Method MainMethod;
		public readonly List<Method> MethodList; // Includes MainStatement (at 0)

		public readonly List<VarDeclaration> Constants;
		public readonly List<VarDeclaration> Variables; // Global Variables

		public bool HasDisplay { get { return DisplayHeight * DisplayWidth > 0; } }
		public int DisplayOffsetX;
		public int DisplayOffsetY;

		public readonly int DisplayWidth;
		public readonly int DisplayHeight;

		public Program(SourceCodePosition pos, Program_Header hdr, List<VarDeclaration> c, List<VarDeclaration> g, Method m, List<Method> mlst)
			: base(hdr.Position)
		{
			this.Identifier = hdr.Identifier;
			this.Constants = c;
			this.Variables = g;
			this.MainMethod = m;
			this.MethodList = mlst.ToList();

			this.DisplayWidth = hdr.DisplayWidth;
			this.DisplayHeight = hdr.DisplayHeight;

			MethodList.Insert(0, MainMethod);

			MainMethod.AddReference(null);

			addPredefConstants();

			MethodList.ForEach(pm => pm.Owner = this);
			Constants.ForEach(pc => pc.setConstant());

			testConstantsForDefinition();
			testGlobalVarsForDefinition();
			testDuplicateIdentifierCondition();
		}

		public override string getDebugString()
		{
			return string.Format("#Program [{0}|{1}] ({2})\n[\n#Constants:\n{3}\n#Variables:\n{4}\n#Body:\n{5}\n]",
				DisplayWidth,
				DisplayHeight,
				Identifier,
				indent(getDebugStringForList(Constants)),
				indent(getDebugStringForList(Variables)),
				indent(getDebugStringForList(MethodList))
				);
		}

		public string getWellFormattedHeader()
		{
			if (HasDisplay)
			{
				return string.Format("{0} : display [{1}, {2}]", Identifier, DisplayWidth, DisplayHeight);
			}
			else
			{
				return Identifier;
			}
		}

		private void addPredefConstants()
		{
			Constants.Insert(0, new VarDeclaration_Value(
				new SourceCodePosition(),
				new BType_Int(new SourceCodePosition()),
				"DISPLAY_SIZE",
				new Literal_Int(new SourceCodePosition(), DisplayWidth * DisplayHeight)));

			Constants.Insert(0, new VarDeclaration_Value(
				new SourceCodePosition(),
				new BType_Int(new SourceCodePosition()),
				"DISPLAY_HEIGHT",
				new Literal_Int(new SourceCodePosition(), DisplayHeight)));

			Constants.Insert(0, new VarDeclaration_Value(
				new SourceCodePosition(),
				new BType_Int(new SourceCodePosition()),
				"DISPLAY_WIDTH",
				new Literal_Int(new SourceCodePosition(), DisplayWidth)));
		}

		private void testConstantsForDefinition()
		{
			foreach (VarDeclaration v in Constants)
				if (!v.hasCompleteUserDefiniedInitialValue)
					throw new UndefiniedValueInitConstantException(v.Position, v.Identifier);
		}

		private void testGlobalVarsForDefinition()
		{
			foreach (VarDeclaration v in Variables)
				if (v.hasCompleteUserDefiniedInitialValue)
					throw new UndefiniedValueInitConstantException(v.Position, v.Identifier);
		}

		private void testDuplicateIdentifierCondition()
		{
			// Duplicate in Global variables
			if (Variables.Any(lp1 => Variables.Any(lp2 => lp1.Identifier.ToLower() == lp2.Identifier.ToLower() && lp1 != lp2)))
			{
				VarDeclaration err = Variables.Last(lp1 => Variables.Any(lp2 => lp1.Identifier.ToLower() == lp2.Identifier.ToLower()));
				throw new DuplicateIdentifierException(err.Position, err.Identifier);
			}

			// Duplicate in Constants
			if (Constants.Any(lp1 => Constants.Any(lp2 => lp1.Identifier.ToLower() == lp2.Identifier.ToLower() && lp1 != lp2)))
			{
				VarDeclaration err = Constants.Last(lp1 => Constants.Any(lp2 => lp1.Identifier.ToLower() == lp2.Identifier.ToLower()));
				throw new DuplicateIdentifierException(err.Position, err.Identifier);
			}

			// Name Conflict Global variables <-> Constants
			if (Constants.Any(lp1 => Variables.Any(lp2 => lp1.Identifier.ToLower() == lp2.Identifier.ToLower() && lp1 != lp2)))
			{
				VarDeclaration err = Constants.Last(lp1 => Variables.Any(lp2 => lp1.Identifier.ToLower() == lp2.Identifier.ToLower()));
				throw new DuplicateIdentifierException(err.Position, err.Identifier);
			}

			// Name Methods
			if (MethodList.Any(lp1 => MethodList.Any(lp2 => lp1.Identifier.ToLower() == lp2.Identifier.ToLower() && lp1 != lp2)))
			{
				Method err = MethodList.Last(lp1 => MethodList.Any(lp2 => lp1.Identifier.ToLower() == lp2.Identifier.ToLower()));
				throw new DuplicateIdentifierException(err.Position, err.Identifier);
			}

			// Name Conflict Local variables <-> (Constants & Global variables)
			foreach (Method m in MethodList)
			{
				foreach (VarDeclaration t in m.Variables)
				{
					if (Constants.Any(pl => pl.Identifier.ToLower() == t.Identifier.ToLower()) || Variables.Any(pl => pl.Identifier.ToLower() == t.Identifier.ToLower()))
					{
						throw new DuplicateIdentifierException(t.Position, t.Identifier);
					}
				}
			}
		}

		#region Prepare

		public void prepare()
		{
			// Reset ID-Counter
			Method.resetCounter();
			VarDeclaration.resetCounter();
			Statement.resetCounter();

			integrateStatementLists();	// Flattens StatementList Hierachie && Cleans it up (Removes NOP's, empty StmtLists)
			forceMethodReturn();		// Every Method must always end with a RETURN && No Return in Main {{CODE-MANIPULATION}}
			addressMethods();			// Methods get their Address
			addressCodePoints();		// CodeAdressesTargets get their Address
			linkVariables();			// Variable-uses get their ID
			inlineConstants();			// ValuePointer to Constants become Literals {{CODE-MANIPULATION}}
			evaluateExpressions();		// Simplify Expressions if possible {{CODE-MANIPULATION}}
			linkMethods();				// Methodcalls get their ID   &&   Labels + MethodCalls get their CodePointAddress
			removeUnreferencedMethods();
			linkResultTypes();			// Statements get their Result-Type (and implicit casting is added)
		}

		private void integrateStatementLists()
		{
			foreach (Method m in MethodList)
				m.integrateStatementLists();
		}

		private void addressMethods()
		{
			foreach (Method m in MethodList)
				m.createCodeAddress();
		}

		private void addressCodePoints()
		{
			foreach (Method m in MethodList)
				m.addressCodePoints();
		}

		private void linkVariables()
		{
			foreach (Method m in MethodList)
				m.linkVariables();
		}

		private void inlineConstants()
		{
			if (Constants.Count == 0)
				return;

			foreach (Method m in MethodList)
				m.inlineConstants();
		}

		private void linkMethods()
		{
			foreach (Method m in MethodList)
				m.linkMethods(this);
		}

		private void removeUnreferencedMethods()
		{
			if (CGO.RemUnreferencedMethods)
			{
				for (int i = MethodList.Count - 1; i >= 0; i--)
				{
					if (MethodList[i].ReferenceCount == 0)
					{
						MethodList.RemoveAt(i);
					}
				}
			}
		}

		private void linkResultTypes()
		{
			foreach (Method m in MethodList)
				m.linkResultTypes();
		}

		private void forceMethodReturn()
		{
			foreach (Method m in MethodList)
				m.forceMethodReturn(m == MainMethod);

			MainMethod.raiseErrorOnReturnStatement();
		}

		private void evaluateExpressions()
		{
			if (!CGO.CompileTimeEvaluateExpressions)
				return; // It's disabled

			foreach (Method m in MethodList)
				m.evaluateExpressions();
		}

		#endregion

		public Method findMethodByIdentifier(string ident)
		{
			return MethodList.Count(p => p.Identifier.ToLower() == ident.ToLower()) == 1 ? MethodList.Single(p => p.Identifier.ToLower() == ident.ToLower()) : null;
		}

		public int getMaxReturnValueWidth()
		{
			return MathExt.Max(1, MethodList.Select(p => p.ResultType.GetSize()).ToArray());
		}

		public CodePiece generateCode(string initialDisp = "")
		{
			// v {TEMP..}
			// 1 v{STACKFLOODER}        <
			//    {++++++++++++}
			//    ###############
			//    ###############
			//    ##           ##
			//    ## {DISPLAY} ##
			//    ##           ##
			//    ###############
			//    ###############       |
			// v                        <
			// :# $   {GLOBALVAR}       #
			// !# $   {GLOBALVAR}       !
			// ## $
			// ># $   {METHOD}
			// |# $   {++++++}
			//  # $   {++++++}
			//  ##$
			//  #>$   {METHOD}
			//  #|$   {++++++}
			//  # $   {++++++}
			//  # $   {METHOD}
			//  # $   {++++++}

			List<Tuple<MathExt.Point, CodePiece>> meth_pieces = new List<Tuple<MathExt.Point, CodePiece>>();

			CodePiece p = new CodePiece();

			int maxReturnValWidth = getMaxReturnValueWidth();

			int meth_offset_x = 4 + CodeGenConstants.LANE_VERTICAL_MARGIN;

			#region Generate Top Lane

			CodePiece p_TopLane = new CodePiece();

			p_TopLane[0, 0] = BCHelper.PC_Down;

			p_TopLane[0, 1] = BCHelper.Digit_0;
			p_TopLane[2, 1] = BCHelper.PC_Down;

			CodePiece p_flooder = CodePieceStore.BooleanStackFlooder();
			p_TopLane.SetAt(3, 1, p_flooder);

			p_TopLane[CodeGenConstants.TMP_FIELD_IO_ARR.X, CodeGenConstants.TMP_FIELD_IO_ARR.Y] = BCHelper.chr(CGO.DefaultTempSymbol, new TemporaryCodeField_Tag());
			p_TopLane[CodeGenConstants.TMP_FIELD_OUT_ARR.X, CodeGenConstants.TMP_FIELD_OUT_ARR.Y] = BCHelper.chr(CGO.DefaultTempSymbol, new TemporaryCodeField_Tag());
			p_TopLane[CodeGenConstants.TMP_FIELD_JMP_ADDR.X, CodeGenConstants.TMP_FIELD_JMP_ADDR.Y] = BCHelper.chr(CGO.DefaultTempSymbol, new TemporaryCodeField_Tag());
			p_TopLane.Fill(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X, CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y,
				CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X + maxReturnValWidth, CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y + 1,
				BCHelper.chr(CGO.DefaultResultTempSymbol),
				new TemporaryResultCodeField_Tag(maxReturnValWidth));
			p_TopLane.setText(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X + maxReturnValWidth + 4,
				CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y,
				"// generated by BefunGen v" + CodeGenConstants.BEFUNGEN_VERSION);

			CodePiece displayValue = generateCode_DisplayValue(initialDisp);

			CodePiece p_display = generateCode_Display(displayValue);

			DisplayOffsetX = 3;
			DisplayOffsetY = 3;

			p_TopLane.SetAt(DisplayOffsetX, DisplayOffsetY, p_display);

			int topLane_bottomRow = 3 + p_display.Height;

			DisplayOffsetX += CGO.DisplayBorderThickness;
			DisplayOffsetY += CGO.DisplayBorderThickness;


			p_TopLane[0, topLane_bottomRow] = BCHelper.PC_Down;
			p_TopLane[1, topLane_bottomRow] = BCHelper.Walkway;

			p_TopLane.FillColWW(0, 2, topLane_bottomRow);
			p_TopLane.FillColWW(2, 2, topLane_bottomRow + 1);

			p.SetAt(0, 0, p_TopLane);

			#endregion

			int lane_start_y = p.MaxY;
			int meth_offset_y = p.MaxY; // +3 For the MinY=3 of VerticalLaneTurnout_Dec

			#region Insert VariableSpace

			CodePiece p_vars = generateCode_Variables(meth_offset_x, meth_offset_y);

			p.SetAt(meth_offset_x, meth_offset_y, p_vars);

			meth_offset_y += Math.Max(0, p_vars.Height - 3); // -3 For the MinY=3 of VerticalLaneTurnout_Dec

			#endregion

			meth_offset_y += 3; // +3 For the MinY=3 of VerticalLaneTurnout_Dec

			#region Insert Methods

			for (int i = 0; i < MethodList.Count; i++)
			{
				Method m = MethodList[i];

				CodePiece p_method = m.generateCode(meth_offset_x, meth_offset_y);

				if (p.hasActiveTag(typeof(MethodEntry_FullInitialization_Tag))) // Force MethodEntry_FullIntialization Distance (at least so that lanes can be generated)
				{
					int pLast = p.findAllActiveCodeTags(typeof(MethodEntry_FullInitialization_Tag)).Last().Y;
					int pNext = p_method.findAllActiveCodeTags(typeof(MethodEntry_FullInitialization_Tag)).First().Y + (meth_offset_y - p_method.MinY);
					int overflow = (pNext - pLast) - CodePieceStore.VerticalLaneTurnout_Dec(false).Height;

					if (overflow < 0)
					{
						meth_offset_y -= overflow;
					}
				}

				int mx = meth_offset_x - p_method.MinX;
				int my = meth_offset_y - p_method.MinY;

				meth_pieces.Add(Tuple.Create(new MathExt.Point(mx, my), p_method));

				p.SetAt(mx, my, p_method);

				meth_offset_y += p_method.Height + CodeGenConstants.VERTICAL_METHOD_DISTANCE;
			}

			#endregion

			int highway_x = p.MaxX;

			#region Generate Lane Chooser

			p.FillRowWW(1, 3 + p_flooder.Width, highway_x);
			p.FillRowWW(topLane_bottomRow, 3, highway_x);

			p[highway_x, 1] = BCHelper.PC_Left;
			p[highway_x, topLane_bottomRow - 1] = BCHelper.If_Vertical;
			p[highway_x, topLane_bottomRow + 0] = BCHelper.PC_Left;
			p[highway_x, topLane_bottomRow + 1] = BCHelper.PC_Jump;
			p[highway_x, topLane_bottomRow + 2] = BCHelper.Not;

			p.FillColWW(highway_x, 2, topLane_bottomRow - 1);

			#endregion

			#region Generate Lanes (Left Lane && Right Lane)

			List<TagLocation> method_entries = p.findAllActiveCodeTags(typeof(MethodEntry_FullInitialization_Tag)) // Left Lane
				.OrderBy(tp => tp.Y)
				.ToList();
			List<TagLocation> code_entries = p.findAllActiveCodeTags(typeof(MethodCall_HorizontalReEntry_Tag)) // Right Lane
				.OrderBy(tp => tp.Y)
				.ToList();

			int last;
			bool first;

			//######### LEFT LANE #########

			first = true;
			last = lane_start_y;
			foreach (TagLocation method_entry in method_entries)
			{
				CodePiece p_turnout = CodePieceStore.VerticalLaneTurnout_Dec(first);

				p.FillColWW(0, last, method_entry.Y + p_turnout.MinY);
				p.SetAt(0, method_entry.Y, p_turnout);
				p.FillRowWW(method_entry.Y, 4, method_entry.X);
				last = method_entry.Y + p_turnout.MaxY;
				first = false;
			}
			//p.FillColWW(0, last, p.MaxY);

			//######### RIGHT LANE #########

			first = true;
			last = lane_start_y;
			foreach (TagLocation code_entry in code_entries)
			{
				CodePiece p_turnout = CodePieceStore.VerticalLaneTurnout_Test();

				p.FillColWW(2, last, code_entry.Y + p_turnout.MinY);
				p.SetAt(2, code_entry.Y, p_turnout);
				p.CreateRowWW(code_entry.Y, 4, code_entry.X);
				last = code_entry.Y + p_turnout.MaxY;
				first = false;
			}
			//p.FillColWW(2, last, p.MaxY);

			//######### MIDDLE LANE #########

			p.Fill(1, lane_start_y, 2, p.MaxY, BCHelper.PC_Jump);

			//######### POP LANE #########

			p.Fill(3, lane_start_y, 4, p.MaxY, BCHelper.Stack_Pop);

			#endregion

			#region Generate Highway (Path on right side of code)

			List<TagLocation> code_exits = p.findAllActiveCodeTags(typeof(MethodCall_HorizontalExit_Tag))
				.OrderBy(tp => tp.Y)
				.ToList();

			first = true;
			last = topLane_bottomRow + 3;
			foreach (TagLocation exit in code_exits)
			{
				p.FillColWW(highway_x, last, exit.Y);
				p[highway_x, exit.Y] = BCHelper.PC_Up;
				p.CreateRowWW(exit.Y, exit.X + 1, highway_x);
				last = exit.Y + 1;

				exit.Tag.deactivate();

				first = false;
			}

			#endregion

			return p;
		}

		private CodePiece generateCode_Variables(int mo_x, int mo_y)
		{
			CodePiece p = new CodePiece();

			int paramX = 0;
			int paramY = 0;

			int max_arr = 0;
			if (Variables.Count(t => t is VarDeclaration_Array) > 0)
				max_arr = Variables.Where(t => t is VarDeclaration_Array).Select(t => t as VarDeclaration_Array).Max(t => t.Size);

			int maxwidth = Math.Max(max_arr, CGO.DefaultVarDeclarationWidth);

			for (int i = 0; i < Variables.Count; i++)
			{
				VarDeclaration var = Variables[i];

				CodePiece lit = new CodePiece();

				if (paramX >= maxwidth)
				{	// Next Line
					paramX = 0;
					paramY++;
				}

				if (paramX > 0 && var is VarDeclaration_Array && (paramX + (var as VarDeclaration_Array).Size) > maxwidth)
				{	// Next Line
					paramX = 0;
					paramY++;
				}

				if (var is VarDeclaration_Value)
				{
					lit[0, 0] = BCHelper.chr(CGO.DefaultVarDeclarationSymbol, new VarDeclaration_Tag(var));
				}
				else
				{
					int sz = (var as VarDeclaration_Array).Size;
					lit.Fill(0, 0, sz, 1, BCHelper.chr(CGO.DefaultVarDeclarationSymbol), new VarDeclaration_Tag(var));
				}

				var.CodePositionX = mo_x + paramX;
				var.CodePositionY = mo_y + paramY;

				p.SetAt(paramX, paramY, lit);
				paramX += lit.Width;
			}

			return p;
		}

		private CodePiece generateCode_Display(CodePiece val)
		{
			MathExt.Point s = new MathExt.Point(DisplayWidth, DisplayHeight);

			int b = CGO.DisplayBorderThickness;

			CodePiece p = new CodePiece();

			if (s.Size == 0)
				return p;

			p.SetAt(b, b, val);

			// 44111111
			// 44111111
			// 44    22
			// 44    22
			// 44    22
			// 44    22
			// 33333322
			// 33333322

			p.Fill(b, 0, s.X + 2 * b, b, BCHelper.chr(CGO.DisplayBorder));						// 1
			p.Fill(s.X + b, b, s.X + 2 * b, s.Y + 2 * b, BCHelper.chr(CGO.DisplayBorder));		// 2
			p.Fill(0, s.Y + b, s.X + b, s.Y + 2 * b, BCHelper.chr(CGO.DisplayBorder));			// 3
			p.Fill(0, 0, b, s.Y + b, BCHelper.chr(CGO.DisplayBorder));							// 4

			p.SetTag(0, 0, new Display_TopLeft_Tag(this, DisplayWidth + 2 * b, DisplayHeight + 2 * b));

			return p;
		}

		private CodePiece generateCode_DisplayValue(string dv)
		{
			CodePiece r = new CodePiece();

			int w = dv.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Max(s => s.Length);
			int h = dv.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Length;

			if (dv == "")
			{
				w = 0;
				h = 0;
			}

			if (w > DisplayWidth || h > DisplayHeight)
				throw new InitialDisplayValueTooBigException(DisplayWidth, DisplayHeight, w, h);

			string[] split = dv.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

			BefungeCommand def = BCHelper.chr(CGO.DefaultDisplayValue);
			if (def.Type == BefungeCommandType.NOP)
				def = BCHelper.Walkway;

			for (int y = 0; y < DisplayHeight; y++)
			{
				for (int x = 0; x < DisplayWidth; x++)
				{
					r[x, y] = (y < split.Length && x < split[y].Length) ? BCHelper.chr(split[y][x]) : def;
				}
			}

			return r;
		}
	}

	public class Program_Footer : ASTObject // TEMPORARY -- NOT IN RESULTING AST
	{
		public Program_Footer(SourceCodePosition pos)
			: base(pos)
		{
		}

		public override string getDebugString()
		{
			throw new AccessTemporaryASTObjectException(Position);
		}
	}

	public class Program_Header : ASTObject // TEMPORARY -- NOT IN RESULTING AST
	{
		public readonly string Identifier;

		public readonly int DisplayWidth;
		public readonly int DisplayHeight;

		public Program_Header(SourceCodePosition pos, string id)
			: this(pos, id, 0, 0)
		{
			// --
		}

		public Program_Header(SourceCodePosition pos, string ident, int w, int h)
			: base(pos)
		{
			this.Identifier = ident;

			if (ASTObject.isKeyword(ident))
			{
				throw new IllegalIdentifierException(Position, ident);
			}

			if (w * h == 0)
			{
				DisplayWidth = 0;
				DisplayHeight = 0;
			}
			else
			{
				DisplayWidth = w;
				DisplayHeight = h;
			}
		}

		public override string getDebugString()
		{
			throw new AccessTemporaryASTObjectException(Position);
		}
	}
}