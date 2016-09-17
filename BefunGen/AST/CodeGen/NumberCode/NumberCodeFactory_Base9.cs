using System;

namespace BefunGen.AST.CodeGen.NumberCode
{
	public class NumberCodeFactoryBase9
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
			CodePiece p = new CodePiece();

			bool isneg;
			if (isneg = lit < 0)
			{
				lit *= -1;
			}

			string rep = ConvertToBase(lit, 9);
			int pos = 0;

			if (isneg)
			{
				p[pos++, 0] = BCHelper.Digit0;
			}

			for (int i = 0; i < rep.Length; i++)
			{
				p[pos++, 0] = BCHelper.Dig((byte)(rep[rep.Length - i - 1] - '0'));

				if (i + 1 != rep.Length)
					p[pos++, 0] = BCHelper.Dig(9);
			}

			int count = rep.Length - 1;

			for (int i = 0; i < count; i++)
			{
				p[pos++, 0] = BCHelper.Mult;
				p[pos++, 0] = BCHelper.Add;
			}

			if (isneg)
			{
				p[pos++, 0] = BCHelper.Sub;
			}

			return p;
		}

		public static string ConvertToBase(long decimalNumber, int radix)
		{
			const int bitsInLong = 64;
			const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			if (radix < 2 || radix > digits.Length)
				throw new ArgumentException("The radix must be >= 2 and <= " + digits.Length.ToString());

			if (decimalNumber == 0)
				return "0";

			int index = bitsInLong - 1;
			long currentNumber = Math.Abs(decimalNumber);
			char[] charArray = new char[bitsInLong];

			while (currentNumber != 0)
			{
				int remainder = (int)(currentNumber % radix);
				charArray[index--] = digits[remainder];
				currentNumber = currentNumber / radix;
			}

			string result = new String(charArray, index + 1, bitsInLong - index - 1);
			if (decimalNumber < 0)
			{
				result = "-" + result;
			}

			return result;
		}
	}
}
