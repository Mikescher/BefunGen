
namespace BefunGen.AST.CodeGen.Tags
{
	public class MethodCallHorizontalExitTag : CodeTag
	{
		public MethodCallHorizontalExitTag(Method target)
			: base("Horizontal_MethodCall_Exit", target)
		{
			//NOP
		}

		public MethodCallHorizontalExitTag(StatementLabel target)
			: base("Horizontal_MethodCall_Exit (Label)", target)
		{
			//NOP
		}

		public MethodCallHorizontalExitTag(object target)
			: base("Horizontal_MethodCall_Exit ( ??? )", target)
		{
			//NOP
		}

		public MethodCallHorizontalExitTag()
			: base("Vertical_MethodCall_Exit (PARAMLESS)")
		{
			//NOP
		}
	}
}
