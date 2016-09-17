
namespace BefunGen.AST.CodeGen.NumberCode
{
	public class NumberCodeFactoryStringmodeChar
	{
		public static CodePiece GenerateCode(long value, bool reversed)
		{
			CodePiece p = GenerateCode(value);

			if (p == null)
				return null;

			if (reversed)
				p.ReverseX(false);
			return p;
		}

		public static CodePiece GenerateCode(long value)
		{
			CodePiece p = new CodePiece();

			if (value == -(int)'"')
			{
				p[0, 0] = BCHelper.Digit1;
				p[1, 0] = BCHelper.Stringmode;
				p[2, 0] = BCHelper.Chr(-value + 1);
				p[3, 0] = BCHelper.Stringmode;
				p[4, 0] = BCHelper.Sub;

				return p;
			}
			else if (value == (int)'"')
			{
				p[0, 0] = BCHelper.Digit1;
				p[1, 0] = BCHelper.Stringmode;
				p[2, 0] = BCHelper.Chr(value - 1);
				p[3, 0] = BCHelper.Stringmode;
				p[4, 0] = BCHelper.Add;

				return p;
			}
			else if (value <= -(int)' ' && value >= -(int)'~')
			{
				p[0, 0] = BCHelper.Digit0;
				p[1, 0] = BCHelper.Stringmode;
				p[2, 0] = BCHelper.Chr(-value);
				p[3, 0] = BCHelper.Stringmode;
				p[4, 0] = BCHelper.Sub;

				return p;
			}
			else if (value >= (int)' ' && value <= (int)'~')
			{
				p[0, 0] = BCHelper.Stringmode;
				p[1, 0] = BCHelper.Chr(value);
				p[2, 0] = BCHelper.Stringmode;

				return p;
			}
			else
			{
				return null;
			}
		}
	}
}
