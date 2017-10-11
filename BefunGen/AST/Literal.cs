using BefunGen.AST.CodeGen;
using BefunGen.AST.CodeGen.NumberCode;
using BefunGen.AST.Exceptions;
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

		public abstract CodePiece GenerateCode(CodeGenEnvironment env, bool reversed);

		public abstract long AsNumber();
		public abstract IEnumerable<long> AsNumberList();
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

		public override long AsNumber() => GetValueAsInt();
		public override IEnumerable<long> AsNumberList() { throw new InternalCodeRunException("Cannot cast Number to Array"); }
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

		public abstract CodePiece GenerateCode(CodeGenEnvironment env, int pos, bool reversed);

		public abstract bool IsUniform();

		public override long AsNumber() { throw new InternalCodeRunException("Cannot cast Array to Number"); }

		public abstract long GetValueAsNumber(int idx);
		public abstract void SetValueToNumber(int idx, long value);
		public abstract LiteralArray Clone();

		public char[] ToCharArray() => AsNumberList().Select(p => (char)p).ToArray();
	}

	public abstract class LiteralStack : Literal
	{
		public int StackCapacity => GetStackCapacity();
		public int StackCount    => GetStackCount();

		public LiteralStack(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		protected abstract int GetStackCapacity();
		protected abstract int GetStackCount();

		public override long AsNumber() { throw new InternalCodeRunException("Cannot cast Stack to Number"); }

		public abstract void PushValue(long v);
		public abstract long PopValue();
		public abstract long PeekValue();

		public abstract LiteralStack Clone();
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

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
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

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
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

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
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

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
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

		public override IEnumerable<long> AsNumberList() => Value;

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

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
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

		public override CodePiece GenerateCode(CodeGenEnvironment env, int pos, bool reversed)
		{
			return NumberCodeHelper.GenerateCode(Value[pos], reversed);
		}

		public override long GetValueAsNumber(int idx) => Value[idx];

		public override void SetValueToNumber(int idx, long value)
		{
			Value[idx] = value;
		}

		public override LiteralArray Clone()
		{
			return new LiteralIntArr(Position, Value.ToList());
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

		public override IEnumerable<long> AsNumberList() => Value.Select(p => (long)p);

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

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
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

		public override CodePiece GenerateCode(CodeGenEnvironment env, int pos, bool reversed)
		{
			return NumberCodeFactoryStringmodeChar.GenerateCode(Value[pos], reversed) ?? NumberCodeHelper.GenerateCode(pos, reversed);
		}

		public override long GetValueAsNumber(int idx) => Value[idx];

		public override void SetValueToNumber(int idx, long value)
		{
			Value[idx] = (char)value;
		}

		public override LiteralArray Clone()
		{
			return new LiteralCharArr(Position, Value.ToList());
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

		public override IEnumerable<long> AsNumberList() => Value.Select(p => p?1L:0L);

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

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
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

		public override CodePiece GenerateCode(CodeGenEnvironment env, int pos, bool reversed)
		{
			return NumberCodeFactoryBoolean.GenerateCode(Value[pos]);
		}

		public override long GetValueAsNumber(int idx) => Value[idx] ? 1 : 0;

		public override void SetValueToNumber(int idx, long value)
		{
			Value[idx] = (value!=0);
		}

		public override LiteralArray Clone()
		{
			return new LiteralBoolArr(Position, Value.ToList());
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

		public override IEnumerable<long> AsNumberList() => Value.Select(p => (long)p);

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

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
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

		public override CodePiece GenerateCode(CodeGenEnvironment env, int pos, bool reversed)
		{
			return NumberCodeFactoryDigit.GenerateCode(Value[pos]);
		}

		public override long GetValueAsNumber(int idx) => Value[idx];

		public override void SetValueToNumber(int idx, long value)
		{
			Value[idx] = (byte)value;
		}

		public override LiteralArray Clone()
		{
			return new LiteralDigitArr(Position, Value.ToList());
		}
	}

	#endregion Array Literals

	#region Stack Literals

	public class LiteralIntStack : LiteralStack
	{
		public readonly int capacity;
		public List<long> Value = new List<long>();

		public LiteralIntStack(SourceCodePosition pos, List<long> v, int c)
			: base(pos)
		{
			Value = v.ToList();
			capacity = c;
		}

		public override string GetDebugString()
		{
			return "stack<int," + capacity + ">";
		}

		public override IEnumerable<long> AsNumberList() => Value;

		protected override int GetStackCapacity()
		{
			return capacity;
		}

		protected override int GetStackCount()
		{
			return Value.Count;
		}

		public override BType GetBType()
		{
			return new BTypeIntStack(new SourceCodePosition(), StackCapacity);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			throw new NotImplementedException();
		}

		public override void PushValue(long v)
		{
			Value.Add(v);
		}

		public override long PopValue()
		{
			var r = Value[Value.Count - 1];
			Value.RemoveAt(Value.Count - 1);
			return r;
		}

		public override long PeekValue()
		{
			return Value[Value.Count - 1];
		}

		public override LiteralStack Clone()
		{
			return new LiteralIntStack(Position, Value, capacity);
		}
	}

	public class LiteralCharStack : LiteralStack
	{
		public readonly int capacity;
		public List<char> Value = new List<char>();

		public LiteralCharStack(SourceCodePosition pos, List<char> v, int c)
			: base(pos)
		{
			capacity = c;
			Value = v.ToList();
		}

		public override string GetDebugString()
		{
			return "stack<char," + capacity + ">";
		}

		public override IEnumerable<long> AsNumberList() => Value.Select(p => (long)p);

		protected override int GetStackCapacity()
		{
			return capacity;
		}

		protected override int GetStackCount()
		{
			return Value.Count;
		}

		public override BType GetBType()
		{
			return new BTypeCharStack(new SourceCodePosition(), StackCapacity);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			throw new NotImplementedException();
		}

		public override void PushValue(long v)
		{
			Value.Add((char)v);
		}

		public override long PopValue()
		{
			var r = Value[Value.Count - 1];
			Value.RemoveAt(Value.Count - 1);
			return r;
		}

		public override long PeekValue()
		{
			return Value[Value.Count - 1];
		}

		public override LiteralStack Clone()
		{
			return new LiteralCharStack(Position, Value.ToList(), capacity);
		}
	}

	public class LiteralBoolStack : LiteralStack
	{
		public readonly int capacity;
		public List<bool> Value = new List<bool>();

		public LiteralBoolStack(SourceCodePosition pos, List<bool> v, int c)
			: base(pos)
		{
			capacity = c;
			Value = v.ToList();
		}

		public override string GetDebugString()
		{
			return "stack<bool," + capacity + ">";
		}

		public override IEnumerable<long> AsNumberList() => Value.Select(p => p?1L:0L);

		protected override int GetStackCapacity()
		{
			return capacity;
		}

		protected override int GetStackCount()
		{
			return Value.Count;
		}

		public override BType GetBType()
		{
			return new BTypeBoolStack(new SourceCodePosition(), StackCapacity);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			throw new NotImplementedException();
		}

		public override void PushValue(long v)
		{
			Value.Add(v!=0);
		}

		public override long PopValue()
		{
			var r = Value[Value.Count - 1];
			Value.RemoveAt(Value.Count - 1);
			return r?1:0;
		}

		public override long PeekValue()
		{
			return Value[Value.Count - 1]?1:0;
		}

		public override LiteralStack Clone()
		{
			return new LiteralBoolStack(Position, Value.ToList(), capacity);
		}
	}

	public class LiteralDigitStack : LiteralStack
	{
		public readonly int capacity;
		public List<byte> Value = new List<byte>();

		public LiteralDigitStack(SourceCodePosition pos, List<byte> v, int c)
			: base(pos)
		{
			capacity = c;
			Value = v.ToList();
		}

		public override string GetDebugString()
		{
			return "stack<digit," + capacity + ">";
		}

		public override IEnumerable<long> AsNumberList() => Value.Select(p => (long)p);

		protected override int GetStackCapacity()
		{
			return capacity;
		}

		protected override int GetStackCount()
		{
			return Value.Count;
		}

		public override BType GetBType()
		{
			return new BTypeDigitStack(new SourceCodePosition(), StackCapacity);
		}

		public override CodePiece GenerateCode(CodeGenEnvironment env, bool reversed)
		{
			throw new NotImplementedException();
		}

		public override void PushValue(long v)
		{
			Value.Add((byte)v);
		}

		public override long PopValue()
		{
			var r = Value[Value.Count - 1];
			Value.RemoveAt(Value.Count - 1);
			return r;
		}

		public override long PeekValue()
		{
			return Value[Value.Count - 1];
		}

		public override LiteralStack Clone()
		{
			return new LiteralDigitStack(Position, Value.ToList(), capacity);
		}
	}

	#endregion
}