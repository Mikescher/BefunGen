using BefunGen.AST.CodeGen;
using BefunGen.AST.Exceptions;
using System;
using System.Collections.Generic;

namespace BefunGen.AST
{
	/// <summary>
	/// These Lists are only temporary on AST-Creation - They should NEVER appear in the resulting AST
	/// </summary>
	public abstract class AstList : ASTObject
	{
		public AstList(SourceCodePosition pos)
			: base(pos)
		{
		}

		public override string GetDebugString()
		{
			throw new AccessTemporaryASTObjectException(Position);
		}
	}

	#region Lists

	public class ListExpressions : AstList
	{
		public List<Expression> List = new List<Expression>();

		public ListExpressions(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListExpressions(SourceCodePosition pos, Expression e)
			: base(pos)
		{
			List.Add(e);
		}

		public ListExpressions Append(Expression e)
		{
			List.Add(e);
			return this;
		}
	}

	public class ListStatements : AstList
	{
		public List<Statement> List = new List<Statement>();

		public ListStatements(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListStatements(SourceCodePosition pos, Statement s)
			: base(pos)
		{
			List.Add(s);
		}

		public ListStatements Append(Statement s)
		{
			List.Add(s);
			return this;
		}
	}

	public class ListVarDeclarations : AstList
	{
		public List<VarDeclaration> List = new List<VarDeclaration>();

		public ListVarDeclarations(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListVarDeclarations(SourceCodePosition pos, VarDeclaration d)
			: base(pos)
		{
			List.Add(d);
		}

		public ListVarDeclarations Append(VarDeclaration d)
		{
			List.Add(d);
			return this;
		}

		public ListVarDeclarations Append(ListVarDeclarations d)
		{
			List.AddRange(d.List);
			return this;
		}
	}

	public class ListMethods : AstList
	{
		public List<Method> List = new List<Method>();

		public ListMethods(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListMethods(SourceCodePosition pos, Method d)
			: base(pos)
		{
			List.Add(d);
		}

		public ListMethods Append(Method d)
		{
			List.Add(d);
			return this;
		}
	}

	public class ListSwitchs : AstList
	{
		public List<SwitchCase> List = new List<SwitchCase>();

		public ListSwitchs(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListSwitchs(SourceCodePosition pos, LiteralValue l, Statement s)
			: base(pos)
		{
			List.Add(new SwitchCase(l, s));
		}

		public ListSwitchs Append(LiteralValue l, Statement s)
		{
			List.Add(new SwitchCase(l, s));
			return this;
		}

		public ListSwitchs Prepend(LiteralValue l, Statement s)
		{
			List.Insert(0, new SwitchCase(l, s));
			return this;
		}
	}

	public class ListOutfElements : AstList
	{
		public class OutfUnion
		{
			public readonly LiteralCharArr String;
			public readonly Expression Expr;

			public bool IsString { get { return String != null; } }
			public bool IsExpression { get { return Expr != null; } }

			public OutfUnion(LiteralCharArr v)
			{
				String = v;
				Expr = null;
			}

			public OutfUnion(Expression v)
			{
				String = null;
				Expr = v;
			}

			public Statement CreateStatement()
			{
				if (IsString)
				{
					return new StatementOutCharArrLiteral(String.Position, String);
				}
				else if (IsExpression)
				{
					return new StatementOut(Expr.Position, Expr);
				}
				else
				{
					throw new InternalCodeGenException();
				}
			}
		}

		public List<OutfUnion> List = new List<OutfUnion>();

		public ListOutfElements(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListOutfElements(SourceCodePosition pos, Expression v)
			: base(pos)
		{
			List.Add(new OutfUnion(v));
		}

		public ListOutfElements(SourceCodePosition pos, LiteralCharArr v)
			: base(pos)
		{
			List.Add(new OutfUnion(v));
		}

		public ListOutfElements Append(Expression v)
		{
			List.Add(new OutfUnion(v));
			return this;
		}

		public ListOutfElements Append(LiteralCharArr v)
		{
			List.Add(new OutfUnion(v));
			return this;
		}

		public ListOutfElements Prepend(Expression v)
		{
			List.Insert(0, new OutfUnion(v));
			return this;
		}

		public ListOutfElements Prepend(LiteralCharArr v)
		{
			List.Insert(0, new OutfUnion(v));
			return this;
		}
	}

	public class ListIdentifier : AstList
	{
		public List<String> List = new List<String>();

		public ListIdentifier(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListIdentifier(SourceCodePosition pos, String e)
			: base(pos)
		{
			List.Add(e);
		}

		public ListIdentifier Append(String e)
		{
			List.Add(e);
			return this;
		}
	}

	#endregion Lists

	#region Literals Lists

	public class ListLiteralDigits : AstList
	{
		public List<LiteralDigit> List = new List<LiteralDigit>();

		public ListLiteralDigits(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListLiteralDigits(SourceCodePosition pos, LiteralDigit e)
			: base(pos)
		{
			List.Add(e);
		}

		public ListLiteralDigits Append(LiteralDigit e)
		{
			List.Add(e);
			return this;
		}
	}

	public class ListLiteralInts : AstList
	{
		public List<LiteralInt> List = new List<LiteralInt>();

		public ListLiteralInts(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListLiteralInts(SourceCodePosition pos, LiteralInt e)
			: base(pos)
		{
			List.Add(e);
		}

		public ListLiteralInts Append(LiteralInt e)
		{
			List.Add(e);
			return this;
		}
	}

	public class ListLiteralChars : AstList
	{
		public List<LiteralChar> List = new List<LiteralChar>();

		public ListLiteralChars(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListLiteralChars(SourceCodePosition pos, LiteralChar e)
			: base(pos)
		{
			List.Add(e);
		}

		public ListLiteralChars Append(LiteralChar e)
		{
			List.Add(e);
			return this;
		}
	}

	public class ListLiteralBools : AstList
	{
		public List<LiteralBool> List = new List<LiteralBool>();

		public ListLiteralBools(SourceCodePosition pos)
			: base(pos)
		{
		}

		public ListLiteralBools(SourceCodePosition pos, LiteralBool e)
			: base(pos)
		{
			List.Add(e);
		}

		public ListLiteralBools Append(LiteralBool e)
		{
			List.Add(e);
			return this;
		}
	}

	#endregion Literals Lists

	#region Helper

	public class SwitchCase
	{
		public LiteralValue Value;
		public Statement Body;

		public SwitchCase(LiteralValue v, Statement s)
		{
			Value = v;
			Body = s;
		}
	}

	#endregion
}