
namespace BefunGen.AST.CodeGen.Tags
{
	public class MethodCallVerticalExitTag : CodeTag
	{
		public MethodCallVerticalExitTag(Method target)
			: base("Vertical_MethodCall_Exit (Method)", target)
		{
			//NOP
		}

		public MethodCallVerticalExitTag(StatementLabel target)
			: base("Vertical_MethodCall_Exit (Label)", target)
		{
			//NOP
		}

		public MethodCallVerticalExitTag(object target)
			: base("Vertical_MethodCall_Exit (" + target.GetType().Name + ")", target)
		{
			//NOP
		}

		public MethodCallVerticalExitTag()
			: base("Vertical_MethodCall_Exit (PARAMLESS)")
		{
			//NOP
		}
	}
}
