using System;

namespace BefunGen.AST.CodeGen
{
	public class TwoDirectionCodePiece
	{
		private Tuple<CodePiece, CodePiece> content;

		public CodePiece LeftToRight { get { return content.Item1; } }
		public CodePiece RightToLeft { get { return content.Item2; } }

		public CodePiece Normal { get { return content.Item1; } }
		public CodePiece Reversed { get { return content.Item2; } }

		public int WidthL2R { get { return content.Item1.Width; } }
		public int WidthR2L { get { return content.Item2.Width; } }

		public int WidthNormal { get { return content.Item1.Width; } }
		public int WidthReversed { get { return content.Item2.Width; } }

		public int MaxWidth { get { return Math.Max(content.Item1.Width, content.Item2.Width); } }

		public  TwoDirectionCodePiece(CodePiece norm, CodePiece rev)
		{
			content = Tuple.Create(norm, rev);
		}

		public TwoDirectionCodePiece()
			: this(new CodePiece(), new CodePiece())
		{
		}
	}
}
