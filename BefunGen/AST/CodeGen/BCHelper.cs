
using BefunGen.AST.CodeGen.Tags;
using BefunGen.AST.Exceptions;
using System;
namespace BefunGen.AST.CodeGen
{
	public static class BCHelper
	{
		#region Normal

		public static BefungeCommand Unused
		{
			get { return new BefungeCommand(BefungeCommandType.NOP); }
		}

		public static BefungeCommand Walkway
		{
			get { return new BefungeCommand(BefungeCommandType.Walkway); }
		}

		public static BefungeCommand Add
		{
			get { return new BefungeCommand(BefungeCommandType.Add); }
		}

		public static BefungeCommand Sub
		{
			get { return new BefungeCommand(BefungeCommandType.Sub); }
		}

		public static BefungeCommand Mult
		{
			get { return new BefungeCommand(BefungeCommandType.Mult); }
		}

		public static BefungeCommand Div
		{
			get { return new BefungeCommand(BefungeCommandType.Div); }
		}

		public static BefungeCommand Modulo
		{
			get { return new BefungeCommand(BefungeCommandType.Modulo); }
		}

		public static BefungeCommand Not
		{
			get { return new BefungeCommand(BefungeCommandType.Not); }
		}

		public static BefungeCommand GreaterThan
		{
			get { return new BefungeCommand(BefungeCommandType.GreaterThan); }
		}

		public static BefungeCommand PCRight
		{
			get { return new BefungeCommand(BefungeCommandType.PCRight); }
		}

		public static BefungeCommand PCLeft
		{
			get { return new BefungeCommand(BefungeCommandType.PCLeft); }
		}

		public static BefungeCommand PCUp
		{
			get { return new BefungeCommand(BefungeCommandType.PCUp); }
		}

		public static BefungeCommand PCDown
		{
			get { return new BefungeCommand(BefungeCommandType.PCDown); }
		}

		public static BefungeCommand PCRandom
		{
			get { return new BefungeCommand(BefungeCommandType.PCRandom); }
		}

		public static BefungeCommand IfHorizontal
		{
			get { return new BefungeCommand(BefungeCommandType.IfHorizontal); }
		}

		public static BefungeCommand IfVertical
		{
			get { return new BefungeCommand(BefungeCommandType.IfVertical); }
		}

		public static BefungeCommand Stringmode
		{
			get { return new BefungeCommand(BefungeCommandType.Stringmode); }
		}

		public static BefungeCommand StackDup
		{
			get { return new BefungeCommand(BefungeCommandType.StackDup); }
		}

		public static BefungeCommand StackSwap
		{
			get { return new BefungeCommand(BefungeCommandType.StackSwap); }
		}

		public static BefungeCommand StackPop
		{
			get { return new BefungeCommand(BefungeCommandType.StackPop); }
		}

		public static BefungeCommand OutInt
		{
			get { return new BefungeCommand(BefungeCommandType.OutInt); }
		}

		public static BefungeCommand OutASCII
		{
			get { return new BefungeCommand(BefungeCommandType.OutASCII); }
		}

		public static BefungeCommand PCJump
		{
			get { return new BefungeCommand(BefungeCommandType.PCJump); }
		}

		public static BefungeCommand ReflectSet
		{
			get { return new BefungeCommand(BefungeCommandType.ReflectSet); }
		}

		public static BefungeCommand ReflectGet
		{
			get { return new BefungeCommand(BefungeCommandType.ReflectGet); }
		}

		public static BefungeCommand InInt
		{
			get { return new BefungeCommand(BefungeCommandType.InInt); }
		}

		public static BefungeCommand InASCII
		{
			get { return new BefungeCommand(BefungeCommandType.InASCII); }
		}

		public static BefungeCommand Stop
		{
			get { return new BefungeCommand(BefungeCommandType.Stop); }
		}

		public static BefungeCommand Digit0
		{
			get { return Dig(0); }
		}

		public static BefungeCommand Digit1
		{
			get { return Dig(1); }
		}

		public static BefungeCommand Digit2
		{
			get { return Dig(2); }
		}

		public static BefungeCommand Digit3
		{
			get { return Dig(3); }
		}

		public static BefungeCommand Digit4
		{
			get { return Dig(4); }
		}

		public static BefungeCommand Digit5
		{
			get { return Dig(5); }
		}

		public static BefungeCommand Digit6
		{
			get { return Dig(6); }
		}

		public static BefungeCommand Digit7
		{
			get { return Dig(7); }
		}

		public static BefungeCommand Digit8
		{
			get { return Dig(8); }
		}

		public static BefungeCommand Digit9
		{
			get { return Dig(9); }
		}

		#endregion

		#region Other

		public static BefungeCommand Chr(int v)
		{
			return new BefungeCommand(BefungeCommandType.Other, v);
		}

		public static BefungeCommand Chr(long v)
		{
			return new BefungeCommand(BefungeCommandType.Other, (int)v); // Hardcode down - hopefully never use such big numbers ...
		}

		public static BefungeCommand Dig(byte v)
		{
			if (v < 10)
				return new BefungeCommand(BefungeCommandType.Other, '0' + v);
			else
				throw new ArgumentException();
		}

		#endregion

		#region Normal (Tagged)

		public static BefungeCommand Unused_tagged(CodeTag tag)
		{
			throw new InternalCodeGenException(); // There is nothing like an tagged unused ...
		}

		public static BefungeCommand Walkway_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Walkway, tag);
		}

		public static BefungeCommand Add_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Add, tag);
		}

		public static BefungeCommand Sub_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Sub, tag);
		}

		public static BefungeCommand Mult_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Mult, tag);
		}

		public static BefungeCommand Div_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Div, tag);
		}

		public static BefungeCommand Modulo_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Modulo, tag);
		}

		public static BefungeCommand Not_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Not, tag);
		}

		public static BefungeCommand GreaterThan_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.GreaterThan, tag);
		}

		public static BefungeCommand PC_Right_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.PCRight, tag);
		}

		public static BefungeCommand PC_Left_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.PCLeft, tag);
		}

		public static BefungeCommand PC_Up_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.PCUp, tag);
		}

		public static BefungeCommand PC_Down_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.PCDown, tag);
		}

		public static BefungeCommand PC_Random_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.PCRandom, tag);
		}

		public static BefungeCommand If_Horizontal_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.IfHorizontal, tag);
		}

		public static BefungeCommand If_Vertical_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.IfVertical, tag);
		}

		public static BefungeCommand Stringmode_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Stringmode, tag);
		}

		public static BefungeCommand Stack_Dup_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.StackDup, tag);
		}

		public static BefungeCommand Stack_Swap_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.StackSwap, tag);
		}

		public static BefungeCommand Stack_Pop_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.StackPop, tag);
		}

		public static BefungeCommand Out_Int_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.OutInt, tag);
		}

		public static BefungeCommand Out_ASCII_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.OutASCII, tag);
		}

		public static BefungeCommand PC_Jump_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.PCJump, tag);
		}

		public static BefungeCommand Reflect_Set_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.ReflectSet, tag);
		}

		public static BefungeCommand Reflect_Get_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.ReflectGet, tag);
		}

		public static BefungeCommand In_Int_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.InInt, tag);
		}

		public static BefungeCommand In_ASCII_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.InASCII, tag);
		}

		public static BefungeCommand Stop_tagged(CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Stop, tag);
		}

		public static BefungeCommand Digit_0_tagged(CodeTag tag)
		{
			return Dig(0, tag);
		}

		public static BefungeCommand Digit_1_tagged(CodeTag tag)
		{
			return Dig(1, tag);
		}

		public static BefungeCommand Digit_2_tagged(CodeTag tag)
		{
			return Dig(2, tag);
		}

		public static BefungeCommand Digit_3_tagged(CodeTag tag)
		{
			return Dig(3, tag);
		}

		public static BefungeCommand Digit_4_tagged(CodeTag tag)
		{
			return Dig(4, tag);
		}

		public static BefungeCommand Digit_5_tagged(CodeTag tag)
		{
			return Dig(5, tag);
		}

		public static BefungeCommand Digit_6_tagged(CodeTag tag)
		{
			return Dig(6, tag);
		}

		public static BefungeCommand Digit_7_tagged(CodeTag tag)
		{
			return Dig(7, tag);
		}

		public static BefungeCommand Digit_8_tagged(CodeTag tag)
		{
			return Dig(8, tag);
		}

		public static BefungeCommand Digit_9_tagged(CodeTag tag)
		{
			return Dig(9, tag);
		}

		#endregion

		#region Other (Tagged)

		public static BefungeCommand Chr(int v, CodeTag tag)
		{
			return new BefungeCommand(BefungeCommandType.Other, v, tag);
		}

		public static BefungeCommand Dig(byte v, CodeTag tag)
		{
			if (v < 10)
				return new BefungeCommand(BefungeCommandType.Other, '0' + v, tag);
			else
				throw new ArgumentException();
		}

		#endregion

		#region Helper

		public static BefungeCommand FindCommand(char c)
		{
			switch (c)
			{
				case ' ':
					return BCHelper.Walkway;
				case '+':
					return BCHelper.Add;
				case '-':
					return BCHelper.Sub;
				case '*':
					return BCHelper.Mult;
				case '/':
					return BCHelper.Div;
				case '%':
					return BCHelper.Modulo;
				case '!':
					return BCHelper.Not;
				case '`':
					return BCHelper.GreaterThan;
				case '>':
					return BCHelper.PCRight;
				case '<':
					return BCHelper.PCLeft;
				case '^':
					return BCHelper.PCUp;
				case 'v':
					return BCHelper.PCDown;
				case '?':
					return BCHelper.PCRandom;
				case '_':
					return BCHelper.IfHorizontal;
				case '|':
					return BCHelper.IfVertical;
				case '"':
					return BCHelper.Stringmode;
				case ':':
					return BCHelper.StackDup;
				case '\\':
					return BCHelper.StackSwap;
				case '$':
					return BCHelper.StackPop;
				case '.':
					return BCHelper.OutInt;
				case ',':
					return BCHelper.OutASCII;
				case '#':
					return BCHelper.PCJump;
				case 'p':
					return BCHelper.ReflectSet;
				case 'g':
					return BCHelper.ReflectGet;
				case '&':
					return BCHelper.InInt;
				case '~':
					return BCHelper.InASCII;
				case '@':
					return BCHelper.Stop;
				case '0':
					return BCHelper.Digit0;
				case '1':
					return BCHelper.Digit1;
				case '2':
					return BCHelper.Digit2;
				case '3':
					return BCHelper.Digit3;
				case '4':
					return BCHelper.Digit4;
				case '5':
					return BCHelper.Digit5;
				case '6':
					return BCHelper.Digit6;
				case '7':
					return BCHelper.Digit7;
				case '8':
					return BCHelper.Digit8;
				case '9':
					return BCHelper.Digit9;
				default:
					return BCHelper.Chr(c);
			}
		}

		#endregion
	}
}
