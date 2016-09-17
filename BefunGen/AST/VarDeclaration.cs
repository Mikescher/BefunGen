using BefunGen.AST.CodeGen;
using BefunGen.AST.CodeGen.NumberCode;
using BefunGen.AST.Exceptions;
namespace BefunGen.AST
{
	public abstract class VarDeclaration : ASTObject
	{
		private static int _vIDCounter = 100;
		protected static int V_ID_COUNTER { get { return _vIDCounter++; } }

		public readonly BType Type;
		public readonly string Identifier;
		public readonly Literal Initial;
		public readonly int ID;

		public bool HasCompleteUserDefiniedInitialValue;
		public bool IsConstant { get; private set; }

		private int codePositionX = -1;
		public int CodePositionX
		{
			get
			{
				if (IsConstant)
					throw new ConstantValueChangedException(Position, Identifier);

				if (codePositionX < 0)
					throw new InternalCodeGenException();
				else
					return codePositionX;
			}

			set
			{
				if (IsConstant)
					throw new ConstantValueChangedException(Position, Identifier);

				codePositionX = value;
			}
		}

		private int codePositionY = -1;
		public int CodePositionY
		{
			get
			{
				if (IsConstant)
					throw new ConstantValueChangedException(Position, Identifier);

				if (codePositionY < 0)
					throw new InternalCodeGenException();
				else
					return codePositionY;
			}

			set
			{
				if (IsConstant)
					throw new ConstantValueChangedException(Position, Identifier);

				codePositionY = value;
			}
		}

		public VarDeclaration(SourceCodePosition pos, BType t, string ident, Literal init)
			: base(pos)
		{
			this.Type = t;
			this.Identifier = ident;
			this.ID = V_ID_COUNTER;
			this.IsConstant = false;

			if (ASTObject.IsKeyword(ident))
			{
				throw new IllegalIdentifierException(Position, ident);
			}

			if (init == null)
			{
				this.Initial = t.GetDefaultValue();
				HasCompleteUserDefiniedInitialValue = false;
			}
			else
			{
				this.Initial = init;
				HasCompleteUserDefiniedInitialValue = true;
			}
		}

		public void SetConstant()
		{
			IsConstant = true;

			if (this is VarDeclarationArray)
				throw new ConstantArrayException(Position);
		}

		public override string GetDebugString()
		{
			return string.Format("{0} {{{1}}} ::= {2}", Type.GetDebugString(), ID, Initial == null ? "NULL" : Initial.GetDebugString());
		}

		public string GetWellFormattedDecalaration()
		{
			return string.Format("{0} {1}", Type.GetDebugString(), Identifier);
		}

		public string GetShortDebugString()
		{
			return string.Format("{{{0}}}", ID);
		}

		public static void ResetCounter()
		{
			_vIDCounter = 1;
		}

		// Code for Variable Initialization
		public abstract CodePiece GenerateCode(bool reversed);
		public abstract CodePiece GenerateCode_SetToStackVal(bool reversed);
	}

	#region Children

	public class VarDeclarationValue : VarDeclaration
	{
		public VarDeclarationValue(SourceCodePosition pos, BTypeValue t, string id)
			: base(pos, t, id, null)
		{
		}

		public VarDeclarationValue(SourceCodePosition pos, BTypeValue t, string id, LiteralValue v)
			: base(pos, t, id, v)
		{
			if (!v.GetBType().IsImplicitCastableTo(t))
				throw new ImplicitCastException(pos, v.GetBType(), t);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			if (IsConstant)
				throw new ConstantValueChangedException(Position, Identifier);

			CodePiece p = new CodePiece();

			int varX = CodePositionX;
			int varY = CodePositionY;

			if (reversed)
			{
				p.AppendLeft((Initial as LiteralValue).GenerateCode(reversed));
				p.AppendLeft(NumberCodeHelper.GenerateCode(varX, reversed));
				p.AppendLeft(NumberCodeHelper.GenerateCode(varY, reversed));
				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight((Initial as LiteralValue).GenerateCode(reversed));
				p.AppendRight(NumberCodeHelper.GenerateCode(varX, reversed));
				p.AppendRight(NumberCodeHelper.GenerateCode(varY, reversed));
				p.AppendRight(BCHelper.ReflectSet);
			}

			p.NormalizeX();

			return p;
		}

		public override CodePiece GenerateCode_SetToStackVal(bool reversed)
		{
			if (IsConstant)
				throw new ConstantValueChangedException(Position, Identifier);

			CodePiece p = new CodePiece();

			int varX = CodePositionX;
			int varY = CodePositionY;

			if (reversed)
			{
				p.AppendLeft(NumberCodeHelper.GenerateCode(varX, reversed));
				p.AppendLeft(NumberCodeHelper.GenerateCode(varY, reversed));
				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight(NumberCodeHelper.GenerateCode(varX, reversed));
				p.AppendRight(NumberCodeHelper.GenerateCode(varY, reversed));
				p.AppendRight(BCHelper.ReflectSet);
			}

			p.NormalizeX();

			return p;
		}
	}

	public class VarDeclarationArray : VarDeclaration
	{
		public BTypeValue InternalType { get { return (Type as BTypeArray).InternalType; } }

		public int Size { get { return (Type as BTypeArray).Size; } }

		public VarDeclarationArray(SourceCodePosition pos, BTypeArray t, string id)
			: base(pos, t, id, null)
		{
			if (Size < 2)
			{
				throw new ArrayTooSmallException(Position);
			}
		}

		public VarDeclarationArray(SourceCodePosition pos, BTypeArray t, string id, LiteralArray v)
			: base(pos, t, id, v)
		{
			int literalSize = ((LiteralArray)Initial).Count;

			if (!v.GetBType().IsImplicitCastableTo(t))
				throw new ImplicitCastException(pos, v.GetBType(), t);

			if (literalSize > t.Size)
			{
				throw new ArrayLiteralTooBigException(pos);
			}
			else if (literalSize < t.Size)
			{
				((LiteralArray)Initial).AppendDefaultValues(t.Size - literalSize);
				HasCompleteUserDefiniedInitialValue = false;
			}
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			if (IsConstant)
				throw new ConstantValueChangedException(Position, Identifier);

			LiteralArray value = Initial as LiteralArray;

			if (value.IsUniform())
			{
				return GenerateCode_Uniform(reversed, value);
			}
			else
			{
				return GenerateCode_Universal(reversed, value);
			}
		}

		private CodePiece GenerateCode_Uniform(bool reversed, LiteralArray value)
		{
			int varXStart = CodePositionX;
			int varXEnd = CodePositionX + Size - 1;
			int varY = CodePositionY;

			if (reversed)
			{
				// $_v#!`\{X2}:p{Y}\{V}:<{X1}
				//   >1+                ^
				CodePiece p = new CodePiece();

				p.AppendRight(BCHelper.StackPop);
				p.AppendRight(BCHelper.IfHorizontal);

				int botStart = p.MaxX;

				p.AppendRight(BCHelper.PCDown);
				p.AppendRight(BCHelper.PCJump);
				p.AppendRight(BCHelper.Not);
				p.AppendRight(BCHelper.GreaterThan);
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(NumberCodeHelper.GenerateCode(varXEnd, reversed));
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(BCHelper.ReflectSet);
				p.AppendRight(NumberCodeHelper.GenerateCode(varY, reversed));
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(value.GenerateCode(0, reversed));
				p.AppendRight(BCHelper.StackDup);

				int botEnd = p.MaxX;

				p.AppendRight(BCHelper.PCLeft);
				p.AppendRight(NumberCodeHelper.GenerateCode(varXStart, reversed));

				p[botStart + 0, 1] = BCHelper.PCRight;
				p[botStart + 1, 1] = BCHelper.Digit1;
				p[botStart + 2, 1] = BCHelper.Add;

				p.FillRowWw(1, botStart + 3, botEnd);

				p[botEnd, 1] = BCHelper.PCUp;

				return p;
			}
			else
			{
				// {X1}>:{V}\{Y}p:{X2}\`#v_$
				//     ^+1               < 
				CodePiece p = new CodePiece();

				p.AppendRight(NumberCodeHelper.GenerateCode(varXStart, reversed));

				int botStart = p.MaxX;

				p.AppendRight(BCHelper.PCRight);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(value.GenerateCode(0, reversed));
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(NumberCodeHelper.GenerateCode(varY, reversed));
				p.AppendRight(BCHelper.ReflectSet);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(NumberCodeHelper.GenerateCode(varXEnd, reversed));
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(BCHelper.GreaterThan);
				p.AppendRight(BCHelper.PCJump);

				int botEnd = p.MaxX;

				p.AppendRight(BCHelper.PCDown);
				p.AppendRight(BCHelper.IfHorizontal);
				p.AppendRight(BCHelper.StackPop);

				p[botStart + 0, 1] = BCHelper.PCUp;
				p[botStart + 1, 1] = BCHelper.Add;
				p[botStart + 2, 1] = BCHelper.Digit1;

				p.FillRowWw(1, botStart + 3, botEnd);

				p[botEnd, 1] = BCHelper.PCLeft;

				return p;
			}
		}

		private CodePiece GenerateCode_Universal(bool reversed, LiteralArray value)
		{
			CodePiece p = new CodePiece();

			int varX = CodePositionX - 1;
			int varY = CodePositionY;

			if (reversed)
			{
				p.AppendLeft(BCHelper.Digit0);

				for (int i = 0; i < Size; i++)
				{
					p.AppendLeft(value.GenerateCode(i, reversed));
					p.AppendLeft(NumberCodeHelper.GenerateCode(i + 1, reversed));
				}

				// ################################

				//   >       v
				// $_^#!:pY+X<
				CodePiece op = new CodePiece();

				op.AppendLeft(BCHelper.PCLeft);
				op.AppendLeft(NumberCodeHelper.GenerateCode(varX, reversed));
				op.AppendLeft(BCHelper.Add);
				op.AppendLeft(NumberCodeHelper.GenerateCode(varY, reversed));
				op.AppendLeft(BCHelper.ReflectSet);
				op.AppendLeft(BCHelper.StackDup);
				op.AppendLeft(BCHelper.Not);
				op.AppendLeft(BCHelper.PCJump);
				op.AppendLeft(BCHelper.PCUp);

				op[-1, -1] = BCHelper.PCDown;
				op.FillRowWw(-1, op.MinX + 1, -1);
				op[op.MinX, -1] = BCHelper.PCRight;

				op.AppendLeft(BCHelper.IfHorizontal);
				op.AppendLeft(BCHelper.StackPop);

				// ################################

				p.AppendLeft(op);
			}
			else
			{
				p.AppendRight(BCHelper.Digit0);

				for (int i = 0; i < Size; i++)
				{
					p.AppendRight(value.GenerateCode(i, reversed));
					p.AppendRight(NumberCodeHelper.GenerateCode(i + 1, reversed));
				}

				// ################################

				// >X+Yp:#v_$
				// ^      <
				CodePiece op = new CodePiece();

				op.AppendRight(BCHelper.PCRight);
				op.AppendRight(NumberCodeHelper.GenerateCode(varX, reversed));
				op.AppendRight(BCHelper.Add);
				op.AppendRight(NumberCodeHelper.GenerateCode(varY, reversed));
				op.AppendRight(BCHelper.ReflectSet);
				op.AppendRight(BCHelper.StackDup);
				op.AppendRight(BCHelper.PCJump);
				op.AppendRight(BCHelper.PCDown);

				op[0, 1] = BCHelper.PCUp;
				op.FillRowWw(1, 1, op.MaxX - 1);
				op[op.MaxX - 1, 1] = BCHelper.PCLeft;

				op.AppendRight(BCHelper.IfHorizontal);
				op.AppendRight(BCHelper.StackPop);

				// ################################

				p.AppendRight(op);
			}

			return p;
		}

		public override CodePiece GenerateCode_SetToStackVal(bool reversed)
		{
			if (IsConstant)
				throw new ConstantValueChangedException(Position, Identifier);

			return CodePieceStore.WriteArrayFromStack(this, reversed);
		}
	}

	#endregion
}