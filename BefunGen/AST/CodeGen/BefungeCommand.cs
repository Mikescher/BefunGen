using BefunGen.AST.CodeGen.Tags;
using BefunGen.AST.Exceptions;
using Newtonsoft.Json;

namespace BefunGen.AST.CodeGen
{
	public enum BefungeCommandType
	{
		NOP,
		Walkway, // Like NOP - But PC can appear here
		Add,
		Sub,
		Mult,
		Div,
		Modulo,
		Not,
		GreaterThan,
		PCRight,
		PCLeft,
		PCUp,
		PCDown,
		PCRandom,
		IfHorizontal,
		IfVertical,
		Stringmode,
		StackDup,
		StackSwap,
		StackPop,
		OutInt,
		OutASCII,
		PCJump,
		ReflectSet,
		ReflectGet,
		InInt,
		InASCII,
		Stop,
		Other
	}

	// Immutable Object
	public class BefungeCommand
	{
		public readonly BefungeCommandType Type;

		public readonly int Param;

		public readonly CodeTag Tag;

		public BefungeCommand(BefungeCommandType t)
			: this(t, 0)
		{
			//--
		}

		public BefungeCommand(BefungeCommandType t, CodeTag p)
			: this(t, 0, p)
		{
			//--
		}

		public BefungeCommand(BefungeCommandType t, int p)
			: this(t, p, null)
		{
			//--
		}

		[JsonConstructor]
		public BefungeCommand(BefungeCommandType t, int p, CodeTag g)
		{
			Type = t;
			Param = p;
			Tag = g;
		}

		public char GetCommandCode()
		{
			switch (Type)
			{
				case BefungeCommandType.NOP:
					return ASTObject.CGO.SetNOPCellsToCustom ? ASTObject.CGO.CustomNOPSymbol : ' ';

				case BefungeCommandType.Walkway:
					return ' ';

				case BefungeCommandType.Add:
					return '+';

				case BefungeCommandType.Sub:
					return '-';

				case BefungeCommandType.Mult:
					return '*';

				case BefungeCommandType.Div:
					return '/';

				case BefungeCommandType.Modulo:
					return '%';

				case BefungeCommandType.Not:
					return '!';

				case BefungeCommandType.GreaterThan:
					return '`';

				case BefungeCommandType.PCRight:
					return '>';

				case BefungeCommandType.PCLeft:
					return '<';

				case BefungeCommandType.PCUp:
					return '^';

				case BefungeCommandType.PCDown:
					return 'v';

				case BefungeCommandType.PCRandom:
					return '?';

				case BefungeCommandType.IfHorizontal:
					return '_';

				case BefungeCommandType.IfVertical:
					return '|';

				case BefungeCommandType.Stringmode:
					return '"';

				case BefungeCommandType.StackDup:
					return ':';

				case BefungeCommandType.StackSwap:
					return '\\';

				case BefungeCommandType.StackPop:
					return '$';

				case BefungeCommandType.OutInt:
					return '.';

				case BefungeCommandType.OutASCII:
					return ',';

				case BefungeCommandType.PCJump:
					return '#';

				case BefungeCommandType.ReflectSet:
					return 'p';

				case BefungeCommandType.ReflectGet:
					return 'g';

				case BefungeCommandType.InInt:
					return '&';

				case BefungeCommandType.InASCII:
					return '~';

				case BefungeCommandType.Stop:
					return '@';

				case BefungeCommandType.Other:
					return (char)Param;

				default:
					throw new InvalidBefungeCommandTypeException(new SourceCodePosition());
			}
		}

		public BefungeCommand CopyWithTag(CodeTag g)
		{
			return new BefungeCommand(Type, Param, g);
		}

		public bool IsDeltaIndependent()
		{
			return IsXDeltaIndependent() && IsYDeltaIndependent();
		}

		public bool IsXDeltaIndependent()
		{
			switch (Type)
			{
				case BefungeCommandType.NOP:
				case BefungeCommandType.Walkway:
				case BefungeCommandType.Add:
				case BefungeCommandType.Sub:
				case BefungeCommandType.Mult:
				case BefungeCommandType.Div:
				case BefungeCommandType.Modulo:
				case BefungeCommandType.Not:
				case BefungeCommandType.GreaterThan:
				case BefungeCommandType.PCRandom:
				case BefungeCommandType.Stringmode:
				case BefungeCommandType.StackDup:
				case BefungeCommandType.StackSwap:
				case BefungeCommandType.StackPop:
				case BefungeCommandType.OutInt:
				case BefungeCommandType.OutASCII:
				case BefungeCommandType.PCJump:
				case BefungeCommandType.PCUp:
				case BefungeCommandType.PCDown:
				case BefungeCommandType.IfVertical:
				case BefungeCommandType.ReflectSet:
				case BefungeCommandType.ReflectGet:
				case BefungeCommandType.InInt:
				case BefungeCommandType.InASCII:
				case BefungeCommandType.Stop:
				case BefungeCommandType.Other:
					return true;
				case BefungeCommandType.PCRight:
				case BefungeCommandType.PCLeft:
				case BefungeCommandType.IfHorizontal:
					return false;
				default:
					throw new InvalidBefungeCommandTypeException(new SourceCodePosition());
			}
		}

		public bool IsYDeltaIndependent()
		{
			switch (Type)
			{
				case BefungeCommandType.NOP:
				case BefungeCommandType.Walkway:
				case BefungeCommandType.Add:
				case BefungeCommandType.Sub:
				case BefungeCommandType.Mult:
				case BefungeCommandType.Div:
				case BefungeCommandType.Modulo:
				case BefungeCommandType.Not:
				case BefungeCommandType.GreaterThan:
				case BefungeCommandType.PCRandom:
				case BefungeCommandType.Stringmode:
				case BefungeCommandType.StackDup:
				case BefungeCommandType.StackSwap:
				case BefungeCommandType.StackPop:
				case BefungeCommandType.OutInt:
				case BefungeCommandType.OutASCII:
				case BefungeCommandType.PCJump:
				case BefungeCommandType.PCLeft:
				case BefungeCommandType.PCRight:
				case BefungeCommandType.IfHorizontal:
				case BefungeCommandType.ReflectSet:
				case BefungeCommandType.ReflectGet:
				case BefungeCommandType.InInt:
				case BefungeCommandType.InASCII:
				case BefungeCommandType.Stop:
				case BefungeCommandType.Other:
					return true;
				case BefungeCommandType.PCUp:
				case BefungeCommandType.PCDown:
				case BefungeCommandType.IfVertical:
					return false;
				default:
					throw new InvalidBefungeCommandTypeException(new SourceCodePosition());
			}
		}

		public bool IsCompressable()
		{
			switch (Type)
			{
				case BefungeCommandType.NOP:
				case BefungeCommandType.Walkway:
				case BefungeCommandType.PCUp:
				case BefungeCommandType.PCDown:
				case BefungeCommandType.PCRight:
				case BefungeCommandType.PCLeft:
				case BefungeCommandType.Stop:
					return true;
				case BefungeCommandType.Add:
				case BefungeCommandType.Sub:
				case BefungeCommandType.Mult:
				case BefungeCommandType.Div:
				case BefungeCommandType.Modulo:
				case BefungeCommandType.Not:
				case BefungeCommandType.GreaterThan:
				case BefungeCommandType.PCRandom:
				case BefungeCommandType.Stringmode:
				case BefungeCommandType.StackDup:
				case BefungeCommandType.StackSwap:
				case BefungeCommandType.StackPop:
				case BefungeCommandType.OutInt:
				case BefungeCommandType.OutASCII:
				case BefungeCommandType.PCJump:
				case BefungeCommandType.IfVertical:
				case BefungeCommandType.ReflectSet:
				case BefungeCommandType.ReflectGet:
				case BefungeCommandType.InInt:
				case BefungeCommandType.InASCII:
				case BefungeCommandType.Other:
				case BefungeCommandType.IfHorizontal:
					return false;
				default:
					throw new InvalidBefungeCommandTypeException(new SourceCodePosition());
			}
		}

		public bool EqualsTagLess(BefungeCommand c)
		{
			return !HasTag() && !c.HasTag() && this.Type == c.Type && this.Param == c.Param;
		}

		public bool HasTag()
		{
			return Tag != null;
		}
	}
}