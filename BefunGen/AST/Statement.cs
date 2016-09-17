using BefunGen.AST.CodeGen;
using BefunGen.AST.CodeGen.NumberCode;
using BefunGen.AST.CodeGen.Tags;
using BefunGen.AST.Exceptions;
using BefunGen.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BefunGen.AST
{
	public abstract class Statement : ASTObject
	{
		private static int _codepointAddressCounter = 0;
		protected static int CODEPOINT_ADDRESS_COUNTER { get { return _codepointAddressCounter++; } }

		public Statement(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public static void ResetCounter()
		{
			_codepointAddressCounter = 0;
		}

		public CodePiece ExtendVerticalMcTagsUpwards(CodePiece p)
		{
			List<TagLocation> entries = p.FindAllActiveCodeTags(typeof(MethodCallVerticalReEntryTag))
				.OrderByDescending(tp => (tp.Tag.TagParam as ICodeAddressTarget).CodePointAddr)
				.ToList();
			List<TagLocation> exits = p.FindAllActiveCodeTags(typeof(MethodCallVerticalExitTag))
				.OrderByDescending(tp => tp.X)
				.ToList();

			//########################

			int posYExitline = p.MinY - 1;
			TagLocation lastExit = null;

			foreach (TagLocation exit in exits)
			{
				MethodCallVerticalExitTag tagExit = exit.Tag as MethodCallVerticalExitTag;
				tagExit.Deactivate();

				BefungeCommand currExitCmd = BCHelper.PC_Right_tagged(new MethodCallHorizontalExitTag(tagExit.TagParam));
				p[exit.X, posYExitline] = currExitCmd;
				TagLocation currExit = new TagLocation(exit.X, posYExitline, currExitCmd);

				try
				{
					p.CreateColWw(exit.X, posYExitline + 1, exit.Y);
				}
				catch (InvalidCodeManipulationException ce)
				{
					throw new CommandPathFindingFailureException(ce.Message);
				}

				if (lastExit != null)
				{
					p.CreateRowWw(posYExitline, currExit.X + 1, lastExit.X);

					currExit.Tag.Deactivate();
				}

				lastExit = currExit;
			}

			//########################

			int entrycount = entries.Count;
			int posYEntry = posYExitline - entrycount * 3 + 2;

			foreach (TagLocation entry in entries)
			{
				MethodCallVerticalReEntryTag tagEntry = entry.Tag as MethodCallVerticalReEntryTag;
				tagEntry.Deactivate();

				p[entry.X, posYEntry] = BCHelper.PC_Down_tagged(new MethodCallHorizontalReEntryTag((ICodeAddressTarget)tagEntry.TagParam));

				try
				{
					p.CreateColWw(entry.X, posYEntry + 1, entry.Y);
				}
				catch (InvalidCodeManipulationException ce)
				{
					throw new CommandPathFindingFailureException(ce.Message);
				}

				posYEntry -= 3;
			}

			return p;
		}

		public abstract void IntegrateStatementLists();
		public abstract void LinkVariables(Method owner);
		public abstract void InlineConstants();
		public abstract void AddressCodePoints();
		public abstract void LinkResultTypes(Method owner);
		public abstract void LinkMethods(Program owner);
		public abstract bool AllPathsReturn();
		public abstract StatementReturn HasReturnStatement();
		public abstract void EvaluateExpressions();

		public abstract StatementLabel FindLabelByIdentifier(string ident);

		public abstract CodePiece GenerateCode(bool reversed);
	}

	#region Interfaces

	public interface ICodeAddressTarget
	{
		int CodePointAddr
		{
			get;
			set;
		}
	}

	#endregion

	#region Other

	public class StatementStatementList : Statement
	{
		public List<Statement> List;

		public StatementStatementList(SourceCodePosition pos, List<Statement> sl)
			: base(pos)
		{
			List = sl.ToList();
		}

		public override string GetDebugString()
		{
			return string.Format("#StatementList\n[\n{0}\n]", Indent(GetDebugStringForList(List)));
		}

		public override void IntegrateStatementLists()
		{
			List<Statement> listNew = new List<Statement>();

			for (int i = 0; i < List.Count; i++)
			{
				List[i].IntegrateStatementLists();

				if (List[i] is StatementNOP)
				{
					// Do nothing
				}
				else if (List[i] is StatementStatementList)
				{
					if ((List[i] as StatementStatementList).List.Count > 0)
					{
						foreach (Statement stmt in (List[i] as StatementStatementList).List)
						{
							listNew.Add(stmt);
						}
					}
				}
				else
				{
					listNew.Add(List[i]);
				}
			}

			List = listNew;
		}

		public override void LinkVariables(Method owner)
		{
			foreach (Statement s in List)
				s.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			for (int i = 0; i < List.Count; i++)
				List[i].InlineConstants();
		}

		public override void AddressCodePoints()
		{
			for (int i = 0; i < List.Count; i++)
			{
				List[i].AddressCodePoints();
			}
		}

		public override void LinkMethods(Program owner)
		{
			foreach (Statement s in List)
			{
				s.LinkMethods(owner);
			}
		}

		public override void LinkResultTypes(Method owner)
		{
			foreach (Statement s in List)
				s.LinkResultTypes(owner);
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			StatementLabel result = null;

			foreach (Statement s in List)
			{
				StatementLabel found = s.FindLabelByIdentifier(ident);
				if (found != null && result != null)
					return null;
				if (found != null && result == null)
					result = found;
			}

			return result;
		}

		public override bool AllPathsReturn()
		{
			for (int i = 0; i < List.Count; i++)
			{
				if (List[i].AllPathsReturn())
					return true;
			}
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			for (int i = 0; i < List.Count; i++)
			{
				StatementReturn r;
				if ((r = List[i].HasReturnStatement()) != null)
					return r;
			}
			return null;
		}

		public override void EvaluateExpressions()
		{
			foreach (Statement s in List)
				s.EvaluateExpressions();
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();

			#region Special Cases

			if (List.Count == 0)
			{
				return new CodePiece();
			}
			else if (List.Count == 1)
			{
				return ExtendVerticalMcTagsUpwards(List[0].GenerateCode(reversed));
			}

			#endregion

			#region Get Statements

			List<Statement> stmts = List.ToList();
			if (stmts.Count % 2 == 0)
				stmts.Add(new StatementNOP(Position));

			#endregion

			#region Generate Codepieces

			List<CodePiece> cps = new List<CodePiece>();
			for (int i = 0; i < stmts.Count; i++)
			{
				cps.Add(ExtendVerticalMcTagsUpwards(stmts[i].GenerateCode(reversed ^ (i % 2 != 0))));
				cps[i].NormalizeX();

				if (cps[i].Height == 0) // No total empty statements
					cps[i][0, 0] = BCHelper.Walkway;
			}

			#endregion

			#region Calculate Y-Positions

			List<int> ypos = new List<int>();
			ypos.Add(0);
			for (int i = 1; i < cps.Count; i++)
			{
				ypos.Add(ypos[i - 1] + cps[i - 1].MaxY - cps[i].MinY);
			}

			#endregion

			#region Combine Pieces

			if (reversed)
			{
				#region Reversed

				// ##### WIDTHS ######

				List<int> widths = new List<int>();
				for (int i = 0; i < cps.Count; i += 2)
				{
					int a = i - 1;
					int b = i;

					bool first = (i == 0);
					bool last = (i == cps.Count - 1);

					int wA;
					int wB;

					if (first)
						wA = 0;
					else
						wA = cps[a].Width;

					if (last)
						wB = cps[b].Width - 1;
					else
						wB = cps[b].Width;

					int w = Math.Max(wA, wB);

					if (!first)
						widths.Add(w);
					widths.Add(w);
				}

				int maxwidth = MathExt.Max(widths[0], widths.ToArray());

				// ##### PC's ######

				for (int i = 0; i < cps.Count; i++)
				{
					bool currRev = (i % 2 == 0);
					bool first = (i == 0);
					bool last = (i == cps.Count - 1);

					if (first)
					{
						p[-1, ypos[i]] = BCHelper.PCDown;
					}
					else if (last)
					{
						p[widths[i], ypos[i]] = BCHelper.PCLeft;
					}
					else if (currRev) // Reversed
					{
						p[-1, ypos[i]] = BCHelper.PCDown;
						p[widths[i], ypos[i]] = BCHelper.PCLeft;
					}
					else // Normal
					{
						p[-1, ypos[i]] = BCHelper.PCRight;
						p[widths[i], ypos[i]] = BCHelper.PCDown;
					}
				}

				// ##### Walkways ######

				for (int i = 0; i < cps.Count; i++)
				{
					bool currRev = (i % 2 == 0);
					bool first = (i == 0);
					bool last = (i == cps.Count - 1);

					if (first)
					{
						p.FillRowWw(ypos[i], cps[i].Width, maxwidth + 1);
						p.FillColWw(-1, ypos[i] + 1, ypos[i] + cps[i].MaxY);
					}
					else if (last)
					{
						p.FillRowWw(ypos[i], cps[i].Width - 1, widths[i]);
						p.FillColWw(widths[i], ypos[i] + cps[i].MinY, ypos[i]);
					}
					else
					{
						p.FillRowWw(ypos[i], cps[i].Width, widths[i]);

						if (currRev) // Reversed
						{
							p.FillColWw(widths[i], ypos[i] + cps[i].MinY, ypos[i]);
							p.FillColWw(-1, ypos[i] + 1, ypos[i] + cps[i].MaxY);
						}
						else
						{
							p.FillColWw(-1, ypos[i] + cps[i].MinY, ypos[i]);
							p.FillColWw(widths[i], ypos[i] + 1, ypos[i] + cps[i].MaxY);
						}
					}
				}

				// ##### Outer-Walkway ######

				int lastypos = ypos[ypos.Count - 1];
				p[-2, lastypos] = BCHelper.PCUp;
				p[-2, 0] = BCHelper.PCLeft;

				p.FillColWw(-2, 1, lastypos);

				// ##### Statements ######

				for (int i = 0; i < cps.Count; i++)
				{
					bool last = (i == cps.Count - 1);
					int x = last ? -1 : 0;
					int y = ypos[i];
					CodePiece c = cps[i];

					p.SetAt(x, y, c);
				}

				#endregion
			}
			else
			{
				#region Normal

				// ##### WIDTHS ######

				List<int> widths = new List<int>();
				for (int i = 0; i < cps.Count; i += 2)
				{
					int a = i;
					int b = i + 1;

					bool first = (i == 0);
					bool last = (i == cps.Count - 1);

					int wA;
					int wB;

					if (first)
						wA = cps[a].Width - 1;
					else
						wA = cps[a].Width;

					if (last)
						wB = 0;
					else
						wB = cps[b].Width;

					int w = Math.Max(wA, wB);

					widths.Add(w);
					if (!last)
						widths.Add(w);
				}

				int right = MathExt.Max(widths[0], widths.ToArray()) + 1;

				// ##### PC's ######

				for (int i = 0; i < cps.Count; i++)
				{
					bool currRev = (i % 2 != 0);
					bool first = (i == 0);
					bool last = (i == cps.Count - 1);

					if (first)
					{
						p[widths[i], ypos[i]] = BCHelper.PCDown;
					}
					else if (last)
					{
						p[-1, ypos[i]] = BCHelper.PCRight;
					}
					else if (currRev) // Reversed
					{
						p[-1, ypos[i]] = BCHelper.PCDown;
						p[widths[i], ypos[i]] = BCHelper.PCLeft;
					}
					else // Normal
					{
						p[-1, ypos[i]] = BCHelper.PCRight;
						p[widths[i], ypos[i]] = BCHelper.PCDown;
					}
				}

				// ##### Walkways ######

				for (int i = 0; i < cps.Count; i++)
				{
					bool currRev = (i % 2 != 0);
					bool first = (i == 0);
					bool last = (i == cps.Count - 1);

					if (first)
					{
						p.FillRowWw(ypos[i], cps[i].Width - 1, widths[i]);
						p.FillColWw(widths[i], ypos[i] + 1, ypos[i] + cps[i].MaxY);
					}
					else if (last)
					{
						p.FillRowWw(ypos[i], cps[i].Width, right);
						p.FillColWw(-1, ypos[i] + cps[i].MinY, ypos[i]);
					}
					else
					{
						p.FillRowWw(ypos[i], cps[i].Width, widths[i]);

						if (currRev) // Reversed
						{
							p.FillColWw(widths[i], ypos[i] + cps[i].MinY, ypos[i]);
							p.FillColWw(-1, ypos[i] + 1, ypos[i] + cps[i].MaxY);
						}
						else
						{
							p.FillColWw(-1, ypos[i] + cps[i].MinY, ypos[i]);
							p.FillColWw(widths[i], ypos[i] + 1, ypos[i] + cps[i].MaxY);
						}
					}
				}

				// ##### Outer-Walkway ######

				int lastypos = ypos[ypos.Count - 1];
				p[right, lastypos] = BCHelper.PCUp;
				p[right, 0] = BCHelper.PCRight;

				p.FillColWw(right, 1, lastypos);

				// ##### Statements ######

				for (int i = 0; i < cps.Count; i++)
				{
					bool first = (i == 0);
					int x = first ? -1 : 0;
					int y = ypos[i];
					CodePiece c = cps[i];

					p.SetAt(x, y, c);
				}

				#endregion

			}

			p.NormalizeX();

			#endregion

			#region Extend MehodCall-Tags

			List<TagLocation> entries = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalReEntryTag));
			List<TagLocation> exits = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalExitTag));

			foreach (TagLocation entry in entries)
			{
				MethodCallHorizontalReEntryTag tagEntry = entry.Tag as MethodCallHorizontalReEntryTag;

				p.CreateRowWw(entry.Y, p.MinX, entry.X);

				tagEntry.Deactivate();

				p.SetTag(p.MinX, entry.Y, new MethodCallHorizontalReEntryTag(tagEntry.TagParam as ICodeAddressTarget), true);
			}

			foreach (TagLocation exit in exits)
			{
				MethodCallHorizontalExitTag tagExit = exit.Tag as MethodCallHorizontalExitTag;

				p.CreateRowWw(exit.Y, exit.X + 1, p.MaxX);

				tagExit.Deactivate();

				p.SetTag(p.MaxX - 1, exit.Y, new MethodCallHorizontalExitTag(tagExit.TagParam), true);
			}

			#endregion

			return p;
		}

		public CodePiece GenerateStrippedCode()
		{
			// Always normal direction

			CodePiece p = new CodePiece();

			#region Special Cases

			if (List.Count == 0)
			{
				return new CodePiece();
			}
			else if (List.Count == 1)
			{
				return ExtendVerticalMcTagsUpwards(List[0].GenerateCode(false));
			}

			#endregion

			#region Get Statements

			List<Statement> stmts = List.ToList();
			if (stmts.Count % 2 == 0)
				stmts.Add(new StatementNOP(Position));

			#endregion

			#region Generate Codepieces

			List<CodePiece> cps = new List<CodePiece>();
			for (int i = 0; i < stmts.Count; i++)
			{
				cps.Add(ExtendVerticalMcTagsUpwards(stmts[i].GenerateCode(i % 2 != 0)));
				cps[i].NormalizeX();

				if (cps[i].Height == 0) // No total empty statements
					cps[i][0, 0] = BCHelper.Walkway;
			}

			#endregion

			#region Calculate Y-Positions

			List<int> ypos = new List<int>();
			ypos.Add(0);
			for (int i = 1; i < cps.Count; i++)
			{
				ypos.Add(ypos[i - 1] + cps[i - 1].MaxY - cps[i].MinY);
			}

			#endregion

			#region Combine Pieces

			// ##### WIDTHS ######

			List<int> widths = new List<int>();
			for (int i = 0; i < cps.Count; i += 2)
			{
				int a = i;
				int b = i + 1;

				bool first = (i == 0);
				bool last = (i == cps.Count - 1);

				int wA;
				int wB;

				if (first)
					wA = cps[a].Width - 1;
				else
					wA = cps[a].Width;

				if (last)
					wB = 0;
				else
					wB = cps[b].Width;

				int w = Math.Max(wA, wB);

				widths.Add(w);
				if (!last)
					widths.Add(w);
			}

			int right = MathExt.Max(widths[0], widths.ToArray()) + 1;

			// ##### PC's ######

			for (int i = 0; i < cps.Count; i++)
			{
				bool currRev = (i % 2 != 0);
				bool first = (i == 0);
				bool last = (i == cps.Count - 1);

				if (first)
				{
					p[widths[i], ypos[i]] = BCHelper.PCDown;
				}
				else if (last)
				{
					p[-1, ypos[i]] = BCHelper.PCRight;
				}
				else if (currRev) // Reversed
				{
					p[-1, ypos[i]] = BCHelper.PCDown;
					p[widths[i], ypos[i]] = BCHelper.PCLeft;
				}
				else // Normal
				{
					p[-1, ypos[i]] = BCHelper.PCRight;
					p[widths[i], ypos[i]] = BCHelper.PCDown;
				}
			}

			// ##### Walkways ######

			for (int i = 0; i < cps.Count; i++)
			{
				bool currRev = (i % 2 != 0);
				bool first = (i == 0);
				bool last = (i == cps.Count - 1);

				if (first)
				{
					p.FillRowWw(ypos[i], cps[i].Width - 1, widths[i]);
					p.FillColWw(widths[i], ypos[i] + 1, ypos[i] + cps[i].MaxY);
				}
				else if (last)
				{
					p.FillRowWw(ypos[i], cps[i].Width, right);
					p.FillColWw(-1, ypos[i] + cps[i].MinY, ypos[i]);
				}
				else
				{
					p.FillRowWw(ypos[i], cps[i].Width, widths[i]);

					if (currRev) // Reversed
					{
						p.FillColWw(widths[i], ypos[i] + cps[i].MinY, ypos[i]);
						p.FillColWw(-1, ypos[i] + 1, ypos[i] + cps[i].MaxY);
					}
					else
					{
						p.FillColWw(-1, ypos[i] + cps[i].MinY, ypos[i]);
						p.FillColWw(widths[i], ypos[i] + 1, ypos[i] + cps[i].MaxY);
					}
				}
			}

			// ##### Statements ######

			for (int i = 0; i < cps.Count; i++)
			{
				bool first = (i == 0);
				int x = first ? -1 : 0;
				int y = ypos[i];
				CodePiece c = cps[i];

				p.SetAt(x, y, c);
			}

			p.NormalizeX();

			#endregion

			#region Extend MehodCall-Tags

			List<TagLocation> entries = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalReEntryTag));
			List<TagLocation> exits = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalExitTag));

			foreach (TagLocation entry in entries)
			{
				MethodCallHorizontalReEntryTag tagEntry = entry.Tag as MethodCallHorizontalReEntryTag;

				p.CreateRowWw(entry.Y, p.MinX, entry.X);

				tagEntry.Deactivate();

				p.SetTag(p.MinX, entry.Y, new MethodCallHorizontalReEntryTag(tagEntry.TagParam as ICodeAddressTarget), true);
			}

			foreach (TagLocation exit in exits)
			{
				MethodCallHorizontalExitTag tagExit = exit.Tag as MethodCallHorizontalExitTag;

				p.CreateRowWw(exit.Y, exit.X + 1, p.MaxX);

				tagExit.Deactivate();

				p.SetTag(p.MaxX - 1, exit.Y, new MethodCallHorizontalExitTag(tagExit.TagParam), true);
			}

			#endregion

			#region Strip LastLine

			if (List.Count % 2 == 0 && p.LastRowIsSingle(true))
			{
				p.RemoveRow(p.MaxY - 1);
			}

			#endregion

			return p;
		}
	}

	public class StatementMethodCall : Statement, ICodeAddressTarget
	{
		public readonly List<Expression> CallParameter;

		public string Identifier; // Temporary -- before linking;
		public Method Target;

		public Method Owner;

		private int codePointAddr = -1;
		public int CodePointAddr
		{
			get
			{
				return codePointAddr;
			}
			set
			{
				throw new Exception(); // NotWriteable
			}
		}

		public StatementMethodCall(SourceCodePosition pos, string id)
			: base(pos)
		{
			this.Identifier = id;
			this.CallParameter = new List<Expression>();
		}

		public StatementMethodCall(SourceCodePosition pos, string id, List<Expression> cp)
			: base(pos)
		{
			this.Identifier = id;
			this.CallParameter = cp.ToList();
		}

		public override string GetDebugString()
		{
			return string.Format("#MethodCall {{{0}}} ::{1}:: --> #Parameter: ({2})", Target.MethodAddr, CodePointAddr, GetDebugCommaStringForList(CallParameter));
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			this.Owner = owner;

			foreach (Expression e in CallParameter)
				e.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			for (int i = 0; i < CallParameter.Count; i++)
				CallParameter[i] = CallParameter[i].InlineConstants();
		}

		public override void AddressCodePoints()
		{
			foreach (Expression e in CallParameter)
				e.AddressCodePoints();

			codePointAddr = CODEPOINT_ADDRESS_COUNTER;
		}

		public override void LinkMethods(Program owner)
		{
			foreach (Expression e in CallParameter)
				e.LinkMethods(owner);

			if (Target != null) // Already linked
				return;

			Target = owner.FindMethodByIdentifier(Identifier) as Method;

			if (Target == null)
				throw new UnresolvableReferenceException(Identifier, Position);

			Target.AddReference(this);
		}

		public override void LinkResultTypes(Method owner)
		{
			foreach (Expression e in CallParameter)
				e.LinkResultTypes(owner);

			if (CallParameter.Count != Target.Parameter.Count)
				throw new WrongParameterCountException(CallParameter.Count, Target.Parameter.Count, Position);

			for (int i = 0; i < CallParameter.Count; i++)
			{
				BType present = CallParameter[i].GetResultType();
				BType expected = Target.Parameter[i].Type;

				if (present != expected)
				{
					if (present.IsImplicitCastableTo(expected))
						CallParameter[i] = new ExpressionCast(CallParameter[i].Position, expected, CallParameter[i]);
					else
						throw new ImplicitCastException(CallParameter[i].Position, present, expected);
				}
			}
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			for (int i = 0; i < CallParameter.Count; i++)
			{
				CallParameter[i] = CallParameter[i].EvaluateExpressions();
			}
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			return GenerateCode(reversed, true);
		}

		public CodePiece GenerateCode(bool reversed, bool popResult)
		{
			if (CodePointAddr < 0)
				throw new InvalidAstStateException(Position);

			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed

				// ######## BEFORE EXIT::JUMP-IN ########

				#region EXIT::JUMPIN

				// Put own Variables on Stack

				p.AppendLeft(GenerateCode_varFrame_JumpIn(reversed));

				// Put own JumpBack-Adress on Stack

				p.AppendLeft(NumberCodeHelper.GenerateCode(CodePointAddr, reversed));

				// Put Parameter on Stack

				for (int i = 0; i < CallParameter.Count; i++)
				{
					p.AppendLeft(CallParameter[i].GenerateCode(reversed));
				}

				// Put TargetAdress on Stack

				p.AppendLeft(NumberCodeHelper.GenerateCode(Target.MethodAddr, reversed));

				// Put Lane Switch on Stack

				p.AppendLeft(BCHelper.Digit1);

				#endregion

				// ######## JUMPS ########

				p.AppendLeft(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag(Target)));
				p.AppendLeft(BCHelper.PC_Left_tagged(new MethodCallVerticalReEntryTag(this)));

				// ######## AFTER ENTRY::JUMP-BACK ########

				#region ENTRY::JUMP-BACK

				// Store Result int TMP_RESULT Field

				if (popResult)
				{
					if (Target.ResultType is BTypeVoid)
					{
						p.AppendLeft(BCHelper.StackPop);
					}
					else if (Target.ResultType is BTypeValue)
					{
						p.AppendLeft(BCHelper.StackPop);
					}
					else if (Target.ResultType is BTypeArray)
					{
						p.AppendLeft(CodePieceStore.PopMultipleStackValues((Target.ResultType as BTypeArray).Size, reversed));
					}
					else
						throw new WTFException();
				}
				else if (Target.ResultType is BTypeVoid)
				{
					p.AppendLeft(BCHelper.StackPop); // Nobody cares about the result ...
				}
				else if (Target.ResultType is BTypeValue)
				{
					p.AppendLeft(NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X, reversed));
					p.AppendLeft(NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y, reversed));

					p.AppendLeft(BCHelper.ReflectSet);
				}
				else if (Target.ResultType is BTypeArray)
				{
					p.AppendLeft(CodePieceStore.WriteArrayFromStack((
						Target.ResultType as BTypeArray).Size,
						CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X,
						CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y,
						reversed));
				}
				else
					throw new WTFException();

				// Restore Variables

				p.AppendLeft(GenerateCode_varFrame_JumpBack(reversed));

				// Put ReturnValue Back to Stack

				if (popResult)
				{
					// Do nothing - no really ...
				}
				else if (Target.ResultType is BTypeVoid)
				{
					// DO nothing - Nobody cares about the result ...
				}
				else if (Target.ResultType is BTypeValue)
				{
					p.AppendLeft(NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X, reversed));
					p.AppendLeft(NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y, reversed));

					p.AppendLeft(BCHelper.ReflectGet);
				}
				else if (Target.ResultType is BTypeArray)
				{
					p.AppendLeft(CodePieceStore.ReadArrayToStack((
						Target.ResultType as BTypeArray).Size,
						CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X,
						CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y,
						reversed));
				}
				else
					throw new WTFException();

				#endregion

				#endregion
			}
			else
			{
				#region Normal

				// ######## BEFORE EXIT::JUMP-IN ########

				#region EXIT::JUMPIN

				// Put own Variables on Stack

				p.AppendRight(GenerateCode_varFrame_JumpIn(reversed));

				// Put own JumpBack-Adress on Stack

				p.AppendRight(NumberCodeHelper.GenerateCode(CodePointAddr, reversed));

				// Put Parameter on Stack

				for (int i = 0; i < CallParameter.Count; i++)
				{
					p.AppendRight(CallParameter[i].GenerateCode(reversed));
				}

				// Put TargetAdress on Stack

				p.AppendRight(NumberCodeHelper.GenerateCode(Target.MethodAddr, reversed));

				// Put Lane Switch on Stack

				p.AppendRight(BCHelper.Digit1);

				#endregion

				// ######## JUMPS ########

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag(Target)));
				p.AppendRight(BCHelper.PC_Right_tagged(new MethodCallVerticalReEntryTag(this)));

				// ######## AFTER ENTRY::JUMP-BACK ########

				#region ENTRY::JUMP-BACK

				// Store Result int TMP_RESULT Field

				if (popResult)
				{
					if (Target.ResultType is BTypeVoid)
					{
						p.AppendRight(BCHelper.StackPop);
					}
					else if (Target.ResultType is BTypeValue)
					{
						p.AppendRight(BCHelper.StackPop);
					}
					else if (Target.ResultType is BTypeArray)
					{
						p.AppendRight(CodePieceStore.PopMultipleStackValues((Target.ResultType as BTypeArray).Size, reversed));

					}
					else
						throw new WTFException();
				}
				else if (Target.ResultType is BTypeVoid)
				{
					p.AppendRight(BCHelper.StackPop); // Nobody cares about the result ...
				}
				else if (Target.ResultType is BTypeValue)
				{
					p.AppendRight(NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X, reversed));
					p.AppendRight(NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y, reversed));

					p.AppendRight(BCHelper.ReflectSet);
				}
				else if (Target.ResultType is BTypeArray)
				{
					p.AppendRight(CodePieceStore.WriteArrayFromStack((
						Target.ResultType as BTypeArray).Size,
						CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X,
						CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y,
						reversed));
				}
				else
					throw new WTFException();

				// Restore Variables

				p.AppendRight(GenerateCode_varFrame_JumpBack(reversed));

				// Put ReturnValue Back to Stack

				if (popResult)
				{
					// Do nothing - no really ...
				}
				else if (Target.ResultType is BTypeVoid)
				{
					// Do nothing - Nobody cares about the result ...
				}
				else if (Target.ResultType is BTypeValue)
				{
					p.AppendRight(NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X, reversed));
					p.AppendRight(NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y, reversed));

					p.AppendRight(BCHelper.ReflectGet);
				}
				else if (Target.ResultType is BTypeArray)
				{
					p.AppendRight(CodePieceStore.ReadArrayToStack((
						Target.ResultType as BTypeArray).Size,
						CodeGenConstants.TMP_ARRFIELD_RETURNVAL.X,
						CodeGenConstants.TMP_ARRFIELD_RETURNVAL.Y,
						reversed));
				}
				else
					throw new WTFException();

				#endregion

				#endregion
			}

			p.NormalizeX();

			return p;

		}

		private CodePiece GenerateCode_varFrame_JumpIn(bool initialReversed)
		{
			List<CodePiece> pieces = new List<CodePiece>();
			CodePiece current = new CodePiece();

			bool reversed = initialReversed;

			for (int i = 0; i < Owner.Variables.Count; i++)
			{
				if (Owner.Variables[i] is VarDeclarationValue)
				{
					VarDeclarationValue var = Owner.Variables[i] as VarDeclarationValue;

					if (reversed)
						current.AppendLeft(new ExpressionDirectValuePointer(Position, var).GenerateCode(reversed));
					else
						current.AppendRight(new ExpressionDirectValuePointer(Position, var).GenerateCode(reversed));
				}
				else if (Owner.Variables[i] is VarDeclarationArray)
				{
					VarDeclarationArray var = Owner.Variables[i] as VarDeclarationArray;

					if (reversed)
						current.AppendLeft(CodePieceStore.ReadArrayToStack(var, reversed));
					else
						current.AppendRight(CodePieceStore.ReadArrayToStack(var, reversed));
				}
				else
					throw new WTFException();

				if (current.Width >= CodeGenConstants.MAX_JUMPIN_VARFRAME_LENGTH)
				{
					pieces.Add(current);
					current = new CodePiece();
					reversed = !reversed;
				}
			}

			pieces.Add(current);

			if (pieces.Count == 0)
				return new CodePiece();
			if (pieces.Count == 1)
				return pieces[0];

			if (pieces.Count % 2 == 0)
				pieces.Add(new CodePiece());

			int maxlen = pieces.Max(lp => lp.Width);

			pieces.ForEach(lp => lp.ExtendWithWalkwayLeft(maxlen));
			pieces.ForEach(lp => lp.Normalize());


			if (initialReversed)
			{
				#region Reversed

				for (int i = 0; i < (pieces.Count - 1); i++)
					pieces[i][-1, 0] = (i % 2 == 0) ? BCHelper.PCDown : BCHelper.PCRight;

				for (int i = 1; i < pieces.Count; i++)
					pieces[i][maxlen, 0] = (i % 2 == 0) ? BCHelper.PCLeft : BCHelper.PCDown;


				pieces[0][maxlen, 0] = BCHelper.Walkway;
				pieces[pieces.Count - 1][-2, 0] = BCHelper.PCUp;
				pieces[pieces.Count - 1][-1, 0] = BCHelper.Walkway;
				pieces[0][-2, 0] = BCHelper.PCLeft;

				for (int i = 1; i < (pieces.Count - 1); i++)
					pieces[i][-2, 0] = BCHelper.Walkway;

				#endregion
			}
			else
			{
				#region Normal

				for (int i = 0; i < (pieces.Count - 1); i++)
					pieces[i][maxlen, 0] = (i % 2 == 0) ? BCHelper.PCDown : BCHelper.PCLeft;

				for (int i = 1; i < pieces.Count; i++)
					pieces[i][-1, 0] = (i % 2 == 0) ? BCHelper.PCRight : BCHelper.PCDown;

				pieces[0][-1, 0] = BCHelper.Walkway;
				pieces[pieces.Count - 1][maxlen + 1, 0] = BCHelper.PCUp;
				pieces[pieces.Count - 1][maxlen, 0] = BCHelper.Walkway;
				pieces[0][maxlen + 1, 0] = BCHelper.PCRight;

				for (int i = 1; i < (pieces.Count - 1); i++)
					pieces[i][maxlen + 1, 0] = BCHelper.Walkway;

				#endregion
			}

			return CodePiece.CreateFromVerticalList(pieces);
		}

		private CodePiece GenerateCode_varFrame_JumpBack(bool initialReversed)
		{
			List<CodePiece> pieces = new List<CodePiece>();
			CodePiece current = new CodePiece();

			bool reversed = initialReversed;

			for (int i = Owner.Variables.Count - 1; i >= 0; i--)
			{
				if (reversed)
					current.AppendLeft(Owner.Variables[i].GenerateCode_SetToStackVal(reversed));
				else
					current.AppendRight(Owner.Variables[i].GenerateCode_SetToStackVal(reversed));

				if (current.Width >= CodeGenConstants.MAX_JUMPIN_VARFRAME_LENGTH)
				{
					pieces.Add(current);
					current = new CodePiece();
					reversed = !reversed;
				}
			}

			pieces.Add(current);

			if (pieces.Count == 0)
				return new CodePiece();
			if (pieces.Count == 1)
				return pieces[0];

			if (pieces.Count % 2 == 0)
				pieces.Add(new CodePiece());

			int maxlen = pieces.Max(lp => lp.Width);

			pieces.ForEach(lp => lp.ExtendWithWalkwayLeft(maxlen));
			pieces.ForEach(lp => lp.Normalize());


			if (initialReversed)
			{
				#region Reversed

				for (int i = 0; i < (pieces.Count - 1); i++)
					pieces[i][-1, 0] = (i % 2 == 0) ? BCHelper.PCDown : BCHelper.PCRight;

				for (int i = 1; i < pieces.Count; i++)
					pieces[i][maxlen, 0] = (i % 2 == 0) ? BCHelper.PCLeft : BCHelper.PCDown;


				pieces[0][maxlen, 0] = BCHelper.Walkway;
				pieces[pieces.Count - 1][-2, 0] = BCHelper.PCUp;
				pieces[pieces.Count - 1][-1, 0] = BCHelper.Walkway;
				pieces[0][-2, 0] = BCHelper.PCLeft;

				for (int i = 1; i < (pieces.Count - 1); i++)
					pieces[i][-2, 0] = BCHelper.Walkway;

				#endregion
			}
			else
			{
				#region Normal

				for (int i = 0; i < (pieces.Count - 1); i++)
					pieces[i][maxlen, 0] = (i % 2 == 0) ? BCHelper.PCDown : BCHelper.PCLeft;

				for (int i = 1; i < pieces.Count; i++)
					pieces[i][-1, 0] = (i % 2 == 0) ? BCHelper.PCRight : BCHelper.PCDown;

				pieces[0][-1, 0] = BCHelper.Walkway;
				pieces[pieces.Count - 1][maxlen + 1, 0] = BCHelper.PCUp;
				pieces[pieces.Count - 1][maxlen, 0] = BCHelper.Walkway;
				pieces[0][maxlen + 1, 0] = BCHelper.PCRight;

				for (int i = 1; i < (pieces.Count - 1); i++)
					pieces[i][maxlen + 1, 0] = BCHelper.Walkway;

				#endregion
			}

			return CodePiece.CreateFromVerticalList(pieces);
		}
	}

	#endregion

	#region Keywords

	public class StatementLabel : Statement, ICodeAddressTarget
	{
		public readonly string Identifier;

		private int codePointAddr = -1;
		public int CodePointAddr
		{
			get
			{
				return codePointAddr;
			}
			set
			{
				throw new Exception(); // Not writeable
			}
		}

		public StatementLabel(SourceCodePosition pos, string ident)
			: base(pos)
		{
			if (ASTObject.IsKeyword(ident))
			{
				throw new IllegalIdentifierException(Position, ident);
			}

			this.Identifier = ident;
		}

		public override string GetDebugString()
		{
			return string.Format("#LABEL: {{{0}}}", CodePointAddr);
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			codePointAddr = CODEPOINT_ADDRESS_COUNTER;
		}

		public override void LinkVariables(Method owner)
		{
			//NOP
		}

		public override void InlineConstants()
		{
			//NOP
		}

		public override void LinkMethods(Program owner)
		{
			//NOP
		}

		public override void LinkResultTypes(Method owner)
		{
			//NOP
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override void EvaluateExpressions()
		{
			//NOP
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return ident.ToLower() == Identifier.ToLower() ? this : null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			if (reversed)
			{
				return new CodePiece(BCHelper.PC_Left_tagged(new MethodCallVerticalReEntryTag(this)));
			}
			else
			{
				return new CodePiece(BCHelper.PC_Right_tagged(new MethodCallVerticalReEntryTag(this)));
			}
		}
	}

	public class StatementGoto : Statement
	{
		public string TargetIdentifier; // Temporary - before linking
		public StatementLabel Target;

		public StatementGoto(SourceCodePosition pos, string id)
			: base(pos)
		{
			this.TargetIdentifier = id;
		}

		public override string GetDebugString()
		{
			return string.Format("#GOTO: {{{0}}}", Target.CodePointAddr);
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			Target = owner.FindLabelByIdentifier(TargetIdentifier);
			if (Target == null)
				throw new UnresolvableReferenceException(TargetIdentifier, Position);
			TargetIdentifier = null;
		}

		public override void InlineConstants()
		{
			//NOP
		}

		public override void LinkMethods(Program owner)
		{
			//NOP
		}

		public override void LinkResultTypes(Method owner)
		{
			//NOP
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			//NOP
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(NumberCodeHelper.GenerateCode(Target.CodePointAddr, reversed));

				p.AppendLeft(BCHelper.Digit0); // Right Lane

				p.AppendLeft(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag(Target)));
			}
			else
			{
				p.AppendRight(NumberCodeHelper.GenerateCode(Target.CodePointAddr, reversed));

				p.AppendRight(BCHelper.Digit0); // Right Lane

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag(Target)));
			}

			p.NormalizeX();

			return p;
		}
	}

	public class StatementReturn : Statement
	{
		public Expression Value;

		public BType ResultType;

		public StatementReturn(SourceCodePosition pos)
			: base(pos)
		{
			this.Value = new ExpressionVoidValuePointer(pos);
		}

		public StatementReturn(SourceCodePosition pos, Expression v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string GetDebugString()
		{
			return string.Format("#RETURN: {0}", Value.GetDebugString());
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			Value.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			Value = Value.InlineConstants();
		}

		public override void AddressCodePoints()
		{
			Value.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Value.LinkMethods(owner);
		}

		public override void LinkResultTypes(Method owner)
		{
			Value.LinkResultTypes(owner);

			BType present = Value.GetResultType();
			BType expected = owner.ResultType;

			if (present != expected)
			{
				if (present.IsImplicitCastableTo(expected))
					Value = new ExpressionCast(Value.Position, expected, Value);
				else
					throw new ImplicitCastException(Value.Position, present, expected);
			}

			ResultType = owner.ResultType;
		}

		public override bool AllPathsReturn()
		{
			return true;
		}

		public override void EvaluateExpressions()
		{
			Value = Value.EvaluateExpressions();
		}

		public override StatementReturn HasReturnStatement()
		{
			return this;
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			if (ResultType is BTypeVoid)
			{
				return GenerateCode_Void(reversed);
			}
			else if (ResultType is BTypeValue)
			{
				return GenerateCode_Value(reversed);
			}
			else if (ResultType is BTypeArray)
			{
				return GenerateCode_Array(reversed);
			}
			else
				throw new WTFException();
		}

		private CodePiece GenerateCode_Void(bool reversed)
		{
			CodePiece p = CodePiece.ParseFromLine(@"0\0");

			p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

			if (reversed)
				p.ReverseX(false);

			return p;

		}

		private CodePiece GenerateCode_Value(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				p.AppendRight(BCHelper.Digit0); // Right Lane

				p.AppendRight(BCHelper.StackSwap); // Swap BackjumpAddr back to Stack-Front

				p.AppendRight(Value.GenerateCode(reversed));

				#endregion
			}
			else
			{
				#region Normal

				p.AppendRight(Value.GenerateCode(reversed));

				p.AppendRight(BCHelper.StackSwap); // Swap BackjumpAddr back to Stack-Front

				p.AppendRight(BCHelper.Digit0); // Right Lane

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				#endregion

			}

			p.NormalizeX();
			return p;
		}

		private CodePiece GenerateCode_Array(bool reversed)
		{
			CodePiece p = new CodePiece();

			BTypeArray rType = ResultType as BTypeArray;

			if (reversed)
			{
				#region Reversed

				p.AppendLeft(Value.GenerateCode(reversed));


				// Switch ReturnValue (Array)  and  BackJumpAddr

				p.AppendLeft(CodePieceStore.WriteArrayFromStack(rType.Size, CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendLeft(CodePieceStore.WriteValueToField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));

				p.AppendLeft(CodePieceStore.ReadArrayToStack(rType.Size, CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendLeft(CodePieceStore.ReadValueFromField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));


				p.AppendLeft(BCHelper.Digit0); // Right Lane

				p.AppendLeft(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				#endregion
			}
			else
			{
				#region Normal

				p.AppendRight(Value.GenerateCode(reversed));


				// Switch ReturnValue (Array)  and  BackJumpAddr

				p.AppendRight(CodePieceStore.WriteArrayFromStack(rType.Size, CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendRight(CodePieceStore.WriteValueToField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));

				p.AppendRight(CodePieceStore.ReadArrayToStack(rType.Size, CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendRight(CodePieceStore.ReadValueFromField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));


				p.AppendRight(BCHelper.Digit0); // Right Lane

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				#endregion

			}

			p.NormalizeX();
			return p;
		}
	}

	public class StatementOut : Statement
	{
		public enum OutMode { OUT_INT, OUT_CHAR, OUT_CHAR_ARR };

		public Expression Value;

		public OutMode Mode;

		public StatementOut(SourceCodePosition pos, Expression v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string GetDebugString()
		{
			return string.Format("#OUT {0}", Value.GetDebugString());
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			Value.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			Value = Value.InlineConstants();
		}

		public override void AddressCodePoints()
		{
			Value.AddressCodePoints();
		}

		public override void LinkResultTypes(Method owner)
		{
			Value.LinkResultTypes(owner);

			BType r = Value.GetResultType();

			BTypeChar tChar = new BTypeChar(Position);
			BTypeInt tInt = new BTypeInt(Position);
			BTypeCharArr tChararr = (r is BTypeArray) ? new BTypeCharArr(Position, (r as BTypeArray).Size) : new BTypeCharArr(Position, 0);

			bool implToChar = r.IsImplicitCastableTo(tChar);
			bool implToInt = r.IsImplicitCastableTo(tInt);
			bool implToCharArr = (r is BTypeArray) && r.IsImplicitCastableTo(tChararr);

			if (implToInt)
			{
				Mode = OutMode.OUT_INT;

				if (r != tInt)
				{
					Value = new ExpressionCast(Position, tInt, Value);
				}
			}
			else if (implToChar)
			{
				Mode = OutMode.OUT_CHAR;

				if (r != tInt)
				{
					Value = new ExpressionCast(Position, tChar, Value);
				}
			}
			else if (implToCharArr)
			{
				Mode = OutMode.OUT_CHAR_ARR;

				if (r != tInt)
				{
					Value = new ExpressionCast(Position, tChararr, Value);
				}
			}
			else
			{
				throw new ImplicitCastException(Position, r, tInt, tChar, tChararr);
			}
		}

		public override void LinkMethods(Program owner)
		{
			Value.LinkMethods(owner);
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			Value = Value.EvaluateExpressions();
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			switch (Mode)
			{
				case OutMode.OUT_INT:
					return GenerateCode_Int(reversed);
				case OutMode.OUT_CHAR:
					return GenerateCode_Char(reversed);
				case OutMode.OUT_CHAR_ARR:
					return GenerateCode_CharArr(reversed);
				default:
					throw new WTFException();
			}
		}

		private CodePiece GenerateCode_Int(bool reversed)
		{
			CodePiece p = Value.GenerateCode(reversed);

			if (reversed)
				p.AppendLeft(BCHelper.OutInt);
			else
				p.AppendRight(BCHelper.OutInt);

			p.NormalizeX();

			return p;
		}

		private CodePiece GenerateCode_Char(bool reversed)
		{
			CodePiece p = Value.GenerateCode(reversed);

			if (reversed)
				p.AppendLeft(BCHelper.OutASCII);
			else
				p.AppendRight(BCHelper.OutASCII);

			p.NormalizeX();

			return p;
		}

		private CodePiece GenerateCode_CharArr(bool reversed)
		{
			BTypeCharArr typeRight = Value.GetResultType() as BTypeCharArr;

			CodePiece pLen = NumberCodeHelper.GenerateCode(typeRight.Size - 1, reversed);

			CodePiece pTpx = NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_FIELD_OUT_ARR.X, reversed);
			CodePiece pTpy = NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_FIELD_OUT_ARR.Y, reversed);

			CodePiece pTpxR = NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_FIELD_OUT_ARR.X, !reversed);
			CodePiece pTpyR = NumberCodeHelper.GenerateCode(CodeGenConstants.TMP_FIELD_OUT_ARR.Y, !reversed);


			if (reversed)
			{
				// $_v#!g{TY}:{TX}, <p{TY}{TX}{M}
				//   >:{TY}g1-\{TY}p^
				CodePiece p = new CodePiece();

				#region Reversed

				p.AppendRight(BCHelper.StackPop);
				p.AppendRight(BCHelper.IfHorizontal);

				p.AppendRight(BCHelper.PCDown);
				p[p.MaxX - 1, 1] = BCHelper.PCRight;

				CodePiece pTop = new CodePiece();
				{
					pTop.AppendRight(BCHelper.PCJump);
					pTop.AppendRight(BCHelper.Not);
					pTop.AppendRight(BCHelper.ReflectGet);

					pTop.AppendRight(pTpy);
					pTop.AppendRight(BCHelper.StackDup);
					pTop.AppendRight(pTpx);
					pTop.AppendRight(BCHelper.OutASCII);
				}

				CodePiece pBot = new CodePiece();
				{
					pBot.AppendRight(BCHelper.StackDup);

					pBot.AppendRight(pTpyR);
					pBot.AppendRight(BCHelper.ReflectGet);
					pBot.AppendRight(BCHelper.Digit1);
					pBot.AppendRight(BCHelper.Sub);
					pBot.AppendRight(BCHelper.StackSwap);

					pBot.AppendRight(pTpyR);

					pBot.AppendRight(BCHelper.ReflectSet);
				}

				int topBotStart = p.MaxX;
				int topBotEnd = topBotStart + Math.Max(pTop.Width, pBot.Width);

				p[topBotEnd + 0, 1] = BCHelper.PCUp;

				p[topBotEnd + 0, 0] = BCHelper.PCLeft;
				p[topBotEnd + 1, 0] = BCHelper.ReflectSet;

				p.AppendRight(pTpy);
				p.AppendRight(pTpx);
				p.AppendRight(pLen);

				p.SetAt(topBotStart, 0, pTop);
				p.SetAt(topBotStart, 1, pBot);

				p.FillRowWw(0, topBotStart + pTop.Width, topBotEnd);
				p.FillRowWw(1, topBotStart + pBot.Width, topBotEnd);

				p.AppendRight(Value.GenerateCode(reversed));

				#endregion

				return p;
			}
			else
			{
				// {M}{TX}{TY}p>,{TX}:{TY}g  #v_$
				//             ^p{TY}\-1g{TY}:<
				CodePiece p = Value.GenerateCode(reversed);

				#region Normal

				p.AppendRight(pLen);
				p.AppendRight(pTpx);
				p.AppendRight(pTpy);
				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(BCHelper.PCRight);
				p[p.MaxX - 1, 1] = BCHelper.PCUp;

				CodePiece pTop = new CodePiece();
				{
					pTop.AppendRight(BCHelper.OutASCII);
					pTop.AppendRight(pTpx);
					pTop.AppendRight(BCHelper.StackDup);
					pTop.AppendRight(pTpy);
					pTop.AppendRight(BCHelper.ReflectGet);
				}

				CodePiece pBot = new CodePiece();
				{
					pBot.AppendRight(BCHelper.ReflectSet);

					pBot.AppendRight(pTpyR);

					pBot.AppendRight(BCHelper.StackSwap);
					pBot.AppendRight(BCHelper.Sub);
					pBot.AppendRight(BCHelper.Digit1);
					pBot.AppendRight(BCHelper.ReflectGet);
					pBot.AppendRight(pTpyR);
				}

				int topBotStart = p.MaxX;
				int topBotEnd = topBotStart + Math.Max(pTop.Width, pBot.Width);

				p[topBotEnd + 0, 1] = BCHelper.StackDup;
				p[topBotEnd + 1, 1] = BCHelper.PCLeft;

				p[topBotEnd + 0, 0] = BCHelper.PCJump;
				p[topBotEnd + 1, 0] = BCHelper.PCDown;
				p[topBotEnd + 2, 0] = BCHelper.IfHorizontal;
				p[topBotEnd + 3, 0] = BCHelper.StackPop;

				p.SetAt(topBotStart, 0, pTop);
				p.SetAt(topBotStart, 1, pBot);

				p.FillRowWw(0, topBotStart + pTop.Width, topBotEnd);
				p.FillRowWw(1, topBotStart + pBot.Width, topBotEnd);

				#endregion

				return p;
			}
		}
	}

	public class StatementOutCharArrLiteral : Statement
	{
		public readonly LiteralCharArr Value;

		public StatementOutCharArrLiteral(SourceCodePosition pos, LiteralCharArr v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string GetDebugString()
		{
			return string.Format("#OUT {0}", Value.GetDebugString());
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			//NOP
		}

		public override void InlineConstants()
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			//NOP
		}

		public override void LinkResultTypes(Method owner)
		{
			//NOP
		}

		public override void LinkMethods(Program owner)
		{
			//NOP
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override void EvaluateExpressions()
		{
			//NOP
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			if (Value.Count == 0)
				return new CodePiece();

			if (reversed)
			{
				CodePiece p = new CodePiece();

				#region reversed

				if (Value.Value.Count <= 10)
				{
					// ,,,,,"???"
					for (int i = 0; i < Value.Value.Count; i++)
						p.AppendRight(BCHelper.OutASCII);

					p.AppendRight(Value.GenerateCode(reversed));
				}
				else
				{
					// $_>#!,#:<"???"0
					p.AppendLeft(BCHelper.Digit0);

					p.AppendLeft(Value.GenerateCode(reversed));

					p.AppendLeft(BCHelper.PCLeft);
					p.AppendLeft(BCHelper.StackDup);
					p.AppendLeft(BCHelper.PCJump);
					p.AppendLeft(BCHelper.OutASCII);
					p.AppendLeft(BCHelper.Not);
					p.AppendLeft(BCHelper.PCJump);
					p.AppendLeft(BCHelper.PCRight);
					p.AppendLeft(BCHelper.IfHorizontal);
					p.AppendLeft(BCHelper.StackPop);

					p.NormalizeX();
				}

				#endregion

				return p;
			}
			else
			{
				CodePiece p = new CodePiece();

				#region Normal

				if (Value.Value.Count <= 7)
				{
					// "???",,,,,
					p.AppendRight(Value.GenerateCode(reversed));

					for (int i = 0; i < Value.Value.Count; i++)
						p.AppendRight(BCHelper.OutASCII);
				}
				else
				{
					// 0"???">:#,_$
					p.AppendRight(BCHelper.Digit0);

					p.AppendRight(Value.GenerateCode(reversed));

					p.AppendRight(BCHelper.PCRight);
					p.AppendRight(BCHelper.StackDup);
					p.AppendRight(BCHelper.PCJump);
					p.AppendRight(BCHelper.OutASCII);
					p.AppendRight(BCHelper.IfHorizontal);
					p.AppendRight(BCHelper.StackPop);

					p.NormalizeX();
				}

				#endregion

				return p;
			}
		}
	}

	public class StatementIn : Statement
	{
		public enum InMode { IN_INT, IN_CHAR, IN_CHAR_ARR, IN_INT_ARR };

		public readonly ExpressionValuePointer ValueTarget;

		public InMode Mode;

		public StatementIn(SourceCodePosition pos, ExpressionValuePointer vt)
			: base(pos)
		{
			this.ValueTarget = vt;
		}

		public override string GetDebugString()
		{
			return string.Format("#IN {0}", ValueTarget.GetDebugString());
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			ValueTarget.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			ValueTarget.AddressCodePoints();
		}

		public override void LinkResultTypes(Method owner)
		{
			ValueTarget.LinkResultTypes(owner);

			BType present = ValueTarget.GetResultType();

			BType expecInt = new BTypeInt(Position);
			BType expecChar = new BTypeChar(Position);
			BType expecChararr = (present is BTypeArray) ? new BTypeCharArr(Position, (present as BTypeArray).Size) : new BTypeCharArr(Position, 0);
			BType expecIntarr = (present is BTypeArray) ? new BTypeIntArr(Position, (present as BTypeArray).Size) : new BTypeIntArr(Position, 0);

			if (present == expecChar)
			{
				Mode = InMode.IN_CHAR;
			}
			else if (present == expecInt)
			{
				Mode = InMode.IN_INT;
			}
			else if (present == expecChararr)
			{
				Mode = InMode.IN_CHAR_ARR;
			}
			else if (present == expecIntarr)
			{
				Mode = InMode.IN_INT_ARR;
			}
			else
			{
				throw new WrongTypeException(ValueTarget.Position, present, expecChar, expecInt, expecChararr, expecIntarr);
			}
		}

		public override void LinkMethods(Program owner)
		{
			ValueTarget.LinkMethods(owner);
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			//NOP
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			switch (Mode)
			{
				case InMode.IN_INT:
					return GenerateCode_Int(reversed);
				case InMode.IN_CHAR:
					return GenerateCode_Char(reversed);
				case InMode.IN_CHAR_ARR:
					return GenerateCode_CharArr(reversed);
				case InMode.IN_INT_ARR:
					return GenerateCode_IntArr(reversed);
				default:
					throw new WTFException();
			}
		}

		private CodePiece GenerateCode_Int(bool reversed)
		{
			CodePiece p = new CodePiece();

			p[0, 0] = BCHelper.InInt;

			if (reversed)
			{
				p.AppendLeft(ValueTarget.GenerateCodeSingle(reversed));
				p.AppendLeft(BCHelper.ReflectSet);
				p.NormalizeX();
			}
			else
			{
				p.AppendRight(ValueTarget.GenerateCodeSingle(reversed));
				p.AppendRight(BCHelper.ReflectSet);
				p.NormalizeX();
			}

			return p;
		}

		private CodePiece GenerateCode_Char(bool reversed)
		{
			CodePiece p = new CodePiece();

			p[0, 0] = BCHelper.InASCII;

			if (reversed)
			{
				p.AppendLeft(ValueTarget.GenerateCodeSingle(reversed));
				p.AppendLeft(BCHelper.ReflectSet);
				p.NormalizeX();
			}
			else
			{
				p.AppendRight(ValueTarget.GenerateCodeSingle(reversed));
				p.AppendRight(BCHelper.ReflectSet);
				p.NormalizeX();
			}

			return p;
		}

		private CodePiece GenerateCode_CharArr(bool reversed)
		{
			ExpressionDirectValuePointer vp = ValueTarget as ExpressionDirectValuePointer;
			int len = (ValueTarget.GetResultType() as BTypeArray).Size;

			CodePiece pLen = NumberCodeHelper.GenerateCode(len, reversed);
			CodePiece pWrite = CodePieceStore.WriteArrayFromReversedStack(len, vp.Target.CodePositionX, vp.Target.CodePositionY, reversed);

			if (reversed)
			{
				CodePiece p = CodePiece.ParseFromLine(@"$_>#!:$#-\#1\>#~<");

				p.AppendRight(pLen);
				p.AppendLeft(pWrite);

				p.NormalizeX();

				return p;
			}
			else
			{
				CodePiece p = CodePiece.ParseFromLine(@">~#<\1#\-#$:_$");

				p.AppendLeft(pLen);
				p.AppendRight(pWrite);

				p.NormalizeX();

				return p;
			}
		}

		private CodePiece GenerateCode_IntArr(bool reversed)
		{
			ExpressionDirectValuePointer vp = ValueTarget as ExpressionDirectValuePointer;
			int len = (ValueTarget.GetResultType() as BTypeArray).Size;

			CodePiece pLen = NumberCodeHelper.GenerateCode(len, reversed);
			CodePiece pWrite = CodePieceStore.WriteArrayFromReversedStack(len, vp.Target.CodePositionX, vp.Target.CodePositionY, reversed);

			if (reversed)
			{
				CodePiece p = CodePiece.ParseFromLine(@"$_>#!:$#-\#1\>#&<");

				p.AppendRight(pLen);
				p.AppendLeft(pWrite);

				p.NormalizeX();

				return p;
			}
			else
			{
				CodePiece p = CodePiece.ParseFromLine(@">&#<\1#\-#$:_$");

				p.AppendLeft(pLen);
				p.AppendRight(pWrite);

				p.NormalizeX();

				return p;
			}
		}
	}

	public class StatementQuit : Statement
	{
		public StatementQuit(SourceCodePosition pos)
			: base(pos)
		{
		}

		public override string GetDebugString()
		{
			return "#QUIT";
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			//NOP
		}

		public override void InlineConstants()
		{
			//NOP
		}

		public override void LinkResultTypes(Method owner)
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			//NOP
		}

		public override void LinkMethods(Program owner)
		{
			//NOP
		}

		public override bool AllPathsReturn()
		{
			return true;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			//NOP
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();

			p[0, 0] = BCHelper.Stop;

			return p;
		}
	}

	public class StatementNOP : Statement // NO OPERATION
	{
		public StatementNOP(SourceCodePosition pos)
			: base(pos)
		{
		}

		public override string GetDebugString()
		{
			return "#NOP";
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			//NOP
		}

		public override void InlineConstants()
		{
			//NOP
		}

		public override void LinkResultTypes(Method owner)
		{
			//NOP
		}

		public override void LinkMethods(Program owner)
		{
			//NOP
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			//NOP
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			return new CodePiece(); // easy as that ¯\_(ツ)_/¯
		}
	}

	#endregion Keywords

	#region Operations

	public class StatementInc : Statement
	{
		public readonly ExpressionValuePointer Target;

		public StatementInc(SourceCodePosition pos, ExpressionValuePointer id)
			: base(pos)
		{
			this.Target = id;
		}

		public override string GetDebugString()
		{
			return string.Format("#INC {0}", Target.GetDebugString());
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			Target.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			Target.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Target.LinkMethods(owner);
		}

		public override void LinkResultTypes(Method owner)
		{
			Target.LinkResultTypes(owner);

			BType present = Target.GetResultType();

			if (!(present == new BTypeInt(Position) || present == new BTypeDigit(Position) || present == new BTypeChar(Position)))
			{
				throw new WrongTypeException(Target.Position, present, new BTypeInt(Position), new BTypeDigit(Position), new BTypeChar(Position));
			}
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			//NOP
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(Target.GenerateCodeDoubleX(reversed));
				p.AppendLeft(BCHelper.ReflectGet);
				p.AppendLeft(BCHelper.Digit1);
				p.AppendLeft(BCHelper.Add);
				p.AppendLeft(BCHelper.StackSwap);
				p.AppendLeft(Target.GenerateCodeSingleY(reversed));
				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight(Target.GenerateCodeDoubleX(reversed));
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(BCHelper.Digit1);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(Target.GenerateCodeSingleY(reversed));
				p.AppendRight(BCHelper.ReflectSet);
			}

			p.NormalizeX();

			return p;
		}
	}

	public class StatementDec : Statement
	{
		public readonly ExpressionValuePointer Target;

		public StatementDec(SourceCodePosition pos, ExpressionValuePointer id)
			: base(pos)
		{
			this.Target = id;
		}

		public override string GetDebugString()
		{
			return string.Format("#DEC {0}", Target.GetDebugString());
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			Target.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			Target.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Target.LinkMethods(owner);
		}

		public override void LinkResultTypes(Method owner)
		{
			Target.LinkResultTypes(owner);

			BType present = Target.GetResultType();

			if (!(present == new BTypeInt(Position) || present == new BTypeDigit(Position) || present == new BTypeChar(Position)))
			{
				throw new WrongTypeException(Target.Position, present, new BTypeInt(Position), new BTypeDigit(Position), new BTypeChar(Position));
			}
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			//NOP
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(Target.GenerateCodeDoubleX(reversed));
				p.AppendLeft(BCHelper.ReflectGet);
				p.AppendLeft(BCHelper.Digit1);
				p.AppendLeft(BCHelper.Sub);
				p.AppendLeft(BCHelper.StackSwap);
				p.AppendLeft(Target.GenerateCodeSingleY(reversed));
				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight(Target.GenerateCodeDoubleX(reversed));
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(BCHelper.Digit1);
				p.AppendRight(BCHelper.Sub);
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(Target.GenerateCodeSingleY(reversed));
				p.AppendRight(BCHelper.ReflectSet);
			}

			p.NormalizeX();

			return p;
		}
	}

	public class StatementAssignment : Statement
	{
		public ExpressionValuePointer Target;
		public Expression Expr;

		public StatementAssignment(SourceCodePosition pos, ExpressionValuePointer t, Expression e)
			: base(pos)
		{
			this.Target = t;
			this.Expr = e;
		}

		public override string GetDebugString()
		{
			return string.Format("#ASSIGN {0} = ({1})", Target.GetDebugString(), Expr.GetDebugString());
		}

		public override void IntegrateStatementLists()
		{
			//NOP
		}

		public override void LinkVariables(Method owner)
		{
			Target.LinkVariables(owner);
			Expr.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			Target = (ExpressionValuePointer)Target.InlineConstants();
			Expr = Expr.InlineConstants();
		}

		public override void AddressCodePoints()
		{
			Target.AddressCodePoints();
			Expr.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Target.LinkMethods(owner);
			Expr.LinkMethods(owner);
		}

		public override void LinkResultTypes(Method owner)
		{
			Target.LinkResultTypes(owner);
			Expr.LinkResultTypes(owner);

			BType tLeft = Target.GetResultType();
			BType tRight = Expr.GetResultType();

			if (tLeft != tRight)
			{
				if (tRight.IsImplicitCastableTo(tLeft))
					Expr = new ExpressionCast(Expr.Position, tLeft, Expr);
				else
					throw new ImplicitCastException(Expr.Position, tRight, tLeft);
			}
		}

		public override bool AllPathsReturn()
		{
			return false;
		}

		public override StatementReturn HasReturnStatement()
		{
			return null;
		}

		public override void EvaluateExpressions()
		{
			Expr = Expr.EvaluateExpressions();
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			if (Target.GetResultType() is BTypeArray)
			{
				return GenerateCode_Array(reversed);
			}
			else if (Target.GetResultType() is BTypeValue)
			{
				return GenerateCode_Value(reversed);
			}
			else
			{
				throw new InvalidAstStateException(Position);
			}
		}

		private CodePiece GenerateCode_Value(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(Expr.GenerateCode(reversed));
				p.AppendLeft(Target.GenerateCodeSingle(reversed));

				p.AppendLeft(BCHelper.ReflectSet);

				p.NormalizeX();
			}
			else
			{
				p.AppendRight(Expr.GenerateCode(reversed));
				p.AppendRight(Target.GenerateCodeSingle(reversed));

				p.AppendRight(BCHelper.ReflectSet);

				p.NormalizeX();
			}

			return p;
		}

		private CodePiece GenerateCode_Array(bool reversed)
		{
			CodePiece p = new CodePiece();

			BTypeArray type = Target.GetResultType() as BTypeArray;
			ExpressionDirectValuePointer vPointer = Target as ExpressionDirectValuePointer;

			if (reversed)
			{
				p.AppendLeft(Expr.GenerateCode(reversed));
				p.AppendLeft(CodePieceStore.WriteArrayFromStack(type.Size, vPointer.Target.CodePositionX, vPointer.Target.CodePositionY, reversed));

				p.NormalizeX();
			}
			else
			{
				p.AppendRight(Expr.GenerateCode(reversed));
				p.AppendRight(CodePieceStore.WriteArrayFromStack(type.Size, vPointer.Target.CodePositionX, vPointer.Target.CodePositionY, reversed));

				p.NormalizeX();
			}

			return p;
		}
	}

	#endregion Operations

	#region Constructs

	public class StatementIf : Statement
	{
		public Expression Condition;
		public readonly StatementStatementList Body;
		public readonly Statement Else;

		public StatementIf(SourceCodePosition pos, Expression c, StatementStatementList b)
			: base(pos)
		{
			this.Condition = c;
			this.Body = b;
			this.Else = new StatementNOP(new SourceCodePosition());
		}

		public StatementIf(SourceCodePosition pos, Expression c, StatementStatementList b, Statement e)
			: base(pos)
		{
			this.Condition = c;
			this.Body = b;
			this.Else = e;
		}

		public override string GetDebugString()
		{
			return string.Format("#IF ({0})\n{1}\n#IFELSE\n{2}", Condition.GetDebugString(), Indent(Body.GetDebugString()), Else == null ? "  NULL" : Indent(Else.GetDebugString()));
		}

		public override void IntegrateStatementLists()
		{
			Body.IntegrateStatementLists();
			Else.IntegrateStatementLists();
		}

		public override void LinkVariables(Method owner)
		{
			Condition.LinkVariables(owner);
			Body.LinkVariables(owner);
			Else.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			Condition = Condition.InlineConstants();
			Body.InlineConstants();
			Else.InlineConstants();
		}

		public override void AddressCodePoints()
		{
			Condition.AddressCodePoints();
			Body.AddressCodePoints();
			Else.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Condition.LinkMethods(owner);
			Body.LinkMethods(owner);
			Else.LinkMethods(owner);
		}

		public override void LinkResultTypes(Method owner)
		{
			Condition.LinkResultTypes(owner);
			Body.LinkResultTypes(owner);
			Else.LinkResultTypes(owner);

			BType present = Condition.GetResultType();
			BType expected = new BTypeBool(Position);

			if (present != expected)
			{
				if (present.IsImplicitCastableTo(expected))
					Condition = new ExpressionCast(Condition.Position, expected, Condition);
				else
					throw new ImplicitCastException(Condition.Position, present, expected);
			}
		}

		public override bool AllPathsReturn()
		{
			return Body.AllPathsReturn() && Else.AllPathsReturn();
		}

		public override StatementReturn HasReturnStatement()
		{
			return Body.HasReturnStatement() ?? Else.HasReturnStatement();
		}

		public override void EvaluateExpressions()
		{
			Condition = Condition.EvaluateExpressions();

			Body.EvaluateExpressions();

			Else.EvaluateExpressions();
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			StatementLabel lBody = Body.FindLabelByIdentifier(ident);
			StatementLabel lElse = Else.FindLabelByIdentifier(ident);

			if (lBody != null && lElse != null)
				return null;

			return lBody ?? lElse;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p;

			if (Else.GetType() == typeof(StatementNOP))
			{
				p = GenerateCode_If(reversed);
			}
			else
			{
				p = GenerateCode_IfElse(reversed);
			}

			#region Extend MehodCall-Tags

			#region Entries

			p.NormalizeX();

			List<TagLocation> entries = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalReEntryTag));

			// Cant generate Path - because it would collide on the left side at X==0
			bool hasLeftOutCollisions = entries.Any(x => p[0, x.Y].Type == BefungeCommandType.Walkway || p[0, x.Y].Type == BefungeCommandType.NOP);

			if (hasLeftOutCollisions)
			{

				p[-1, 0] = BCHelper.Walkway;
				foreach (TagLocation entry in entries)
				{
					MethodCallHorizontalReEntryTag tagEntry = entry.Tag as MethodCallHorizontalReEntryTag;

					if (p[0, entry.Y].Type == BefungeCommandType.Walkway || p[0, entry.Y].Type == BefungeCommandType.NOP)
					{
						p.CreateRowWw(entry.Y, -1, entry.X);

						tagEntry.Deactivate();

						p.SetTag(-1, entry.Y, new MethodCallHorizontalReEntryTag(tagEntry.TagParam as ICodeAddressTarget), true);
					}
					else
					{
						p.CreateRowWw(entry.Y, 1, entry.X);
						p[-1, entry.Y] = BCHelper.PCJump;

						tagEntry.Deactivate();

						p.SetTag(-1, entry.Y, new MethodCallHorizontalReEntryTag(tagEntry.TagParam as ICodeAddressTarget), true);
					}
				}
				p.NormalizeX();
			}
			else
			{
				foreach (TagLocation entry in entries)
				{
					MethodCallHorizontalReEntryTag tagEntry = entry.Tag as MethodCallHorizontalReEntryTag;

					p.CreateRowWw(entry.Y, 0, entry.X);
					p[0, entry.Y] = BCHelper.PCJump;

					tagEntry.Deactivate();

					p.SetTag(0, entry.Y, new MethodCallHorizontalReEntryTag(tagEntry.TagParam as ICodeAddressTarget), true);
				}
			}

			#endregion

			#region Exits

			List<TagLocation> exits = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalExitTag));

			foreach (TagLocation exit in exits)
			{
				MethodCallHorizontalExitTag tagExit = exit.Tag as MethodCallHorizontalExitTag;

				if (p[p.MaxX - 1, exit.Y].Type == BefungeCommandType.Walkway || p[p.MaxX - 1, exit.Y].Type == BefungeCommandType.NOP)
				{
					p.CreateRowWw(exit.Y, exit.X + 1, p.MaxX);

					tagExit.Deactivate();

					p.SetTag(p.MaxX - 1, exit.Y, new MethodCallHorizontalExitTag(tagExit.TagParam), true);
				}
				else
				{
					p.CreateRowWw(exit.Y, exit.X + 1, p.MaxX - 2);

					p.ReplaceWalkway(p.MaxX - 2, exit.Y, BCHelper.PCJump, true);

					tagExit.Deactivate();

					p.SetTag(p.MaxX - 1, exit.Y, new MethodCallHorizontalExitTag(tagExit.TagParam), true);
				}


			}

			#endregion

			#endregion

			return p;
		}

		public CodePiece GenerateCode_If(bool reversed)
		{
			CodePiece cpCond = Condition.GenerateCode(reversed);
			cpCond.NormalizeX();

			CodePiece cpBodyIf = Body.GenerateCode(reversed);
			cpBodyIf.NormalizeX();

			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed

				// _v#!   CONDITION
				//  
				// 1>             v
				// 
				// ^      IF      <

				int right = Math.Max(cpCond.Width + 1, cpBodyIf.Width);
				int mid = cpCond.MaxY;
				int bot = (mid + 1) - cpBodyIf.MinY;

				// Top-Left '_v#!'
				p[-1, 0] = BCHelper.IfHorizontal;
				p[0, 0] = BCHelper.PCDown;
				p[1, 0] = BCHelper.PCJump;
				p[2, 0] = BCHelper.Not;
				// Mid_Left '0>'
				p[-1, mid] = BCHelper.Digit1;
				p[0, mid] = BCHelper.PCRight;
				// Mid-Right 'v'
				p[right, mid] = BCHelper.PCDown;
				// Bottom-Left '^'
				p[-1, bot] = BCHelper.PCUp;
				// Bottom-right '<'
				p[right, bot] = BCHelper.PCLeft;

				// Walkway Top (Condition -> end)
				p.FillRowWw(0, cpCond.Width + 3, right + 1);
				// Walkway Mid ('0>' -> 'v')
				p.FillRowWw(mid, 1, right);
				// Walkway Bot (Body_If -> '<')
				p.FillRowWw(bot, cpBodyIf.Width, right);
				// Walkway Left-Upper_1 ('_' -> '0')
				p.FillColWw(-1, 1, mid);
				// Walkway Left-Upper_2 ('v' -> '>')
				p.FillColWw(0, 1, mid);
				// Walkway Left-Lower ('0' -> '^')
				p.FillColWw(-1, mid + 1, bot);
				// Walkway Right-Lower ('v' -> '<')
				p.FillColWw(right, mid + 1, bot);

				// Set Condition
				p.SetAt(3, 0, cpCond);
				// Set Body
				p.SetAt(0, bot, cpBodyIf);

				#endregion
			}
			else
			{
				#region Normal

				// CONDITION #v_
				// 
				// v            <0
				// 
				// >   IF        ^

				int right = Math.Max(cpCond.Width, cpBodyIf.Width - 1);
				int mid = cpCond.MaxY;
				int bot = (mid + 1) - cpBodyIf.MinY;

				// Top-Right '#v_'
				p[right - 1, 0] = BCHelper.PCJump;
				p[right, 0] = BCHelper.PCDown;
				p[right + 1, 0] = BCHelper.IfHorizontal;
				// Mid-Left 'v'
				p[-1, mid] = BCHelper.PCDown;
				// Mid-Right '<0'
				p[right, mid] = BCHelper.PCLeft;
				p[right + 1, mid] = BCHelper.Digit0;
				// Bottom-Left '>'
				p[-1, bot] = BCHelper.PCRight;
				// Bottom-Right '^'
				p[right + 1, bot] = BCHelper.PCUp;

				// Walkway Top  (Condition -> '#v_')
				p.FillRowWw(0, cpCond.Width - 1, right - 1);
				// Walkway Mid  ('v' -> '<0')
				p.FillRowWw(mid, 0, right);
				// Walkway Bot  (Body_If -> '^')
				p.FillRowWw(bot, cpBodyIf.Width, right + 1);
				// Walkway Left-Lower  ('v' -> '>')
				p.FillColWw(-1, mid + 1, bot);
				// Walkway Right-Upper_1  ('v' -> '<')
				p.FillColWw(right, 1, mid);
				// Walkway Right-Upper_2  ('_' -> '0')
				p.FillColWw(right + 1, 1, mid);
				// Walkway Right-Lower  ('0' -> '^')
				p.FillColWw(right + 1, mid + 1, bot);

				// Set Condition
				p.SetAt(-1, 0, cpCond);
				// Set Body
				p.SetAt(0, bot, cpBodyIf);

				#endregion
			}

			return p;
		}

		public CodePiece GenerateCode_IfElse(bool reversed)
		{
			CodePiece cpCond = Condition.GenerateCode(reversed);
			cpCond.NormalizeX();

			CodePiece cpIf = Body.GenerateCode(reversed);
			cpIf.NormalizeX();

			CodePiece cpElse = Else.GenerateCode(reversed);
			cpElse.NormalizeX();

			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed

				// <v  CONDITION
				// 
				//  >          v
				// 
				//             #
				// ^     IF    <
				//             |
				// 
				// ^    ELSE   <

				int right = MathExt.Max(cpCond.Width, cpIf.Width, cpElse.Width) - 1;
				int mid = cpCond.MaxY;
				int yif = mid + MathExt.Max(-cpIf.MinY + 1, 2);
				int yelse = yif + MathExt.Max(cpIf.MaxY + -cpElse.MinY, 2);

				// Top-Left '<v'
				p[-2, 0] = BCHelper.PCLeft;
				p[-1, 0] = BCHelper.PCDown;
				// Mid-Left '>'
				p[-1, mid] = BCHelper.PCRight;
				// Mid-Right 'v'
				p[right, mid] = BCHelper.PCDown;
				// yif-Left '^'
				p[-2, yif] = BCHelper.PCUp;
				// yif-Right '#' '<' '|'
				p[right, yif - 1] = BCHelper.PCJump;
				p[right, yif] = BCHelper.PCLeft;
				p[right, yif + 1] = BCHelper.IfVertical;
				// yelse-Left '^'
				p[-2, yelse] = BCHelper.PCUp;
				// yelse-Right '<'
				p[right, yelse] = BCHelper.PCLeft;

				// Walkway Top (Condition -> end)
				p.FillRowWw(0, cpCond.Width, right + 1);
				// Walkway Mid ('>' -> 'v')
				p.FillRowWw(mid, 0, right);
				// Walkway yif (If -> '<')
				p.FillRowWw(yif, cpIf.Width - 1, right);
				// Walkway yelse (Else -> '<')
				p.FillRowWw(yelse, cpElse.Width - 1, right);
				// Walkway Left-Upper_1 ('<' -> '^')
				p.FillColWw(-2, 1, yif);
				// Walkway Left-Upper_2 ('v' -> '>')
				p.FillColWw(-1, 1, mid);
				// Walkway Left-Lower ('^' -> '^')
				p.FillColWw(-2, yif + 1, yelse);
				// Walkway Right-Upper ('v' -> '<')
				p.FillColWw(right, mid + 1, yif - 1);
				// Walkway Right-Lower ('<' -> '<')
				p.FillColWw(right, yif + 2, yelse);

				// Insert Condition
				p.SetAt(0, 0, cpCond);
				// Insert If
				p.SetAt(-1, yif, cpIf);
				// Insert Else
				p.SetAt(-1, yelse, cpElse);

				#endregion
			}
			else
			{
				#region Normal

				// CONDITION   v>
				// 
				// v           <
				// 
				// #
				// >     IF     ^
				// |
				// 
				// >    ELSE    ^

				int right = MathExt.Max(cpCond.Width, cpIf.Width, cpElse.Width) - 1;
				int mid = cpCond.MaxY;
				int yif = mid + MathExt.Max(-cpIf.MinY + 1, 2);
				int yelse = yif + MathExt.Max(cpIf.MaxY + -cpElse.MinY, 2);

				// Top-Right 'v>'
				p[right, 0] = BCHelper.PCDown;
				p[right + 1, 0] = BCHelper.PCRight;
				// Mid-Left 'v'
				p[-1, mid] = BCHelper.PCDown;
				// Mid-Right '<'
				p[right, mid] = BCHelper.PCLeft;
				// yif-Left '#' '>' '|'
				p[-1, yif - 1] = BCHelper.PCJump;
				p[-1, yif] = BCHelper.PCRight;
				p[-1, yif + 1] = BCHelper.IfVertical;
				// yif-Right '^'
				p[right + 1, yif] = BCHelper.PCUp;
				// yelse-Left '>'
				p[-1, yelse] = BCHelper.PCRight;
				// yelse-Right '^'
				p[right + 1, yelse] = BCHelper.PCUp;

				// Walkway Top (Condition -> 'v>')
				p.FillRowWw(0, cpCond.Width - 1, right);
				// Walkway Mid ('v' -> '>')
				p.FillRowWw(mid, 0, right);
				// Walkway yif (If -> '^')
				p.FillRowWw(yif, cpIf.Width, right + 1);
				// Walkway yelse (Else -> '^')
				p.FillRowWw(yelse, cpElse.Width, right + 1);
				// Walkway Left-Upper ('v' -> '#')
				p.FillColWw(-1, mid + 1, yif - 1);
				// Walkway Left-Lower ('|' -> '>')
				p.FillColWw(-1, yif + 2, yelse);
				// Walkway Right-Upper_1 ('v' -> '<')
				p.FillColWw(right, 1, mid);
				// Walkway Right-Upper_2 ('>' -> '^')
				p.FillColWw(right + 1, 1, yif);
				// Walkway Right-Lower ('^' -> '^')
				p.FillColWw(right + 1, yif + 1, yelse);

				// Insert Condition
				p.SetAt(-1, 0, cpCond);
				// Insert If
				p.SetAt(0, yif, cpIf);
				// Insert Else
				p.SetAt(0, yelse, cpElse);

				#endregion
			}

			return p;
		}
	}

	public class StatementWhile : Statement
	{
		public Expression Condition;
		public readonly StatementStatementList Body;

		public StatementWhile(SourceCodePosition pos, Expression c, StatementStatementList b)
			: base(pos)
		{
			this.Condition = c;
			this.Body = b;
		}

		public override string GetDebugString()
		{
			return string.Format("#WHILE ({0})\n{1}", Condition.GetDebugString(), Indent(Body.GetDebugString()));
		}

		public override void IntegrateStatementLists()
		{
			Body.IntegrateStatementLists();
		}

		public override void LinkVariables(Method owner)
		{
			Condition.LinkVariables(owner);
			Body.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			Condition = Condition.InlineConstants();
			Body.InlineConstants();
		}

		public override void AddressCodePoints()
		{
			Condition.AddressCodePoints();
			Body.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Condition.LinkMethods(owner);
			Body.LinkMethods(owner);
		}

		public override void LinkResultTypes(Method owner)
		{
			Condition.LinkResultTypes(owner);
			Body.LinkResultTypes(owner);

			BType present = Condition.GetResultType();
			BType expected = new BTypeBool(Position);

			if (present != expected)
			{
				if (present.IsImplicitCastableTo(expected))
					Condition = new ExpressionCast(Condition.Position, expected, Condition);
				else
					throw new ImplicitCastException(Condition.Position, present, expected);
			}
		}

		public override bool AllPathsReturn()
		{
			return false; // Its possible that the Body isnt executed at all
		}

		public override StatementReturn HasReturnStatement()
		{
			return Body.HasReturnStatement();
		}

		public override void EvaluateExpressions()
		{
			Condition = Condition.EvaluateExpressions();

			Body.EvaluateExpressions();
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return Body.FindLabelByIdentifier(ident);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece cpBody = Body.GenerateCode(!reversed);
			cpBody.NormalizeX();

			CodePiece cpCond = Condition.GenerateCode(reversed);
			cpCond.NormalizeX();

			if (reversed)
			{
				// _v#! CONDITION <
				//  >  STATEMENT  ^
				CodePiece p = new CodePiece();

				int top = cpBody.MinY - cpCond.MaxY;
				int right = Math.Max(cpBody.Width, cpCond.Width + 2);

				// Bottom-Left '>'
				p[-1, 0] = BCHelper.PCRight;
				// Top-Left '_v#!'
				p[-2, top] = BCHelper.IfHorizontal;
				p[-1, top] = BCHelper.PCDown;
				p[0, top] = BCHelper.PCJump;
				p[1, top] = BCHelper.Not;
				// Top-Right '<'
				p[right, top] = BCHelper.PCLeft;
				// Bottom Right '^'
				p[right, 0] = BCHelper.PCUp;

				// Fill Walkway between condition and Left
				p.FillRowWw(top, cpCond.Width + 2, right);
				// Fill Walkway between body and '<'
				p.FillRowWw(0, cpBody.Width, right);
				// Walkway Leftside Up
				p.FillColWw(-1, top + 1, 0);
				// Walkway righside down
				p.FillColWw(right, top + 1, 0);


				// Insert Condition
				p.SetAt(2, top, cpCond);
				// Insert Body
				p.SetAt(0, 0, cpBody);

				p.NormalizeX();
				p.AddYOffset(-top); // Set Offset relative to condition (and to insert/exit Points)

				return p;
			}
			else
			{
				// > CONDITION #v_
				// ^  STATEMENT <
				CodePiece p = new CodePiece();

				int top = cpBody.MinY - cpCond.MaxY;
				int right = Math.Max(cpBody.Width, cpCond.Width + 1);

				// Bottom-Left '^'
				p[-1, 0] = BCHelper.PCUp;
				// Top-Left '>'
				p[-1, top] = BCHelper.PCRight;
				// Tester Top-Right '#v_'
				p[right - 1, top] = BCHelper.PCJump;
				p[right, top] = BCHelper.PCDown;
				p[right + 1, top] = BCHelper.IfHorizontal;
				// Bottom Right '<'
				p[right, 0] = BCHelper.PCLeft;

				// Fill Walkway between condition and Tester
				p.FillRowWw(top, cpCond.Width, right - 1);
				// Fill Walkway between body and '<'
				p.FillRowWw(0, cpBody.Width, right);
				// Walkway Leftside Up
				p.FillColWw(-1, top + 1, 0);
				// Walkway righside down
				p.FillColWw(right, top + 1, 0);

				// Insert Condition
				p.SetAt(0, top, cpCond);
				// Insert Body
				p.SetAt(0, 0, cpBody);

				p.NormalizeX();
				p.AddYOffset(-top); // Set Offset relative to condition (and to insert/exit Points)

				return p;
			}
		}

		public static StatementStatementList GenerateForLoop(SourceCodePosition p, Statement init, Expression cond, Statement op, StatementStatementList body)
		{
			body.List.Add(op);

			StatementWhile sWhile = new StatementWhile(p, cond, body);

			return new StatementStatementList(p, new List<Statement>() { init, sWhile });
		}
	}

	public class StatementRepeatUntil : Statement
	{
		public Expression Condition;
		public readonly StatementStatementList Body;

		public StatementRepeatUntil(SourceCodePosition pos, Expression c, StatementStatementList b)
			: base(pos)
		{
			this.Condition = c;
			this.Body = b;
		}

		public override string GetDebugString()
		{
			return string.Format("#REPEAT-UNTIL ({0})\n{1}", Condition.GetDebugString(), Indent(Body.GetDebugString()));
		}

		public override void IntegrateStatementLists()
		{
			Body.IntegrateStatementLists();
		}

		public override void LinkVariables(Method owner)
		{
			Body.LinkVariables(owner);
			Condition.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			Body.InlineConstants();
			Condition = Condition.InlineConstants();
		}

		public override void AddressCodePoints()
		{
			Body.AddressCodePoints();
			Condition.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Body.LinkMethods(owner);
			Condition.LinkMethods(owner);
		}

		public override void LinkResultTypes(Method owner)
		{
			Body.LinkResultTypes(owner);
			Condition.LinkResultTypes(owner);

			BType present = Condition.GetResultType();
			BType expected = new BTypeBool(Position);

			if (present != expected)
			{
				if (present.IsImplicitCastableTo(expected))
					Condition = new ExpressionCast(Condition.Position, expected, Condition);
				else
					throw new ImplicitCastException(Condition.Position, present, expected);
			}
		}

		public override bool AllPathsReturn()
		{
			return Body.AllPathsReturn(); // Body is executed at least once
		}

		public override StatementReturn HasReturnStatement()
		{
			return Body.HasReturnStatement();
		}

		public override void EvaluateExpressions()
		{
			Body.EvaluateExpressions();

			Condition = Condition.EvaluateExpressions();
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			return Body.FindLabelByIdentifier(ident);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece cpBody = Body.GenerateCode(reversed);
			cpBody.NormalizeX();

			CodePiece cpCond = ExtendVerticalMcTagsUpwards(Condition.GenerateCode(!reversed));
			cpCond.NormalizeX();

			if (reversed)
			{
				// <v  STATEMENT  <
				// ^             _^
				//  >  CONDITION ^
				CodePiece p = new CodePiece();

				int mid = cpBody.MaxY;
				int bottom = (mid + 1) - cpCond.MinY;
				int right = Math.Max(cpBody.Width, cpCond.Width + 1);

				// Top-Left '<v'
				p[-2, 0] = BCHelper.PCLeft;
				p[-1, 0] = BCHelper.PCDown;
				// Top-Right '<'
				p[right, 0] = BCHelper.PCLeft;
				// Mid-Left '^'
				p[-2, mid] = BCHelper.PCUp;
				// Mid-Right '_^'
				p[right, mid] = BCHelper.PCUp;
				p[right - 1, mid] = BCHelper.IfHorizontal;
				//Bottom-Left '>'
				p[-1, bottom] = BCHelper.PCRight;
				//Bottom-Right '^'
				p[right - 1, bottom] = BCHelper.PCUp;

				// Walkway top (Statement to '<')
				p.FillRowWw(0, cpBody.Width, right);
				// Walkway bottom (Condition to '^')
				p.FillRowWw(bottom, cpCond.Width, right - 1);
				// Walkway left-lower ('<' to '^')
				p.FillColWw(-2, 1, mid);
				// Walkway left-full ('v' to '>')
				p.FillColWw(-1, 1, bottom);
				// Walkway right-lower ('^' to '_')
				p.FillColWw(right, 1, mid);
				// Walkway right-upper ('^' to '<')
				p.FillColWw(right - 1, mid + 1, bottom);
				// Walkway middle ('^' to '_^')
				p.FillRowWw(mid, 0, right - 1);

				// Insert Statement
				p.SetAt(0, 0, cpBody);
				// Inser Condition
				p.SetAt(0, bottom, cpCond);

				p.NormalizeX();

				return p;
			}
			else
			{
				// >  STATEMENT  v>
				// ^_             ^
				//  ^! CONDITION <
				CodePiece p = new CodePiece();

				int mid = cpBody.MaxY;
				int bottom = (mid + 1) - cpCond.MinY;
				int right = Math.Max(cpBody.Width, cpCond.Width + 2);

				// Top-Left '>'
				p[-1, 0] = BCHelper.PCRight;
				// Top-Right 'v>'
				p[right, 0] = BCHelper.PCDown;
				p[right + 1, 0] = BCHelper.PCRight;
				// Mid-Left '^_'
				p[0, mid] = BCHelper.IfHorizontal;
				p[-1, mid] = BCHelper.PCUp;
				// Mid-Right '^'
				p[right + 1, mid] = BCHelper.PCUp;
				//Bottom-Left '^!'
				p[0, bottom] = BCHelper.PCUp;
				p[1, bottom] = BCHelper.Not;
				//Bottom-Right '<'
				p[right, bottom] = BCHelper.PCLeft;

				// Walkway top (Statement to 'v')
				p.FillRowWw(0, cpBody.Width, right);
				// Walkway bottom (Condition to '<')
				p.FillRowWw(bottom, cpCond.Width + 2, right);
				// Walkway left-lower ('>' to '^')
				p.FillColWw(-1, 1, mid);
				// Walkway left-upper ('_' to '^')
				p.FillColWw(0, mid + 1, bottom);
				// Walkway right-lower ('>' to '^')
				p.FillColWw(right + 1, 1, mid);
				// Walkway right-full ('v' to '<')
				p.FillColWw(right, 1, bottom);
				// Walkway middle ('^_' to '^')
				p.FillRowWw(mid, 1, right);

				// Insert Statement
				p.SetAt(0, 0, cpBody);
				// Inser Condition
				p.SetAt(2, bottom, cpCond);

				p.NormalizeX();

				return p;
			}
		}
	}

	public class StatementSwitch : Statement
	{
		public Expression Condition;
		public List<SwitchCase> Cases;
		public Statement DefaultCase;

		public StatementSwitch(SourceCodePosition pos, Expression c, ListSwitchs lst)
			: base(pos)
		{
			this.Condition = c;

			Cases = lst.List.Where(p => p.Value != null).ToList();

			if (lst.List.Count(p => p.Value == null) == 1)
			{
				DefaultCase = lst.List.Single(p => p.Value == null).Body;
			}
			else
			{
				DefaultCase = new StatementNOP(pos);
			}
		}

		public override string GetDebugString() // Whoop Whoop - I can do it in one line
		{
			return
				string.Format("#SWITCH ({0}){1}{2}",
					Condition.GetDebugString(),
					Environment.NewLine,
					Indent(
						String.Join(
							Environment.NewLine + Environment.NewLine,
							Cases.Select(p =>
								p.Value.GetDebugString() +
								":" +
								Environment.NewLine +
								Indent(
									p.Body.GetDebugString()
								)
							)
						) +
						Environment.NewLine +
						"default:" +
						Environment.NewLine +
						Indent(
							DefaultCase.GetDebugString()
						)
					)
				);
		}

		public override void IntegrateStatementLists()
		{
			Cases.ForEach(p => p.Body.IntegrateStatementLists());
			DefaultCase.IntegrateStatementLists();
		}

		public override void LinkVariables(Method owner)
		{
			Condition.LinkVariables(owner);

			foreach (SwitchCase sc in Cases)
			{
				sc.Body.LinkVariables(owner);
			}

			DefaultCase.LinkVariables(owner);
		}

		public override void InlineConstants()
		{
			Condition = Condition.InlineConstants();

			foreach (SwitchCase sc in Cases)
			{
				sc.Body.InlineConstants();
			}

			DefaultCase.InlineConstants();
		}

		public override void AddressCodePoints()
		{
			Condition.AddressCodePoints();

			foreach (SwitchCase sc in Cases)
			{
				sc.Body.AddressCodePoints();
			}

			DefaultCase.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Condition.LinkMethods(owner);

			foreach (SwitchCase sc in Cases)
			{
				sc.Body.LinkMethods(owner);
			}

			DefaultCase.LinkMethods(owner);
		}

		public override void LinkResultTypes(Method owner)
		{
			Condition.LinkResultTypes(owner);

			foreach (SwitchCase sc in Cases)
			{
				sc.Body.LinkResultTypes(owner);
			}

			DefaultCase.LinkResultTypes(owner);

			// Test for correct Types: //
			//#########################//

			BType expected = Condition.GetResultType();

			foreach (SwitchCase sc in Cases)
			{
				BType present = sc.Value.GetBType();

				if (present != expected)
					throw new WrongTypeException(sc.Value.Position, present, expected);
			}

			// Test for duplicate Cases: //
			//###########################//

			foreach (SwitchCase sc in Cases)
			{
				if (Cases.Where(p => p != sc).Any(p => p.Value.ValueEquals(sc.Value)))
				{
					throw new DuplicateSwitchCaseException(sc.Value.Position, sc.Value);
				}
			}
		}

		public override bool AllPathsReturn()
		{
			bool result = true;

			foreach (SwitchCase sc in Cases)
			{
				result &= sc.Body.AllPathsReturn();
			}

			result &= DefaultCase.AllPathsReturn();

			return result; // Its possible that the Body isnt executed at all
		}

		public override StatementReturn HasReturnStatement()
		{
			foreach (SwitchCase sc in Cases)
			{
				StatementReturn r;
				if ((r = sc.Body.HasReturnStatement()) != null)
					return r;
			}
			return null;
		}

		public override void EvaluateExpressions()
		{
			Condition = Condition.EvaluateExpressions();

			foreach (SwitchCase sc in Cases)
				sc.Body.EvaluateExpressions();
		}

		public override StatementLabel FindLabelByIdentifier(string ident)
		{
			StatementLabel result = null;

			result = DefaultCase.FindLabelByIdentifier(ident);

			foreach (SwitchCase sc in Cases)
			{
				StatementLabel found = sc.Body.FindLabelByIdentifier(ident);
				if (found != null && result != null)
					return null;
				if (found != null && result == null)
					result = found;
			}

			return result;
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed
				//              v1<              v1<              v1<              v1<              
				//              >v+              >v+              >v+              >v+              
				//<v$<   {...}  \<|:-  3\3       \<|:-  2\2       \<|:-  1\1       \<|:- 0\0  {V}  0
				// >v             +                +                +                +              
				//   ^            <                <                <                <     
				// v_v
				// >v 
				//    ######
				//   > "0"   v
				//    ######
				//^          <
				// v_v
				// >v
				//    ######
				//   > "1" 
				//    ######
				// v_v
				// >v
				//  @ ######
				//   > "d" 
				//    ######
				p = GenerateCode_Top(reversed);
				int topWidth = p.Width;

				List<Statement> stmts = Cases.Select(pp => pp.Body).ToList();
				stmts.Add(DefaultCase);

				for (int i = 0; i < stmts.Count; i++)
				{
					CodePiece pStmt = stmts[i].GenerateCode(false);

					CodePiece turnout = CodePieceStore.SwitchLaneTurnout();

					pStmt.SetAt(pStmt.MinX - turnout.Width, pStmt.MinY - turnout.Height, turnout);

					pStmt.NormalizeX();

					pStmt[2, 0] = BCHelper.PCRight;
					pStmt.FillColWw(2, pStmt.MinY + 2, 0);

					if (i + 1 != stmts.Count)
						pStmt.FillColWw(1, pStmt.MinY + 2, pStmt.MaxY);
					else // last one
						pStmt[1, pStmt.MinY + 2] = BCHelper.Stop_tagged(new UnreachableTag());

					pStmt[pStmt.MaxX, 0] = BCHelper.PCDown;
					pStmt[pStmt.MaxX - 1, pStmt.MaxY] = BCHelper.PCLeft;
					pStmt.FillColWw(pStmt.MaxX - 1, 1, pStmt.MaxY - 1);
					pStmt.FillRowWw(pStmt.MaxY - 1, 0, pStmt.MaxX - 1);
					pStmt[-1, pStmt.MaxY - 1] = BCHelper.PC_Up_tagged(new SwitchStmtCaseExitTag());


					p.AppendBottom(pStmt);
				}

				p.FillRowWw(0, topWidth, p.Width);

				List<TagLocation> scExits = p.FindAllActiveCodeTags(typeof(SwitchStmtCaseExitTag)).OrderBy(pp => pp.Y).ToList();

				int lastY = 0;

				foreach (TagLocation exit in scExits)
				{
					p.CreateColWw(-1, lastY + 1, exit.Y);
					lastY = exit.Y;

					exit.Tag.Deactivate();
				}

				p[-1, 0] = BCHelper.PCLeft;

				p.NormalizeX();

				#endregion
			}
			else
			{
				#region Normal
				//               >1v              >1v     
				//               +v<              +v<     
				// 0  {V}  0\0 -:|>\       1\1  -:|>\   {...}   v>
				//               +                +             
				//               >                >             v
				//  v                                          $<
				// v_v
				// >v 
				//    ######
				//   > "0"                                       ^
				//    ######
				// v_v
				// >v
				//    ######
				//   > "1"                                       ^
				//    ######
				// v_v
				// >v
				//  @ ######
				//   > "d"                                       ^
				//    ######
				p = GenerateCode_Top(reversed);

				List<Statement> stmts = Cases.Select(pp => pp.Body).ToList();
				stmts.Add(DefaultCase);

				for (int i = 0; i < stmts.Count; i++)
				{
					CodePiece pStmt = stmts[i].GenerateCode(false);

					CodePiece turnout = CodePieceStore.SwitchLaneTurnout();

					pStmt.SetAt(pStmt.MinX - turnout.Width, pStmt.MinY - turnout.Height, turnout);

					pStmt.NormalizeX();

					pStmt[2, 0] = BCHelper.PCRight;
					pStmt.FillColWw(2, pStmt.MinY + 2, 0);

					if (i + 1 != stmts.Count)
						pStmt.FillColWw(1, pStmt.MinY + 2, pStmt.MaxY);
					else // last one
						pStmt[1, pStmt.MinY + 2] = BCHelper.Stop_tagged(new UnreachableTag());

					pStmt[pStmt.MaxX, 0] = BCHelper.PC_Right_tagged(new SwitchStmtCaseExitTag());

					p.AppendBottom(pStmt);
				}

				List<TagLocation> scExits = p.FindAllActiveCodeTags(typeof(SwitchStmtCaseExitTag)).OrderBy(pp => pp.Y).ToList();

				int rightLaneX = p.MaxX;

				p[rightLaneX, 0] = BCHelper.PCRight;

				int lastY = 0;

				foreach (TagLocation exit in scExits)
				{
					p[rightLaneX, exit.Y] = BCHelper.PCUp;
					p.CreateRowWw(exit.Y, exit.X + 1, rightLaneX);
					p.CreateColWw(rightLaneX, lastY + 1, exit.Y);
					lastY = exit.Y;

					exit.Tag.Deactivate();
				}

				#endregion
			}

			#region Extend MehodCall-Tags

			List<TagLocation> entries = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalReEntryTag));
			List<TagLocation> exits = p.FindAllActiveCodeTags(typeof(MethodCallHorizontalExitTag));

			foreach (TagLocation entry in entries)
			{
				MethodCallHorizontalReEntryTag tagEntry = entry.Tag as MethodCallHorizontalReEntryTag;

				p.CreateRowWw(entry.Y, p.MinX, entry.X);

				tagEntry.Deactivate();

				p.SetTag(p.MinX, entry.Y, new MethodCallHorizontalReEntryTag(tagEntry.TagParam as ICodeAddressTarget), true);
			}

			foreach (TagLocation exit in exits)
			{
				MethodCallHorizontalExitTag tagExit = exit.Tag as MethodCallHorizontalExitTag;

				p.CreateRowWw(exit.Y, exit.X + 1, p.MaxX);

				tagExit.Deactivate();

				p.SetTag(p.MaxX - 1, exit.Y, new MethodCallHorizontalExitTag(tagExit.TagParam), true);
			}

			#endregion

			p.NormalizeX();

			return p;
		}

		private CodePiece GenerateCode_Top(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed
				//              v1<              v1<              v1<              v1<              
				//              >v+              >v+              >v+              >v+              
				// v$<   {...}  \<|:-  3\3       \<|:-  2\2       \<|:-  1\1       \<|:- 0\0  {V}  0
				// >v             +                +                +                +              
				//   ^            <                <                <                <              

				p.AppendLeft(BCHelper.Digit0);

				p.AppendLeft(Condition.GenerateCode(reversed));

				for (int i = 0; i < Cases.Count; i++)
				{
					CodePiece pSc = new CodePiece();

					SwitchCase sc = Cases[i];

					pSc.AppendLeft(sc.Value.GenerateCode(reversed));
					pSc.AppendLeft(BCHelper.StackSwap);
					pSc.AppendLeft(sc.Value.GenerateCode(reversed));

					pSc.FillRowWw(2, pSc.MinX, pSc.MaxX);

					pSc.AppendLeft(CodePieceStore.SwitchStatementTester(reversed));

					p.AppendLeft(pSc);
				}

				CodePiece pDef = new CodePiece();

				pDef[0, 0] = BCHelper.PCDown;
				pDef[0, 1] = BCHelper.PCRight;

				pDef[1, 0] = BCHelper.StackPop;
				pDef[1, 1] = BCHelper.PCDown;
				pDef[1, 2] = BCHelper.Walkway;

				pDef[2, 0] = BCHelper.PCLeft;
				pDef[2, 1] = BCHelper.Walkway;
				pDef[2, 2] = BCHelper.PCUp;

				p.AppendLeft(pDef);

				#endregion
			}
			else
			{
				#region Normal
				//               >1v              >1v              >1v              >1v    
				//               +v<              +v<              +v<              +v<    
				// 0  {V}  0\0 -:|>\       1\1  -:|>\       2\2  -:|>\       3\3  -:|>\  {...}   v
				//               +                +                +                +            
				//               >                >                >                >            v
				//  v                                                                           $<

				p.AppendRight(BCHelper.Digit0);

				p.AppendRight(Condition.GenerateCode(reversed));

				for (int i = 0; i < Cases.Count; i++)
				{
					CodePiece pSc = new CodePiece();

					SwitchCase sc = Cases[i];

					pSc.AppendRight(sc.Value.GenerateCode(reversed));
					pSc.AppendRight(BCHelper.StackSwap);
					pSc.AppendRight(sc.Value.GenerateCode(reversed));

					pSc.FillRowWw(2, pSc.MinX, pSc.MaxX);

					pSc.AppendRight(CodePieceStore.SwitchStatementTester(reversed));

					p.AppendRight(pSc);
				}

				CodePiece pDef = new CodePiece();

				pDef[0, 0] = BCHelper.PCDown;
				pDef[0, 1] = BCHelper.Walkway;
				pDef[0, 2] = BCHelper.PCDown;
				pDef[0, 3] = BCHelper.PCLeft;

				p.AppendRight(pDef);


				p[1, 3] = BCHelper.PCDown;
				p[p.MaxX - 2, 3] = BCHelper.StackPop;

				p.FillRowWw(3, 2, p.MaxX - 2);

				#endregion
			}

			p.NormalizeX();
			return p;
		}
	}

	#endregion Constructs
}