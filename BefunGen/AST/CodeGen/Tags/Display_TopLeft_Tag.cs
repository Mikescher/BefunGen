
using System;
namespace BefunGen.AST.CodeGen.Tags
{
	public class DisplayTopLeftTag : CodeTag
	{
		public readonly int Width;
		public readonly int Height;

		public DisplayTopLeftTag(Program target, int w, int h)
			: base("Display_TopLeft_Tag", target)
		{
			this.Width = w;
			this.Height = h;
		}

		public override string ToString()
		{
			return String.Format(@"{0} [{1}x{2}]", base.ToString(), Width, Height);
		}
	}
}
