using BefunGen.AST.CodeGen;
using BefunGen.AST.Exceptions;
using System.Linq;
using System;
using BefunGen.AST.CodeGen.NumberCode;
using BefunGen.MathExtensions;
using BefunGen.AST.CodeGen.Tags;

namespace BefunGen.AST
{
	#region Parents

	public abstract class BType : ASTObject
	{
		protected const int PRIORITY_VOID = 0;
		protected const int PRIORITY_BOOL = 1;
		protected const int PRIORITY_DIGIT = 2;
		protected const int PRIORITY_CHAR = 3;
		protected const int PRIORITY_INT = 4;
		protected const int PRIORITY_UNION = 99;

		public BType(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public abstract int GetCodeSize();

		public override bool Equals(System.Object obj)
		{
			if (obj == null)
				return false;

			return this.Equals(obj as BType);
		}

		public virtual bool Equals(BType p)
		{
			if ((object)p == null)
				return false;

			return GetType() == p.GetType();
		}

		public static bool operator ==(BType a, BType b)
		{
			return (object)a != null && a.Equals(b);
		}

		public static bool operator !=(BType a, BType b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return -GetPriority();
		}

		public override string ToString()
		{
			return GetDebugString();
		}

		public abstract Literal GetDefaultValue();
		public abstract bool IsImplicitCastableTo(BType other);
		public abstract int GetPriority();

		public abstract CodePiece GenerateCodeAssignment(SourceCodePosition pos, Expression source, ExpressionValuePointer target, bool reversed);
		public abstract CodePiece GenerateCodePopValueFromStack(SourceCodePosition pos, bool reversed);
		public abstract CodePiece GenerateCodeWriteFromStackToGrid(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed);
		public abstract CodePiece GenerateCodeReadFromGridToStack(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed);
		public abstract CodePiece GenerateCodeReturnFromMethodCall(SourceCodePosition pos, Expression value, bool reversed);
	}

	public abstract class BTypeValue : BType
	{
		public BTypeValue(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override int GetCodeSize()
		{
			return 1;
		}

		public override CodePiece GenerateCodeAssignment(SourceCodePosition pos, Expression source, ExpressionValuePointer target, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				p.AppendLeft(source.GenerateCode(reversed));
				p.AppendLeft(target.GenerateCodeSingle(reversed));

				p.AppendLeft(BCHelper.ReflectSet);

				p.NormalizeX();
			}
			else
			{
				p.AppendRight(source.GenerateCode(reversed));
				p.AppendRight(target.GenerateCodeSingle(reversed));

				p.AppendRight(BCHelper.ReflectSet);

				p.NormalizeX();
			}

			return p;
		}

		public override CodePiece GenerateCodePopValueFromStack(SourceCodePosition pos, bool reversed)
		{
			return new CodePiece(BCHelper.StackPop);
		}

		public override CodePiece GenerateCodeWriteFromStackToGrid(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			CodePiece p = new CodePiece();
			if (reversed)
			{
				p.AppendLeft(NumberCodeHelper.GenerateCode(gridPos.X, reversed));
				p.AppendLeft(NumberCodeHelper.GenerateCode(gridPos.Y, reversed));
				p.AppendLeft(BCHelper.ReflectSet);
			}
			else
			{
				p.AppendRight(NumberCodeHelper.GenerateCode(gridPos.X, reversed));
				p.AppendRight(NumberCodeHelper.GenerateCode(gridPos.Y, reversed));

				p.AppendRight(BCHelper.ReflectSet);
			}
			return p;
		}

		public override CodePiece GenerateCodeReadFromGridToStack(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			CodePiece p = new CodePiece();
			if (reversed)
			{
				p.AppendLeft(NumberCodeHelper.GenerateCode(gridPos.X, reversed));
				p.AppendLeft(NumberCodeHelper.GenerateCode(gridPos.Y, reversed));

				p.AppendLeft(BCHelper.ReflectGet);
			}
			else
			{
				p.AppendRight(NumberCodeHelper.GenerateCode(gridPos.X, reversed));
				p.AppendRight(NumberCodeHelper.GenerateCode(gridPos.Y, reversed));

				p.AppendRight(BCHelper.ReflectGet);
			}
			return p;
		}

		public override CodePiece GenerateCodeReturnFromMethodCall(SourceCodePosition pos, Expression value, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				p.AppendRight(BCHelper.Digit0); // Right Lane

				p.AppendRight(BCHelper.StackSwap); // Swap BackjumpAddr back to Stack-Front

				p.AppendRight(value.GenerateCode(reversed));

				#endregion
			}
			else
			{
				#region Normal

				p.AppendRight(value.GenerateCode(reversed));

				p.AppendRight(BCHelper.StackSwap); // Swap BackjumpAddr back to Stack-Front

				p.AppendRight(BCHelper.Digit0); // Right Lane

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				#endregion

			}

			p.NormalizeX();
			return p;
		}
	}

	public abstract class BTypeArray : BType
	{
		public BTypeValue InternalType { get { return GetInternType(); } }

		public readonly int ArraySize;

		public BTypeArray(SourceCodePosition pos, int sz)
			: base(pos)
		{
			ArraySize = sz;
		}

		public override int GetCodeSize()
		{
			return ArraySize;
		}

		public override bool Equals(BType p)
		{
			if ((object)p == null)
				return false;

			return this.GetType() == p.GetType() && (p as BTypeArray).ArraySize == ArraySize;
		}

		public override int GetHashCode()
		{
			return 10000 + GetPriority() * (ArraySize + 1);
		}

		protected abstract BTypeValue GetInternType();

		public override CodePiece GenerateCodeAssignment(SourceCodePosition pos, Expression source, ExpressionValuePointer target, bool reversed)
		{
			CodePiece p = new CodePiece();

			BTypeArray type = target.GetResultType() as BTypeArray;
			ExpressionDirectValuePointer vPointer = target as ExpressionDirectValuePointer;

			if (reversed)
			{
				p.AppendLeft(source.GenerateCode(reversed));
				p.AppendLeft(CodePieceStore.WriteArrayFromStack(vPointer.Target.CodeDeclarationPos, reversed));

				p.NormalizeX();
			}
			else
			{
				p.AppendRight(source.GenerateCode(reversed));
				p.AppendRight(CodePieceStore.WriteArrayFromStack(vPointer.Target.CodeDeclarationPos, reversed));

				p.NormalizeX();
			}

			return p;
		}

		public override CodePiece GenerateCodePopValueFromStack(SourceCodePosition pos, bool reversed)
		{
			return CodePieceStore.PopMultipleStackValues(ArraySize, reversed);
		}

		public override CodePiece GenerateCodeWriteFromStackToGrid(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			return CodePieceStore.WriteArrayFromStack(gridPos, reversed);
		}

		public override CodePiece GenerateCodeReadFromGridToStack(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			return CodePieceStore.ReadArrayToStack(gridPos, reversed);
		}

		public override CodePiece GenerateCodeReturnFromMethodCall(SourceCodePosition pos, Expression value, bool reversed)
		{
			CodePiece p = new CodePiece();
			
			if (reversed)
			{
				#region Reversed

				p.AppendLeft(value.GenerateCode(reversed));

				// Switch ReturnValue (Array)  and  BackJumpAddr

				p.AppendLeft(CodePieceStore.WriteArrayFromStack(CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendLeft(CodePieceStore.WriteValueToField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));

				p.AppendLeft(CodePieceStore.ReadArrayToStack(CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendLeft(CodePieceStore.ReadValueFromField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));


				p.AppendLeft(BCHelper.Digit0); // Right Lane

				p.AppendLeft(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				#endregion
			}
			else
			{
				#region Normal

				p.AppendRight(value.GenerateCode(reversed));

				// Switch ReturnValue (Array)  and  BackJumpAddr

				p.AppendRight(CodePieceStore.WriteArrayFromStack(CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendRight(CodePieceStore.WriteValueToField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));

				p.AppendRight(CodePieceStore.ReadArrayToStack(CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendRight(CodePieceStore.ReadValueFromField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));


				p.AppendRight(BCHelper.Digit0); // Right Lane

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				#endregion

			}

			p.NormalizeX();
			return p;
		}
	}

	public class BTypeVoid : BType // neither Array nor Value ...
	{
		public BTypeVoid(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override int GetCodeSize()
		{
			return 1;
		}

		public override string GetDebugString()
		{
			return "void";
		}

		public override Literal GetDefaultValue()
		{
			throw new VoidObjectCallException(Position);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return false;
		}

		public override int GetPriority()
		{
			return PRIORITY_VOID;
		}

		public override CodePiece GenerateCodeAssignment(SourceCodePosition pos, Expression source, ExpressionValuePointer target, bool reversed)
		{
			throw new InvalidAstStateException(pos);
		}

		public override CodePiece GenerateCodePopValueFromStack(SourceCodePosition pos, bool reversed)
		{
			return new CodePiece(BCHelper.StackPop);
		}

		public override CodePiece GenerateCodeWriteFromStackToGrid(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			return new CodePiece(BCHelper.StackPop); // Nobody cares about the result ...
		}

		public override CodePiece GenerateCodeReadFromGridToStack(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			return CodePiece.Empty; // Do nothing
		}

		public override CodePiece GenerateCodeReturnFromMethodCall(SourceCodePosition pos, Expression value, bool reversed)
		{
			CodePiece p = CodePiece.ParseFromLine(@"0\0");

			p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

			if (reversed) p.ReverseX(false);

			return p;
		}
	}

	public class BTypeUnion : BType // Only for internal cast - is castable to everything
	{
		public BTypeUnion(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override int GetCodeSize()
		{
			throw new InvalidAstStateException(Position);
		}

		public override string GetDebugString()
		{
			return "union";
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralInt(new SourceCodePosition(), 0);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return true;
		}

		public override int GetPriority()
		{
			return PRIORITY_UNION;
		}

		public override CodePiece GenerateCodeAssignment(SourceCodePosition pos, Expression source, ExpressionValuePointer target, bool reversed)
		{
			throw new InvalidAstStateException(pos);
		}

		public override CodePiece GenerateCodePopValueFromStack(SourceCodePosition pos, bool reversed)
		{
			throw new InvalidAstStateException(pos);
		}

		public override CodePiece GenerateCodeWriteFromStackToGrid(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			throw new InvalidAstStateException(pos);
		}

		public override CodePiece GenerateCodeReadFromGridToStack(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			throw new InvalidAstStateException(pos);
		}

		public override CodePiece GenerateCodeReturnFromMethodCall(SourceCodePosition pos, Expression value, bool reversed)
		{
			throw new InvalidAstStateException(pos);
		}
	}

	public abstract class BTypeStack : BType
	{
		public BTypeValue InternalType { get { return GetInternType(); } }

		public readonly int StackSize;

		public BTypeStack(SourceCodePosition pos, int sz)
			: base(pos)
		{
			StackSize = sz;
		}

		public override int GetCodeSize()
		{
			return StackSize + 1;
		}

		public override bool Equals(BType p)
		{
			if ((object)p == null)
				return false;

			return this.GetType() == p.GetType() && (p as BTypeStack).StackSize == StackSize;
		}

		public override int GetHashCode()
		{
			return 20000 + GetPriority() * (StackSize + 1);
		}

		protected abstract BTypeValue GetInternType();

		public override CodePiece GenerateCodeAssignment(SourceCodePosition pos, Expression source, ExpressionValuePointer target, bool reversed)
		{
			CodePiece p = new CodePiece();

			BTypeArray type = target.GetResultType() as BTypeArray;
			ExpressionDirectValuePointer vPointer = target as ExpressionDirectValuePointer;

			if (reversed)
			{
				p.AppendLeft(source.GenerateCode(reversed));
				p.AppendLeft(CodePieceStore.WriteArrayFromStack(vPointer.Target.CodeDeclarationPos, reversed));

				p.NormalizeX();
			}
			else
			{
				p.AppendRight(source.GenerateCode(reversed));
				p.AppendRight(CodePieceStore.WriteArrayFromStack(vPointer.Target.CodeDeclarationPos, reversed));

				p.NormalizeX();
			}

			return p;
		}

		public override CodePiece GenerateCodePopValueFromStack(SourceCodePosition pos, bool reversed)
		{
			return CodePieceStore.PopMultipleStackValues(StackSize + 1, reversed);
		}

		public override CodePiece GenerateCodeWriteFromStackToGrid(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			return CodePieceStore.WriteArrayFromStack(gridPos, reversed);
		}

		public override CodePiece GenerateCodeReadFromGridToStack(SourceCodePosition pos, VarDeclarationPosition gridPos, bool reversed)
		{
			return CodePieceStore.ReadArrayToStack(gridPos, reversed);
		}

		public override CodePiece GenerateCodeReturnFromMethodCall(SourceCodePosition pos, Expression value, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed

				p.AppendLeft(value.GenerateCode(reversed));

				// Switch ReturnValue (Array)  and  BackJumpAddr

				p.AppendLeft(CodePieceStore.WriteArrayFromStack(CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendLeft(CodePieceStore.WriteValueToField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));

				p.AppendLeft(CodePieceStore.ReadArrayToStack(CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendLeft(CodePieceStore.ReadValueFromField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));
				
				p.AppendLeft(BCHelper.Digit0); // Right Lane

				p.AppendLeft(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				#endregion
			}
			else
			{
				#region Normal

				p.AppendRight(value.GenerateCode(reversed));

				// Switch ReturnValue (Array)  and  BackJumpAddr

				p.AppendRight(CodePieceStore.WriteArrayFromStack(CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendRight(CodePieceStore.WriteValueToField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));

				p.AppendRight(CodePieceStore.ReadArrayToStack(CodeGenConstants.TMP_ARRFIELD_RETURNVAL, reversed));
				p.AppendRight(CodePieceStore.ReadValueFromField(CodeGenConstants.TMP_FIELD_JMP_ADDR, reversed));
				
				p.AppendRight(BCHelper.Digit0); // Right Lane

				p.AppendRight(BCHelper.PC_Up_tagged(new MethodCallVerticalExitTag()));

				#endregion

			}

			p.NormalizeX();
			return p;
		}
	}

	#endregion

	#region Value Types

	public class BTypeInt : BTypeValue
	{
		public BTypeInt(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override string GetDebugString()
		{
			return "int";
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralInt(new SourceCodePosition(), CGO.DefaultNumeralValue);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeInt);
		}

		public override int GetPriority()
		{
			return PRIORITY_INT;
		}
	}

	public class BTypeDigit : BTypeValue
	{
		public BTypeDigit(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override string GetDebugString()
		{
			return "digit";
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralDigit(new SourceCodePosition(), CGO.DefaultNumeralValue);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeDigit || other is BTypeInt);
		}

		public override int GetPriority()
		{
			return PRIORITY_DIGIT;
		}
	}

	public class BTypeChar : BTypeValue
	{
		public BTypeChar(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override string GetDebugString()
		{
			return "char";
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralChar(new SourceCodePosition(), CGO.DefaultCharacterValue);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeChar);
		}

		public override int GetPriority()
		{
			return PRIORITY_CHAR;
		}
	}

	public class BTypeBool : BTypeValue
	{
		public BTypeBool(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override string GetDebugString()
		{
			return "bool";
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralBool(new SourceCodePosition(), CGO.DefaultBooleanValue);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeBool);
		}

		public override int GetPriority()
		{
			return PRIORITY_BOOL;
		}
	}

	#endregion Value Types

	#region Array Types

	public class BTypeIntArr : BTypeArray
	{
		public BTypeIntArr(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("int[{0}]", ArraySize);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralIntArr(new SourceCodePosition(), Enumerable.Repeat((long)CGO.DefaultNumeralValue, ArraySize).ToList());
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeArray && (other as BTypeArray).ArraySize == ArraySize && (other is BTypeIntArr));
		}

		public override int GetPriority()
		{
			return PRIORITY_INT;
		}

		protected override BTypeValue GetInternType()
		{
			return new BTypeInt(Position);
		}
	}

	public class BTypeCharArr : BTypeArray
	{
		public BTypeCharArr(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("char[{0}]", ArraySize);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralCharArr(new SourceCodePosition(), Enumerable.Repeat(CGO.DefaultCharacterValue, ArraySize).ToList());
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeArray && (other as BTypeArray).ArraySize == ArraySize && (other is BTypeCharArr));
		}

		public override int GetPriority()
		{
			return PRIORITY_CHAR;
		}

		protected override BTypeValue GetInternType()
		{
			return new BTypeChar(Position);
		}
	}

	public class BTypeDigitArr : BTypeArray
	{
		public BTypeDigitArr(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("digit[{0}]", ArraySize);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralDigitArr(new SourceCodePosition(), Enumerable.Repeat(CGO.DefaultNumeralValue, ArraySize).ToList());
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeArray && (other as BTypeArray).ArraySize == ArraySize && (other is BTypeDigitArr || other is BTypeIntArr));
		}

		public override int GetPriority()
		{
			return PRIORITY_DIGIT;
		}

		protected override BTypeValue GetInternType()
		{
			return new BTypeDigit(Position);
		}
	}

	public class BTypeBoolArr : BTypeArray
	{
		public BTypeBoolArr(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("bool[{0}]", ArraySize);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralBoolArr(new SourceCodePosition(), Enumerable.Repeat(CGO.DefaultBooleanValue, ArraySize).ToList());
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeArray && (other as BTypeArray).ArraySize == ArraySize && (other is BTypeBoolArr));
		}

		public override int GetPriority()
		{
			return PRIORITY_BOOL;
		}

		protected override BTypeValue GetInternType()
		{
			return new BTypeBool(Position);
		}
	}

	#endregion Array Types

	#region Stack Types

	public class BTypeIntStack : BTypeStack
	{
		public BTypeIntStack(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("int_stack<{0}>", StackSize);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralIntStack(new SourceCodePosition(), StackSize);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeStack && (other as BTypeStack).StackSize == StackSize && (other is BTypeIntStack));
		}

		public override int GetPriority()
		{
			return PRIORITY_INT;
		}

		protected override BTypeValue GetInternType()
		{
			return new BTypeInt(Position);
		}
	}

	public class BTypeCharStack : BTypeStack
	{
		public BTypeCharStack(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("char_stack<{0}>", StackSize);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralCharStack(new SourceCodePosition(), StackSize);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeStack && (other as BTypeStack).StackSize == StackSize && (other is BTypeCharStack));
		}

		public override int GetPriority()
		{
			return PRIORITY_CHAR;
		}

		protected override BTypeValue GetInternType()
		{
			return new BTypeChar(Position);
		}
	}

	public class BTypeDigitStack : BTypeStack
	{
		public BTypeDigitStack(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("digit_stack<{0}>", StackSize);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralDigitStack(new SourceCodePosition(), StackSize);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeStack && (other as BTypeStack).StackSize == StackSize && (other is BTypeDigitStack));
		}

		public override int GetPriority()
		{
			return PRIORITY_DIGIT;
		}

		protected override BTypeValue GetInternType()
		{
			return new BTypeDigit(Position);
		}
	}

	public class BTypeBoolStack : BTypeStack
	{
		public BTypeBoolStack(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("bool_stack<{0}>", StackSize);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralBoolStack(new SourceCodePosition(), StackSize);
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeStack && (other as BTypeStack).StackSize == StackSize && (other is BTypeBoolStack));
		}

		public override int GetPriority()
		{
			return PRIORITY_BOOL;
		}

		protected override BTypeValue GetInternType()
		{
			return new BTypeBool(Position);
		}
	}

	#endregion
}