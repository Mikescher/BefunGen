using System;
using BefunGen.AST.CodeGen;
using BefunGen.AST.CodeGen.NumberCode;
using BefunGen.AST.DirectRun;
using BefunGen.AST.Exceptions;
using BefunGen.MathExtensions;

namespace BefunGen.AST
{
	public abstract class Expression : ASTObject
	{
		public Expression(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public long? GetValueLiteral_Value()
		{
			if (this is ExpressionLiteral && (this as ExpressionLiteral).Value is LiteralValue)
				return ((this as ExpressionLiteral).Value as LiteralValue).GetValueAsInt();
			else
				return null;
		}

		public bool? GetValueLiteral_Bool_Value()
		{
			if (this is ExpressionLiteral && (this as ExpressionLiteral).Value is LiteralBool)
				return ((this as ExpressionLiteral).Value as LiteralBool).Value;
			else
				return null;
		}

		public abstract void LinkVariables(Method owner);
		public abstract void LinkResultTypes(Method owner);
		public abstract void LinkMethods(Program owner);
		public abstract void AddressCodePoints();

		public abstract Expression InlineConstants();
		public abstract Expression EvaluateExpressions();

		public abstract BType GetResultType();

		public abstract CodePiece GenerateCode(CodeGenEnvironment env, bool reversed);

		public abstract long EvaluateDirect(RunnerEnvironment env);
	}

	#region Parents

	public abstract class ExpressionBinary : Expression
	{
		public Expression Left;
		public Expression Right;

		public ExpressionBinary(SourceCodePosition pos, Expression l, Expression r)
			: base(pos)
		{
			this.Left = l;
			this.Right = r;
		}

		public override void LinkVariables(Method owner)
		{
			Left.LinkVariables(owner);
			Right.LinkVariables(owner);
		}

		public override Expression InlineConstants()
		{
			Left = Left.InlineConstants();
			Right = Right.InlineConstants();

			return this;
		}

		public override void AddressCodePoints()
		{
			Left.AddressCodePoints();
			Right.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Left.LinkMethods(owner);
			Right.LinkMethods(owner);
		}

		public void EvaluateSubExpressions()
		{
			Left = Left.EvaluateExpressions();
			Right = Right.EvaluateExpressions();
		}
	}

	public abstract class ExpressionBinaryMathOperation : ExpressionBinary
	{
		public ExpressionBinaryMathOperation(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
		}

		public override void LinkResultTypes(Method owner)
		{
			Left.LinkResultTypes(owner);
			Right.LinkResultTypes(owner);

			BType presentL = Left.GetResultType();
			BType wantedL = new BTypeInt(Position);

			BType presentR = Right.GetResultType();
			BType wantedR = new BTypeInt(Position);

			if (presentL != wantedL)
			{
				if (presentL.IsImplicitCastableTo(wantedL))
					Left = new ExpressionCast(Position, wantedL, Left);
				else
					throw new ImplicitCastException(Position, presentL, wantedL);
			}

			if (presentR != wantedR)
			{
				if (presentR.IsImplicitCastableTo(wantedR))
					Right = new ExpressionCast(Position, wantedR, Right);
				else
					throw new ImplicitCastException(Position, presentR, wantedR);
			}
		}

		public override BType GetResultType()
		{
			if (Left.GetResultType() != Right.GetResultType())
				throw new InvalidAstStateException(Position);

			return Left.GetResultType();
		}

		protected CodePiece GenerateCode_Operands(CodeGenEnvironment env, bool reversed, BefungeCommand cmd)
		{
			CodePiece cpL = Left.GenerateCode(env, reversed);
			CodePiece cpR = Right.GenerateCode(env, reversed);

			if (reversed)
			{
				MathExt.Swap(ref cpL, ref cpR); // In Reverse Mode l & r are reversed and then they are reversed added
			}

			if (CGO.StripDoubleStringmodeToogle)
			{
				if (cpL.LastColumnIsSingle() && cpR.FirstColumnIsSingle() && cpL[cpL.MaxX - 1, 0].Type == BefungeCommandType.Stringmode && cpR[0, 0].Type == BefungeCommandType.Stringmode)
				{
					cpL.RemoveColumn(cpL.MaxX - 1);
					cpR.RemoveColumn(0);
				}
			}

			CodePiece p = CodePiece.CombineHorizontal(cpL, cpR);

			if (reversed)
			{
				p.AppendLeft(cmd);
			}
			else
			{
				p.AppendRight(cmd);
			}

			p.NormalizeX();

			return p;
		}
	}

	public abstract class ExpressionBinaryBoolOperation : ExpressionBinary
	{
		public ExpressionBinaryBoolOperation(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
		}

		public override void LinkResultTypes(Method owner)
		{
			Left.LinkResultTypes(owner);
			Right.LinkResultTypes(owner);

			BType presentL = Left.GetResultType();
			BType wantedL = new BTypeBool(Position);

			BType presentR = Right.GetResultType();
			BType wantedR = new BTypeBool(Position);

			if (presentL != wantedL)
			{
				if (presentL.IsImplicitCastableTo(wantedL))
					Left = new ExpressionCast(Position, wantedL, Left);
				else
					throw new ImplicitCastException(Position, presentL, wantedL);
			}

			if (presentR != wantedR)
			{
				if (presentR.IsImplicitCastableTo(wantedR))
					Right = new ExpressionCast(Position, wantedR, Right);
				else
					throw new ImplicitCastException(Position, presentR, wantedR);
			}
		}

		public override BType GetResultType()
		{
			if (Left.GetResultType() != Right.GetResultType())
				throw new InvalidAstStateException(Position);

			return Left.GetResultType();
		}
	}

	public abstract class ExpressionCompare : ExpressionBinary
	{
		public ExpressionCompare(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override void LinkResultTypes(Method owner)
		{
			Left.LinkResultTypes(owner);
			Right.LinkResultTypes(owner);

			BType presentL = Left.GetResultType();

			BType presentR = Right.GetResultType();

			if (!(presentL is BTypeValue) || !(presentR is BTypeValue))
				throw new InvalidCompareException(presentL, presentR, Position);

			if (presentL != presentR)
			{
				if (presentR.IsImplicitCastableTo(presentL) && presentL.IsImplicitCastableTo(presentR))
				{
					if (presentR.GetPriority() > presentL.GetPriority())
						Right = new ExpressionCast(Position, presentL, Right);
					else
						Left = new ExpressionCast(Position, presentR, Left);
				}
				else if (presentR.IsImplicitCastableTo(presentL))
				{
					Right = new ExpressionCast(Position, presentL, Right);
				}
				else if (presentL.IsImplicitCastableTo(presentR))
				{
					Left = new ExpressionCast(Position, presentR, Left);
				}
				else
				{
					throw new InvalidCompareException(presentL, presentR, Position);
				}
			}
		}

		public override BType GetResultType()
		{
			if (Left.GetResultType() != Right.GetResultType())
				throw new InvalidAstStateException(Position);

			return new BTypeBool(new SourceCodePosition());
		}
	}

	public abstract class ExpressionUnary : Expression
	{
		public Expression Expr;

		public ExpressionUnary(SourceCodePosition pos, Expression e)
			: base(pos)
		{
			this.Expr = e;
		}

		public override void LinkVariables(Method owner)
		{
			Expr.LinkVariables(owner);
		}

		public override Expression InlineConstants()
		{
			Expr = Expr.InlineConstants();

			return this;
		}

		public override void AddressCodePoints()
		{
			Expr.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Expr.LinkMethods(owner);
		}

		public void EvaluateSubExpressions()
		{
			Expr = Expr.EvaluateExpressions();
		}
	}

	public abstract class ExpressionValuePointer : Expression
	{
		public ExpressionValuePointer(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override void LinkMethods(Program owner)
		{
			//NOP
		}

		public override void AddressCodePoints()
		{
			//NOP
		}

		// Puts X and Y on the stack: [X, Y]
		public abstract CodePiece GenerateCodeSingle(CodeGenEnvironment env, bool reversed);
		// Puts X and Y 1 1/2 -times on the stack: [X, X, Y]
		public abstract CodePiece GenerateCodeDoubleX(CodeGenEnvironment env, bool reversed);
		// Puts Y on the stack: [Y]
		public abstract CodePiece GenerateCodeSingleY(CodeGenEnvironment env, bool reversed);

		public abstract void EvaluateSetDirect(RunnerEnvironment env, long newValue);
	}

	public abstract class ExpressionRand : Expression
	{
		public ExpressionRand(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}
	}

	public abstract class ExpressionCrement : Expression
	{
		public ExpressionValuePointer Target;

		public ExpressionCrement(SourceCodePosition pos, ExpressionValuePointer v)
			: base(pos)
		{
			Target = v;
		}

		public override Expression EvaluateExpressions()
		{
			return this;
		}

		public override void LinkVariables(Method owner)
		{
			Target.LinkVariables(owner);
		}

		public override Expression InlineConstants()
		{
			Target.InlineConstants();
			return this;
		}

		public override void AddressCodePoints()
		{
			Target.AddressCodePoints();
		}

		public override void LinkMethods(Program owner)
		{
			Target.LinkMethods(owner);
		}

		public override BType GetResultType()
		{
			return Target.GetResultType();
		}

		public override void LinkResultTypes(Method owner)
		{
			Target.LinkResultTypes(owner);

			if (!(Target.GetResultType() is BTypeInt || Target.GetResultType() is BTypeDigit || Target.GetResultType() is BTypeChar))
				throw new ImplicitCastException(Position, Target.GetResultType(), new BTypeInt(Position), new BTypeDigit(Position), new BTypeChar(Position));
		}
	}

	#endregion

	#region ValuePointer

	public class ExpressionDirectValuePointer : ExpressionValuePointer
	{
		public string Identifier; // Temporary -- before linking;
		public VarDeclaration Target; // Could also be an array without index

		public ExpressionDirectValuePointer(SourceCodePosition pos, string id)
			: base(pos)
		{
			this.Identifier = id;
		}

		public ExpressionDirectValuePointer(SourceCodePosition pos, VarDeclaration target)
			: base(pos)
		{
			this.Identifier = null;
			this.Target = target;
		}

		public override string GetDebugString()
		{
			return Target.GetShortDebugString();
		}

		public override void LinkVariables(Method owner)
		{
			if (Target != null && Identifier == null) // Already linked
				return;

			Target = owner.FindVariableByIdentifier(Identifier) as VarDeclaration;

			if (Target == null)
				throw new UnresolvableReferenceException(Identifier, Position);

			Identifier = null;
		}

		public override Expression InlineConstants()
		{
			if (Target.IsConstant)
			{
				return new ExpressionLiteral(Position, Target.Initial);
			}
			else
			{
				return this;
			}
		}

		public override void LinkResultTypes(Method owner)
		{
			//NOP
		}

		public override BType GetResultType()
		{
			return Target.Type;
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			return Target.Type.GenerateCodeReadFromGridToStack(env, Position, Target.CodeDeclarationPos, reversed);
		}

		public override Expression EvaluateExpressions()
		{
			return this;
		}

		// Puts X and Y on the stack: [X, Y]
		public override CodePiece GenerateCodeSingle(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed));
				p.AppendLeft(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed));
			}
			else
			{
				p.AppendRight(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed));
				p.AppendRight(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed));
			}

			p.NormalizeX();

			return p;
		}

		// Puts X and Y 1 1/2 -times on the stack: [X, X, Y]
		public override CodePiece GenerateCodeDoubleX(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed));
				p.AppendLeft(BCHelper.StackDup);
				p.AppendLeft(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed));
			}
			else
			{
				p.AppendRight(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed));
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed));
			}

			p.NormalizeX();

			return p;
		}

		/// Puts Y on the stack: [Y]
		public override CodePiece GenerateCodeSingleY(CodeGenEnvironment env, bool reversed)
		{
			return NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed);
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			return env.GetVariableNumber(Target);
		}

		public override void EvaluateSetDirect(RunnerEnvironment env, long newValue)
		{
			env.SetVariable(Target, newValue);
		}
	}

	public class ExpressionDisplayValuePointer : ExpressionDirectValuePointer
	{
		private Program owner;

		public Expression TargetX;
		public Expression TargetY;

		public ExpressionDisplayValuePointer(SourceCodePosition pos, Expression x, Expression y)
			: base(pos, "@display")
		{
			Target = null;

			TargetX = x;
			TargetY = y;
		}

		public override string GetDebugString()
		{
			return string.Format(@"DISPLAY[{0}][{1}]", TargetX.GetDebugString(), TargetY.GetDebugString());
		}

		public override void LinkVariables(Method owner)
		{
			this.owner = owner.Owner;

			if (this.owner.DisplayWidth * this.owner.DisplayHeight == 0)
				throw new EmptyDisplayAccessException(Position);

			TargetX.LinkVariables(owner);
			TargetY.LinkVariables(owner);
		}

		public override Expression InlineConstants()
		{
			TargetX = TargetX.InlineConstants();
			TargetY = TargetY.InlineConstants();
			return this;
		}

		public override void LinkResultTypes(Method owner)
		{
			TargetX.LinkResultTypes(owner);
			TargetY.LinkResultTypes(owner);

			BType wanted = new BTypeInt(Position);
			BType presentX = TargetX.GetResultType();
			BType presentY = TargetY.GetResultType();

			if (presentX != wanted)
			{
				if (presentX.IsImplicitCastableTo(wanted))
					TargetX = new ExpressionCast(Position, wanted, TargetX);
				else
					throw new ImplicitCastException(Position, presentX, wanted);
			}

			if (presentY != wanted)
			{
				if (presentY.IsImplicitCastableTo(wanted))
					TargetY = new ExpressionCast(Position, wanted, TargetY);
				else
					throw new ImplicitCastException(Position, presentY, wanted);
			}
		}

		public override Expression EvaluateExpressions()
		{
			TargetX = TargetX.EvaluateExpressions();
			TargetY = TargetY.EvaluateExpressions();

			return this;
		}

		public override BType GetResultType()
		{
			return new BTypeChar(Position);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = GenerateCodeSingle(env, reversed);

			if (reversed)
			{
				p.AppendLeft(BCHelper.ReflectGet);
			}
			else
			{
				p.AppendRight(BCHelper.ReflectGet);
			}

			p.NormalizeX();

			return p;
		}

		// Puts X and Y on the stack: [X, Y]
		public override CodePiece GenerateCodeSingle(CodeGenEnvironment env, bool reversed)
		{
			if (reversed)
			{
				return CodePiece.CombineHorizontal(GenerateCodeSingleY(env, reversed), GenerateCodeSingleX(env, reversed));
			}
			else
			{
				return CodePiece.CombineHorizontal(GenerateCodeSingleX(env, reversed), GenerateCodeSingleY(env, reversed));
			}
		}

		// Puts X and Y 1 1/2 -times on the stack: [X, X, Y]
		public override CodePiece GenerateCodeDoubleX(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(GenerateCodeSingleX(env, reversed));
				p.AppendLeft(BCHelper.StackDup);
				p.AppendLeft(GenerateCodeSingleY(env, reversed));
			}
			else
			{
				p.AppendRight(GenerateCodeSingleX(env, reversed));
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(GenerateCodeSingleY(env, reversed));
			}

			p.NormalizeX();

			return p;
		}

		/// Puts X on the stack: [X]
		public CodePiece GenerateCodeSingleX(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed
				//  +{(MODULO)}{TargetX}{OffsetX}

				p.AppendLeft(NumberCodeHelper.GenerateCode(owner.DisplayOffsetX));
				p.AppendLeft(TargetX.GenerateCode(env, reversed));
				if (CGO.DisplayModuloAccess)
					p.AppendLeft(CodePieceStore.ModuloRangeLimiter(owner.DisplayWidth, reversed));
				p.AppendLeft(BCHelper.Add);

				#endregion
			}
			else
			{
				#region Normal
				//  {OffsetX}{TargetX}{(MODULO)}+

				p.AppendRight(NumberCodeHelper.GenerateCode(owner.DisplayOffsetX));
				p.AppendRight(TargetX.GenerateCode(env, reversed));
				if (CGO.DisplayModuloAccess)
					p.AppendRight(CodePieceStore.ModuloRangeLimiter(owner.DisplayWidth, reversed));
				p.AppendRight(BCHelper.Add);

				#endregion
			}

			p.NormalizeX();

			return p;
		}

		/// Puts Y on the stack: [Y]
		public override CodePiece GenerateCodeSingleY(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed
				//  +{(MODULO)}{TargetY}{OffsetY}

				p.AppendLeft(NumberCodeHelper.GenerateCode(owner.DisplayOffsetY));
				p.AppendLeft(TargetY.GenerateCode(env, reversed));
				if (CGO.DisplayModuloAccess)
					p.AppendLeft(CodePieceStore.ModuloRangeLimiter(owner.DisplayHeight, reversed));
				p.AppendLeft(BCHelper.Add);

				#endregion
			}
			else
			{
				#region Normal
				//  {OffsetY}{TargetY}{(MODULO)}+

				p.AppendRight(NumberCodeHelper.GenerateCode(owner.DisplayOffsetY));
				p.AppendRight(TargetY.GenerateCode(env, reversed));
				if (CGO.DisplayModuloAccess)
					p.AppendRight(CodePieceStore.ModuloRangeLimiter(owner.DisplayHeight, reversed));
				p.AppendRight(BCHelper.Add);

				#endregion
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var xx = TargetX.EvaluateDirect(env);
			var yy = TargetY.EvaluateDirect(env);

			return env.GetDisplay(xx, yy);
		}
	}

	public class ExpressionArrayValuePointer : ExpressionValuePointer
	{
		public string Identifier; // Temporary - before linkng
		public VarDeclarationArray Target;

		public Expression Index;

		public ExpressionArrayValuePointer(SourceCodePosition pos, string id, Expression idx)
			: base(pos)
		{
			this.Identifier = id;
			this.Index = idx;
		}

		public override string GetDebugString()
		{
			return Target.GetShortDebugString() + "[" + Index.GetDebugString() + "]";
		}

		public override void LinkVariables(Method owner)
		{
			if (Target != null && Identifier == null) // Already linked
				return;

			Index.LinkVariables(owner);

			Target = owner.FindVariableByIdentifier(Identifier) as VarDeclarationArray;

			if (Target == null)
				throw new UnresolvableReferenceException(Identifier, Position);
			if (!typeof(BTypeArray).IsAssignableFrom(Target.Type.GetType()))
				throw new IndexOperatorNotDefiniedException(Position);

			Identifier = null;
		}

		public override Expression InlineConstants()
		{
			Index = Index.InlineConstants();

			return this;
		}

		public override void LinkResultTypes(Method owner)
		{
			Index.LinkResultTypes(owner);

			BType present = Index.GetResultType();
			BType wanted = new BTypeInt(Position);

			if (present != wanted)
			{
				if (present.IsImplicitCastableTo(wanted))
					Index = new ExpressionCast(Position, wanted, Index);
				else
					throw new ImplicitCastException(Position, present, wanted);
			}
		}

		public override Expression EvaluateExpressions()
		{
			Index = Index.EvaluateExpressions();
			return this;
		}

		public override BType GetResultType()
		{
			return Target.InternalType;
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (Target.CodeDeclarationPos.IsSingleLine())
			{
				#region Single Line

				//{IDX}{X}+{Y}g

				p.Append(Index.GenerateCode(env, reversed), reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed), reversed);
				p.Append(BCHelper.Add, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed), reversed);
				p.Append(BCHelper.ReflectGet, reversed);

				p.NormalizeX();

				#endregion
			}
			else
			{
				#region Multiline

				//{IDX}:{X}\{W}%+\{Y}\{W}/+g

				p.Append(Index.GenerateCode(env, reversed), reversed);
				p.Append(BCHelper.StackDup, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed), reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Width, reversed), reversed);
				p.Append(BCHelper.Modulo, reversed);
				p.Append(BCHelper.Add, reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed), reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Width, reversed), reversed);
				p.Append(BCHelper.Div, reversed);
				p.Append(BCHelper.Add, reversed);
				p.Append(BCHelper.ReflectGet, reversed);

				p.NormalizeX();

				#endregion
			}
			
			return p;
		}

		// This puts X and Y of the var on the stack
		public override CodePiece GenerateCodeSingle(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (Target.CodeDeclarationPos.IsSingleLine())
			{
				#region Single Line

				//{IDX}{X}+{Y}

				p.Append(Index.GenerateCode(env, reversed), reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed), reversed);
				p.Append(BCHelper.Add, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed), reversed);

				p.NormalizeX();

				#endregion
			}
			else
			{
				#region Multiline

				//{IDX}:{X}\{W}%+\{Y}\{W}/+

				p.Append(Index.GenerateCode(env, reversed), reversed);
				p.Append(BCHelper.StackDup, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed), reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Width, reversed), reversed);
				p.Append(BCHelper.Modulo, reversed);
				p.Append(BCHelper.Add, reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed), reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Width, reversed), reversed);
				p.Append(BCHelper.Div, reversed);
				p.Append(BCHelper.Add, reversed);

				p.NormalizeX();

				#endregion
			}

			return p;
		}

		// Puts X and Y 1 1/2 -times on the stack: [X, X, Y]
		public override CodePiece GenerateCodeDoubleX(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (Target.CodeDeclarationPos.IsSingleLine())
			{
				#region Single Line

				//{IDX}{X}+:{Y}

				p.Append(Index.GenerateCode(env, reversed), reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed), reversed);
				p.Append(BCHelper.Add, reversed);
				p.Append(BCHelper.StackDup, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed), reversed);

				p.NormalizeX();

				#endregion
			}
			else
			{
				#region Multiline

				//{IDX}:{X}\{W}%+\{Y}\{W}/+{TX}{TY}p:{TX}{TY}g

				p.Append(Index.GenerateCode(env, reversed), reversed);
				p.Append(BCHelper.StackDup, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.X, reversed), reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Width, reversed), reversed);
				p.Append(BCHelper.Modulo, reversed);
				p.Append(BCHelper.Add, reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed), reversed);
				p.Append(BCHelper.StackSwap, reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Width, reversed), reversed);
				p.Append(BCHelper.Div, reversed);
				p.Append(BCHelper.Add, reversed);
				p.Append(NumberCodeHelper.GenerateCode(env.TMP_FIELD_GENERAL.X), reversed);
				p.Append(NumberCodeHelper.GenerateCode(env.TMP_FIELD_GENERAL.Y), reversed);
				p.Append(BCHelper.ReflectSet, reversed);
				p.Append(BCHelper.StackDup, reversed);
				p.Append(NumberCodeHelper.GenerateCode(env.TMP_FIELD_GENERAL.X), reversed);
				p.Append(NumberCodeHelper.GenerateCode(env.TMP_FIELD_GENERAL.Y), reversed);
				p.Append(BCHelper.ReflectGet, reversed);

				p.NormalizeX();

				#endregion
			}

			return p;
		}

		/// Puts Y on the stack: [Y]
		public override CodePiece GenerateCodeSingleY(CodeGenEnvironment env, bool reversed)
		{
			if (Target.CodeDeclarationPos.IsSingleLine())
			{
				// Single Line

				return NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed);
			}
			else
			{
				// Multiline

				//{Y}{IDX}{W}/+

				CodePiece p = new CodePiece();
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Y, reversed), reversed);
				p.Append(Index.GenerateCode(env, reversed), reversed);
				p.Append(NumberCodeHelper.GenerateCode(Target.CodeDeclarationPos.Width, reversed), reversed);
				p.Append(BCHelper.Div, reversed);
				p.Append(BCHelper.Add, reversed);
				return p;
			}
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			return env.GetVariableNumberList(Target)[(int)Index.EvaluateDirect(env)];
		}

		public override void EvaluateSetDirect(RunnerEnvironment env, long newValue)
		{
			env.GetVariableNumberList(Target)[(int)Index.EvaluateDirect(env)] = newValue;
		}
	}

	public class ExpressionVoidValuePointer : ExpressionValuePointer
	{
		public ExpressionVoidValuePointer(SourceCodePosition pos)
			: base(pos)
		{
		}

		public override string GetDebugString()
		{
			return "void";
		}

		public override void LinkVariables(Method owner)
		{
			//NOP
		}

		public override void LinkResultTypes(Method owner)
		{
			//NOP
		}

		public override Expression InlineConstants()
		{
			return this;
		}

		public override Expression EvaluateExpressions()
		{
			return this;
		}

		public override BType GetResultType()
		{
			return new BTypeVoid(Position);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			throw new InvalidAstStateException(Position);
		}

		public override CodePiece GenerateCodeSingle(CodeGenEnvironment env, bool reversed)
		{
			throw new InvalidAstStateException(Position);
		}

		public override CodePiece GenerateCodeDoubleX(CodeGenEnvironment env, bool reversed)
		{
			throw new InvalidAstStateException(Position);
		}

		public override CodePiece GenerateCodeSingleY(CodeGenEnvironment env, bool reversed)
		{
			throw new InvalidAstStateException(Position);
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			throw new InternalCodeRunException();
		}

		public override void EvaluateSetDirect(RunnerEnvironment env, long newValue)
		{
			throw new InternalCodeRunException();
		}
	}

	#endregion ValuePointer

	#region BinaryMathOperation

	public class ExpressionMult : ExpressionBinaryMathOperation
	{
		public ExpressionMult(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			long? l = Left.GetValueLiteral_Value();
			long? r = Right.GetValueLiteral_Value();

			if (l == 0)
			{
				return Left; // Left == 0
			}
			else if (r == 0)
			{
				return Right; // Right == 0
			}
			else if (l == 1)
			{
				return Right;
			}
			else if (r == 1)
			{
				return Left;
			}
			else if (l.HasValue && r.HasValue)
			{
				return new ExpressionLiteral(Left.Position, new LiteralInt(Left.Position, l.Value * r.Value));
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} * {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = GenerateCode_Operands(env, reversed, BCHelper.Mult);

			return p;
		}

		public static StatementAssignment CreateAugmentedStatement(SourceCodePosition p, ExpressionValuePointer v, Expression e)
		{
			return new StatementAssignment(p, v, new ExpressionMult(p, v, e));
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return e1 * e2;
		}
	}

	public class ExpressionDiv : ExpressionBinaryMathOperation
	{
		public ExpressionDiv(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			long? l = Left.GetValueLiteral_Value();
			long? r = Right.GetValueLiteral_Value();

			if (l == 0)
			{
				return Left; // Left == 0
			}
			else if (r == 1)
			{
				return Left;
			}
			else if (l.HasValue && r.HasValue && r != 0)
			{
				return new ExpressionLiteral(Left.Position, new LiteralInt(Left.Position, l.Value / r.Value));
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} / {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = GenerateCode_Operands(env, reversed, BCHelper.Div);

			return p;
		}

		public static StatementAssignment CreateAugmentedStatement(SourceCodePosition p, ExpressionValuePointer v, Expression e)
		{
			return new StatementAssignment(p, v, new ExpressionDiv(p, v, e));
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			if (e2 == 0) return 0;
			return e1 / e2;
		}
	}

	public class ExpressionMod : ExpressionBinaryMathOperation
	{
		public ExpressionMod(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			long? l = Left.GetValueLiteral_Value();
			long? r = Right.GetValueLiteral_Value();

			if (l == 0 && r.HasValue && r != 0)
			{
				return Left; // Left == 0
			}
			else if (r == 1)
			{
				return new ExpressionLiteral(Left.Position, new LiteralInt(Left.Position, 0));
			}
			else if (l.HasValue && r.HasValue && r != 0)
			{
				return new ExpressionLiteral(Left.Position, new LiteralInt(Left.Position, l.Value % r.Value));
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} % {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = GenerateCode_Operands(env, reversed, BCHelper.Modulo);

			return p;
		}

		public static StatementAssignment CreateAugmentedStatement(SourceCodePosition p, ExpressionValuePointer v, Expression e)
		{
			return new StatementAssignment(p, v, new ExpressionMod(p, v, e));
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			if (e2 == 0) return 0;
			return e1 % e2;
		}
	}

	public class ExpressionAdd : ExpressionBinaryMathOperation
	{
		public ExpressionAdd(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			long? l = Left.GetValueLiteral_Value();
			long? r = Right.GetValueLiteral_Value();

			if (l == 0)
			{
				return Right;
			}
			else if (r == 0)
			{
				return Left;
			}
			else if (l.HasValue && r.HasValue)
			{
				return new ExpressionLiteral(Left.Position, new LiteralInt(Left.Position, l.Value + r.Value));
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} + {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = GenerateCode_Operands(env, reversed, BCHelper.Add);

			return p;
		}

		public static StatementAssignment CreateAugmentedStatement(SourceCodePosition p, ExpressionValuePointer v, Expression e)
		{
			return new StatementAssignment(p, v, new ExpressionAdd(p, v, e));
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return e1 + e2;
		}
	}

	public class ExpressionSub : ExpressionBinaryMathOperation
	{
		public ExpressionSub(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			long? l = Left.GetValueLiteral_Value();
			long? r = Right.GetValueLiteral_Value();

			if (r == 0)
			{
				return Left;
			}
			else if (l.HasValue && r.HasValue)
			{
				return new ExpressionLiteral(Left.Position, new LiteralInt(Left.Position, l.Value - r.Value));
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} - {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = GenerateCode_Operands(env, reversed, BCHelper.Sub);

			return p;
		}

		public static StatementAssignment CreateAugmentedStatement(SourceCodePosition p, ExpressionValuePointer v, Expression e)
		{
			return new StatementAssignment(p, v, new ExpressionSub(p, v, e));
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return e1 - e2;
		}
	}

	#endregion Binary

	#region BinaryBoolOperation

	public class ExpressionAnd : ExpressionBinaryBoolOperation
	{
		public ExpressionAnd(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			bool? l = Left.GetValueLiteral_Bool_Value();
			bool? r = Right.GetValueLiteral_Bool_Value();

			if (r == true)
			{
				return Left;
			}
			else if (l == true)
			{
				return Right;
			}
			else if (r == false)
			{
				return Right; // Right == false
			}
			else if (l == false)
			{
				return Left; // Left == false
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} AND {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			if (reversed)
			{
				// v  0<
				// v 1_^
				// <  | 
				// ^0$< 
				CodePiece p = new CodePiece();

				p[0, -2] = BCHelper.PCDown;
				p[1, -2] = BCHelper.Walkway;
				p[2, -2] = BCHelper.Walkway;
				p[3, -2] = BCHelper.Digit0;
				p[4, -2] = BCHelper.PCLeft;

				p[0, -1] = BCHelper.PCDown;
				p[1, -1] = BCHelper.Walkway;
				p[2, -1] = BCHelper.Digit1;
				p[3, -1] = BCHelper.IfHorizontal;
				p[4, -1] = BCHelper.PCUp;

				p[0, 0] = BCHelper.PCLeft;
				p[1, 0] = BCHelper.Unused;
				p[2, 0] = BCHelper.Unused;
				p[3, 0] = BCHelper.IfVertical;
				p[4, 0] = BCHelper.Walkway;

				p[0, 1] = BCHelper.PCUp;
				p[1, 1] = BCHelper.Digit0;
				p[2, 1] = BCHelper.StackPop;
				p[3, 1] = BCHelper.PCLeft;
				p[4, 1] = BCHelper.Unused;

				p.AppendRight(Right.GenerateCode(env, reversed));
				p.AppendRight(Left.GenerateCode(env, reversed));

				p.NormalizeX();

				return p;
			}
			else
			{
				// >1  v
				// ^_0 v
				//  |  >
				//  >$0^
				CodePiece p = new CodePiece();

				p[0, -2] = BCHelper.PCRight;
				p[1, -2] = BCHelper.Digit1;
				p[2, -2] = BCHelper.Walkway;
				p[3, -2] = BCHelper.Walkway;
				p[4, -2] = BCHelper.PCDown;

				p[0, -1] = BCHelper.PCUp;
				p[1, -1] = BCHelper.IfHorizontal;
				p[2, -1] = BCHelper.Digit0;
				p[3, -1] = BCHelper.Walkway;
				p[4, -1] = BCHelper.PCDown;

				p[0, 0] = BCHelper.Walkway;
				p[1, 0] = BCHelper.IfVertical;
				p[2, 0] = BCHelper.Unused;
				p[3, 0] = BCHelper.Unused;
				p[4, 0] = BCHelper.PCRight;

				p[0, 1] = BCHelper.Unused;
				p[1, 1] = BCHelper.PCRight;
				p[2, 1] = BCHelper.StackPop;
				p[3, 1] = BCHelper.Digit0;
				p[4, 1] = BCHelper.PCUp;

				p.AppendLeft(Right.GenerateCode(env, reversed));
				p.AppendLeft(Left.GenerateCode(env, reversed));

				p.NormalizeX();

				return p;
			}
		}

		public static StatementAssignment CreateAugmentedStatement(SourceCodePosition p, ExpressionValuePointer v, Expression e)
		{
			return new StatementAssignment(p, v, new ExpressionAnd(p, v, e));
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env) != 0;
			var e2 = Right.EvaluateDirect(env) != 0;

			return (e1 && e2) ? 1 : 0;
		}
	}

	public class ExpressionOr : ExpressionBinaryBoolOperation
	{
		public ExpressionOr(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			bool? l = Left.GetValueLiteral_Bool_Value();
			bool? r = Right.GetValueLiteral_Bool_Value();

			if (r == true)
			{
				return Right; // Right == true
			}
			else if (l == true)
			{
				return Left; // Left == true
			}
			else if (r == false)
			{
				return Left;
			}
			else if (l == false)
			{
				return Right;
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} OR {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			if (reversed)
			{
				// v1$<
				// <  |
				// ^ 1_v
				// ^  0<
				CodePiece p = new CodePiece();

				p[0, -1] = BCHelper.PCDown;
				p[1, -1] = BCHelper.Digit1;
				p[2, -1] = BCHelper.StackPop;
				p[3, -1] = BCHelper.PCLeft;
				p[4, -1] = BCHelper.Unused;

				p[0, 0] = BCHelper.PCLeft;
				p[1, 0] = BCHelper.Unused;
				p[2, 0] = BCHelper.Unused;
				p[3, 0] = BCHelper.IfVertical;
				p[4, 0] = BCHelper.Walkway;

				p[0, 1] = BCHelper.PCUp;
				p[1, 1] = BCHelper.Walkway;
				p[2, 1] = BCHelper.Digit1;
				p[3, 1] = BCHelper.IfHorizontal;
				p[4, 1] = BCHelper.PCDown;

				p[0, 2] = BCHelper.PCUp;
				p[1, 2] = BCHelper.Walkway;
				p[2, 2] = BCHelper.Walkway;
				p[3, 2] = BCHelper.Digit0;
				p[4, 2] = BCHelper.PCLeft;

				p.AppendRight(Right.GenerateCode(env, reversed));
				p.AppendRight(Left.GenerateCode(env, reversed));
				p.NormalizeX();

				return p;

			}
			else
			{
				//  >$1v
				//  |  >
				// v_0 ^
				// >1  ^
				CodePiece p = new CodePiece();

				p[0, -1] = BCHelper.Unused;
				p[1, -1] = BCHelper.PCRight;
				p[2, -1] = BCHelper.StackPop;
				p[3, -1] = BCHelper.Digit1;
				p[4, -1] = BCHelper.PCDown;

				p[0, 0] = BCHelper.Walkway;
				p[1, 0] = BCHelper.IfVertical;
				p[2, 0] = BCHelper.Unused;
				p[3, 0] = BCHelper.Unused;
				p[4, 0] = BCHelper.PCRight;

				p[0, 1] = BCHelper.PCDown;
				p[1, 1] = BCHelper.IfHorizontal;
				p[2, 1] = BCHelper.Digit0;
				p[3, 1] = BCHelper.Walkway;
				p[4, 1] = BCHelper.PCUp;

				p[0, 2] = BCHelper.PCRight;
				p[1, 2] = BCHelper.Digit1;
				p[2, 2] = BCHelper.Walkway;
				p[3, 2] = BCHelper.Walkway;
				p[4, 2] = BCHelper.PCUp;

				p.AppendLeft(Right.GenerateCode(env, reversed));
				p.AppendLeft(Left.GenerateCode(env, reversed));
				p.NormalizeX();

				return p;
			}
		}

		public static StatementAssignment CreateAugmentedStatement(SourceCodePosition p, ExpressionValuePointer v, Expression e)
		{
			return new StatementAssignment(p, v, new ExpressionOr(p, v, e));
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env) != 0;
			var e2 = Right.EvaluateDirect(env) != 0;

			return (e1 || e2) ? 1 : 0;
		}
	}

	public class ExpressionXor : ExpressionBinaryBoolOperation
	{
		public ExpressionXor(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			bool? l = Left.GetValueLiteral_Bool_Value();
			bool? r = Right.GetValueLiteral_Bool_Value();

			if (r == false)
			{
				return Left;
			}
			else if (l == false)
			{
				return Right;
			}
			else if (l.HasValue && r.HasValue)
			{
				return new ExpressionLiteral(Left.Position, new LiteralBool(Left.Position, l.Value ^ r.Value));
			}
			else if (r == true)
			{
				return new ExpressionNot(Left.Position, Left);
			}
			else if (l == true)
			{
				return new ExpressionNot(Right.Position, Right);
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} XOR {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			if (reversed)
			{
				// v < 
				//v0_v 
				//<<|##
				//^1_^ 
				// ^ < 
				CodePiece p = new CodePiece();

				p[0, -2] = BCHelper.Unused;
				p[1, -2] = BCHelper.PCDown;
				p[2, -2] = BCHelper.Walkway;
				p[3, -2] = BCHelper.PCLeft;
				p[4, -2] = BCHelper.Unused;

				p[0, -1] = BCHelper.PCDown;
				p[1, -1] = BCHelper.Digit0;
				p[2, -1] = BCHelper.IfHorizontal;
				p[3, -1] = BCHelper.PCDown;
				p[4, -1] = BCHelper.Unused;

				p[0, 0] = BCHelper.PCLeft;
				p[1, 0] = BCHelper.PCLeft;
				p[2, 0] = BCHelper.IfVertical;
				p[3, 0] = BCHelper.PCJump;
				p[4, 0] = BCHelper.PCJump;

				p[0, 1] = BCHelper.PCUp;
				p[1, 1] = BCHelper.Digit1;
				p[2, 1] = BCHelper.IfHorizontal;
				p[3, 1] = BCHelper.PCUp;
				p[4, 1] = BCHelper.Unused;

				p[0, 2] = BCHelper.Unused;
				p[1, 2] = BCHelper.PCUp;
				p[2, 2] = BCHelper.Walkway;
				p[3, 2] = BCHelper.PCLeft;
				p[4, 2] = BCHelper.Unused;

				p.AppendRight(Right.GenerateCode(env, reversed));
				p.AppendRight(Left.GenerateCode(env, reversed));
				p.NormalizeX();

				return p;
			}
			else
			{
				//  > v 
				//  v_1v
				// ##|>>
				//  ^_0^
				//  > ^
				CodePiece p = new CodePiece();

				p[0, -2] = BCHelper.Unused;
				p[1, -2] = BCHelper.PCRight;
				p[2, -2] = BCHelper.Walkway;
				p[3, -2] = BCHelper.PCDown;
				p[4, -2] = BCHelper.Unused;

				p[0, -1] = BCHelper.Unused;
				p[1, -1] = BCHelper.PCDown;
				p[2, -1] = BCHelper.IfHorizontal;
				p[3, -1] = BCHelper.Digit1;
				p[4, -1] = BCHelper.PCDown;

				p[0, 0] = BCHelper.PCJump;
				p[1, 0] = BCHelper.PCJump;
				p[2, 0] = BCHelper.IfVertical;
				p[3, 0] = BCHelper.PCRight;
				p[4, 0] = BCHelper.PCRight;

				p[0, 1] = BCHelper.Unused;
				p[1, 1] = BCHelper.PCUp;
				p[2, 1] = BCHelper.IfHorizontal;
				p[3, 1] = BCHelper.Digit0;
				p[4, 1] = BCHelper.PCUp;

				p[0, 2] = BCHelper.Unused;
				p[1, 2] = BCHelper.PCRight;
				p[2, 2] = BCHelper.Walkway;
				p[3, 2] = BCHelper.PCUp;
				p[4, 2] = BCHelper.Unused;

				p.AppendLeft(Right.GenerateCode(env, reversed));
				p.AppendLeft(Left.GenerateCode(env, reversed));
				p.NormalizeX();

				return p;
			}
		}

		public static StatementAssignment CreateAugmentedStatement(SourceCodePosition p, ExpressionValuePointer v, Expression e)
		{
			return new StatementAssignment(p, v, new ExpressionXor(p, v, e));
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env) != 0;
			var e2 = Right.EvaluateDirect(env) != 0;

			return (e1 ^ e2) ? 1 : 0;
		}
	}

	#endregion

	#region Compare

	public class ExpressionEquals : ExpressionCompare
	{
		public ExpressionEquals(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			if (Left is ExpressionLiteral && Right is ExpressionLiteral && Left.GetResultType() == Right.GetResultType())
			{
				long? l = Left.GetValueLiteral_Value();
				long? r = Right.GetValueLiteral_Value();

				if (l.HasValue && r.HasValue)
				{
					return new ExpressionLiteral(Left.Position, new LiteralBool(Left.Position, l.Value == r.Value));
				}
				else
				{
					return this;
				}
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} == {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			//  >0v
			// -| >
			//  >1^
			CodePiece p = new CodePiece();

			p[0, -1] = BCHelper.Unused;
			p[1, -1] = BCHelper.PCRight;
			p[2, -1] = BCHelper.Digit0;
			p[3, -1] = BCHelper.PCDown;

			p[0, 0] = BCHelper.Sub;
			p[1, 0] = BCHelper.IfVertical;
			p[2, 0] = BCHelper.Unused;
			p[3, 0] = BCHelper.PCRight;

			p[0, 1] = BCHelper.Unused;
			p[1, 1] = BCHelper.PCRight;
			p[2, 1] = BCHelper.Digit1;
			p[3, 1] = BCHelper.PCUp;

			if (reversed)
			{
				p.ReverseX(true);
			}

			if (reversed)
			{
				p.AppendRight(Right.GenerateCode(env, reversed));
				p.AppendRight(Left.GenerateCode(env, reversed));
			}
			else
			{
				p.AppendLeft(Right.GenerateCode(env, reversed));
				p.AppendLeft(Left.GenerateCode(env, reversed));
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return (e1 == e2) ? 1 : 0;
		}
	}

	public class ExpressionUnequals : ExpressionCompare
	{
		public ExpressionUnequals(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			if (Left is ExpressionLiteral && Right is ExpressionLiteral && Left.GetResultType() == Right.GetResultType())
			{
				long? l = Left.GetValueLiteral_Value();
				long? r = Right.GetValueLiteral_Value();

				if (l.HasValue && r.HasValue)
				{
					return new ExpressionLiteral(Left.Position, new LiteralBool(Left.Position, l.Value != r.Value));
				}
				else
				{
					return this;
				}
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} != {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			//  >1v
			// -| >
			//  >0^
			CodePiece p = new CodePiece();

			p[0, -1] = BCHelper.Unused;
			p[1, -1] = BCHelper.PCRight;
			p[2, -1] = BCHelper.Digit1;
			p[3, -1] = BCHelper.PCDown;

			p[0, 0] = BCHelper.Sub;
			p[1, 0] = BCHelper.IfVertical;
			p[2, 0] = BCHelper.Unused;
			p[3, 0] = BCHelper.PCRight;

			p[0, 1] = BCHelper.Unused;
			p[1, 1] = BCHelper.PCRight;
			p[2, 1] = BCHelper.Digit0;
			p[3, 1] = BCHelper.PCUp;

			if (reversed)
			{
				p.ReverseX(true);
			}

			if (reversed)
			{
				p.AppendRight(Right.GenerateCode(env, reversed));
				p.AppendRight(Left.GenerateCode(env, reversed));
			}
			else
			{
				p.AppendLeft(Right.GenerateCode(env, reversed));
				p.AppendLeft(Left.GenerateCode(env, reversed));
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return (e1 != e2) ? 1 : 0;
		}
	}

	public class ExpressionGreater : ExpressionCompare
	{
		public ExpressionGreater(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			if (Left is ExpressionLiteral && Right is ExpressionLiteral && Left.GetResultType() == Right.GetResultType())
			{
				long? l = Left.GetValueLiteral_Value();
				long? r = Right.GetValueLiteral_Value();

				if (l.HasValue && r.HasValue)
				{
					return new ExpressionLiteral(Left.Position, new LiteralBool(Left.Position, l.Value > r.Value));
				}
				else
				{
					return this;
				}
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} > {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p;

			if (reversed)
			{
				//First Left than Right -->  RIGHT < LEFT
				p = Left.GenerateCode(env, reversed);
				p.AppendLeft(Right.GenerateCode(env, reversed));

				p.AppendLeft(BCHelper.GreaterThan);
			}
			else
			{
				//First Left than Right -->  RIGHT < LEFT
				p = Left.GenerateCode(env, reversed);
				p.AppendRight(Right.GenerateCode(env, reversed));

				p.AppendRight(BCHelper.GreaterThan);
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return (e1 > e2) ? 1 : 0;
		}
	}

	public class ExpressionLesser : ExpressionCompare
	{
		public ExpressionLesser(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			if (Left is ExpressionLiteral && Right is ExpressionLiteral && Left.GetResultType() == Right.GetResultType())
			{
				long? l = Left.GetValueLiteral_Value();
				long? r = Right.GetValueLiteral_Value();

				if (l.HasValue && r.HasValue)
				{
					return new ExpressionLiteral(Left.Position, new LiteralBool(Left.Position, l.Value < r.Value));
				}
				else
				{
					return this;
				}
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} < {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p;

			if (reversed)
			{
				//First Right than Left -->  LEFT < RIGHT
				p = Right.GenerateCode(env, reversed);
				p.AppendLeft(Left.GenerateCode(env, reversed));

				p.AppendLeft(BCHelper.GreaterThan);
			}
			else
			{
				//First Right than Left -->  LEFT < RIGHT
				p = Right.GenerateCode(env, reversed);
				p.AppendRight(Left.GenerateCode(env, reversed));

				p.AppendRight(BCHelper.GreaterThan);
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return (e1 < e2) ? 1 : 0;
		}
	}

	public class ExpressionGreaterEquals : ExpressionCompare
	{
		public ExpressionGreaterEquals(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			if (Left is ExpressionLiteral && Right is ExpressionLiteral && Left.GetResultType() == Right.GetResultType())
			{
				long? l = Left.GetValueLiteral_Value();
				long? r = Right.GetValueLiteral_Value();

				if (l.HasValue && r.HasValue)
				{
					return new ExpressionLiteral(Left.Position, new LiteralBool(Left.Position, l.Value >= r.Value));
				}
				else
				{
					return this;
				}
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} >= {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			if (reversed)
			{
				// <1$_v#!:-
				// ^`\0<

				//First Right than Left -->  RIGHT <= LEFT
				CodePiece ep = Right.GenerateCode(env, reversed);
				ep.AppendLeft(Left.GenerateCode(env, reversed));

				CodePiece p = new CodePiece();
				p[0, 0] = BCHelper.PCLeft;
				p[1, 0] = BCHelper.Digit1;
				p[2, 0] = BCHelper.StackPop;
				p[3, 0] = BCHelper.IfHorizontal;
				p[4, 0] = BCHelper.PCDown;
				p[5, 0] = BCHelper.PCJump;
				p[6, 0] = BCHelper.Not;
				p[7, 0] = BCHelper.StackDup;
				p[8, 0] = BCHelper.Sub;

				p[0, 1] = BCHelper.PCUp;
				p[1, 1] = BCHelper.GreaterThan;
				p[2, 1] = BCHelper.StackSwap;
				p[3, 1] = BCHelper.Digit0;
				p[4, 1] = BCHelper.PCLeft;
				p[5, 1] = BCHelper.Unused;
				p[6, 1] = BCHelper.Unused;
				p[7, 1] = BCHelper.Unused;
				p[8, 1] = BCHelper.Unused;

				ep.AppendLeft(p);

				p.NormalizeX();

				return ep;
			}
			else
			{
				// -:#v_$1>
				//    >0\`^

				//First Right than Left -->  RIGHT <= LEFT
				CodePiece ep = Right.GenerateCode(env, reversed);
				ep.AppendRight(Left.GenerateCode(env, reversed));

				CodePiece p = new CodePiece();
				p[0, 0] = BCHelper.Sub;
				p[1, 0] = BCHelper.StackDup;
				p[2, 0] = BCHelper.PCJump;
				p[3, 0] = BCHelper.PCDown;
				p[4, 0] = BCHelper.IfHorizontal;
				p[5, 0] = BCHelper.StackPop;
				p[6, 0] = BCHelper.Digit1;
				p[7, 0] = BCHelper.PCRight;

				p[0, 1] = BCHelper.Unused;
				p[1, 1] = BCHelper.Unused;
				p[2, 1] = BCHelper.Unused;
				p[3, 1] = BCHelper.PCRight;
				p[4, 1] = BCHelper.Digit0;
				p[5, 1] = BCHelper.StackSwap;
				p[6, 1] = BCHelper.GreaterThan;
				p[7, 1] = BCHelper.PCUp;

				ep.AppendRight(p);

				p.NormalizeX();

				return ep;
			}
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return (e1 >= e2) ? 1 : 0;
		}
	}

	public class ExpressionLesserEquals : ExpressionCompare
	{
		public ExpressionLesserEquals(SourceCodePosition pos, Expression l, Expression r)
			: base(pos, l, r)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			if (Left is ExpressionLiteral && Right is ExpressionLiteral && Left.GetResultType() == Right.GetResultType())
			{
				long? l = Left.GetValueLiteral_Value();
				long? r = Right.GetValueLiteral_Value();

				if (l.HasValue && r.HasValue)
				{
					return new ExpressionLiteral(Left.Position, new LiteralBool(Left.Position, l.Value <= r.Value));
				}
				else
				{
					return this;
				}
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("({0} <= {1})", Left.GetDebugString(), Right.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			if (reversed)
			{
				// <1$_v#!:-
				// ^`\0<

				//First Left than Right -->  LEFT <= RIGHT
				CodePiece ep = Left.GenerateCode(env, reversed);
				ep.AppendLeft(Right.GenerateCode(env, reversed));

				CodePiece p = new CodePiece();
				p[0, 0] = BCHelper.PCLeft;
				p[1, 0] = BCHelper.Digit1;
				p[2, 0] = BCHelper.StackPop;
				p[3, 0] = BCHelper.IfHorizontal;
				p[4, 0] = BCHelper.PCDown;
				p[5, 0] = BCHelper.PCJump;
				p[6, 0] = BCHelper.Not;
				p[7, 0] = BCHelper.StackDup;
				p[8, 0] = BCHelper.Sub;

				p[0, 1] = BCHelper.PCUp;
				p[1, 1] = BCHelper.GreaterThan;
				p[2, 1] = BCHelper.StackSwap;
				p[3, 1] = BCHelper.Digit0;
				p[4, 1] = BCHelper.PCLeft;
				p[5, 1] = BCHelper.Unused;
				p[6, 1] = BCHelper.Unused;
				p[7, 1] = BCHelper.Unused;
				p[8, 1] = BCHelper.Unused;

				ep.AppendLeft(p);

				p.NormalizeX();

				return ep;
			}
			else
			{
				// -:#v_$1>
				//    >0\`^

				//First Left than Right -->  LEFT <= RIGHT
				CodePiece ep = Left.GenerateCode(env, reversed);
				ep.AppendRight(Right.GenerateCode(env, reversed));

				CodePiece p = new CodePiece();
				p[0, 0] = BCHelper.Sub;
				p[1, 0] = BCHelper.StackDup;
				p[2, 0] = BCHelper.PCJump;
				p[3, 0] = BCHelper.PCDown;
				p[4, 0] = BCHelper.IfHorizontal;
				p[5, 0] = BCHelper.StackPop;
				p[6, 0] = BCHelper.Digit1;
				p[7, 0] = BCHelper.PCRight;

				p[0, 1] = BCHelper.Unused;
				p[1, 1] = BCHelper.Unused;
				p[2, 1] = BCHelper.Unused;
				p[3, 1] = BCHelper.PCRight;
				p[4, 1] = BCHelper.Digit0;
				p[5, 1] = BCHelper.StackSwap;
				p[6, 1] = BCHelper.GreaterThan;
				p[7, 1] = BCHelper.PCUp;

				ep.AppendRight(p);

				p.NormalizeX();

				return ep;
			}
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Left.EvaluateDirect(env);
			var e2 = Right.EvaluateDirect(env);

			return (e1 <= e2) ? 1 : 0;
		}
	}

	#endregion Compare

	#region Unary

	public class ExpressionNot : ExpressionUnary
	{
		public ExpressionNot(SourceCodePosition pos, Expression e)
			: base(pos, e)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			bool? v = Expr.GetValueLiteral_Bool_Value();

			if (v.HasValue)
			{
				return new ExpressionLiteral(Expr.Position, new LiteralBool(Expr.Position, !v.Value));
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("(! {0})", Expr.GetDebugString());
		}

		public override void LinkResultTypes(Method owner)
		{
			Expr.LinkResultTypes(owner);

			if (!(Expr.GetResultType() is BTypeBool))
				throw new ImplicitCastException(Position, Expr.GetResultType(), new BTypeBool(Position));
		}

		public override BType GetResultType()
		{
			return new BTypeBool(Position);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = Expr.GenerateCode(env, reversed);

			if (reversed)
			{
				p.AppendLeft(BCHelper.Not);
			}
			else
			{
				p.AppendRight(BCHelper.Not);
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Expr.EvaluateDirect(env) != 0;

			return (!e1) ? 1 : 0;
		}
	}

	public class ExpressionNegate : ExpressionUnary
	{
		public ExpressionNegate(SourceCodePosition pos, Expression e)
			: base(pos, e)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			long? v = Expr.GetValueLiteral_Value();

			if (v.HasValue)
			{
				return new ExpressionLiteral(Expr.Position, new LiteralInt(Expr.Position, -v.Value));
			}
			else
			{
				return this;
			}
		}

		public override string GetDebugString()
		{
			return string.Format("(- {1})", Expr.GetDebugString());
		}

		public override void LinkResultTypes(Method owner)
		{
			Expr.LinkResultTypes(owner);

			if (!(Expr.GetResultType() is BTypeInt))
				throw new ImplicitCastException(Position, Expr.GetResultType(), new BTypeInt(Position));
		}

		public override BType GetResultType()
		{
			return new BTypeInt(Position);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = Expr.GenerateCode(env, reversed);

			if (reversed)
			{
				p.AppendRight(BCHelper.Digit0);
				p.AppendLeft(BCHelper.Sub);
			}
			else
			{
				p.AppendLeft(BCHelper.Digit0);
				p.AppendRight(BCHelper.Sub);
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var e1 = Expr.EvaluateDirect(env);

			return -e1;
		}
	}

	public class ExpressionCast : ExpressionUnary
	{
		public BType Type;

		public ExpressionCast(SourceCodePosition pos, BType t, Expression e)
			: base(pos, e)
		{
			this.Type = t;
		}

		public override Expression EvaluateExpressions()
		{
			EvaluateSubExpressions();

			return this;
		}

		public override string GetDebugString()
		{
			return string.Format("(({0}){1})", Type.GetDebugString(), Expr.GetDebugString());
		}

		public override void LinkResultTypes(Method owner)
		{
			Expr.LinkResultTypes(owner);
		}

		public override BType GetResultType()
		{
			return Type;
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			if (CGO.ExtendedBooleanCast && Type.GetType() == typeof(BTypeBool))
			{
				CodePiece p = Expr.GenerateCode(env, reversed);

				if (reversed)
				{
					// !!
					CodePiece op = new CodePiece();

					op[0, 0] = BCHelper.Not;
					op[1, 0] = BCHelper.Not;

					p.AppendLeft(op);
					p.NormalizeX();
				}
				else
				{
					// !!
					CodePiece op = new CodePiece();

					op[0, 0] = BCHelper.Not;
					op[1, 0] = BCHelper.Not;

					p.AppendRight(op);
				}

				return p;
			}
			else
			{
				return Expr.GenerateCode(env, reversed);
			}

		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			return Expr.EvaluateDirect(env);
		}
	}

	#endregion Unary

	#region Inc/Decrement

	public class ExpressionPostIncrement : ExpressionCrement
	{
		public ExpressionPostIncrement(SourceCodePosition pos, ExpressionValuePointer v)
			: base(pos, v)
		{
			//--
		}

		public override string GetDebugString()
		{
			return string.Format("{0}++", Target.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(Target.GenerateCode(env, reversed));

				p.AppendLeft(BCHelper.StackDup);

				p.AppendLeft(BCHelper.Digit1);
				p.AppendLeft(BCHelper.Add);

				p.AppendLeft(Target.GenerateCodeSingle(env, reversed));

				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight(Target.GenerateCode(env, reversed));

				p.AppendRight(BCHelper.StackDup);

				p.AppendRight(BCHelper.Digit1);
				p.AppendRight(BCHelper.Add);

				p.AppendRight(Target.GenerateCodeSingle(env, reversed));

				p.AppendRight(BCHelper.ReflectSet);
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var v = Target.EvaluateDirect(env);

			Target.EvaluateSetDirect(env, v + 1);

			return v;
		}
	}

	public class ExpressionPreIncrement : ExpressionCrement
	{
		public ExpressionPreIncrement(SourceCodePosition pos, ExpressionValuePointer v)
			: base(pos, v)
		{
			//--
		}

		public override string GetDebugString()
		{
			return string.Format("++{0}", Target.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(Target.GenerateCode(env, reversed));

				p.AppendLeft(BCHelper.Digit1);
				p.AppendLeft(BCHelper.Add);

				p.AppendLeft(BCHelper.StackDup);

				p.AppendLeft(Target.GenerateCodeSingle(env, reversed));

				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight(Target.GenerateCode(env, reversed));

				p.AppendRight(BCHelper.Digit1);
				p.AppendRight(BCHelper.Add);

				p.AppendRight(BCHelper.StackDup);

				p.AppendRight(Target.GenerateCodeSingle(env, reversed));

				p.AppendRight(BCHelper.ReflectSet);
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var v = Target.EvaluateDirect(env);

			Target.EvaluateSetDirect(env, v + 1);

			return v + 1;
		}
	}

	public class ExpressionPostDecrement : ExpressionCrement
	{
		public ExpressionPostDecrement(SourceCodePosition pos, ExpressionValuePointer v)
			: base(pos, v)
		{
			//--
		}

		public override string GetDebugString()
		{
			return string.Format("{0}--", Target.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(Target.GenerateCode(env, reversed));

				p.AppendLeft(BCHelper.StackDup);

				p.AppendLeft(BCHelper.Digit1);
				p.AppendLeft(BCHelper.Sub);

				p.AppendLeft(Target.GenerateCodeSingle(env, reversed));

				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight(Target.GenerateCode(env, reversed));

				p.AppendRight(BCHelper.StackDup);

				p.AppendRight(BCHelper.Digit1);
				p.AppendRight(BCHelper.Sub);

				p.AppendRight(Target.GenerateCodeSingle(env, reversed));

				p.AppendRight(BCHelper.ReflectSet);
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var v = Target.EvaluateDirect(env);

			Target.EvaluateSetDirect(env, v - 1);

			return v;
		}
	}

	public class ExpressionPreDecrement : ExpressionCrement
	{
		public ExpressionPreDecrement(SourceCodePosition pos, ExpressionValuePointer v)
			: base(pos, v)
		{
			//--
		}

		public override string GetDebugString()
		{
			return string.Format("--{0}", Target.GetDebugString());
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(Target.GenerateCode(env, reversed));

				p.AppendLeft(BCHelper.Digit1);
				p.AppendLeft(BCHelper.Sub);

				p.AppendLeft(BCHelper.StackDup);

				p.AppendLeft(Target.GenerateCodeSingle(env, reversed));

				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight(Target.GenerateCode(env, reversed));

				p.AppendRight(BCHelper.Digit1);
				p.AppendRight(BCHelper.Sub);

				p.AppendRight(BCHelper.StackDup);

				p.AppendRight(Target.GenerateCodeSingle(env, reversed));

				p.AppendRight(BCHelper.ReflectSet);
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var v = Target.EvaluateDirect(env);

			Target.EvaluateSetDirect(env, v- 1);

			return v - 1;
		}
	}

	#endregion

	#region Other

	public class ExpressionLiteral : Expression
	{
		public readonly Literal Value;

		public ExpressionLiteral(SourceCodePosition pos, Literal l)
			: base(pos)
		{
			this.Value = l;
		}

		public override Expression EvaluateExpressions()
		{
			return this;
		}

		public override string GetDebugString()
		{
			return string.Format("{0}", Value.GetDebugString());
		}

		public override void LinkVariables(Method owner)
		{
			//NOP
		}

		public override Expression InlineConstants()
		{
			return this;
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

		public override BType GetResultType()
		{
			return Value.GetBType();
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			return Value.GenerateCode(env, reversed);
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			return Value.AsNumber();
		}
	}

	public class ExpressionBooleanRand : ExpressionRand
	{
		public ExpressionBooleanRand(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override Expression EvaluateExpressions()
		{
			return this;
		}

		public override string GetDebugString()
		{
			return "#RAND#";
		}

		public override void LinkVariables(Method owner)
		{
			//NOP
		}

		public override Expression InlineConstants()
		{
			return this;
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

		public override BType GetResultType()
		{
			return new BTypeBool(Position);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			//  >>1v
			// #^?0>>
			//   > 0^
			CodePiece p = new CodePiece();

			p[0, -1] = BCHelper.Unused;
			p[1, -1] = BCHelper.PCRight;
			p[2, -1] = BCHelper.PCRight;
			p[3, -1] = BCHelper.Digit1;
			p[4, -1] = BCHelper.PCDown;
			p[5, -1] = BCHelper.Unused;

			p[0, 0] = BCHelper.PCJump;
			p[1, 0] = BCHelper.PCUp;
			p[2, 0] = BCHelper.PCRandom;
			p[3, 0] = BCHelper.Digit0;
			p[4, 0] = BCHelper.PCRight;
			p[5, 0] = BCHelper.PCRight;

			p[0, 1] = BCHelper.Unused;
			p[1, 1] = BCHelper.Unused;
			p[2, 1] = BCHelper.PCRight;
			p[3, 1] = BCHelper.Walkway;
			p[4, 1] = BCHelper.Digit0;
			p[5, 1] = BCHelper.PCUp;

			if (reversed)
			{
				p.ReverseX(true);
			}

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			return (env.Random.Next(2) == 0) ? 1 : 0;
		}
	}

	public class ExpressionBase4Rand : ExpressionRand
	{
		public Expression Exponent;

		public ExpressionBase4Rand(SourceCodePosition pos, Expression exp)
			: base(pos)
		{
			Exponent = exp;
		}

		public override Expression EvaluateExpressions()
		{
			Exponent = Exponent.EvaluateExpressions();

			long? v = Exponent.GetValueLiteral_Value();

			if (v.HasValue && v.Value <= 0)
			{
				return new ExpressionLiteral(Exponent.Position, new LiteralInt(Exponent.Position, 0));
			}

			return this;
		}

		public override string GetDebugString()
		{
			return "#RAND_B4 [" + Exponent.GetDebugString() + "]";
		}

		public override void LinkVariables(Method owner)
		{
			Exponent.LinkVariables(owner);
		}

		public override Expression InlineConstants()
		{
			Exponent = Exponent.InlineConstants();
			return this;
		}

		public override void AddressCodePoints()
		{
			//NOP
		}

		public override void LinkResultTypes(Method owner)
		{
			Exponent.LinkResultTypes(owner);

			BType wanted = new BTypeInt(Position);
			BType present = Exponent.GetResultType();

			if (present != wanted)
			{
				if (present.IsImplicitCastableTo(wanted))
					Exponent = new ExpressionCast(Position, wanted, Exponent);
				else
					throw new ImplicitCastException(Position, present, wanted);
			}
		}

		public override void LinkMethods(Program owner)
		{
			//NOP
		}

		public override BType GetResultType()
		{
			return new BTypeInt(Position);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed

				p.AppendRight(CodePieceStore.Base4DigitJoiner(reversed));
				p.AppendRight(CodePieceStore.RandomDigitGenerator(Exponent.GenerateCode(env, reversed), reversed));

				#endregion
			}
			else
			{
				#region Normal

				p.AppendRight(CodePieceStore.RandomDigitGenerator(Exponent.GenerateCode(env, reversed), reversed));
				p.AppendRight(CodePieceStore.Base4DigitJoiner(reversed));

				#endregion
			}

			p.NormalizeX();

			return p;
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			var ee = Exponent.EvaluateDirect(env);

			return env.Random.Next((int)Math.Pow(4, (int)ee));
		}
	}

	public class ExpressionFunctionCall : Expression
	{
		public readonly StatementMethodCall MethodCall;

		public ExpressionFunctionCall(SourceCodePosition pos, StatementMethodCall mc)
			: base(pos)
		{
			this.MethodCall = mc;
		}

		public override Expression EvaluateExpressions()
		{
			MethodCall.EvaluateExpressions();

			return this;
		}

		public override string GetDebugString()
		{
			return MethodCall.GetDebugString();
		}

		public override void LinkVariables(Method owner)
		{
			MethodCall.LinkVariables(owner);
		}

		public override Expression InlineConstants()
		{
			MethodCall.InlineConstants();

			return this;
		}

		public override void AddressCodePoints()
		{
			MethodCall.AddressCodePoints();
		}

		public override void LinkResultTypes(Method owner)
		{
			MethodCall.LinkResultTypes(owner);
		}

		public override void LinkMethods(Program owner)
		{
			MethodCall.LinkMethods(owner);

			if (MethodCall.Target.ResultType is BTypeVoid)
			{
				throw new InlineVoidMethodCallException(Position);
			}
		}

		public override BType GetResultType()
		{
			return MethodCall.Target.ResultType;
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			return MethodCall.GenerateCode(env, reversed, false);
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			MethodCall.RunDirect(env, out long? r);
			return r.Value;
		}
	}

	public class ExpressionClassMethodCall : Expression
	{
		public readonly StatementClassMethodCall MethodCall;

		public ExpressionClassMethodCall(SourceCodePosition pos, StatementClassMethodCall cmc) : base(pos)
		{
			MethodCall = cmc;
		}

		public override void AddressCodePoints()
		{
			MethodCall.AddressCodePoints();
		}

		public override Expression EvaluateExpressions()
		{
			MethodCall.EvaluateExpressions();
			return this;
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			return MethodCall.GenerateCode(env, reversed, false);
		}

		public override string GetDebugString()
		{
			return MethodCall.GetDebugString();
		}

		public override BType GetResultType()
		{
			return MethodCall.ResultType;
		}

		public override Expression InlineConstants()
		{
			MethodCall.InlineConstants();
			return this;
		}

		public override void LinkMethods(Program owner)
		{
			MethodCall.LinkMethods(owner);

			if (MethodCall.ResultType is BTypeVoid) throw new InlineVoidMethodCallException(Position);
		}

		public override void LinkResultTypes(Method owner)
		{
			MethodCall.LinkResultTypes(owner);
		}

		public override void LinkVariables(Method owner)
		{
			MethodCall.LinkVariables(owner);
		}

		public override long EvaluateDirect(RunnerEnvironment env)
		{
			MethodCall.RunDirect(env, out long? r);
			return r.Value;
		}
	}

	#endregion Other
}