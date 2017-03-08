using BefunGen.AST.CodeGen;
using BefunGen.AST.Exceptions;
using System.Linq;
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

		public abstract int GetSize();

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
	}

	public abstract class BTypeValue : BType
	{
		public BTypeValue(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override int GetSize()
		{
			return 1;
		}
	}

	public abstract class BTypeArray : BType
	{
		public BTypeValue InternalType { get { return GetInternType(); } }

		public readonly int Size;

		public BTypeArray(SourceCodePosition pos, int sz)
			: base(pos)
		{
			Size = sz;
		}

		public override int GetSize()
		{
			return Size;
		}

		public override bool Equals(BType p)
		{
			if ((object)p == null)
				return false;

			return this.GetType() == p.GetType() && (p as BTypeArray).Size == Size;
		}

		public override int GetHashCode()
		{
			return 10000 + GetPriority() * (Size + 1);
		}

		protected abstract BTypeValue GetInternType();
	}

	public class BTypeVoid : BType // neither Array nor Value ...
	{
		public BTypeVoid(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override int GetSize()
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
	}

	public class BTypeUnion : BType // Only for internal cast - is castable to everything
	{
		public BTypeUnion(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public override int GetSize()
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
	}

	public abstract class BTypeStack : BType
	{
		public BTypeValue InternalType { get { return GetInternType(); } }

		public readonly int Size;

		public BTypeStack(SourceCodePosition pos, int sz)
			: base(pos)
		{
			Size = sz;
		}

		public override int GetSize()
		{
			return Size;
		}

		public override bool Equals(BType p)
		{
			if ((object)p == null)
				return false;

			return this.GetType() == p.GetType() && (p as BTypeStack).Size == Size;
		}

		public override int GetHashCode()
		{
			return 20000 + GetPriority() * (Size + 1);
		}

		protected abstract BTypeValue GetInternType();
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
			return string.Format("int[{0}]", Size);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralIntArr(new SourceCodePosition(), Enumerable.Repeat((long)CGO.DefaultNumeralValue, Size).ToList());
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeArray && (other as BTypeArray).Size == Size && (other is BTypeIntArr));
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
			return string.Format("char[{0}]", Size);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralCharArr(new SourceCodePosition(), Enumerable.Repeat(CGO.DefaultCharacterValue, Size).ToList());
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeArray && (other as BTypeArray).Size == Size && (other is BTypeCharArr));
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
			return string.Format("digit[{0}]", Size);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralDigitArr(new SourceCodePosition(), Enumerable.Repeat(CGO.DefaultNumeralValue, Size).ToList());
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeArray && (other as BTypeArray).Size == Size && (other is BTypeDigitArr || other is BTypeIntArr));
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
			return string.Format("bool[{0}]", Size);
		}

		public override Literal GetDefaultValue()
		{
			return new LiteralBoolArr(new SourceCodePosition(), Enumerable.Repeat(CGO.DefaultBooleanValue, Size).ToList());
		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeArray && (other as BTypeArray).Size == Size && (other is BTypeBoolArr));
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

	public class BTypeIntStack : BTypeArray
	{
		public BTypeIntStack(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("int_stack<{0}>", Size);
		}

		public override Literal GetDefaultValue()
		{

		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeStack && (other as BTypeStack).Size == Size && (other is BTypeIntStack));
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

	public class BTypeCharStack : BTypeArray
	{
		public BTypeCharStack(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("char_stack<{0}>", Size);
		}

		public override Literal GetDefaultValue()
		{

		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeStack && (other as BTypeStack).Size == Size && (other is BTypeCharStack));
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

	public class BTypeDigitStack : BTypeArray
	{
		public BTypeDigitStack(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("digit_stack<{0}>", Size);
		}

		public override Literal GetDefaultValue()
		{

		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeStack && (other as BTypeStack).Size == Size && (other is BTypeDigitStack));
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

	public class BTypeBoolStack : BTypeArray
	{
		public BTypeBoolStack(SourceCodePosition pos, int sz)
			: base(pos, sz)
		{
		}

		public override string GetDebugString()
		{
			return string.Format("bool_stack<{0}>", Size);
		}

		public override Literal GetDefaultValue()
		{

		}

		public override bool IsImplicitCastableTo(BType other)
		{
			return (other is BTypeStack && (other as BTypeStack).Size == Size && (other is BTypeBoolStack));
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