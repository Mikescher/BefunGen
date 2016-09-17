
namespace BefunGen.AST.CodeGen.Tags
{
	public abstract class CodeTag
	{
		public readonly string UUID;
		public readonly string TagName;
		public readonly object TagParam;

		private bool active = true;
		public bool Active { get { return active; } }


		public CodeTag(string name)
			: this(name, null)
		{
			//-
		}

		public void Deactivate()
		{
			active = false;
		}

		public bool IsActive()
		{
			return Active;
		}

		public CodeTag(string name, object param)
		{
			this.UUID = System.Guid.NewGuid().ToString("D");
			this.TagName = name;
			this.TagParam = param;
		}

		public bool HasParam()
		{
			return TagParam != null;
		}

		public override string ToString()
		{
			return 
				(HasParam()) 
				? 
				(string.Format("[{0}] {1} ({2}) <{3}>", Active ? "+" : "-", TagName, TagParam, UUID))
				:
				(string.Format("[{0}] {1} <{2}>",  Active ? "+" : "-", TagName, UUID));
		}
	}
}
