using BefunGen.AST.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BefunGen.AST.CodeGen.NumberCode
{
	public static class NumberCodeHelper
	{
		public static NumberRep LastRep;

		public static CodePiece GenerateCode(long value, bool reversed)
		{
			CodePiece p = GenerateCode(value);
			if (reversed)
				p.ReverseX(false);
			return p;
		}

		public static CodePiece GenerateCode(long value)
		{
			CodePiece p;

			if (ASTObject.CGO.NumberLiteralRepresentation == NumberRep.Best)
			{
				List<Tuple<NumberRep, CodePiece>> representations = GenerateAllCode(value, true);

				int min = representations.Min(lp => lp.Item2.Width);

				foreach (var rep in representations)
				{
					if (rep.Item2.Width == min)
					{
						LastRep = rep.Item1;
						p = rep.Item2;

						return p;
					}
				}
			}
			else if (ASTObject.CGO.NumberLiteralRepresentation == NumberRep.StringmodeChar)
			{
				p = NumberCodeFactoryStringmodeChar.GenerateCode(value);
				LastRep = NumberRep.StringmodeChar;

				return p;
			}
			else if (ASTObject.CGO.NumberLiteralRepresentation == NumberRep.Base9)
			{
				p = NumberCodeFactoryBase9.GenerateCode(value);
				LastRep = NumberRep.Base9;

				return p;
			}
			else if (ASTObject.CGO.NumberLiteralRepresentation == NumberRep.Factorization)
			{
				p = NumberCodeFactoryFactorization.GenerateCode(value);
				LastRep = NumberRep.Factorization;

				return p;
			}
			else if (ASTObject.CGO.NumberLiteralRepresentation == NumberRep.Stringify)
			{
				p = NumberCodeFactoryStringify.GenerateCode(value);
				LastRep = NumberRep.Stringify;

				return p;
			}
			else if (ASTObject.CGO.NumberLiteralRepresentation == NumberRep.Digit)
			{
				p = NumberCodeFactoryDigit.GenerateCode(value);
				LastRep = NumberRep.Digit;

				return p;
			}
			else if (ASTObject.CGO.NumberLiteralRepresentation == NumberRep.Boolean)
			{
				p = NumberCodeFactoryBoolean.GenerateCode(value);
				LastRep = NumberRep.Boolean;

				return p;
			}

			throw new WTFException();
		}

		public static List<Tuple<NumberRep, CodePiece>> GenerateAllCode(long value, bool filter, bool reversed = false)
		{
			List<Tuple<NumberRep, CodePiece>> result = new List<Tuple<NumberRep, CodePiece>>();

			// Order is Priority !!!

			result.Add(Tuple.Create(NumberRep.Boolean, NumberCodeFactoryBoolean.GenerateCode(value, reversed)));
			result.Add(Tuple.Create(NumberRep.Digit, NumberCodeFactoryDigit.GenerateCode(value, reversed)));
			result.Add(Tuple.Create(NumberRep.Base9, NumberCodeFactoryBase9.GenerateCode(value, reversed)));
			result.Add(Tuple.Create(NumberRep.Factorization, NumberCodeFactoryFactorization.GenerateCode(value, reversed)));
			result.Add(Tuple.Create(NumberRep.StringmodeChar, NumberCodeFactoryStringmodeChar.GenerateCode(value, reversed)));
			result.Add(Tuple.Create(NumberRep.Stringify, NumberCodeFactoryStringify.GenerateCode(value, reversed)));

			if (filter)
				return result.Where(p => p.Item2 != null).ToList();
			else
				return result;

		}

		public static string GenerateBenchmark(int cnt, bool doNeg)
		{
			ASTObject.CGO.NumberLiteralRepresentation = NumberRep.Best;

			int min = (doNeg) ? -(cnt / 2) : (0);
			int max = (doNeg) ? +(cnt / 2) : (cnt);

			List<NumberRep> reps = Enum.GetValues(typeof(NumberRep)).Cast<NumberRep>().Where(p => p != NumberRep.Best).ToList();

			int[] count = new int[reps.Count];
			Array.Clear(count, 0, reps.Count);

			//int mxw = Enumerable.Range(MIN, MAX + 1).Max(p1 => generateAllCode(p1, true).Max(p2 => p2.Item2.Width)) + 3;
			int mxw = 24;

			StringBuilder txt = new StringBuilder();

			txt.AppendFormat("{0, -7} ", "Number");
			txt.AppendFormat("{0, -16} {1, -" + mxw + "}", "Best", "Best");
			reps.ForEach(p => txt.AppendFormat("{0, -" + mxw + "} ", p.ToString()));
			txt.AppendLine();
			txt.AppendLine();

			long ticks = Environment.TickCount;
			for (int i = min; i <= max; i++)
			{
				List<Tuple<NumberRep, CodePiece>> all = GenerateAllCode(i, false);
				CodePiece best = GenerateCode(i);
				NumberRep rbest = LastRep;

				count[reps.IndexOf(rbest)]++;

				txt.AppendFormat("{0, -7} ", i.ToString());
				txt.AppendFormat("{0, -16} ", rbest.ToString());
				txt.AppendFormat("{0, -" + mxw + "} ", best.ToSimpleString());
				reps.ForEach(p => txt.AppendFormat("{0, -" + mxw + "} ", (all.Single(p2 => p2.Item1 == p).Item2 != null) ? (all.Single(p2 => p2.Item1 == p).Item2.ToSimpleString()) : ("")));
				txt.AppendLine();
			}

			txt.AppendLine();
			txt.AppendLine(new String('#', 32));
			txt.AppendLine();

			ticks = Environment.TickCount - ticks;

			reps.ForEach(p => txt.AppendLine(String.Format("{0,-16}: {1}", p.ToString(), count[reps.IndexOf(p)])));

			txt.AppendLine();

			txt.AppendLine(String.Format("Time taken for {0} Elements: {1}ms", cnt, ticks));
			txt.AppendLine(String.Format("Time/Number: {0:0.000}ms", (ticks * 1.0) / cnt));

			return txt.ToString();
		}
	}
}
