using BefunGen.MathExtensions;

namespace BefunGen.AST.CodeGen
{
	public static class CodeGenConstants
	{
		public static string BEFUNGEN_VERSION = "1.2";

		public static MathExt.Point TMP_FIELDPOS_IO_ARR = new MathExt.Point(1, 0);
		public static MathExt.Point TMP_FIELDPOS_OUT_ARR = new MathExt.Point(2, 0);
		public static MathExt.Point TMP_FIELDPOS_JMP_ADDR = new MathExt.Point(3, 0);
		public static MathExt.Point TMP_FIELDPOS_GENERAL = new MathExt.Point(4, 0);
		public static MathExt.Point TMP_ARRFIELDPOS_RETURNVAL_TL = new MathExt.Point(5, 0);

		public static int TOP_COMMENT_X = 16;

		public const int VERTICAL_METHOD_DISTANCE = 0;
		public const int LANE_VERTICAL_MARGIN = 0;

		public const int MAX_JUMPIN_VARFRAME_LENGTH = 16;
		public const int MAX_JUMPBACK_VARFRAME_LENGTH = 16;

		public const int MinVarDeclarationWidth = 8;
	}
}
