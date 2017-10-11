namespace BefunGen.AST.DirectRun
{
	public class RunnerResult
	{
		public enum RRType { Normal, Return, Exit, Jump }

		public readonly RRType ResultType;
		public readonly StatementLabel JumpLabel;

		private RunnerResult(RRType t, StatementLabel lbl = null)
		{
			ResultType = t;
			JumpLabel = lbl;
		}

		public static RunnerResult Normal() => new RunnerResult(RRType.Normal);
		public static RunnerResult Return() => new RunnerResult(RRType.Return);
		public static RunnerResult Exit() => new RunnerResult(RRType.Exit);
		public static RunnerResult Jump(StatementLabel lbl) => new RunnerResult(RRType.Jump, lbl);
	}
}
