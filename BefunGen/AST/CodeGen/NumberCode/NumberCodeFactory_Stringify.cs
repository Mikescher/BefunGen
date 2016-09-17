using BefunGen.AST.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace BefunGen.AST.CodeGen.NumberCode
{
	public class NumberCodeFactoryStringify
	{
		private enum StripOp { Add, Mult }

		private const char MIN_ASCII = ' '; // 32
		private const char MAX_ASCII = '~'; // 126

		public static CodePiece GenerateCode(long value, bool reversed)
		{
			CodePiece p = GenerateCode(value);

			if (p == null)
				return null;

			if (reversed)
				p.ReverseX(false);
			return p;
		}

		public static CodePiece GenerateCode(long lit)
		{
			if (lit < 0)
			{
				CodePiece p = GenerateCode(-lit);
				if (p == null)
					return null;
				p.AppendLeft(BCHelper.Digit0);
				p.AppendRight(BCHelper.Sub);
				p.NormalizeX();
				return p;
			}

			if (lit >= 0 && lit <= 9)
			{
				return new CodePiece(BCHelper.Dig((byte)lit));
			}

			if (lit < MIN_ASCII && lit >= (MIN_ASCII - 9))
			{
				if (lit + 9 == '"')
				{
					CodePiece p = GenerateCode(lit + 8);
					if (p == null)
						return null;
					p.AppendRight(BCHelper.Digit8);
					p.AppendRight(BCHelper.Sub);
					return p;
				}
				else
				{
					CodePiece p = GenerateCode(lit + 9);
					if (p == null)
						return null;
					p.AppendRight(BCHelper.Digit9);
					p.AppendRight(BCHelper.Sub);
					return p;
				}

			}

			if (lit < (MIN_ASCII - 9))
			{
				return null;
			}

			List<char> str;
			List<StripOp> ops;

			if (CalculateStringOps(out str, out ops, lit))
			{
				CodePiece p = new CodePiece();

				p.AppendRight(BCHelper.Stringmode);
				foreach (char c in str)
					p.AppendRight(BCHelper.Chr(c));
				p.AppendRight(BCHelper.Stringmode);

				for (int i = 0; i < ops.Count; i++)
				{
					switch (ops[i])
					{
						case StripOp.Add:
							p.AppendRight(BCHelper.Add);
							break;
						case StripOp.Mult:
							p.AppendRight(BCHelper.Mult);
							break;
						default:
							throw new WTFException();
					}
				}

				return p;
			}

			return null;
		}

		private static bool CalculateStringOps(out List<char> str, out List<StripOp> ops, long val)
		{
			if (val < MIN_ASCII)
			{
				ops = null;
				str = null;
				return false;
			}

			//##########################################################################

			if (val >= MIN_ASCII && val <= MAX_ASCII && val != '"')
			{
				ops = new List<StripOp>();
				str = new List<char>() { (char)val };
				return true;
			}

			//##########################################################################

			for (char curr = MAX_ASCII; curr >= MIN_ASCII; curr--)
			{
				if (curr == '"')
					continue;

				if (val % curr == 0 && val / curr > MIN_ASCII)
				{
					List<char> oStr;
					List<StripOp> oOps;

					if (CalculateStringOps(out oStr, out oOps, val / curr))
					{
						str = oStr.ToList();
						ops = oOps.ToList();

						str.Insert(0, curr);
						ops.Add(StripOp.Mult);

						return true;
					}
				}
			}

			//##########################################################################


			for (char curr = MAX_ASCII; curr >= MIN_ASCII; curr--)
			{
				if (curr == '"')
					continue;

				List<char> oStr;
				List<StripOp> oOps;

				if (CalculateStringOps(out oStr, out oOps, val - curr))
				{
					str = oStr.ToList();
					ops = oOps.ToList();

					str.Insert(0, curr);
					ops.Add(StripOp.Add);

					return true;
				}
			}

			str = null;
			ops = null;
			return false;
		}
	}
}
