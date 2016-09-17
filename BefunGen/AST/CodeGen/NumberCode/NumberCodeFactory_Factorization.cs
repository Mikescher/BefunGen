using BefunGen.AST.Exceptions;
using System.Collections.Generic;

namespace BefunGen.AST.CodeGen.NumberCode
{
	public static class NumberCodeFactoryFactorization
	{
		public static CodePiece GenerateCode(long value, bool reversed)
		{
			CodePiece p = GenerateCode(value);
			if (reversed)
				p.ReverseX(false);
			return p;
		}

		public static CodePiece GenerateCode(long lit)
		{
			bool isneg;
			if (isneg = lit < 0)
			{
				lit *= -1;
			}

			CodePiece p = new CodePiece();

			if (lit == 0)
			{
				p[0, 0] = BCHelper.Digit0;
				return p;
			}

			if (isneg)
			{
				p.AppendRight(BCHelper.Digit0);
			}

			GetFactors(p, lit);

			if (isneg)
			{
				p.AppendRight(BCHelper.Sub);
			}

			return p;
		}

		private static void GetFactors(CodePiece p, long a) // Wenn nicht möglich so gut wie mögl und am ende add
		{
			List<int> result = new List<int>();

			if (a < 10)
			{
				p.AppendRight(BCHelper.Dig((byte)a));
				return;
			}

			for (byte i = 9; i > 1; i--)
			{
				if (a % i == 0)
				{
					GetFactors(p, a / i);
					p.AppendRight(BCHelper.Dig(i));
					p.AppendRight(BCHelper.Mult);
					return;
				}
			}

			for (byte i = 1; i < 10; i++)
			{
				if ((a - i) % 9 == 0)
				{
					GetFactors(p, a - i);
					p.AppendRight(BCHelper.Dig(i));
					p.AppendRight(BCHelper.Add);
					return;
				}
			}

			throw new WTFException();
		}
	}
}
