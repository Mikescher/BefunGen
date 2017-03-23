using System;
using BefunGen.MathExtensions;

namespace BefunGen.AST.CodeGen
{
	public class CodeGenEnvironment
	{
		public MathExt.Point TMP_FIELD_IO_ARR;
		public MathExt.Point TMP_FIELD_OUT_ARR;
		public MathExt.Point TMP_FIELD_JMP_ADDR;
		public MathExt.Point TMP_FIELD_GENERAL;

		public VarDeclarationPosition TMP_ARRFIELD_RETURNVAL = null;

		public int MaxVarDeclarationWidth = -1;

		public static CodeGenEnvironment CreateDummy()
		{
			return new CodeGenEnvironment()
			{
				TMP_FIELD_IO_ARR = CodeGenConstants.TMP_FIELDPOS_IO_ARR,
				TMP_FIELD_OUT_ARR = CodeGenConstants.TMP_FIELDPOS_OUT_ARR,
				TMP_FIELD_JMP_ADDR = CodeGenConstants.TMP_FIELDPOS_JMP_ADDR,
				TMP_FIELD_GENERAL = CodeGenConstants.TMP_FIELDPOS_GENERAL,
				TMP_ARRFIELD_RETURNVAL = new VarDeclarationPosition(CodeGenConstants.TMP_ARRFIELDPOS_RETURNVAL_TL, 128, 1, 128),
				MaxVarDeclarationWidth = CodeGenConstants.MinVarDeclarationWidth,
			};
		}
	}
}
