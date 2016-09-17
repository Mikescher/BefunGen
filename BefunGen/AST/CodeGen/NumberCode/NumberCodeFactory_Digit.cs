
namespace BefunGen.AST.CodeGen.NumberCode
{
	public class NumberCodeFactoryDigit
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

		public static CodePiece GenerateCode(long d)
		{
			if (d < 0 || d > 9)
				return null;

			CodePiece p = new CodePiece();
			p[0, 0] = BCHelper.Dig((byte)d);
			return p;
		}
	}
}
