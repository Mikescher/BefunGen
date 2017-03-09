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

		public Program(SourceCodePosition pos, ProgramHeader hdr, List<VarDeclaration> c, List<VarDeclaration> g, Method m, List<Method> mlst)
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

			AddPredefConstants();

			MethodList.ForEach(pm => pm.Owner = this);
			Constants.ForEach(pc => pc.SetConstant());

			TestConstantsForDefinition();
			TestGlobalVarsForDefinition();
			TestDuplicateIdentifierCondition();
		}

		public override string GetDebugString()
		{
			return string.Format("#Program [{0}|{1}] ({2})\n[\n#Constants:\n{3}\n#Variables:\n{4}\n#Body:\n{5}\n]",
				DisplayWidth,
				DisplayHeight,
				Identifier,
				Indent(GetDebugStringForList(Constants)),
				Indent(GetDebugStringForList(Variables)),
				Indent(GetDebugStringForList(MethodList))
				);
		}

		public string GetWellFormattedHeader()
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

		private void AddPredefConstants()
		{
			Constants.Insert(0, new VarDeclarationValue(
				new SourceCodePosition(),
				new BTypeInt(new SourceCodePosition()),
				"DISPLAY_SIZE",
				new LiteralInt(new SourceCodePosition(), DisplayWidth * DisplayHeight)));

			Constants.Insert(0, new VarDeclarationValue(
				new SourceCodePosition(),
				new BTypeInt(new SourceCodePosition()),
				"DISPLAY_HEIGHT",
				new LiteralInt(new SourceCodePosition(), DisplayHeight)));

			Constants.Insert(0, new VarDeclarationValue(
				new SourceCodePosition(),
				new BTypeInt(new SourceCodePosition()),
				"DISPLAY_WIDTH",
				new LiteralInt(new SourceCodePosition(), DisplayWidth)));
		}

		private void TestConstantsForDefinition()
		{
			foreach (VarDeclaration v in Constants)
				if (!v.HasCompleteUserDefiniedInitialValue)
					throw new UndefiniedValueInitConstantException(v.Position, v.Identifier);
		}

		private void TestGlobalVarsForDefinition()
		{
			foreach (VarDeclaration v in Variables)
				if (v.HasCompleteUserDefiniedInitialValue)
					throw new UndefiniedValueInitConstantException(v.Position, v.Identifier);
		}

		private void TestDuplicateIdentifierCondition()
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

		public void Prepare()
		{
			// Reset ID-Counter
			Method.ResetCounter();
			VarDeclaration.ResetCounter();
			Statement.ResetCounter();

			IntegrateStatementLists();	// Flattens StatementList Hierachie && Cleans it up (Removes NOP's, empty StmtLists)
			ForceMethodReturn();		// Every Method must always end with a RETURN && No Return in Main {{CODE-MANIPULATION}}
			AddressMethods();			// Methods get their Address
			AddressCodePoints();		// CodeAdressesTargets get their Address
			LinkVariables();			// Variable-uses get their ID
			InlineConstants();			// ValuePointer to Constants become Literals {{CODE-MANIPULATION}}
			EvaluateExpressions();		// Simplify Expressions if possible {{CODE-MANIPULATION}}
			LinkMethods();				// Methodcalls get their ID   &&   Labels + MethodCalls get their CodePointAddress
			RemoveUnreferencedMethods();
			LinkResultTypes();			// Statements get their Result-Type (and implicit casting is added)
		}

		private void IntegrateStatementLists()
		{
			foreach (Method m in MethodList)
				m.IntegrateStatementLists();
		}

		private void AddressMethods()
		{
			foreach (Method m in MethodList)
				m.CreateCodeAddress();
		}

		private void AddressCodePoints()
		{
			foreach (Method m in MethodList)
				m.AddressCodePoints();
		}

		private void LinkVariables()
		{
			foreach (Method m in MethodList)
				m.LinkVariables();
		}

		private void InlineConstants()
		{
			if (Constants.Count == 0)
				return;

			foreach (Method m in MethodList)
				m.InlineConstants();
		}

		private void LinkMethods()
		{
			foreach (Method m in MethodList)
				m.LinkMethods(this);
		}

		private void RemoveUnreferencedMethods()
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

		private void LinkResultTypes()
		{
			foreach (Method m in MethodList)
				m.LinkResultTypes();
		}

		private void ForceMethodReturn()
		{
			foreach (Method m in MethodList)
				m.ForceMethodReturn(m == MainMethod);

			MainMethod.RaiseErrorOnReturnStatement();
		}

		private void EvaluateExpressions()
		{
			if (!CGO.CompileTimeEvaluateExpressions)
				return; // It's disabled

			foreach (Method m in MethodList)
				m.EvaluateExpressions();
		}

		#endregion

		public Method FindMethodByIdentifier(string ident)
		{
			return MethodList.Count(p => p.Identifier.ToLower() == ident.ToLower()) == 1 ? MethodList.Single(p => p.Identifier.ToLower() == ident.ToLower()) : null;
		}

		public int GetMaxReturnValueWidth()
		{
			return MathExt.Max(1, MethodList.Select(p => p.ResultType.GetCodeSize()).ToArray());
		}

		public CodePiece GenerateCode(string initialDisp = "")
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

			List<Tuple<MathExt.Point, CodePiece>> methPieces = new List<Tuple<MathExt.Point, CodePiece>>();

			CodePiece p = new CodePiece();

			int maxReturnValWidth = GetMaxReturnValueWidth();

			int methOffsetX = 4 + CodeGenConstants.LANE_VERTICAL_MARGIN;

			#region Generate Top Lane

			CodePiece pTopLane = new CodePiece();

			pTopLane[0, 0] = BCHelper.PCDown;

			pTopLane[0, 1] = BCHelper.Digit0;
			pTopLane[2, 1] = BCHelper.PCDown;

			CodePiece pFlooder = CodePieceStore.BooleanStackFlooder();
			pTopLane.SetAt(3, 1, pFlooder);

			pTopLane[CodeGenConstants.TMP_FIELD_IO_ARR.X, CodeGenConstants.TMP_FIELD_IO_ARR.Y] = BCHelper.Chr(CGO.DefaultTempSymbol, new TemporaryCodeFieldTag());
			pTopLane[CodeGenConstants.TMP_FIELD_OUT_ARR.X, CodeGenConstants.TMP_FIELD_OUT_ARR.Y] = BCHelper.Chr(CGO.DefaultTempSymbol, new TemporaryCodeFieldTag());
			pTopLane[CodeGenConstants.TMP_FIELD_JMP_ADDR.X, CodeGenConstants.TMP_FIELD_JMP_ADDR.Y] = BCHelper.Chr(CGO.DefaultTempSymbol, new TemporaryCodeFieldTag());
			pTopLane.Fill(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X, CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y,
				CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X + maxReturnValWidth, CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y + 1,
				BCHelper.Chr(CGO.DefaultResultTempSymbol),
				new TemporaryResultCodeFieldTag(maxReturnValWidth));
			pTopLane.SetText(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X + maxReturnValWidth + 4,
				CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y,
				"// generated by BefunGen v" + CodeGenConstants.BEFUNGEN_VERSION);

			CodePiece displayValue = GenerateCode_DisplayValue(initialDisp);

			CodePiece pDisplay = GenerateCode_Display(displayValue);

			DisplayOffsetX = 3;
			DisplayOffsetY = 3;

			pTopLane.SetAt(DisplayOffsetX, DisplayOffsetY, pDisplay);

			int topLaneBottomRow = 3 + pDisplay.Height;

			DisplayOffsetX += CGO.DisplayBorderThickness;
			DisplayOffsetY += CGO.DisplayBorderThickness;


			pTopLane[0, topLaneBottomRow] = BCHelper.PCDown;
			pTopLane[1, topLaneBottomRow] = BCHelper.Walkway;

			pTopLane.FillColWw(0, 2, topLaneBottomRow);
			pTopLane.FillColWw(2, 2, topLaneBottomRow + 1);

			p.SetAt(0, 0, pTopLane);

			#endregion

			int laneStartY = p.MaxY;
			int methOffsetY = p.MaxY; // +3 For the MinY=3 of VerticalLaneTurnout_Dec

			#region Insert VariableSpace

			CodePiece pVars = GenerateCode_Variables(methOffsetX, methOffsetY);

			p.SetAt(methOffsetX, methOffsetY, pVars);

			methOffsetY += Math.Max(0, pVars.Height - 3); // -3 For the MinY=3 of VerticalLaneTurnout_Dec

			#endregion

			methOffsetY += 3; // +3 For the MinY=3 of VerticalLaneTurnout_Dec

			#region Insert Methods

			for (int i = 0; i < MethodList.Count; i++)
			{
				Method m = MethodList[i];

				CodePiece pMethod = m.GenerateCode(methOffsetX, methOffsetY);

				if (p.HasActiveTag(typeof(MethodEntryFullInitializationTag))) // Force MethodEntry_FullIntialization Distance (at least so that lanes can be generated)
				{
					int pLast = p.FindAllActiveCodeTags(typeof(MethodEntryFullInitializationTag)).Last().Y;
					int pNext = pMethod.FindAllActiveCodeTags(typeof(MethodEntryFullInitializationTag)).First().Y + (methOffsetY - pMethod.MinY);
					int overflow = (pNext - pLast) - CodePieceStore.VerticalLaneTurnout_Dec(false).Height;

					if (overflow < 0)
					{
						methOffsetY -= overflow;
					}
				}

				int mx = methOffsetX - pMethod.MinX;
				int my = methOffsetY - pMethod.MinY;

				methPieces.Add(Tuple.Create(new MathExt.Point(mx, my), pMethod));

				p.SetAt(mx, my, pMethod);

				methOffsetY += pMethod.Height + CodeGenConstants.VERTICAL_METHOD_DISTANCE;
			}

			#endregion

			int highwayX = p.MaxX;

			#region Generate Lane Chooser

			p.FillRowWw(1, 3 + pFlooder.Width, highwayX);
			p.FillRowWw(topLaneBottomRow, 3, highwayX);

			p[highwayX, 1] = BCHelper.PCLeft;
			p[highwayX, topLaneBottomRow - 1] = BCHelper.IfVertical;
			p[highwayX, topLaneBottomRow + 0] = BCHelper.PCLeft;
			p[highwayX, topLaneBottomRow + 1] = BCHelper.PCJump;
			p[highwayX, topLaneBottomRow + 2] = BCHelper.Not;

			p.FillColWw(highwayX, 2, topLaneBottomRow - 1);

			#endregion

			#region Generate Lanes (Left Lane && Right Lane)

			List<TagLocation> methodEntries = p.FindAllActiveCodeTags(typeof(MethodEntryFullInitializationTag)) // Left Lane
				.OrderBy(tp => tp.Y)
				.ToList();
			List<TagLocation> codeEntries = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalReEntryTag)) // Right Lane
				.OrderBy(tp => tp.Y)
				.ToList();

			int last;
			bool first;

			//######### LEFT LANE #########

			first = true;
			last = laneStartY;
			foreach (TagLocation methodEntry in methodEntries)
			{
				CodePiece pTurnout = CodePieceStore.VerticalLaneTurnout_Dec(first);

				p.FillColWw(0, last, methodEntry.Y + pTurnout.MinY);
				p.SetAt(0, methodEntry.Y, pTurnout);
				p.FillRowWw(methodEntry.Y, 4, methodEntry.X);
				last = methodEntry.Y + pTurnout.MaxY;
				first = false;
			}
			//p.FillColWW(0, last, p.MaxY);

			//######### RIGHT LANE #########

			first = true;
			last = laneStartY;
			foreach (TagLocation codeEntry in codeEntries)
			{
				CodePiece pTurnout = CodePieceStore.VerticalLaneTurnout_Test();

				p.FillColWw(2, last, codeEntry.Y + pTurnout.MinY);
				p.SetAt(2, codeEntry.Y, pTurnout);
				p.CreateRowWw(codeEntry.Y, 4, codeEntry.X);
				last = codeEntry.Y + pTurnout.MaxY;
				first = false;
			}
			//p.FillColWW(2, last, p.MaxY);

			//######### MIDDLE LANE #########

			p.Fill(1, laneStartY, 2, p.MaxY, BCHelper.PCJump);

			//######### POP LANE #########

			p.Fill(3, laneStartY, 4, p.MaxY, BCHelper.StackPop);

			#endregion

			#region Generate Highway (Path on right side of code)

			List<TagLocation> codeExits = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalExitTag))
				.OrderBy(tp => tp.Y)
				.ToList();

			first = true;
			last = topLaneBottomRow + 3;
			foreach (TagLocation exit in codeExits)
			{
				p.FillColWw(highwayX, last, exit.Y);
				p[highwayX, exit.Y] = BCHelper.PCUp;
				p.CreateRowWw(exit.Y, exit.X + 1, highwayX);
				last = exit.Y + 1;

				exit.Tag.Deactivate();

				first = false;
			}

			#endregion

			return p;
		}

		private CodePiece GenerateCode_Variables(int moX, int moY)
		{
			CodePiece p = new CodePiece();

			int paramX = 0;
			int paramY = 0;

			int maxArr = 0;
			if (Variables.Count(t => t is VarDeclarationArray) > 0)
				maxArr = Variables.Where(t => t is VarDeclarationArray).Select(t => t as VarDeclarationArray).Max(t => t.Size);

			int maxwidth = Math.Max(maxArr, CGO.DefaultVarDeclarationWidth);

			for (int i = 0; i < Variables.Count; i++)
			{
				VarDeclaration var = Variables[i];

				CodePiece lit = new CodePiece();

				if (paramX >= maxwidth)
				{	// Next Line
					paramX = 0;
					paramY++;
				}

				if (paramX > 0 && var is VarDeclarationArray && (paramX + (var as VarDeclarationArray).Size) > maxwidth)
				{	// Next Line
					paramX = 0;
					paramY++;
				}

				if (var is VarDeclarationValue)
				{
					lit[0, 0] = BCHelper.Chr(CGO.DefaultVarDeclarationSymbol, new VarDeclarationTag(var));
				}
				else
				{
					int sz = (var as VarDeclarationArray).Size;
					lit.Fill(0, 0, sz, 1, BCHelper.Chr(CGO.DefaultVarDeclarationSymbol), new VarDeclarationTag(var));
				}

				var.CodePositionX = moX + paramX;
				var.CodePositionY = moY + paramY;

				p.SetAt(paramX, paramY, lit);
				paramX += lit.Width;
			}

			return p;
		}

		private CodePiece GenerateCode_Display(CodePiece val)
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

			p.Fill(b, 0, s.X + 2 * b, b, BCHelper.Chr(CGO.DisplayBorder));						// 1
			p.Fill(s.X + b, b, s.X + 2 * b, s.Y + 2 * b, BCHelper.Chr(CGO.DisplayBorder));		// 2
			p.Fill(0, s.Y + b, s.X + b, s.Y + 2 * b, BCHelper.Chr(CGO.DisplayBorder));			// 3
			p.Fill(0, 0, b, s.Y + b, BCHelper.Chr(CGO.DisplayBorder));							// 4

			p.SetTag(0, 0, new DisplayTopLeftTag(this, DisplayWidth + 2 * b, DisplayHeight + 2 * b));

			return p;
		}

		private CodePiece GenerateCode_DisplayValue(string dv)
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

			BefungeCommand def = BCHelper.Chr(CGO.DefaultDisplayValue);
			if (def.Type == BefungeCommandType.NOP)
				def = BCHelper.Walkway;

			for (int y = 0; y < DisplayHeight; y++)
			{
				for (int x = 0; x < DisplayWidth; x++)
				{
					r[x, y] = (y < split.Length && x < split[y].Length) ? BCHelper.Chr(split[y][x]) : def;
				}
			}

			return r;
		}
	}

	public class ProgramFooter : ASTObject // TEMPORARY -- NOT IN RESULTING AST
	{
		public ProgramFooter(SourceCodePosition pos)
			: base(pos)
		{
		}

		public override string GetDebugString()
		{
			throw new AccessTemporaryASTObjectException(Position);
		}
	}

	public class ProgramHeader : ASTObject // TEMPORARY -- NOT IN RESULTING AST
	{
		public readonly string Identifier;

		public readonly int DisplayWidth;
		public readonly int DisplayHeight;

		public ProgramHeader(SourceCodePosition pos, string id)
			: this(pos, id, 0, 0)
		{
			// --
		}

		public ProgramHeader(SourceCodePosition pos, string ident, int w, int h)
			: base(pos)
		{
			this.Identifier = ident;

			if (ASTObject.IsKeyword(ident))
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

		public override string GetDebugString()
		{
			throw new AccessTemporaryASTObjectException(Position);
		}
	}
}