
namespace BefunGen.AST.CodeGen.Tags
{
	public class SwitchStmtCaseExitTag : CodeTag
	{
		public SwitchStmtCaseExitTag(bool active = true)
			: base("SwitchStmt_Case_Exit")
		{
			if (!active)
				Deactivate();
		}
	}
}
