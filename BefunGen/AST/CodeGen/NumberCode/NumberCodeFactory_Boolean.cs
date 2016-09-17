
namespace BefunGen.AST.CodeGen.NumberCode
{
	public class NumberCodeFactoryBoolean
	{
		public static CodePiece GenerateCode(long value)
		{
			return GenerateCode(value, false);
		}

		public static CodePiece GenerateCode(long value, bool reversed)
		{
			if (value == 0 || value == 1)
			{
				return GenerateCode(value == 1, reversed);
			}
			else
			{
				return null;
			}
		}

		public static CodePiece GenerateCode(bool value, bool reversed)
		{
			CodePiece p = GenerateCode(value);
			if (reversed)
				p.ReverseX(false);
			return p;
		}

		public static CodePiece GenerateCode(bool value)
		{
			CodePiece p = new CodePiece();
			p[0, 0] = BCHelper.Dig(value ? (byte)1 : (byte)0);
			return p;
		}
	}
}
