﻿using BefunGen.AST.CodeGen.Tags;
using BefunGen.AST.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BefunGen.AST.CodeGen
{
	public class CodePiece
	{
		#region Properties

		public int MinX { get; private set; } // Minimal ::> Inclusive

		public int MinY { get; private set; }

		public int MaxX { get; private set; } // Maximal + 1 ::> Exclusive

		public int MaxY { get; private set; }

		public int Width { get { return MaxX - MinX; } }

		public int Height { get { return MaxY - MinY; } }

		public int Size { get { return Width * Height; } }

		private HashSet<CodeTag> tagCache = new HashSet<CodeTag>();

		private List<List<BefungeCommand>> commandArr = new List<List<BefungeCommand>>();

		public BefungeCommand this[int x, int y] { get { return get(x, y); } set { set(x, y, value); } }

		#endregion

		#region Construct

		public CodePiece(BefungeCommand cmd)
			: this()
		{
			this[0, 0] = cmd;
		}

		public CodePiece()
		{
			MinX = 0;
			MinY = 0;

			MaxX = 0;
			MaxY = 0;
		}

		//public static CodePiece ParseFromLineFormatted(string l, bool interpretSpaceAsWalkway, params CodePiece[] cpparams)
		//{
		//	return CodePiece.ParseFromLine(string.Format(l, cpparams.Select(p => p.ToSimpleString()).ToArray()), interpretSpaceAsWalkway);
		//}

		public static CodePiece ParseFromLine(string l, bool interpretSpaceAsWalkway = false, bool interpretAtAsNOP = false)
		{
			CodePiece p = new CodePiece();

			for (int i = 0; i < l.Length; i++)
			{
				char c = l[i];

				BefungeCommand cmd;

				if (c == ' ')
				{
					if (interpretSpaceAsWalkway)
					{
						cmd = BCHelper.Walkway;
					}
					else
					{
						throw new InternalCodeGenException(); // Space is undefinied: NOP <> Walkway
					}
				}
				else if (c == '@')
				{
					if (interpretAtAsNOP)
					{
						cmd = BCHelper.Unused;
					}
					else
					{
						cmd = BCHelper.FindCommand(c);
					}
				}
				else
				{
					cmd = BCHelper.FindCommand(c);
				}

				p[i, 0] = cmd;
			}

			return p;
		}

		public static CodePiece CreateFromVerticalList(List<CodePiece> cplist)
		{
			CodePiece p = new CodePiece();

			foreach (var cp in cplist)
				p.AppendBottom(cp);

			return p;
		}

		#endregion

		#region Internal

		private bool IsIncluded(int x, int y)
		{
			return x >= MinX && y >= MinY && x < MaxX && y < MaxY;
		}

		private bool expand(int x, int y)
		{
			bool ex = expandX(x);
			bool ey = expandY(y);

			return ex && ey;
		}

		private bool expandX(int x)
		{
			if (x >= MaxX) // expand Right
			{
				int newMaxX = x + 1;

				while (MaxX < newMaxX)
				{
					commandArr.Add(Enumerable.Repeat(BCHelper.Unused, Height).ToList());

					MaxX++;
				}

				return true;
			}
			else if (x < MinX)
			{
				int newMinX = x;

				while (MinX > newMinX)
				{
					commandArr.Insert(0, Enumerable.Repeat(BCHelper.Unused, Height).ToList());

					MinX--;
				}
			}

			return false;
		}

		private bool expandY(int y)
		{
			if (y >= MaxY) // expand Right
			{
				int newMaxY = y + 1;

				while (MaxY < newMaxY)
				{
					for (int xw = 0; xw < Width; xw++)
					{
						commandArr[xw].Add(BCHelper.Unused);
					}

					MaxY++;
				}

				return true;
			}
			else if (y < MinY)
			{
				int newMinY = y;

				while (MinY > newMinY)
				{
					for (int xw = 0; xw < Width; xw++)
					{
						commandArr[xw].Insert(0, BCHelper.Unused);
					}

					MinY--;
				}
			}

			return false;
		}

		private void set(int x, int y, BefungeCommand value)
		{
			if (!IsIncluded(x, y))
				expand(x, y);

			if (commandArr[x - MinX][y - MinY].Type != BefungeCommandType.NOP)
				throw new InvalidCodeManipulationException("Modification of CodePiece : " + x + "|" + y);

			if (hasTag(value.Tag))
				throw new InvalidCodeManipulationException(string.Format("Duplicate Tag in CodePiece : [{0},{1}] = '{2}' = [{3},{4}])", x, y, value.Tag.ToString(), findTag(value.Tag).X, findTag(value.Tag).Y));

			commandArr[x - MinX][y - MinY] = value;

			if (value.hasTag())
				tagCache.Add(value.Tag);
		}

		private void forceSet(int x, int y, BefungeCommand value) // Suppresses CodeModificationException
		{
			if (!IsIncluded(x, y))
				expand(x, y);

			BefungeCommand prev = commandArr[x - MinX][y - MinY];

			if (prev.hasTag())
				tagCache.Remove(prev.Tag);

			commandArr[x - MinX][y - MinY] = value;

			if (value.hasTag())
				tagCache.Add(value.Tag);
		}

		private BefungeCommand get(int x, int y)
		{
			if (IsIncluded(x, y))
				return commandArr[x - MinX][y - MinY];
			else
				return BCHelper.Unused;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Format("{0}: [{1} - {2}, {3} - {4}] ({5}, {6})", this.GetType().Name, MinX, MaxX, MinY, MaxY, Width, Height));

			builder.AppendLine("{");
			for (int y = MinY; y < MaxY; y++)
			{
				for (int x = MinX; x < MaxX; x++)
				{
					BefungeCommand bc = this[x, y];
					builder.Append(bc.getCommandCode());
				}
				builder.AppendLine();
			}
			builder.AppendLine("}");

			List<TagLocation> tags = findAllTags();
			foreach (TagLocation tag in tags)
				builder.AppendFormat("[ ({0:000}|{1:000}):'{2}' := {3} ]{4}", tag.X, tag.Y, tag.Command.getCommandCode(), tag.Tag, Environment.NewLine);

			return builder.ToString();
		}

		public string ToSimpleString()
		{
			StringBuilder builder = new StringBuilder();
			for (int y = MinY; y < MaxY; y++)
			{
				for (int x = MinX; x < MaxX; x++)
				{
					BefungeCommand bc = this[x, y];
					builder.Append(bc.getCommandCode());
				}
				if (y < MaxY - 1)
					builder.AppendLine();
			}
			return builder.ToString();
		}

		#endregion

		#region Tags

		public TagLocation findTag(CodeTag tag)
		{
			if (!tagCache.Contains(tag))
				return null;

			for (int x = MinX; x < MaxX; x++)
			{
				for (int y = MinY; y < MaxY; y++)
				{
					if (this[x, y].Tag == tag)
						return new TagLocation(x, y, this[x, y]);
				}
			}

			return null;
		}

		public bool hasTag(CodeTag tag)
		{
			return tag != null && tagCache.Contains(tag);
		}

		public bool hasActiveTag(params Type[] filter)
		{
			return findAllActiveCodeTags(filter).Count > 0;
		}

		public List<TagLocation> findAllTags(params Type[] filter)
		{
			bool all = filter.Length == 0;

			List<TagLocation> tl = new List<TagLocation>();

			for (int y = MinY; y < MaxY; y++)
			{
				for (int x = MinX; x < MaxX; x++)
				{
					if (this[x, y].hasTag() && (all || filter.Any(f => (this[x, y].Tag.GetType().IsAssignableFrom(f)))))
						tl.Add(new TagLocation(x, y, this[x, y]));
				}
			}

			return tl;
		}

		public List<TagLocation> findAllActiveCodeTags(params Type[] filter)
		{
			return findAllTags(filter).Where(p => p.Tag.isActive()).ToList();
		}

		public TagLocation findTagSingle(Type t)
		{
			return findAllTags(t).Single();
		}

		public void SetTag(int x, int y, CodeTag tag, bool force = false)
		{
			BefungeCommand cmd = this[x, y];

			if (cmd.hasTag() && !force)
				throw new InvalidCodeManipulationException("Tryed to remove existing Tag: " + cmd.Tag + " with: " + tag);

			BefungeCommand newcmd = new BefungeCommand(cmd.Type, cmd.Param, tag);

			forceSet(x, y, newcmd);
		}



		private void recalcTagCache()
		{
			tagCache.Clear();

			findAllTags(typeof(CodeTag)).ForEach(p => tagCache.Add(p.Tag));
		}

		#endregion

		#region Normalize

		public void normalize()
		{
			normalizeX();
			normalizeY();
		}

		public void normalizeY()
		{
			int oy = -MinY;
			MinY += oy;
			MaxY += oy;
		}

		public void normalizeX()
		{
			int ox = -MinX;
			MinX += ox;
			MaxX += ox;
		}

		#endregion

		#region Copy

		public CodePiece copy()
		{
			CodePiece result = new CodePiece();
			for (int x = 0; x < commandArr.Count; x++)
			{
				for (int y = 0; y < commandArr[x].Count; y++)
				{
					result[x, y] = commandArr[x][y];
				}
			}

			result.MinX = MinX;
			result.MinY = MinY;

			result.MaxX = MaxX;
			result.MaxY = MaxY;

			return result;
		}

		public CodePiece copyNormalized()
		{
			CodePiece result = copy();
			result.normalize();
			return result;
		}

		#endregion

		#region Combine

		public static CodePiece CombineHorizontal(CodePiece first, params CodePiece[] other)
		{
			CodePiece p = first.copy();

			foreach (CodePiece p_o in other)
			{
				p.AppendRight(p_o);
			}

			return p;
		}

		public static CodePiece CombineHorizontal(CodePiece left, CodePiece right)
		{
			CodePiece c_l = left.copy();
			CodePiece c_r = right.copy();

			c_l.normalizeX();
			c_r.normalizeX();

			c_l.AppendRight(c_r);

			return c_l;
		}

		public static CodePiece CombineVertical(CodePiece top, CodePiece bottom)
		{
			CodePiece c_t = top.copy();
			CodePiece c_b = bottom.copy();

			c_t.normalizeY();
			c_b.normalizeY();

			c_t.AppendBottom(c_b);

			return c_t;
		}

		#endregion

		#region Setter

		public void CreateRowWW(int y, int x1, int x2)
		{
			CreateWW(x1, y, x2, y + 1);
		}

		public void CreateColWW(int x, int y1, int y2)
		{
			CreateWW(x, y1, x + 1, y2);
		}

		public void CreateWW(int x1, int y1, int x2, int y2)
		{
			Fill(x1, y1, x2, y2, BCHelper.Walkway, null, true);
		}

		public void FillRowWW(int y, int x1, int x2)
		{
			FillWW(x1, y, x2, y + 1);
		}

		public void FillColWW(int x, int y1, int y2)
		{
			FillWW(x, y1, x + 1, y2);
		}

		public void FillWW(int x1, int y1, int x2, int y2)
		{
			Fill(x1, y1, x2, y2, BCHelper.Walkway);
		}

		// x1, y1 included -- x2, y2 excluded
		public void Fill(int x1, int y1, int x2, int y2, BefungeCommand c, CodeTag topleft_tag = null, bool skipExactCopies = false)
		{
			if (x1 >= x2)
				return;
			if (y1 >= y2)
				return;

			for (int x = x1; x < x2; x++)
				for (int y = y1; y < y2; y++)
				{
					if (skipExactCopies && c.EqualsTagLess(this[x, y]))
					{
						// Do nothing - skiped
					}
					else if (x == x1 && y == y1 && topleft_tag != null)
					{
						this[x, y] = c.copyWithTag(topleft_tag);
					}
					else
					{
						this[x, y] = c;
					}
				}

		}

		public void SetAt(int paramX, int paramY, CodePiece lit, bool skipNop = false)
		{
			for (int x = lit.MinX; x < lit.MaxX; x++)
			{
				for (int y = lit.MinY; y < lit.MaxY; y++)
				{
					if (skipNop && lit[x, y].Type == BefungeCommandType.NOP)
						continue;

					this[x + paramX, y + paramY] = lit[x, y];
				}
			}
		}

		public void setText(int x, int y, string text)
		{
			for (int i = 0; i < text.Length; i++)
			{
				this[x + i, y] = BCHelper.chr(text[i]);
			}
		}

		public void replaceWalkway(int x, int y, BefungeCommand cmd, bool deleteTags)
		{
			if (this[x, y].EqualsTagLess(BCHelper.Walkway) || (deleteTags && this[x, y].Type == BefungeCommandType.Walkway))
			{
				forceSet(x, y, cmd);
			}
			else
			{
				this[x, y] = cmd;
			}
		}

		#endregion

		#region Append

		public void AppendRight(BefungeCommand c)
		{
			AppendRight(0, c);
		}

		public void AppendRight(int row, BefungeCommand c)
		{
			CodePiece p = new CodePiece();
			p[0, row] = c;

			AppendRight(p);
		}

		public void AppendRight(CodePiece right)
		{
			right = right.copy();

			CodePiece compress_conn;
			if (ASTObject.CGO.CompressHorizontalCombining && (compress_conn = doCompressHorizontally(this, right)) != null)
			{
				this.RemoveColumn(this.MaxX - 1);
				right.RemoveColumn(right.MinX);

				this.AppendRightDirect(compress_conn);
			}

			AppendRightDirect(right);
		}

		private void AppendRightDirect(CodePiece right)
		{
			right.normalizeX();

			int offset = MaxX;

			for (int x = right.MinX; x < right.MaxX; x++)
			{
				for (int y = right.MinY; y < right.MaxY; y++)
				{
					this[offset + x, y] = right[x, y];
				}
			}
		}

		public void AppendLeft(BefungeCommand c)
		{
			AppendLeft(0, c);
		}

		public void AppendLeft(int row, BefungeCommand c)
		{
			CodePiece p = new CodePiece();
			p[0, row] = c;

			AppendLeft(p);
		}

		public void AppendLeft(CodePiece left)
		{
			left = left.copy();

			CodePiece compress_conn;
			if (ASTObject.CGO.CompressHorizontalCombining && (compress_conn = doCompressHorizontally(left, this)) != null)
			{
				this.RemoveColumn(this.MinX);
				left.RemoveColumn(left.MaxX - 1);

				this.AppendLeftDirect(compress_conn);
			}

			AppendLeftDirect(left);
		}

		private void AppendLeftDirect(CodePiece left)
		{
			left.normalizeX();

			int offset = MinX - left.MaxX;

			for (int x = left.MinX; x < left.MaxX; x++)
			{
				for (int y = left.MinY; y < left.MaxY; y++)
				{
					this[offset + x, y] = left[x, y];
				}
			}
		}

		public void AppendBottom(BefungeCommand c)
		{
			AppendBottom(0, c);
		}

		public void AppendBottom(int col, BefungeCommand c)
		{
			CodePiece p = new CodePiece();
			p[col, 0] = c;

			AppendBottom(p);
		}


		public void AppendBottom(CodePiece bot)
		{
			bot = bot.copy();

			CodePiece compress_conn;
			if (ASTObject.CGO.CompressVerticalCombining && (compress_conn = doCompressVertically(this, bot)) != null)
			{
				this.RemoveRow(this.MaxY - 1);
				bot.RemoveRow(bot.MinY);

				this.AppendBottomDirect(compress_conn);
			}

			AppendBottomDirect(bot);
		}

		private void AppendBottomDirect(CodePiece bot)
		{
			bot.normalizeY();

			int offset = MaxY;

			for (int x = bot.MinX; x < bot.MaxX; x++)
			{
				for (int y = bot.MinY; y < bot.MaxY; y++)
				{
					this[x, offset + y] = bot[x, y];
				}
			}
		}

		public void AppendTop(BefungeCommand c)
		{
			AppendTop(0, c);
		}

		public void AppendTop(int col, BefungeCommand c)
		{
			CodePiece p = new CodePiece();
			p[col, 0] = c;

			AppendTop(p);
		}

		public void AppendTop(CodePiece top)
		{
			top = top.copy();

			CodePiece compress_conn;
			if (ASTObject.CGO.CompressVerticalCombining && (compress_conn = doCompressVertically(top, this)) != null)
			{
				this.RemoveRow(this.MinY);
				top.RemoveRow(top.MaxY - 1);

				this.AppendTopDirect(compress_conn);
			}

			AppendTopDirect(top);
		}

		private void AppendTopDirect(CodePiece top)
		{
			top.normalizeY();

			int offset = MinY - top.MaxY;

			for (int x = top.MinX; x < top.MaxX; x++)
			{
				for (int y = top.MinY; y < top.MaxY; y++)
				{
					this[x, offset + y] = top[x, y];
				}
			}
		}

		#endregion

		#region Characteristics

		public bool IsHFlat() // Is Horizontal Flat
		{
			return Height == 1;
		}

		public bool IsVFlat() // Is Vertical Flat
		{
			return Width == 1;
		}

		public bool lastColumnIsSingle()
		{
			return IsColumnSingle(Width - 1);
		}

		public bool firstColumnIsSingle()
		{
			return IsColumnSingle(0);
		}

		public bool IsColumnSingle(int r)
		{
			return commandArr[r].Count(p => p.Type != BefungeCommandType.NOP) == 1;
		}

		public int GetColumnCommandCount(int x)
		{
			int cnt = 0;

			for (int y = MinY; y < MaxY; y++)
			{
				if (this[x, y].Type != BefungeCommandType.NOP)
					cnt++;
			}

			return cnt;
		}

		public bool lastRowIsSingle(bool ignoreWalkway = false)
		{
			return IsRowSingle(Height - 1, ignoreWalkway);
		}

		public bool firstRowIsSingle(bool ignoreWalkway = false)
		{
			return IsRowSingle(0, ignoreWalkway);
		}

		public bool IsRowSingle(int r, bool ignoreWalkway = false)
		{
			return GetRowCommandCount(r, ignoreWalkway) == 1;
		}

		public int GetRowCommandCount(int y, bool ignoreWalkway = false)
		{
			int cnt = 0;

			for (int x = MinX; x < MaxX; x++)
			{
				if (this[x, y].Type != BefungeCommandType.NOP && (!ignoreWalkway || this[x, y].Type != BefungeCommandType.Walkway))
					cnt++;
			}

			return cnt;
		}

		#endregion

		#region Optimizing

		public static CodePiece doCompressHorizontally(CodePiece l, CodePiece r)
		{
			if (l.Width == 0 || r.Width == 0)
				return null;

			CodePiece connect = new CodePiece();

			int x_l = l.MaxX - 1;
			int x_r = r.MinX;

			for (int y = Math.Min(l.MinY, r.MinY); y < Math.Max(l.MaxY, r.MaxY); y++)
			{
				CodeTag Tag = null;

				if (l[x_l, y].Tag != null && r[x_r, y].Tag != null)
				{
					return null; // Can't compress - two tags would need to be merged
				}

				Tag = l[x_l, y].Tag ?? r[x_r, y].Tag;

				if (l[x_l, y].Type == BefungeCommandType.NOP && r[x_r, y].Type == BefungeCommandType.NOP)
				{
					connect[0, y] = new BefungeCommand(BefungeCommandType.NOP, Tag);
				}
				else if (l[x_l, y].Type != BefungeCommandType.NOP && r[x_r, y].Type != BefungeCommandType.NOP)
				{
					if (l[x_l, y].Type == r[x_r, y].Type && l[x_l, y].Param == r[x_r, y].Param && l[x_l, y].IsCompressable())
					{
						connect[0, y] = new BefungeCommand(l[x_l, y].Type, l[x_l, y].Param, Tag);
					}
					else
					{
						return null; // Can't compress - two commands are colliding
					}
				}
				else if (l[x_l, y].Type != BefungeCommandType.NOP)
				{
					connect[0, y] = new BefungeCommand(l[x_l, y].Type, l[x_l, y].Param, Tag);
				}
				else if (r[x_r, y].Type != BefungeCommandType.NOP)
				{
					connect[0, y] = new BefungeCommand(r[x_r, y].Type, r[x_r, y].Param, Tag);
				}
				else
				{
					throw new WTFException();
				}
			}

			return connect;
		}

		public static CodePiece doCompressVertically(CodePiece t, CodePiece b)
		{
			if (t.Width == 0 || b.Width == 0)
				return null;

			CodePiece connect = new CodePiece();

			int y_t = t.MaxY - 1;
			int y_b = b.MinY;

			for (int x = Math.Min(t.MinX, b.MinX); x < Math.Max(t.MaxX, b.MaxX); x++)
			{
				CodeTag Tag = null;

				if (t[x, y_t].Tag != null && b[x, y_b].Tag != null)
				{
					return null; // Can't compress - two tags would need to be merged
				}

				Tag = t[x, y_t].Tag ?? b[x, y_b].Tag;

				if (t[x, y_t].Type == BefungeCommandType.NOP && b[x, y_b].Type == BefungeCommandType.NOP)
				{
					connect[x, 0] = new BefungeCommand(BefungeCommandType.NOP, Tag);
				}
				else if (t[x, y_t].Type != BefungeCommandType.NOP && b[x, y_b].Type != BefungeCommandType.NOP)
				{
					if (t[x, y_t].Type == b[x, y_b].Type && t[x, y_t].Param == b[x, y_b].Param && t[x, y_t].IsCompressable())
					{
						connect[x, 0] = new BefungeCommand(t[x, y_t].Type, t[x, y_t].Param, Tag);
					}
					else
					{
						return null; // Can't compress - two commands are colliding
					}
				}
				else if (t[x, y_t].Type != BefungeCommandType.NOP)
				{
					connect[x, 0] = new BefungeCommand(t[x, y_t].Type, t[x, y_t].Param, Tag);
				}
				else if (b[x, y_b].Type != BefungeCommandType.NOP)
				{
					connect[x, 0] = new BefungeCommand(b[x, y_b].Type, b[x, y_b].Param, Tag);
				}
				else
				{
					throw new WTFException();
				}
			}

			return connect;
		}

		public void TrimDoubleStringMode()
		{
			normalize();

			int i = 0;

			while (i < Width - 2)
			{
				if (this[i, 0].EqualsTagLess(this[i + 1, 0]) && this[i, 0].Type == BefungeCommandType.Stringmode)
				{
					if (this.GetColumnCommandCount(i) == 1 && this.GetColumnCommandCount(i + 1) == 1)
					{
						RemoveColumn(i + 1);
						RemoveColumn(i);

						i = 0;
					}
				}
				i++;
			}
		}

		#endregion

		#region Modify

		public void RemoveColumn(int col)
		{
			int abs = col - MinX;

			commandArr.RemoveAt(abs);

			MaxX = MaxX - 1;

			recalcTagCache();
		}

		public void RemoveRow(int row)
		{
			int abs = row - MinY;

			for (int i = 0; i < Width; i++)
			{
				commandArr[i].RemoveAt(abs);
			}

			MaxY = MaxY - 1;

			recalcTagCache();
		}

		public void reverseX(bool nonpedantic)
		{
			CodePiece p = this.copy();

			this.Clear();

			for (int x = p.MinX; x < p.MaxX; x++)
			{
				for (int y = p.MinY; y < p.MaxY; y++)
				{
					if (!p[x, y].IsXDeltaIndependent())
					{
						if (nonpedantic && p[x, y].Type == BefungeCommandType.PC_Left)
						{
							this[-x, y] = BCHelper.PC_Right;
						}
						else if (nonpedantic && p[x, y].Type == BefungeCommandType.PC_Right)
						{
							this[-x, y] = BCHelper.PC_Left;
						}
						else
						{
							throw new CodePieceReverseException(p);
						}
					}
					else
					{
						this[-x, y] = p[x, y];
					}
				}
			}

			this.normalizeX();
		}

		public void Clear()
		{
			commandArr.Clear();

			MinX = 0;
			MinY = 0;

			MaxX = 0;
			MaxY = 0;

			recalcTagCache();
		}

		public void AddXOffset(int ox)
		{
			AddOffset(ox, 0);
		}

		public void AddYOffset(int oy)
		{
			AddOffset(0, oy);
		}

		public void AddOffset(int ox, int oy)
		{
			MinY += oy;
			MaxY += oy;

			MinX += ox;
			MaxX += ox;
		}

		public void forceNonEmpty(BefungeCommand cmd)
		{
			if (Size == 0)
				this[0, 0] = cmd;
		}

		public void ExtendWithWalkwayLeft(int maxlen)
		{
			while (Width < maxlen)
				AppendLeftDirect(new CodePiece(BCHelper.Walkway));

		}

		public void ExtendWithWalkwayRight(int maxlen)
		{
			while (Width < maxlen)
				AppendRightDirect(new CodePiece(BCHelper.Walkway));

		}

		#endregion
	}
}