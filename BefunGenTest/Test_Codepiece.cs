using BefunGen.AST;
using BefunGen.AST.CodeGen;
using BefunGen.AST.CodeGen.Tags;
using BefunGen.AST.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BefunGenTest
{
	[TestClass]
	public class Test_Codepiece
	{
		[TestMethod]
		public void CodePieceTest_Set()
		{
			CodePiece cp = new CodePiece();

			cp[0, 0] = new BefungeCommand(BefungeCommandType.Add);
			Assert.AreEqual(1, cp.Width);
			Assert.AreEqual(1, cp.Height);

			cp[0, 2] = new BefungeCommand(BefungeCommandType.Add);
			Assert.AreEqual(1, cp.Width);
			Assert.AreEqual(3, cp.Height);

			cp[2, 0] = new BefungeCommand(BefungeCommandType.Add);
			Assert.AreEqual(3, cp.Width);
			Assert.AreEqual(3, cp.Height);

			cp[2, 2] = new BefungeCommand(BefungeCommandType.Add);
			Assert.AreEqual(3, cp.Width);
			Assert.AreEqual(3, cp.Height);

			cp[0, -2] = new BefungeCommand(BefungeCommandType.Add);
			Assert.AreEqual(3, cp.Width);
			Assert.AreEqual(5, cp.Height);

			cp[-2, 0] = new BefungeCommand(BefungeCommandType.Add);
			Assert.AreEqual(5, cp.Width);
			Assert.AreEqual(5, cp.Height);

			cp[-2, -2] = new BefungeCommand(BefungeCommandType.Add);
			Assert.AreEqual(5, cp.Width);
			Assert.AreEqual(5, cp.Height);
		}

		[TestMethod]
		public void CodePieceTest_forceNonEmpty()
		{
			CodePiece p = new CodePiece();

			p.ForceNonEmpty(BCHelper.Add);
			Assert.AreEqual(1, p.Size);
		}

		[TestMethod]
		public void CodePieceTest_parseFromLine()
		{
			CodePiece p = CodePiece.ParseFromLine(@"872++:-");
			Assert.AreEqual(7, p.Width);
			Assert.AreEqual(1, p.Height);
		}

		[TestMethod]
		public void CodePieceTest_ModificationDetection()
		{
			CodePiece p = new CodePiece(BCHelper.Digit0);

			try
			{
				p[0, 0] = BCHelper.Digit1;
			}
			catch (InvalidCodeManipulationException)
			{
				return;
			}
			Assert.Fail();
		}

		[TestMethod]
		public void CodePieceTest_Replace()
		{
			CodePiece p = CodePiece.ParseFromLine("  ", true);
			;

			p.SetTag(1, 0, new TemporaryCodeFieldTag());

			p.ReplaceWalkway(0, 0, BCHelper.Sub_tagged(new TemporaryCodeFieldTag()), false);

			p.ReplaceWalkway(1, 0, BCHelper.Add, true);
		}

		[TestMethod]
		public void CodePieceTest_toString()
		{
			Assert.AreEqual(@">:#,_$", CodePiece.ParseFromLine(@">:#,_$").ToSimpleString());
		}

		[TestMethod]
		public void CodePieceTest_hasActiveTag()
		{
			CodePiece p = new CodePiece(BCHelper.PCDown);

			Assert.AreEqual(false, p.HasActiveTag(typeof(TemporaryCodeFieldTag)));
		}

		[TestMethod]
		public void CodePieceTest_setTag()
		{
			CodePiece p = new CodePiece(BCHelper.PCDown);

			p.SetTag(0, 0, new TemporaryCodeFieldTag());

			Assert.AreEqual(true, p.HasActiveTag(typeof(TemporaryCodeFieldTag)));
		}

		[TestMethod]
		public void CodePieceTest_normalize()
		{
			CodePiece p = new CodePiece();

			p[-1, -1] = BCHelper.PCDown;
			p[+1, +1] = BCHelper.PCDown;

			Assert.AreEqual(-1, p.MinX);
			Assert.AreEqual(-1, p.MinY);
			Assert.AreEqual(2, p.MaxX);
			Assert.AreEqual(2, p.MaxY);

			p.NormalizeX();

			Assert.AreEqual(0, p.MinX);
			Assert.AreEqual(-1, p.MinY);
			Assert.AreEqual(3, p.MaxX);
			Assert.AreEqual(2, p.MaxY);

			p.NormalizeY();

			Assert.AreEqual(0, p.MinX);
			Assert.AreEqual(0, p.MinY);
			Assert.AreEqual(3, p.MaxX);
			Assert.AreEqual(3, p.MaxY);
		}

		[TestMethod]
		public void CodePieceTest_CombineHorizontal()
		{
			CodePiece p = CodePiece.CombineHorizontal(new CodePiece(BCHelper.Add), new CodePiece(BCHelper.Sub));

			Assert.AreEqual("+-", p.ToSimpleString());
		}

		[TestMethod]
		public void CodePieceTest_CompressHorizontal()
		{
			ASTObject.CGO.CompressHorizontalCombining = true;

			CodePiece p1 = new CodePiece();

			p1[0, 0] = BCHelper.Add;
			p1[1, 1] = BCHelper.Add;

			CodePiece p2 = new CodePiece();

			p2[0, 0] = BCHelper.Sub;
			p2[1, 1] = BCHelper.Sub;

			CodePiece p = CodePiece.CombineHorizontal(p1, p2);

			Assert.AreEqual(3, p.Width);
			Assert.AreEqual(2, p.Height);
		}

		[TestMethod]
		public void CodePieceTest_Fill()
		{
			CodePiece p = new CodePiece();

			p.Fill(0, 0, 10, 10, BCHelper.Walkway);

			Assert.AreEqual(10, p.Width);
			Assert.AreEqual(10, p.Height);
		}

		[TestMethod]
		public void CodePieceTest_IsFlat()
		{
			CodePiece p = CodePiece.ParseFromLine("13373");

			Assert.AreEqual(true, p.IsHFlat());
			Assert.AreEqual(false, p.IsVFlat());
		}

		[TestMethod]
		public void CodePieceTest_IsSingle()
		{
			CodePiece p = CodePiece.ParseFromLine("13373");

			Assert.AreEqual(true, p.FirstColumnIsSingle());
			Assert.AreEqual(false, p.FirstRowIsSingle());
		}

		[TestMethod]
		public void CodePieceTest_getColumnCommandCount()
		{
			CodePiece p = CodePiece.ParseFromLine("123456");

			Assert.AreEqual(6, p.GetRowCommandCount(0));
		}

		[TestMethod]
		public void CodePieceTest_TrimDoubleStringMode()
		{
			CodePiece p = CodePiece.CombineHorizontal(CodePiece.ParseFromLine("\"111\""), CodePiece.ParseFromLine("\"000\""));

			Assert.AreEqual(10, p.Width);

			p.TrimDoubleStringMode();

			Assert.AreEqual(8, p.Width);
		}

		[TestMethod]
		public void CodePieceTest_RemoveRow()
		{
			CodePiece p = CodePiece.ParseFromLine("123456");

			p.RemoveRow(0);

			Assert.AreEqual(0, p.Size);
		}

		[TestMethod]
		public void CodePieceTest_RemoveColum()
		{
			CodePiece p = CodePiece.ParseFromLine("123456");

			Assert.AreEqual(6, p.Width);

			p.RemoveColumn(0);

			Assert.AreEqual(5, p.Width);
		}

		[TestMethod]
		public void CodePieceTest_reverseX()
		{
			CodePiece p1 = CodePiece.ParseFromLine("123**");
			CodePiece p2 = CodePiece.ParseFromLine(">123**<");

			p1.ReverseX(false);
			p2.ReverseX(true);

			Assert.AreEqual("**321", p1.ToSimpleString());
			Assert.AreEqual(">**321<", p2.ToSimpleString());
		}

		[TestMethod]
		public void CodePieceTest_Clear()
		{
			CodePiece p = CodePiece.ParseFromLine("123**");

			p.Clear();

			Assert.AreEqual(0, p.Size);
		}

		[TestMethod]
		public void CodePieceTest_AddOffset()
		{
			CodePiece p = new CodePiece(BCHelper.Add);

			p.AddOffset(10, 10);

			Assert.AreEqual(BefungeCommandType.Add, p[10, 10].Type);
		}
	}
}
