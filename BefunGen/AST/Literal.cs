using BefunGen.AST.CodeGen;
using BefunGen.AST.CodeGen.NumberCode;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BefunGen.AST
{
	public abstract class Literal : ASTObject
	{
		public Literal(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		protected string EscapeChar(char input)
		{
			using (var writer = new StringWriter())
			{
				using (var provider = CodeDomProvider.CreateProvider("CSharp"))
				{
					provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
					return writer.ToString();
				}
			}
		}

		protected string EscapeString(string input)
		{
			using (var writer = new StringWriter())
			{
				using (var provider = CodeDomProvider.CreateProvider("CSharp"))
				{
					provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
					return writer.ToString();
				}
			}
		}

		public abstract BType GetBType();

		public abstract CodePiece GenerateCode(bool reversed);

	}

	#region Parents

	public abstract class LiteralValue : Literal
	{
		public LiteralValue(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public abstract bool ValueEquals(LiteralValue o);

		public abstract long GetValueAsInt();
	}

	public abstract class LiteralArray : Literal
	{
		public int Count { get { return GetCount(); } }

		public LiteralArray(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		protected abstract int GetCount();
		protected abstract void AppendDefaultValue();

		public void AppendDefaultValues(int cnt)
		{
			for (int i = 0; i < cnt; i++)
				AppendDefaultValue();
		}

		public abstract CodePiece GenerateCode(int pos, bool reversed);

		public abstract bool IsUniform();
	}

	public abstract class LiteralStack : Literal
	{
		public int StackSize { get { return GetStackSize(); } }

		public LiteralStack(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		protected abstract int GetStackSize();
	}

	#endregion Parents

	#region Value Literals

	public class LiteralInt : LiteralValue
	{
		public readonly long Value;

		public LiteralInt(SourceCodePosition pos, long v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string GetDebugString()
		{
			return Value.ToString();
		}

		public override BType GetBType()
		{
			return new BTypeInt(new SourceCodePosition());
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			return NumberCodeHelper.GenerateCode(Value, reversed);
		}

		public override bool ValueEquals(LiteralValue o)
		{
			return (o is LiteralInt) && (o as LiteralInt).Value == this.Value;
		}

		public override long GetValueAsInt()
		{
			return Value;
		}
	}

	public class LiteralChar : LiteralValue
	{
		public readonly char Value;

		public LiteralChar(SourceCodePosition pos, char v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string GetDebugString()
		{
			return EscapeChar(Value);
		}

		public override BType GetBType()
		{
			return new BTypeChar(new SourceCodePosition());
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			return NumberCodeFactoryStringmodeChar.GenerateCode(Value, reversed) ?? NumberCodeHelper.GenerateCode(Value, reversed);
		}

		public override bool ValueEquals(LiteralValue o)
		{
			return (o is LiteralChar) && (o as LiteralChar).Value == this.Value;
		}

		public override long GetValueAsInt()
		{
			return Value;
		}
	}

	public class LiteralBool : LiteralValue
	{
		public readonly bool Value;

		public LiteralBool(SourceCodePosition pos, bool v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string GetDebugString()
		{
			return Value.ToString();
		}

		public override BType GetBType()
		{
			return new BTypeBool(new SourceCodePosition());
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			return NumberCodeFactoryBoolean.GenerateCode(Value);
		}

		public override bool ValueEquals(LiteralValue o)
		{
			return (o is LiteralBool) && (o as LiteralBool).Value == this.Value;
		}

		public override long GetValueAsInt()
		{
			return Value ? 1 : 0;
		}
	}

	public class LiteralDigit : LiteralValue
	{
		public readonly byte Value;

		public LiteralDigit(SourceCodePosition pos, byte v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string GetDebugString()
		{
			return Value.ToString();
		}

		public override BType GetBType()
		{
			return new BTypeDigit(new SourceCodePosition());
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			return NumberCodeFactoryDigit.GenerateCode(Value);
		}

		public override bool ValueEquals(LiteralValue o)
		{
			return (o is LiteralDigit) && (o as LiteralDigit).Value == this.Value;
		}

		public override long GetValueAsInt()
		{
			return Value;
		}
	}

	#endregion Value Literals

	#region Array Literals

	public class LiteralIntArr : LiteralArray
	{
		public List<long> Value = new List<long>();

		public LiteralIntArr(SourceCodePosition pos, List<long> v)
			: base(pos)
		{
			this.Value = v.ToList();
		}

		public override string GetDebugString()
		{
			return "{" + string.Join(",", Value.Select(p => p.ToString())) + "}";
		}

		protected override int GetCount()
		{
			return Value.Count;
		}

		public override BType GetBType()
		{
			return new BTypeIntArr(new SourceCodePosition(), Count);
		}

		public override bool IsUniform()
		{
			return Value.All(p => p == Value[0]);
		}

		protected override void AppendDefaultValue()
		{
			Value.Add(CGO.DefaultNumeralValue);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();

			foreach (int val in Value)
			{
				if (reversed)
					p.AppendRight(NumberCodeHelper.GenerateCode(val, reversed));
				else
					p.AppendLeft(NumberCodeHelper.GenerateCode(val, reversed));
			}

			p.NormalizeX();

			return p;
		}

		public override CodePiece GenerateCode(int pos, bool reversed)
		{
			return NumberCodeHelper.GenerateCode(Value[pos], reversed);
		}
	}

	public class LiteralCharArr : LiteralArray
	{
		public List<char> Value = new List<char>();

		public LiteralCharArr(SourceCodePosition pos, List<char> v)
			: base(pos)
		{
			this.Value = v.ToList();
		}

		public LiteralCharArr(SourceCodePosition pos, string v)
			: base(pos)
		{
			this.Value = v.ToCharArray().ToList();
		}

		public override string GetDebugString()
		{
			return EscapeString(string.Join("", Value));
		}

		protected override int GetCount()
		{
			return Value.Count;
		}

		public override bool IsUniform()
		{
			return Value.All(p => p == Value[0]);
		}

		public override BType GetBType()
		{
			return new BTypeCharArr(new SourceCodePosition(), Count);
		}

		protected override void AppendDefaultValue()
		{
			Value.Add(CGO.DefaultCharacterValue);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				foreach (char val in Value.Reverse<char>()) // Reverse Value -> correct stack order
				{
					p.AppendLeft(NumberCodeFactoryStringmodeChar.GenerateCode(val, reversed) ?? NumberCodeHelper.GenerateCode(val, reversed));
				}
			}
			else
			{
				foreach (char val in Value.Reverse<char>())// Reverse Value -> correct stack order
				{
					p.AppendRight(NumberCodeFactoryStringmodeChar.GenerateCode(val, reversed) ?? NumberCodeHelper.GenerateCode(val, reversed));
				}
			}

			p.NormalizeX();

			p.TrimDoubleStringMode();

			return p;
		}

		public override CodePiece GenerateCode(int pos, bool reversed)
		{
			return NumberCodeFactoryStringmodeChar.GenerateCode(Value[pos], reversed) ?? NumberCodeHelper.GenerateCode(pos, reversed);
		}
	}

	public class LiteralBoolArr : LiteralArray
	{
		public List<bool> Value = new List<bool>();

		public LiteralBoolArr(SourceCodePosition pos, List<bool> v)
			: base(pos)
		{
			this.Value = v.ToList();
		}

		public override string GetDebugString()
		{
			return "{" + string.Join(",", Value.Select(p => p.ToString())) + "}";
		}

		protected override int GetCount()
		{
			return Value.Count;
		}

		public override BType GetBType()
		{
			return new BTypeBoolArr(new SourceCodePosition(), Count);
		}

		public override bool IsUniform()
		{
			return Value.All(p => p == Value[0]);
		}

		protected override void AppendDefaultValue()
		{
			Value.Add(CGO.DefaultBooleanValue);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();
			int i = 0;

			if (reversed)
			{
				foreach (bool val in Value)
				{
					p[i--, 0] = BCHelper.Dig(val ? (byte)1 : (byte)0);
				}
			}
			else
			{
				foreach (bool val in Value)
				{
					p[i++, 0] = BCHelper.Dig(val ? (byte)1 : (byte)0);
				}
			}

			p.NormalizeX();

			return p;
		}

		public override CodePiece GenerateCode(int pos, bool reversed)
		{
			return NumberCodeFactoryBoolean.GenerateCode(Value[pos]);
		}
	}

	public class LiteralDigitArr : LiteralArray
	{
		public List<byte> Value = new List<byte>();

		public LiteralDigitArr(SourceCodePosition pos, List<byte> v)
			: base(pos)
		{
			this.Value = v.ToList();
		}

		public override string GetDebugString()
		{
			return "{" + string.Join(",", Value.Select(p => p.ToString())) + "}";
		}

		protected override int GetCount()
		{
			return Value.Count;
		}

		public override BType GetBType()
		{
			return new BTypeDigitArr(new SourceCodePosition(), Count);
		}

		public override bool IsUniform()
		{
			return Value.All(p => p == Value[0]);
		}

		protected override void AppendDefaultValue()
		{
			Value.Add(CGO.DefaultNumeralValue);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			CodePiece p = new CodePiece();
			int i = 0;

			if (reversed)
			{
				foreach (byte val in Value)
				{
					p[i--, 0] = BCHelper.Dig(val);
				}
			}
			else
			{
				foreach (byte val in Value)
				{
					p[i++, 0] = BCHelper.Dig(val);
				}
			}

			p.NormalizeX();

			return p;
		}

		public override CodePiece GenerateCode(int pos, bool reversed)
		{
			return NumberCodeFactoryDigit.GenerateCode(Value[pos]);
		}
	}

	#endregion Array Literals

	#region Stack Literals

	public class LiteralIntStack : LiteralStack
	{
		private readonly int size;

		public LiteralIntStack(SourceCodePosition pos, int stacksize)
			: base(pos)
		{
			size = stacksize;
		}

		public override string GetDebugString()
		{
			return "stack<int," + size + ">";
		}

		protected override int GetStackSize()
		{
			return size;
		}

		public override BType GetBType()
		{
			return new BTypeIntStack(new SourceCodePosition(), StackSize);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			throw new NotImplementedException();
		}
	}

	public class LiteralCharStack : LiteralStack
	{
		private readonly int size;

		public LiteralCharStack(SourceCodePosition pos, int stacksize)
			: base(pos)
		{
			size = stacksize;
		}

		public override string GetDebugString()
		{
			return "stack<char," + size + ">";
		}

		protected override int GetStackSize()
		{
			return size;
		}

		public override BType GetBType()
		{
			return new BTypeCharStack(new SourceCodePosition(), StackSize);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			throw new NotImplementedException();
		}
	}

	public class LiteralBoolStack : LiteralStack
	{
		private readonly int size;

		public LiteralBoolStack(SourceCodePosition pos, int stacksize)
			: base(pos)
		{
			size = stacksize;
		}

		public override string GetDebugString()
		{
			return "stack<bool," + size + ">";
		}

		protected override int GetStackSize()
		{
			return size;
		}

		public override BType GetBType()
		{
			return new BTypeBoolStack(new SourceCodePosition(), StackSize);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			throw new NotImplementedException();
		}
	}

	public class LiteralDigitStack : LiteralStack
	{
		private readonly int size;

		public LiteralDigitStack(SourceCodePosition pos, int stacksize)
			: base(pos)
		{
			size = stacksize;
		}

		public override string GetDebugString()
		{
			return "stack<digit," + size + ">";
		}

		protected override int GetStackSize()
		{
			return size;
		}

		public override BType GetBType()
		{
			return new BTypeDigitStack(new SourceCodePosition(), StackSize);
		}

		public override CodePiece GenerateCode(bool reversed)
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}