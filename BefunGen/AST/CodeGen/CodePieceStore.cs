using BefunGen.AST.CodeGen.NumberCode;
using BefunGen.AST.CodeGen.Tags;
using BefunGen.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BefunGen.AST.CodeGen
{
	public class CodePieceStoreElement
	{
		public string Name;
		public Func<bool, CodePiece> Function;
		public CodePieceStoreElement(string n, Func<bool, CodePiece> f)
		{
			Name = n;
			Function = f;
		}
	}

	public static class CodePieceStore
	{
		#region Store
		
		public static CodePieceStoreElement[] CODEPIECES = new[]
		{
			new CodePieceStoreElement("ReadLinearArrayToStack",               (r) => ReadLinearArrayToStack(CodeGenEnvironment.CreateDummy(), 10,1,1,r)),
			new CodePieceStoreElement("ReadMultilineArrayToStack",            (r) => ReadMultilineArrayToStack(CodeGenEnvironment.CreateDummy(), new VarDeclarationPosition(1,1,4,3,10), r)),
			new CodePieceStoreElement("WriteLinearArrayFromStack",            (r) => WriteLinearArrayFromStack(CodeGenEnvironment.CreateDummy(), 10,1,1,r)),
			new CodePieceStoreElement("WriteMultilineArrayFromStack",         (r) => WriteMultilineArrayFromStack(CodeGenEnvironment.CreateDummy(), new VarDeclarationPosition(1,1,4,3,10), r)),
			new CodePieceStoreElement("WriteLinearArrayFromReversedStack",    (r) => WriteLinearArrayFromReversedStack(CodeGenEnvironment.CreateDummy(), 10,1,1,r)),
			new CodePieceStoreElement("WriteMultilineArrayFromReversedStack", (r) => WriteMultilineArrayFromReversedStack(CodeGenEnvironment.CreateDummy(), new VarDeclarationPosition(1,1,4,3,10), r)),
			new CodePieceStoreElement("VerticalLaneTurnout_Test",             (r) => VerticalLaneTurnout_Test()),
			new CodePieceStoreElement("VerticalLaneTurnout_Dec",              (r) => VerticalLaneTurnout_Dec(r)),
			new CodePieceStoreElement("BooleanStackFlooder",                  (r) => BooleanStackFlooder()),
			new CodePieceStoreElement("PopMultipleStackValues",               (r) => PopMultipleStackValues(8, r)),
			new CodePieceStoreElement("ReadValueFromField",                   (r) => ReadValueFromField(new MathExt.Point(1,1), r)),
			new CodePieceStoreElement("WriteValueToField",                    (r) => WriteValueToField(new MathExt.Point(1,1), r)),
			new CodePieceStoreElement("ModuloRangeLimiter",                   (r) => ModuloRangeLimiter(8, r)),
			new CodePieceStoreElement("RandomDigitGenerator",                 (r) => RandomDigitGenerator(CodePiece.ParseFromLine("8"), r)),
			new CodePieceStoreElement("Base4DigitJoiner",                     (r) => Base4DigitJoiner(r)),
			new CodePieceStoreElement("SwitchStatementTester",                (r) => SwitchStatementTester(r)),
			new CodePieceStoreElement("SwitchLaneTurnout",                    (r) => SwitchLaneTurnout()),
		};

		#endregion

		#region ReadArrayToStack

		public static CodePiece ReadArrayToStack(CodeGenEnvironment env, VarDeclarationPosition v, bool reversed)
		{
			if (v.IsSingleLine())
				return ReadLinearArrayToStack(env, v.Size, v.X, v.Y, reversed);
			else
				return ReadMultilineArrayToStack(env, v, reversed);
		}
		
		private static CodePiece ReadLinearArrayToStack(CodeGenEnvironment env, int arrLen, int arrX, int arrY, bool reversed)
		{
			// Result: Horizontal     [LEFT, 0] IN ... [RIGHT, 0] OUT (or the other way when reversed)

			// Array will land reversed on Stack
			// [A, B, C, D] ->
			//
			// _____
			// | A |
			// | B |
			// | C |
			// | D |
			// ¯¯¯¯¯
			//

			CodePiece pLen = NumberCodeHelper.GenerateCode(arrLen - 1, reversed);
			CodePiece pArx = NumberCodeHelper.GenerateCode(arrX, reversed);
			CodePiece pAry = NumberCodeHelper.GenerateCode(arrY, reversed);

			if (reversed)
			{
				// $_v#!:\g{Y}+{X}:<{Last}
				//   >1-           ^
				CodePiece p = new CodePiece();

				#region Reversed

				int botStart;
				int botEnd;

				botStart = 0;

				p[-2, 0] = BCHelper.StackPop;
				p[-1, 0] = BCHelper.IfHorizontal;
				p[0, 0] = BCHelper.PCDown;
				p[1, 0] = BCHelper.PCJump;
				p[2, 0] = BCHelper.Not;
				p[3, 0] = BCHelper.StackDup;
				p[4, 0] = BCHelper.StackSwap;
				p[5, 0] = BCHelper.ReflectGet;

				p.AppendRight(pAry);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(pArx);
				p.AppendRight(BCHelper.StackDup);
				botEnd = p.MaxX;
				p.AppendRight(BCHelper.PCLeft);
				p.AppendRight(pLen);

				p[botStart, 1] = BCHelper.PCRight;
				p[botStart + 1, 1] = BCHelper.Digit1;
				p[botStart + 2, 1] = BCHelper.Sub;

				p[botEnd, 1] = BCHelper.PCUp;

				p.FillRowWw(1, botStart + 3, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
			else
			{
				// {Last}>:{X}+{Y}g\:#v_$
				//       ^-1          <
				CodePiece p = new CodePiece();

				#region Normal

				int botStart;
				int botEnd;

				botStart = 0;

				p.AppendRight(BCHelper.PCRight);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(pArx);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(pAry);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(BCHelper.PCJump);

				botEnd = p.MaxX;

				p.AppendRight(BCHelper.PCDown);
				p.AppendRight(BCHelper.IfHorizontal);
				p.AppendRight(BCHelper.StackPop);

				p[botStart, 1] = BCHelper.PCUp;
				p[botStart + 1, 1] = BCHelper.Sub;
				p[botStart + 2, 1] = BCHelper.Digit1;
				p[botEnd, 1] = BCHelper.PCLeft;

				p.AppendLeft(pLen);

				p.FillRowWw(1, botStart + 3, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
		}

		private static CodePiece ReadMultilineArrayToStack(CodeGenEnvironment env, VarDeclarationPosition v, bool reversed)
		{
			// Result: Horizontal     [LEFT, 0] IN ... [RIGHT, 0] OUT (or the other way when reversed)

			// Array will land reversed on Stack
			// [A, B, C, D] ->
			// [E, F, G   ]
			//
			// _____
			// | A |
			// | B |
			// | C |
			// | D |
			// | E |
			// | F |
			// | G |
			// ¯¯¯¯¯
			//

			CodePiece pLast = NumberCodeHelper.GenerateCode(v.Size - 1, reversed);
			CodePiece pArx  = NumberCodeHelper.GenerateCode(v.X, reversed);
			CodePiece pAry  = NumberCodeHelper.GenerateCode(v.Y, reversed);
			CodePiece pArw  = NumberCodeHelper.GenerateCode(v.Width, reversed);

			if (reversed)
			{
				// $_v#!:\g+{Y}/{W}\+{X}%{W}::<{Last}
				//   >1-                      ^
				CodePiece p = new CodePiece();

				#region Reversed

				int botStart;
				int botEnd;

				botStart = 0;

				p[-2, 0] = BCHelper.StackPop;
				p[-1, 0] = BCHelper.IfHorizontal;
				p[0, 0] = BCHelper.PCDown;
				p.AppendRight(BCHelper.PCJump);
				p.AppendRight(BCHelper.Not);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(pAry);
				p.AppendRight(BCHelper.Div);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(pArx);
				p.AppendRight(BCHelper.Modulo);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(BCHelper.StackDup);
				botEnd = p.MaxX;
				p.AppendRight(BCHelper.PCLeft);
				p.AppendRight(pLast);

				p[botStart, 1] = BCHelper.PCRight;
				p[botStart + 1, 1] = BCHelper.Digit1;
				p[botStart + 2, 1] = BCHelper.Sub;

				p[botEnd, 1] = BCHelper.PCUp;

				p.FillRowWw(1, botStart + 3, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
			else
			{
				// {Last}>::{W}%{X}+\{W}/{Y}+g\:#v_$
				//       ^-1                     <
				CodePiece p = new CodePiece();

				#region Normal

				int botStart;
				int botEnd;

				botStart = 0;

				p.AppendRight(BCHelper.PCRight);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.Modulo);
				p.AppendRight(pArx);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.Div);
				p.AppendRight(pAry);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(BCHelper.StackDup);
				p.AppendRight(BCHelper.PCJump);

				botEnd = p.MaxX;

				p.AppendRight(BCHelper.PCDown);
				p.AppendRight(BCHelper.IfHorizontal);
				p.AppendRight(BCHelper.StackPop);

				p[botStart, 1] = BCHelper.PCUp;
				p[botStart + 1, 1] = BCHelper.Sub;
				p[botStart + 2, 1] = BCHelper.Digit1;
				p[botEnd, 1] = BCHelper.PCLeft;

				p.AppendLeft(pLast);

				p.FillRowWw(1, botStart + 3, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
		}

		#endregion

		#region WriteArrayFromStack

		public static CodePiece WriteArrayFromStack(CodeGenEnvironment env, VarDeclarationPosition v, bool reversed)
		{
			if (v.IsSingleLine())
				return WriteLinearArrayFromStack(env, v.Size, v.X, v.Y, reversed);
			else
				return WriteMultilineArrayFromStack(env, v, reversed);
		}
		
		private static CodePiece WriteLinearArrayFromStack(CodeGenEnvironment env, int arrLen, int arrX, int arrY, bool reversed)
		{
			// Result: Horizontal     [LEFT, 0] IN ... [RIGHT, 0] OUT (or the other way when reversed)

			// Array is reversed in Stack --> will land normal on Field
			// _____
			// | A |
			// | B |
			// | C | -->
			// | D |
			// ¯¯¯¯¯
			//			
			// [A, B, C, D]
			//

			CodePiece pTpx = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.X, reversed);
			CodePiece pTpy = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.Y, reversed);

			CodePiece pTpxR = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.X, !reversed);
			CodePiece pTpyR = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.Y, !reversed);

			CodePiece pLast = NumberCodeHelper.GenerateCode(arrLen - 1, reversed);
			CodePiece pArx  = NumberCodeHelper.GenerateCode(arrX, reversed);
			CodePiece pAry  = NumberCodeHelper.GenerateCode(arrY, reversed);

			if (reversed)
			{
				// _v#!-{Last}g{TY}{TX}p{Y}+{X}g{TY}{TX}<p{TY}{TX}0
				//  >{TX}{TY}g1+{TX}{TY}p               ^
				CodePiece p = new CodePiece();

				#region Reversed

				int botStart;
				int botEnd;

				botStart = 0;

				p[-1, 0] = BCHelper.IfHorizontal;
				p[0, 0] = BCHelper.PCDown;

				p.AppendRight(BCHelper.PCJump);

				p.AppendRight(BCHelper.Not);
				p.AppendRight(BCHelper.Sub);
				p.AppendRight(pLast);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				p.AppendRight(BCHelper.ReflectSet);
				p.AppendRight(pAry);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(pArx);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				botEnd = p.MaxX;

				p.AppendRight(BCHelper.PCLeft);
				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				p.AppendRight(BCHelper.Digit0);

				CodePiece pBottom = new CodePiece();
				{
					#region Generate_Bottom

					pBottom.AppendRight(pTpxR);
					pBottom.AppendRight(pTpyR);
					pBottom.AppendRight(BCHelper.ReflectGet);

					pBottom.AppendRight(BCHelper.Digit1);
					pBottom.AppendRight(BCHelper.Add);

					pBottom.AppendRight(pTpxR);
					pBottom.AppendRight(pTpyR);
					pBottom.AppendRight(BCHelper.ReflectSet);

					pBottom.NormalizeX();

					#endregion
				}

				p[botStart, 1] = BCHelper.PCRight;
				p[botEnd, 1] = BCHelper.PCUp;

				p.SetAt(botStart + 1, 1, pBottom);

				p.FillRowWw(1, botStart + 1 + pBottom.Width, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
			else
			{
				// 0{TX}{TY}p>{TX}{TY}g{X}+{Y}p{TX}{TY}g{Last}-#v_
				//           ^p{TY}{TX}+1g{TY}{TX}              < 
				CodePiece p = new CodePiece();

				#region Normal

				int botStart;
				int botEnd;

				p.AppendRight(BCHelper.Digit0);

				p.AppendRight(pTpx);
				p.AppendRight(pTpy);

				p.AppendRight(BCHelper.ReflectSet);
				botStart = p.MaxX;
				p.AppendRight(BCHelper.PCRight);

				p.AppendRight(pTpx);
				p.AppendRight(pTpy);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pArx);
				p.AppendRight(BCHelper.Add);
				p.AppendRight(pAry);
				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(pTpx);
				p.AppendRight(pTpy);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pLast);
				p.AppendRight(BCHelper.Sub);

				p.AppendRight(BCHelper.PCJump);
				botEnd = p.MaxX;
				p.AppendRight(BCHelper.PCDown);
				p.AppendRight(BCHelper.IfHorizontal);

				CodePiece pBottom = new CodePiece();
				{
					#region Generate_Bottom

					pBottom.AppendRight(BCHelper.ReflectSet);
					pBottom.AppendRight(pTpyR);
					pBottom.AppendRight(pTpxR);

					pBottom.AppendRight(BCHelper.Add);
					pBottom.AppendRight(BCHelper.Digit1);

					pBottom.AppendRight(BCHelper.ReflectGet);
					pBottom.AppendRight(pTpyR);
					pBottom.AppendRight(pTpxR);

					pBottom.NormalizeX();

					#endregion
				}

				p[botStart, 1] = BCHelper.PCUp;
				p[botEnd, 1] = BCHelper.PCLeft;

				p.SetAt(botStart + 1, 1, pBottom);

				p.FillRowWw(1, botStart + 1 + pBottom.Width, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
		}

		private static CodePiece WriteMultilineArrayFromStack(CodeGenEnvironment env, VarDeclarationPosition v, bool reversed)
		{
			// Result: Horizontal     [LEFT, 0] IN ... [RIGHT, 0] OUT (or the other way when reversed)

			// Array is reversed in Stack --> will land normal on Field
			// _____
			// | A |
			// | B |
			// | C | -->
			// | D |
			// | E |
			// | F |
			// ¯¯¯¯¯
			//
			// [A, B, C, D]
			// [E, F      ]
			//

			CodePiece pTpx = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.X, reversed);
			CodePiece pTpy = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.Y, reversed);

			CodePiece pTpxR = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.X, !reversed);
			CodePiece pTpyR = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.Y, !reversed);

			CodePiece pLast = NumberCodeHelper.GenerateCode(v.Size - 1, reversed);
			CodePiece pArx = NumberCodeHelper.GenerateCode(v.X, reversed);
			CodePiece pAry = NumberCodeHelper.GenerateCode(v.Y, reversed);
			CodePiece pArw = NumberCodeHelper.GenerateCode(v.Width, reversed);

			if (reversed)
			{
				// _v#!-{Last}g{TY}{TX}p+/g{TY}{TX}{Y}+%{W}g{TY}{TX}{X}<p{TY}{TX}0
				//  >{TX}{TY}g1+{TX}{TY}p                              ^
				CodePiece p = new CodePiece();

				#region Reversed

				int botStart;
				int botEnd;

				botStart = 0;

				p[-1, 0] = BCHelper.IfHorizontal;
				p[0, 0] = BCHelper.PCDown;
				p[1, 0] = BCHelper.PCJump;
				p[2, 0] = BCHelper.Not;
				p[3, 0] = BCHelper.Sub;

				p.AppendRight(pLast);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(BCHelper.Add);
				p.AppendRight(BCHelper.Div);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);
				p.AppendRight(pAry);

				p.AppendRight(BCHelper.Add);
				p.AppendRight(BCHelper.Modulo);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);
				p.AppendRight(pArx);

				botEnd = p.MaxX;

				p.AppendRight(BCHelper.PCLeft);
				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				p.AppendRight(BCHelper.Digit0);

				CodePiece pBottom = new CodePiece();
				{
					#region Generate_Bottom

					pBottom.AppendRight(pTpxR);
					pBottom.AppendRight(pTpyR);
					pBottom.AppendRight(BCHelper.ReflectGet);

					pBottom.AppendRight(BCHelper.Digit1);
					pBottom.AppendRight(BCHelper.Add);

					pBottom.AppendRight(pTpxR);
					pBottom.AppendRight(pTpyR);
					pBottom.AppendRight(BCHelper.ReflectSet);

					pBottom.NormalizeX();

					#endregion
				}

				p[botStart, 1] = BCHelper.PCRight;
				p[botEnd, 1] = BCHelper.PCUp;

				p.SetAt(botStart + 1, 1, pBottom);

				p.FillRowWw(1, botStart + 1 + pBottom.Width, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
			else
			{
				// 0{TX}{TY}p>{X}{TX}{TY}g{W}%+{Y}{TX}{TY}g{W}/+p{TX}{TY}g{Last}-#v_
				//           ^p{TY}{TX}+1g{TY}{TX}                                < 
				CodePiece p = new CodePiece();

				#region Normal

				int botStart;
				int botEnd;

				p.AppendRight(BCHelper.Digit0);

				p.AppendRight(pTpx); // {TX}
				p.AppendRight(pTpy); // {TY}

				p.AppendRight(BCHelper.ReflectSet);
				botStart = p.MaxX;
				p.AppendRight(BCHelper.PCRight);

				p.AppendRight(pArx); // {X}
				p.AppendRight(pTpx); // {TX}
				p.AppendRight(pTpy); // {TY}
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pArw); // {W}
				p.AppendRight(BCHelper.Modulo);
				p.AppendRight(BCHelper.Add);

				p.AppendRight(pAry); // {Y}
				p.AppendRight(pTpx); // {TX}
				p.AppendRight(pTpy); // {TY}
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pArw); // {W}
				p.AppendRight(BCHelper.Div);
				p.AppendRight(BCHelper.Add);

				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(pTpx); // {TX}
				p.AppendRight(pTpy); // {TY}
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pLast); // {Last}
				p.AppendRight(BCHelper.Sub);

				p.AppendRight(BCHelper.PCJump);
				botEnd = p.MaxX;
				p.AppendRight(BCHelper.PCDown);
				p.AppendRight(BCHelper.IfHorizontal);

				CodePiece pBottom = new CodePiece();
				{
					#region Generate_Bottom

					pBottom.AppendRight(BCHelper.ReflectSet);
					pBottom.AppendRight(pTpyR);
					pBottom.AppendRight(pTpxR);

					pBottom.AppendRight(BCHelper.Add);
					pBottom.AppendRight(BCHelper.Digit1);

					pBottom.AppendRight(BCHelper.ReflectGet);
					pBottom.AppendRight(pTpyR);
					pBottom.AppendRight(pTpxR);

					pBottom.NormalizeX();

					#endregion
				}

				p[botStart, 1] = BCHelper.PCUp;
				p[botEnd, 1] = BCHelper.PCLeft;

				p.SetAt(botStart + 1, 1, pBottom);

				p.FillRowWw(1, botStart + 1 + pBottom.Width, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
		}

		#endregion

		#region WriteArrayFromReversedStack

		public static CodePiece WriteArrayFromReversedStack(CodeGenEnvironment env, int arrLen, VarDeclarationPosition v, bool reversed)
		{
			if (v.IsSingleLine())
				return WriteLinearArrayFromReversedStack(env, arrLen, v.X, v.Y, reversed);
			else
				return WriteMultilineArrayFromReversedStack(env, v, reversed);
		}

		private static CodePiece WriteLinearArrayFromReversedStack(CodeGenEnvironment env, int arrLen, int arrX, int arrY, bool reversed)
		{
			// Normally Arrays are reversed on Stack -> this Method is for the reversed case --> Stack is normal on stack

			// Result: Horizontal     [LEFT, 0] IN ... [RIGHT, 0] OUT (or the other way when reversed)

			// Array is !! NOT !! reversed in Stack --> will land normal on Field
			// _____
			// | D |
			// | C |
			// | B | -->
			// | A |
			// ¯¯¯¯¯
			// 
			// [A, B, C, D]
			//

			CodePiece pTpx = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.X, reversed);
			CodePiece pTpy = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.Y, reversed);

			CodePiece pTpxR = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.X, !reversed);
			CodePiece pTpyR = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.Y, !reversed);

			CodePiece pLast = NumberCodeHelper.GenerateCode(arrLen - 1, reversed);
			CodePiece pArx = NumberCodeHelper.GenerateCode(arrX, reversed);
			CodePiece pAry = NumberCodeHelper.GenerateCode(arrY, reversed);

			if (reversed)
			{
				// _v#!p{Y}+g{TY}{TX}{X}\g{TY}{TX}<p{TY}{TX}{Last}
				//  >{TX}:{TY}g1-\{TY}p           ^        
				CodePiece p = new CodePiece();

				#region Reversed

				int botStart;
				int botEnd;

				botStart = 0;

				p[-1, 0] = BCHelper.IfHorizontal;
				p[0, 0] = BCHelper.PCDown;
				p[1, 0] = BCHelper.PCJump;
				p[2, 0] = BCHelper.Not;

				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(pAry);

				p.AppendRight(BCHelper.Add);
				p.AppendRight(BCHelper.ReflectGet);

				p.AppendRight(pTpy);
				p.AppendRight(pTpx);
				p.AppendRight(pArx);

				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(BCHelper.ReflectGet);

				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				botEnd = p.MaxX;

				p.AppendRight(BCHelper.PCLeft);
				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				p.AppendRight(pLast);

				CodePiece pBottom = new CodePiece();
				{
					#region Generate_Bottom

					pBottom.AppendRight(pTpxR);

					pBottom.AppendRight(BCHelper.StackDup);

					pBottom.AppendRight(pTpyR);

					pBottom.AppendRight(BCHelper.ReflectGet);
					pBottom.AppendRight(BCHelper.Digit1);
					pBottom.AppendRight(BCHelper.Sub);
					pBottom.AppendRight(BCHelper.StackSwap);

					pBottom.AppendRight(pTpyR);

					pBottom.AppendRight(BCHelper.ReflectSet);

					pBottom.NormalizeX();

					#endregion
				}

				p[botStart, 1] = BCHelper.PCRight;
				p[botEnd, 1] = BCHelper.PCUp;

				p.SetAt(botStart + 1, 1, pBottom);

				p.FillRowWw(1, botStart + 1 + pBottom.Width, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
			else
			{
				// {Last}{TX}{TY}p>{TX}{TY}g\{X}{TX}{TY}g+{Y}p#v_
				//                ^p{TY}\-1g{TY}:{TX}          < 
				CodePiece p = new CodePiece();

				#region Normal

				int botStart;
				int botEnd;

				p.AppendRight(pLast);

				p.AppendRight(pTpx);
				p.AppendRight(pTpy);

				p.AppendRight(BCHelper.ReflectSet);
				botStart = p.MaxX;
				p.AppendRight(BCHelper.PCRight);

				p.AppendRight(pTpx);
				p.AppendRight(pTpy);

				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(BCHelper.StackSwap);

				p.AppendRight(pArx);
				p.AppendRight(pTpx);
				p.AppendRight(pTpy);

				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(BCHelper.Add);

				p.AppendRight(pAry);

				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(BCHelper.PCJump);
				botEnd = p.MaxX;
				p.AppendRight(BCHelper.PCDown);
				p.AppendRight(BCHelper.IfHorizontal);

				CodePiece pBottom = new CodePiece();
				{
					#region Generate_Bottom

					pBottom[0, 0] = BCHelper.ReflectSet;

					pBottom.AppendRight(pTpyR);

					pBottom.AppendRight(BCHelper.StackSwap);
					pBottom.AppendRight(BCHelper.Sub);
					pBottom.AppendRight(BCHelper.Digit1);
					pBottom.AppendRight(BCHelper.ReflectGet);

					pBottom.AppendRight(pTpyR);

					pBottom.AppendRight(BCHelper.StackDup);

					pBottom.AppendRight(pTpxR);

					pBottom.NormalizeX();

					#endregion
				}

				p[botStart, 1] = BCHelper.PCUp;
				p[botEnd, 1] = BCHelper.PCLeft;

				p.SetAt(botStart + 1, 1, pBottom);

				p.FillRowWw(1, botStart + 1 + pBottom.Width, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
		}

		private static CodePiece WriteMultilineArrayFromReversedStack(CodeGenEnvironment env, VarDeclarationPosition v, bool reversed)
		{
			// Normally Arrays are reversed on Stack -> this Method is for the reversed case --> Stack is normal on stack

			// Result: Horizontal     [LEFT, 0] IN ... [RIGHT, 0] OUT (or the other way when reversed)

			// Array is !! NOT !! reversed in Stack --> will land normal on Field
			// _____
			// | F |
			// | E |
			// | D |
			// | C |
			// | B | -->
			// | A |
			// ¯¯¯¯¯
			// 
			// [A, B, C, D]
			// [E, F      ]
			// 

			CodePiece pTpx = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.X, reversed);
			CodePiece pTpy = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.Y, reversed);

			CodePiece pTpxR = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.X, !reversed);
			CodePiece pTpyR = NumberCodeHelper.GenerateCode(env.TMP_FIELD_IO_ARR.Y, !reversed);

			CodePiece pLen = NumberCodeHelper.GenerateCode(v.Size - 1, reversed);
			CodePiece pArx = NumberCodeHelper.GenerateCode(v.X, reversed);
			CodePiece pAry = NumberCodeHelper.GenerateCode(v.Y, reversed);
			CodePiece pArw = NumberCodeHelper.GenerateCode(v.Width, reversed);

			if (reversed)
			{
				// _v#!p+/{W}g{TY}{TX}{Y}+%{W}g{TY}{TX}{X}\g{TY}{TX}<p{TY}{TX}{Last}
				//  >{TX}:{TY}g1-\{TY}p                             ^
				CodePiece p = new CodePiece();

				#region Reversed

				int botStart;
				int botEnd;

				botStart = 0;

				p[-1, 0] = BCHelper.IfHorizontal;
				p[0, 0] = BCHelper.PCDown;
				p[1, 0] = BCHelper.PCJump;
				p[2, 0] = BCHelper.Not;

				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(BCHelper.Add);
				p.AppendRight(BCHelper.Div);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);
				p.AppendRight(pAry);

				p.AppendRight(BCHelper.Add);
				p.AppendRight(BCHelper.Modulo);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);
				p.AppendRight(pArx);

				p.AppendRight(BCHelper.StackSwap);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				botEnd = p.MaxX;

				p.AppendRight(BCHelper.PCLeft);

				p.AppendRight(BCHelper.ReflectSet);
				p.AppendRight(pTpy);
				p.AppendRight(pTpx);

				p.AppendRight(pLen);

				CodePiece pBottom = new CodePiece();
				{
					#region Generate_Bottom

					pBottom.AppendRight(pTpxR);

					pBottom.AppendRight(BCHelper.StackDup);

					pBottom.AppendRight(pTpyR);

					pBottom.AppendRight(BCHelper.ReflectGet);
					pBottom.AppendRight(BCHelper.Digit1);
					pBottom.AppendRight(BCHelper.Sub);
					pBottom.AppendRight(BCHelper.StackSwap);

					pBottom.AppendRight(pTpyR);

					pBottom.AppendRight(BCHelper.ReflectSet);

					pBottom.NormalizeX();

					#endregion
				}

				p[botStart, 1] = BCHelper.PCRight;
				p[botEnd, 1] = BCHelper.PCUp;

				p.SetAt(botStart + 1, 1, pBottom);

				p.FillRowWw(1, botStart + 1 + pBottom.Width, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
			else
			{
				// {Last}{TX}{TY}p>{TX}{TY}g\{X}{TX}{TY}g{W}%+{Y}{TX}{TY}g{W}/+p#v_
				//                ^p{TY}\-1g{TY}:{TX}                            < 
				CodePiece p = new CodePiece();

				#region Normal

				int botStart;
				int botEnd;

				p.AppendRight(pLen);

				p.AppendRight(pTpx);
				p.AppendRight(pTpy);

				p.AppendRight(BCHelper.ReflectSet);
				botStart = p.MaxX;
				p.AppendRight(BCHelper.PCRight);

				p.AppendRight(pTpx);
				p.AppendRight(pTpy);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(BCHelper.StackSwap);

				p.AppendRight(pArx);
				p.AppendRight(pTpx);
				p.AppendRight(pTpy);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.Modulo);
				p.AppendRight(BCHelper.Add);

				p.AppendRight(pAry);
				p.AppendRight(pTpx);
				p.AppendRight(pTpy);
				p.AppendRight(BCHelper.ReflectGet);
				p.AppendRight(pArw);
				p.AppendRight(BCHelper.Div);
				p.AppendRight(BCHelper.Add);

				p.AppendRight(BCHelper.ReflectSet);

				p.AppendRight(BCHelper.PCJump);
				botEnd = p.MaxX;
				p.AppendRight(BCHelper.PCDown);
				p.AppendRight(BCHelper.IfHorizontal);

				CodePiece pBottom = new CodePiece();
				{
					#region Generate_Bottom

					pBottom[0, 0] = BCHelper.ReflectSet;

					pBottom.AppendRight(pTpyR);

					pBottom.AppendRight(BCHelper.StackSwap);
					pBottom.AppendRight(BCHelper.Sub);
					pBottom.AppendRight(BCHelper.Digit1);
					pBottom.AppendRight(BCHelper.ReflectGet);

					pBottom.AppendRight(pTpyR);

					pBottom.AppendRight(BCHelper.StackDup);

					pBottom.AppendRight(pTpxR);

					pBottom.NormalizeX();

					#endregion
				}

				p[botStart, 1] = BCHelper.PCUp;
				p[botEnd, 1] = BCHelper.PCLeft;

				p.SetAt(botStart + 1, 1, pBottom);

				p.FillRowWw(1, botStart + 1 + pBottom.Width, botEnd);

				p.NormalizeX();

				#endregion

				return p;
			}
		}

		#endregion

		#region VerticalLaneTurnout

		public static CodePiece VerticalLaneTurnout_Test()
		{
			// #
			// >
			// |
			CodePiece p = new CodePiece();

			p[0, -1] = BCHelper.PCJump;
			p[0, +0] = BCHelper.PCRight;
			p[0, +1] = BCHelper.IfVertical;

			return p;
		}

		public static CodePiece VerticalLaneTurnout_Dec(bool stripped)
		{
			if (stripped)
			{
				// :
				// !
				// #
				// >
				// |
				CodePiece p = new CodePiece();

				p[0, -3] = BCHelper.StackDup;
				p[0, -2] = BCHelper.Not;
				p[0, -1] = BCHelper.PCJump;
				p[0, +0] = BCHelper.PCRight;
				p[0, +1] = BCHelper.IfVertical;

				return p;
			}
			else
			{
				// 1
				// -
				// :
				// !
				// #
				// >
				// |
				CodePiece p = new CodePiece();

				p[0, -5] = BCHelper.Digit1;
				p[0, -4] = BCHelper.Sub;
				p[0, -3] = BCHelper.StackDup;
				p[0, -2] = BCHelper.Not;
				p[0, -1] = BCHelper.PCJump;
				p[0, +0] = BCHelper.PCRight;
				p[0, +1] = BCHelper.IfVertical;

				return p;
			}
		}

		#endregion

		public static CodePiece BooleanStackFlooder()
		{
			//Stack Flooder ALWAYS reversed (right -> left)

			// $_v#!:-1<\1+1:
			//   >0\   ^
			CodePiece p = new CodePiece();

			p.SetAt(0, 0, CodePiece.ParseFromLine(@"$_v#!:-1<\1+1:"));
			p.SetAt(2, 1, CodePiece.ParseFromLine(  @">0\   ^", true));

			return p;
		}

		public static CodePiece PopMultipleStackValues(int count, bool reversed)
		{
			CodePiece pCount = NumberCodeHelper.GenerateCode(count, reversed);

			CodePiece p = new CodePiece();

			if (reversed)
			{
				//   >\$1-v
				// $_^# !:<{C}

				p.SetAt(2, -1, CodePiece.ParseFromLine(@">\$1-v"));
				p.SetAt(0, +0, CodePiece.CombineHorizontal(CodePiece.ParseFromLine(@"$_^# !:<", true), pCount));
			}
			else
			{
				// {C}0>-:#v_$
				//     ^1$\<

				p.SetAt(0, 0, CodePiece.ParseFromLine(@"0>-:#v_$"));
				p.SetAt(1, 1, CodePiece.ParseFromLine(@"^1$\<"));

				p.AppendLeft(pCount);
			}

			p.NormalizeX();

			if (p.Width >= count) return CodePiece.Repeat(count, BCHelper.StackPop);

			return p;
		}

		public static CodePiece ReadValueFromField(MathExt.Point pos, bool reversed)
		{
			CodePiece p = CodePiece.CombineHorizontal(NumberCodeHelper.GenerateCode(pos.X), NumberCodeHelper.GenerateCode(pos.Y), new CodePiece(BCHelper.ReflectGet));

			if (reversed)
				p.ReverseX(false);

			return p;
		}

		public static CodePiece ReadValueFromField(int px, int py, bool reversed)
		{
			CodePiece p = CodePiece.CombineHorizontal(NumberCodeHelper.GenerateCode(px), NumberCodeHelper.GenerateCode(py), new CodePiece(BCHelper.ReflectGet));

			if (reversed)
				p.ReverseX(false);

			return p;
		}

		public static CodePiece WriteValueToField(MathExt.Point pos, bool reversed)
		{
			CodePiece p = CodePiece.CombineHorizontal(NumberCodeHelper.GenerateCode(pos.X), NumberCodeHelper.GenerateCode(pos.Y), new CodePiece(BCHelper.ReflectSet));

			if (reversed)
				p.ReverseX(false);

			return p;
		}

		public static CodePiece WriteValueToField(int px, int py, bool reversed)
		{
			CodePiece p = CodePiece.CombineHorizontal(NumberCodeHelper.GenerateCode(px), NumberCodeHelper.GenerateCode(py), new CodePiece(BCHelper.ReflectSet));

			if (reversed)
				p.ReverseX(false);

			return p;
		}

		public static CodePiece ModuloRangeLimiter(int range, bool reversed)
		{
			CodePiece p = new CodePiece();

			CodePiece pR = NumberCodeHelper.GenerateCode(range, reversed);
			CodePiece pRRev = NumberCodeHelper.GenerateCode(range, !reversed);

			if (reversed)
			{
				#region Reversed
				// v\{R}:*-10:   <
				// >#<{R}%-++1#v_^#`0:
				//   ^%{R}     <

				CodePiece pTop = CodePiece.CombineHorizontal(CodePiece.ParseFromLine(@"v\"), pR, CodePiece.ParseFromLine(@":*-10:"));
				CodePiece pMid = CodePiece.CombineHorizontal(CodePiece.ParseFromLine(@">#<"), pRRev, CodePiece.ParseFromLine(@"%-++1#"));
				CodePiece pBot = CodePiece.CombineHorizontal(CodePiece.ParseFromLine(@"^%"), pR);

				pTop.AddXOffset(0);
				pMid.AddXOffset(0);
				pBot.AddXOffset(2);

				int botW = Math.Max(pMid.MaxX, pBot.MaxX);

				pMid.FillRowWw(0, pMid.MaxX, botW);
				pMid[botW + 0, 0] = BCHelper.PCDown;
				pMid[botW + 1, 0] = BCHelper.IfHorizontal;

				pBot.FillRowWw(0, pBot.MaxX, botW);
				pBot[botW, 0] = BCHelper.PCLeft;

				int topW = Math.Max(pTop.MaxX, pMid.MaxX);

				pTop.FillRowWw(0, pTop.MaxX, topW);
				pTop.AppendRight(BCHelper.PCLeft);

				pMid.FillRowWw(0, pMid.MaxX, topW);
				pMid.AppendRight(CodePiece.ParseFromLine(@"^#`0:"));


				p.SetAt(0, -1, pTop);
				p.SetAt(0, +0, pMid);
				p.SetAt(0, +1, pBot);

				#endregion
			}
			else
			{
				#region Normal
				//       >:01-*:{R}\ v
				// :0`#v_^#1++-%{R}>#<
				//     >{R}%       ^

				CodePiece pTop = CodePiece.CombineHorizontal(CodePiece.ParseFromLine(@">:01-*:"), pR);
				CodePiece pMid = CodePiece.CombineHorizontal(CodePiece.ParseFromLine(@":0`#v_^#1++-%"), pRRev);
				CodePiece pBot = CodePiece.CombineHorizontal(new CodePiece(BCHelper.PCRight), pR, new CodePiece(BCHelper.Modulo));

				pTop.AddXOffset(6);
				pMid.AddXOffset(0);
				pBot.AddXOffset(4);

				int max = MathExt.Max(pTop.MaxX, pMid.MaxX, pBot.MaxX);

				pTop.FillRowWw(0, pTop.MaxX, max);
				pMid.FillRowWw(0, pMid.MaxX, max);
				pBot.FillRowWw(0, pBot.MaxX, max);

				pTop.AppendRight(CodePiece.ParseFromLine(@"\ v", true));
				pMid.AppendRight(CodePiece.ParseFromLine(@">#<", false));
				pBot.AppendRight(CodePiece.ParseFromLine(@"^", false));

				p.SetAt(0, -1, pTop);
				p.SetAt(0, +0, pMid);
				p.SetAt(0, +1, pBot);

				#endregion
			}

			return p;
		}

		#region Base4 Random Generator

		public static CodePiece RandomDigitGenerator(CodePiece len, bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed
				//  >       v<   
				//  1  v0<  -1   
				// <|:\<1?v#<|`0:
				//  |  ^2 < 0$   
				// ^<  ^3<  ^<   
				#endregion

				p.SetAt(0, -2, CodePiece.ParseFromLine(@"@>       v<@@@", true, true));
				p.SetAt(0, -1, CodePiece.ParseFromLine(@"@1@@v0<  -1@@@", true, true));
				p.SetAt(0, 00, CodePiece.ParseFromLine(@"<|:\<1?v#<|`0:", true, true));
				p.SetAt(0, +1, CodePiece.ParseFromLine(@" |@@^2 <@0$@@@", true, true));
				p.SetAt(0, +2, CodePiece.ParseFromLine(@"^<@@^3<@@^<@@@", true, true));

				p.AppendRight(len);

				p.AppendRight(BCHelper.Digit9);

			}
			else
			{
				#region Normal
				//    >v       < 
				//    1-  >0v  1 
				// :0`|>#v?1>\:|>
				//    $0 > 2^  | 
				//    >^  >3^  >^

				p.SetAt(0, -2, CodePiece.ParseFromLine(@"@@@>v       <@", true, true));
				p.SetAt(0, -1, CodePiece.ParseFromLine(@"@@@1-@@>0v@@1@", true, true));
				p.SetAt(0, 00, CodePiece.ParseFromLine(@":0`|>#v?1>\:|>", true, true));
				p.SetAt(0, +1, CodePiece.ParseFromLine(@"@@@$0@> 2^@@|@", true, true));
				p.SetAt(0, +2, CodePiece.ParseFromLine(@"@@@>^@@>3^@@>^", true, true));

				p.AppendLeft(len);

				p.AppendLeft(BCHelper.Digit9);

				#endregion
			}

			p.NormalizeX();

			return p;
		}

		public static CodePiece Base4DigitJoiner(bool reversed)
		{
			CodePiece p = new CodePiece();

			if (reversed)
			{
				#region Reversed
				//  v<     
				// $<|`4:\<				
				//   >\4*+^

				p.SetAt(0, -1, CodePiece.ParseFromLine(@"@v<@@@@@", true, true));
				p.SetAt(0, 00, CodePiece.ParseFromLine(@"$<|`4:\<", true, true));
				p.SetAt(0, +1, CodePiece.ParseFromLine(@"@@>\4*+^", true, true));

				#endregion
			}
			else
			{
				#region Normal
				//      >v
				// >\:4`|>$
				// ^+*4\<

				p.SetAt(0, -1, CodePiece.ParseFromLine(@"@@@@@>v@", true, true));
				p.SetAt(0, 00, CodePiece.ParseFromLine(@">\:4`|>$", true, true));
				p.SetAt(0, +1, CodePiece.ParseFromLine(@"^+*4\<@@", true, true));

				#endregion
			}

			p.NormalizeX();

			return p;
		}

		#endregion

		#region Switch Statement

		public static CodePiece SwitchStatementTester(bool reversed)
		{
			//   >1v
			//   +v<
			// -:|>\
			//   +  
			//   >  
			CodePiece p = new CodePiece();

			p.SetAt(0, -2, CodePiece.ParseFromLine(@"@@>1v", true, true));
			p.SetAt(0, -1, CodePiece.ParseFromLine(@"@@+v<", true, true));
			p.SetAt(0, 00, CodePiece.ParseFromLine(@"-:|>\", true, true));
			p.SetAt(0, +1, CodePiece.ParseFromLine(@"@@+@@", true, true));
			p.SetAt(0, +2, CodePiece.ParseFromLine(@"  >  ", true, true));

			if (reversed)
				p.ReverseX(true);

			p.NormalizeX();
			return p;
		}

		public static CodePiece SwitchLaneTurnout()
		{
			// v_v
			// >v

			CodePiece p = new CodePiece();

			p[0, 0] = BCHelper.PCDown;
			p[1, 0] = BCHelper.IfHorizontal;
			p[2, 0] = BCHelper.PCDown;

			p[0, 1] = BCHelper.PCRight;
			p[1, 1] = BCHelper.PCDown;
			p[2, 1] = BCHelper.Walkway;

			return p;
		}

		#endregion

		public static CodePiece CreateSerpentine(List<CodePiece> pieces, bool reversed)
		{
			if (pieces.Count == 0)
				return new CodePiece();
			if (pieces.Count == 1)
				return pieces[0];

			if (pieces.Count % 2 == 0)
				pieces.Add(new CodePiece());

			pieces = pieces.Select(p => p.Copy()).ToList();

			int maxlen = pieces.Max(lp => lp.Width);

			pieces.ForEach(lp => lp.ExtendWithWalkwayLeft(maxlen));
			pieces.ForEach(lp => lp.Normalize());

			if (reversed)
			{
				#region Reversed

				//    
				//    (~ = Walkway)
				//    
				//    <v1111111111111~
				//    ~ 1111111111111 
				//    ~>2222222222222v
				//    ~v3333333333333<
				//    ~>4444444444444v
				//    ~ 4444444444444 
				//    ~ 4444444444444 
				//    ^~5555555555555<
				//    

				for (int i = 0; i < pieces.Count; i++)
				{
					bool first = (i == 0);
					bool last = (i == pieces.Count - 1);

					if (first)
						pieces[i][maxlen, 0] = BCHelper.Walkway;
					else if (i % 2 == 0)
						pieces[i][maxlen, 0] = BCHelper.PCLeft;
					else // if (i%2 != 0)
						pieces[i][maxlen, 0] = BCHelper.PCDown;

					if (last)
						pieces[i][-1, 0] = BCHelper.Walkway;
					else if (i % 2 == 0)
						pieces[i][-1, 0] = BCHelper.PCDown;
					else // if (i%2 != 0)
						pieces[i][-1, 0] = BCHelper.PCRight;

					if (i % 2 == 0)
						pieces[i].CreateColWw(-1, 1, pieces[i].MaxY);
					else // if (i%2 != 0)
						pieces[i].CreateColWw(maxlen, 1, pieces[i].MaxY);
				}

				pieces[pieces.Count - 1][-2, 0] = BCHelper.PCUp;
				for (int i = 0; i < pieces.Count; i++)
				{
					if (i == 0)
						pieces[i].CreateColWw(-2, 1, pieces[i].MaxY);
					else if (i == pieces.Count - 1)
						pieces[i].CreateColWw(-2, 0, pieces[i].MaxY - 1);
					else
						pieces[i].CreateColWw(-2, 0, pieces[i].MaxY);
				}
				pieces[0][-2, 0] = BCHelper.PCLeft;

				#endregion
			}
			else
			{
				#region Normal

				//   
				//    (~ = Walkway)
				//   
				//    ~1111111111111v>
				//     1111111111111~~
				//    v2222222222222<~
				//    >3333333333333v~
				//    v4444444444444<~
				//    ~4444444444444 ~
				//    ~4444444444444 ~
				//    >5555555555555~^
				//   

				for (int i = 0; i < pieces.Count; i++)
				{
					bool first = (i == 0);
					bool last = (i == pieces.Count - 1);

					if (first)
						pieces[i][-1, 0] = BCHelper.Walkway;
					else if (i % 2 == 0)
						pieces[i][-1, 0] = BCHelper.PCRight;
					else // if (i%2 != 0)
						pieces[i][-1, 0] = BCHelper.PCDown;

					if (last)
						pieces[i][maxlen, 0] = BCHelper.Walkway;
					else if (i % 2 == 0)
						pieces[i][maxlen, 0] = BCHelper.PCDown;
					else // if (i%2 != 0)
						pieces[i][maxlen, 0] = BCHelper.PCLeft;

					if (i % 2 == 0)
						pieces[i].CreateColWw(maxlen, 1, pieces[i].MaxY);
					else // if (i%2 != 0)
						pieces[i].CreateColWw(-1, 1, pieces[i].MaxY);
				}

				pieces[pieces.Count - 1][maxlen + 1, 0] = BCHelper.PCUp;
				for (int i = 0; i < pieces.Count; i++)
				{
					if (i == 0)
						pieces[i].CreateColWw(maxlen + 1, 1, pieces[i].MaxY);
					else if (i == pieces.Count - 1)
						pieces[i].CreateColWw(maxlen + 1, 0, pieces[i].MaxY - 1);
					else
						pieces[i].CreateColWw(maxlen + 1, 0, pieces[i].MaxY);
				}
				pieces[0][maxlen + 1, 0] = BCHelper.PCRight;

				#endregion
			}

			return CodePiece.CreateFromVerticalList(pieces);
		}

		public static CodePiece CreateVariableSpace(List<VarDeclaration> variables, int moX, int moY, CodeGenOptions cgo, int maxWidth)
		{
			CodePiece p = new CodePiece();

			int paramX = 0;
			int paramY = 0;
			
			foreach (var var in variables.OrderBy(v => v.Type.GetCodeSize()))
			{
				CodePiece lit = new CodePiece();

				int size = var.Type.GetCodeSize();

				if (paramX > 0 && paramX + size > maxWidth)
				{
					// Next Line
					paramX = 0;
					paramY++;
				}

				if (size > maxWidth)
				{
					// Multiline

					int w = maxWidth;
					int h = (int)Math.Ceiling((size * 1d) / w);

					for (int i = 0; i < h-1; i++)
						lit.Fill(0, i, w, i + 1, BCHelper.Chr(cgo.DefaultVarDeclarationSymbol), new VarDeclarationTag(var));
					lit.Fill(0, h - 1, size % w, h, BCHelper.Chr(cgo.DefaultVarDeclarationSymbol));
					p.SetAt(paramX, paramY, lit);

					var.CodeDeclarationPos = new VarDeclarationPosition(moX + paramX, moY + paramY, w, h, size);

					paramX = 0;
					paramY += h;
				}
				else
				{
					//Single Line

					lit.Fill(0, 0, size, 1, BCHelper.Chr(cgo.DefaultVarDeclarationSymbol), new VarDeclarationTag(var));
					p.SetAt(paramX, paramY, lit);

					var.CodeDeclarationPos = new VarDeclarationPosition(moX + paramX, moY + paramY, size, 1, size);

					paramX += lit.Width;
				}
			}

			return p;
		}


		public static Tuple<CodePiece, VarDeclarationPosition> CreateVariableSpace(int size, CodeGenOptions cgo, int maxWidth, BefungeCommand cmd, CodeTag t)
		{
			CodePiece lit = new CodePiece();

			int w = maxWidth;
			int h = (int)Math.Ceiling((size * 1d) / w);

			for (int i = 0; i < h - 1; i++)
			{
				lit.Fill(0, i, w, i + 1, BCHelper.Chr(cgo.DefaultVarDeclarationSymbol), i==0 ? t : null);
			}

			lit.Fill(0, h - 1, size % w, h, BCHelper.Chr(cgo.DefaultVarDeclarationSymbol));
			
			var cdp = new VarDeclarationPosition(0, 0, w, h, size);

			return Tuple.Create(lit, cdp);
		}
	}
}
