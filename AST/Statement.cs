﻿using BefunGen.AST.CodeGen;
using BefunGen.AST.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BefunGen.AST
{
	public abstract class Statement : ASTObject
	{
		public Statement(SourceCodePosition pos)
			: base(pos)
		{
			//--
		}

		public abstract void linkVariables(Method owner);
		public abstract void linkResultTypes(Method owner);
		public abstract void linkMethods(Program owner);

		public abstract Statement_Label findLabelByIdentifier(string ident);

		public abstract CodePiece generateCode(bool reverse);
	}

	#region Other

	public class Statement_StatementList : Statement
	{
		public List<Statement> List;

		public Statement_StatementList(SourceCodePosition pos, List<Statement> sl)
			: base(pos)
		{
			List = sl.ToList();
		}

		public override string getDebugString()
		{
			return string.Format("#StatementList\n[\n{0}\n]", indent(getDebugStringForList(List)));
		}

		public override void linkVariables(Method owner)
		{
			foreach (Statement s in List)
				s.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			foreach (Statement s in List)
			{
				s.linkMethods(owner);
			}
		}

		public override void linkResultTypes(Method owner)
		{
			foreach (Statement s in List)
				s.linkResultTypes(owner);
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			Statement_Label result = null;

			foreach (Statement s in List)
			{
				Statement_Label found = s.findLabelByIdentifier(ident);
				if (found != null && result != null)
					return null;
				if (found != null && result == null)
					result = found;
			}

			return result;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_MethodCall : Statement
	{
		public List<Expression> CallParameter;

		public string Identifier; // Temporary -- before linking;
		public Method Target;

		public Statement_MethodCall(SourceCodePosition pos, string id)
			: base(pos)
		{
			this.Identifier = id;
			this.CallParameter = new List<Expression>();
		}

		public Statement_MethodCall(SourceCodePosition pos, string id, List<Expression> cp)
			: base(pos)
		{
			this.Identifier = id;
			this.CallParameter = cp.ToList();
		}

		public override string getDebugString()
		{
			return string.Format("#MethodCall {{{0}}} --> #Parameter: ({1})", Target.ID, indent(getDebugCommaStringForList(CallParameter)));
		}

		public override void linkVariables(Method owner)
		{
			foreach (Expression e in CallParameter)
				e.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			Target = owner.findMethodByIdentifier(Identifier) as Method;

			if (Target == null)
				throw new UnresolvableReferenceException(Identifier, Position);

			Identifier = null;
		}

		public override void linkResultTypes(Method owner)
		{
			foreach (Expression e in CallParameter)
				e.linkResultTypes(owner);

			if (CallParameter.Count != Target.Parameter.Count)
				throw new WrongParameterCountException(CallParameter.Count, Target.Parameter.Count, Position);

			for (int i = 0; i < CallParameter.Count; i++)
			{
				BType present = CallParameter[i].getResultType();
				BType expected = Target.Parameter[i].Type;

				if (present != expected)
				{
					if (present.isImplicitCastableTo(expected))
						CallParameter[i] = new Expression_Cast(CallParameter[i].Position, expected, CallParameter[i]);
					else
						throw new ImplicitCastException(present, expected, CallParameter[i].Position);
				}
			}
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	#endregion

	#region Keywords

	public class Statement_Label : Statement
	{
		private static int _L_ID_COUNTER = 100;
		protected static int L_ID_COUNTER { get { return _L_ID_COUNTER++; } }

		public string Identifier;
		public readonly int ID;

		public Statement_Label(SourceCodePosition pos, string ident)
			: base(pos)
		{
			this.Identifier = ident;
			ID = L_ID_COUNTER;
		}

		public static void resetCounter()
		{
			_L_ID_COUNTER = 1;
		}

		public override string getDebugString()
		{
			return string.Format("#LABEL: {{{0}}}", ID);
		}

		public override void linkVariables(Method owner)
		{
			//NOP
		}

		public override void linkMethods(Program owner)
		{
			//NOP
		}

		public override void linkResultTypes(Method owner)
		{
			//NOP
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return ident.ToLower() == Identifier.ToLower() ? this : null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_Goto : Statement
	{
		public string TargetIdentifier;
		public Statement_Label Target;

		public Statement_Goto(SourceCodePosition pos, string id)
			: base(pos)
		{
			this.TargetIdentifier = id;
		}

		public override string getDebugString()
		{
			return string.Format("#GOTO: {{{0}}}", Target.ID);
		}

		public override void linkVariables(Method owner)
		{
			Target = owner.findLabelByIdentifier(TargetIdentifier);
			if (Target == null)
				throw new UnresolvableReferenceException(TargetIdentifier, Position);
		}

		public override void linkMethods(Program owner)
		{
			//NOP
		}

		public override void linkResultTypes(Method owner)
		{
			//NOP
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_Return : Statement
	{
		public Expression Value;

		public Statement_Return(SourceCodePosition pos)
			: base(pos)
		{
			this.Value = new Expression_VoidValuePointer(pos);
		}

		public Statement_Return(SourceCodePosition pos, Expression v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string getDebugString()
		{
			return string.Format("#RETURN: {0}", Value.getDebugString());
		}

		public override void linkVariables(Method owner)
		{
			Value.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			Value.linkMethods(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			Value.linkResultTypes(owner);

			BType present = Value.getResultType();
			BType expected = owner.ResultType;

			if (present != expected)
			{
				if (present.isImplicitCastableTo(expected))
					Value = new Expression_Cast(Value.Position, expected, Value);
				else
					throw new ImplicitCastException(present, expected, Value.Position);
			}
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_Out : Statement
	{
		public Expression Value;

		public Statement_Out(SourceCodePosition pos, Expression v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string getDebugString()
		{
			return string.Format("#OUT {0}", Value.getDebugString());
		}

		public override void linkVariables(Method owner)
		{
			Value.linkVariables(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			Value.linkResultTypes(owner);

			if (Value.getResultType() is BType_Array)
				throw new ImplicitCastException(new BType_Int(Position), Value.getResultType(), Value.Position);
			if (Value.getResultType() is BType_Bool)
				throw new ImplicitCastException(new BType_Int(Position), Value.getResultType(), Value.Position);
		}

		public override void linkMethods(Program owner)
		{
			Value.linkMethods(owner);
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reversed)
		{
			CodePiece p = Value.generateCode(reversed);

			BefungeCommand cmd_out;

			if (Value.getResultType() is BType_Char)
				cmd_out = BCHelper.Out_ASCII;
			else
				cmd_out = BCHelper.Out_Int;

			if (reversed)
				p.AppendLeft(cmd_out);
			else
				p.AppendRight(cmd_out);

			p.normalizeX();

			return p;
		}
	}

	public class Statement_Out_CharArrLiteral : Statement
	{
		public Literal_CharArr Value;

		public Statement_Out_CharArrLiteral(SourceCodePosition pos, Literal_CharArr v)
			: base(pos)
		{
			this.Value = v;
		}

		public override string getDebugString()
		{
			return string.Format("#OUT {0}", Value.getDebugString());
		}

		public override void linkVariables(Method owner)
		{
			//NOP
		}

		public override void linkResultTypes(Method owner)
		{
			//NOP
		}

		public override void linkMethods(Program owner)
		{
			//NOP
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reversed)
		{
			if (reversed)
			{
				// $_ #! #: #,<"???"0
				CodePiece p = new CodePiece();

				p.AppendLeft(BCHelper.Digit_0);

				p.AppendLeft(Value.generateCode(!reversed)); // <NOT reversed> so string is in reversed order on stack

				p.AppendLeft(BCHelper.PC_Left);
				p.AppendLeft(BCHelper.Out_ASCII);
				p.AppendLeft(BCHelper.PC_Jump);
				p.AppendLeft(BCHelper.Walkway);
				p.AppendLeft(BCHelper.Stack_Dup);
				p.AppendLeft(BCHelper.PC_Jump);
				p.AppendLeft(BCHelper.Walkway);
				p.AppendLeft(BCHelper.Not);
				p.AppendLeft(BCHelper.PC_Jump);
				p.AppendLeft(BCHelper.Walkway);
				p.AppendLeft(BCHelper.If_Horizontal);
				p.AppendLeft(BCHelper.Stack_Pop);

				p.normalizeX();

				return p;
			}
			else
			{
				// 0"???">,# :# _$
				CodePiece p = new CodePiece();

				p.AppendRight(BCHelper.Digit_0);

				p.AppendRight(Value.generateCode(!reversed)); // <NOT reversed> so string is in reversed order on stack

				p.AppendRight(BCHelper.PC_Right);
				p.AppendRight(BCHelper.Out_ASCII);
				p.AppendRight(BCHelper.PC_Jump);
				p.AppendRight(BCHelper.Walkway);
				p.AppendRight(BCHelper.Stack_Dup);
				p.AppendRight(BCHelper.PC_Jump);
				p.AppendRight(BCHelper.Walkway);
				p.AppendRight(BCHelper.If_Horizontal);
				p.AppendRight(BCHelper.Stack_Pop);

				p.normalizeX();

				return p;
			}
		}
	}

	public class Statement_In : Statement
	{
		public Expression_ValuePointer ValueTarget;

		public Statement_In(SourceCodePosition pos, Expression_ValuePointer vt)
			: base(pos)
		{
			this.ValueTarget = vt;
		}

		public override string getDebugString()
		{
			return string.Format("#IN {0}", ValueTarget.getDebugString());
		}

		public override void linkVariables(Method owner)
		{
			ValueTarget.linkVariables(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			ValueTarget.linkResultTypes(owner);

			BType present = ValueTarget.getResultType();
			BType expected = new BType_Char(null);

			if (present != expected)
			{
				throw new WrongTypeException(present, expected, ValueTarget.Position);
			}
		}

		public override void linkMethods(Program owner)
		{
			ValueTarget.linkMethods(owner);
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_Quit : Statement
	{
		public Statement_Quit(SourceCodePosition pos)
			: base(pos)
		{
		}

		public override string getDebugString()
		{
			return "#QUIT";
		}

		public override void linkVariables(Method owner)
		{
			//NOP
		}

		public override void linkResultTypes(Method owner)
		{
			//NOP
		}

		public override void linkMethods(Program owner)
		{
			//NOP
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			CodePiece p = new CodePiece();

			p[0, 0] = BCHelper.Stop;

			return p;
		}
	}

	public class Statement_NOP : Statement // NO OPERATION
	{
		public Statement_NOP(SourceCodePosition pos)
			: base(pos)
		{
		}

		public override string getDebugString()
		{
			return "#NOP";
		}

		public override void linkVariables(Method owner)
		{
			//NOP
		}

		public override void linkResultTypes(Method owner)
		{
			//NOP
		}

		public override void linkMethods(Program owner)
		{
			//NOP
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			return new CodePiece(); // easy as that ¯\_(ツ)_/¯
		}
	}

	#endregion Keywords

	#region Operations

	public class Statement_Inc : Statement
	{
		public Expression_ValuePointer Target;

		public Statement_Inc(SourceCodePosition pos, Expression_ValuePointer id)
			: base(pos)
		{
			this.Target = id;
		}

		public override string getDebugString()
		{
			return string.Format("#INC {0}", Target.getDebugString());
		}

		public override void linkVariables(Method owner)
		{
			Target.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			Target.linkMethods(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			Target.linkResultTypes(owner);

			BType present = Target.getResultType();

			if (!(present == new BType_Int(null) || present == new BType_Digit(null) || present == new BType_Char(null)))
			{
				throw new WrongTypeException(present, new List<BType>() { new BType_Int(null), new BType_Digit(null), new BType_Char(null) }, Target.Position);
			}
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_Dec : Statement
	{
		public Expression_ValuePointer Target;

		public Statement_Dec(SourceCodePosition pos, Expression_ValuePointer id)
			: base(pos)
		{
			this.Target = id;
		}

		public override string getDebugString()
		{
			return string.Format("#DEC {0}", Target.getDebugString());
		}

		public override void linkVariables(Method owner)
		{
			Target.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			Target.linkMethods(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			Target.linkResultTypes(owner);

			BType present = Target.getResultType();

			if (!(present == new BType_Int(null) || present == new BType_Digit(null) || present == new BType_Char(null)))
			{
				throw new WrongTypeException(present, new List<BType>() { new BType_Int(null), new BType_Digit(null), new BType_Char(null) }, Target.Position);
			}
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_Assignment : Statement
	{
		public Expression_ValuePointer Target;
		public Expression Expr;

		public Statement_Assignment(SourceCodePosition pos, Expression_ValuePointer t, Expression e)
			: base(pos)
		{
			this.Target = t;
			this.Expr = e;
		}

		public override string getDebugString()
		{
			return string.Format("#ASSIGN {0} = ({1})", Target.getDebugString(), Expr.getDebugString());
		}

		public override void linkVariables(Method owner)
		{
			Target.linkVariables(owner);
			Expr.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			Target.linkMethods(owner);
			Expr.linkMethods(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			Target.linkResultTypes(owner);
			Expr.linkResultTypes(owner);

			BType present = Target.getResultType();
			BType expected = Expr.getResultType();

			if (present != expected)
			{
				if (present.isImplicitCastableTo(expected))
					Expr = new Expression_Cast(Expr.Position, expected, Expr);
				else
					throw new ImplicitCastException(present, expected, Expr.Position);
			}
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	#endregion Operations

	#region Constructs

	public class Statement_If : Statement
	{
		public Expression Condition;
		public Statement Body;
		public Statement Else;

		public Statement_If(SourceCodePosition pos, Expression c, Statement b)
			: base(pos)
		{
			this.Condition = c;
			this.Body = b;
			this.Else = new Statement_NOP(new SourceCodePosition());
		}

		public Statement_If(SourceCodePosition pos, Expression c, Statement b, Statement e)
			: base(pos)
		{
			this.Condition = c;
			this.Body = b;
			this.Else = e;
		}

		public override string getDebugString()
		{
			return string.Format("#IF ({0})\n{1}\n#IFELSE\n{2}", Condition.getDebugString(), indent(Body.getDebugString()), Else == null ? "  NULL" : indent(Else.getDebugString()));
		}

		public override void linkVariables(Method owner)
		{
			Condition.linkVariables(owner);
			Body.linkVariables(owner);
			Else.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			Condition.linkMethods(owner);
			Body.linkMethods(owner);
			Else.linkMethods(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			Condition.linkResultTypes(owner);
			Body.linkResultTypes(owner);
			Else.linkResultTypes(owner);

			BType present = Condition.getResultType();
			BType expected = new BType_Bool(Position);

			if (present != expected)
			{
				if (present.isImplicitCastableTo(expected))
					Condition = new Expression_Cast(Condition.Position, expected, Condition);
				else
					throw new ImplicitCastException(present, expected, Condition.Position);
			}
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_While : Statement
	{
		public Expression Condition;
		public Statement Body;

		public Statement_While(SourceCodePosition pos, Expression c, Statement b)
			: base(pos)
		{
			this.Condition = c;
			this.Body = b;
		}

		public override string getDebugString()
		{
			return string.Format("#WHILE ({0})\n{1}", Condition.getDebugString(), indent(Body.getDebugString()));
		}

		public override void linkVariables(Method owner)
		{
			Condition.linkVariables(owner);
			Body.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			Condition.linkMethods(owner);
			Body.linkMethods(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			Condition.linkResultTypes(owner);
			Body.linkResultTypes(owner);

			BType present = Condition.getResultType();
			BType expected = new BType_Bool(Position);

			if (present != expected)
			{
				if (present.isImplicitCastableTo(expected))
					Condition = new Expression_Cast(Condition.Position, expected, Condition);
				else
					throw new ImplicitCastException(present, expected, Condition.Position);
			}
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	public class Statement_RepeatUntil : Statement
	{
		public Expression Condition;
		public Statement Body;

		public Statement_RepeatUntil(SourceCodePosition pos, Expression c, Statement b)
			: base(pos)
		{
			this.Condition = c;
			this.Body = b;
		}

		public override string getDebugString()
		{
			return string.Format("#REPEAT-UNTIL ({0})\n{1}", Condition.getDebugString(), indent(Body.getDebugString()));
		}

		public override void linkVariables(Method owner)
		{
			Condition.linkVariables(owner);
			Body.linkVariables(owner);
		}

		public override void linkMethods(Program owner)
		{
			Condition.linkMethods(owner);
			Body.linkMethods(owner);
		}

		public override void linkResultTypes(Method owner)
		{
			Condition.linkResultTypes(owner);
			Body.linkResultTypes(owner);

			BType present = Condition.getResultType();
			BType expected = new BType_Bool(Position);

			if (present != expected)
			{
				if (present.isImplicitCastableTo(expected))
					Condition = new Expression_Cast(Condition.Position, expected, Condition);
				else
					throw new ImplicitCastException(present, expected, Condition.Position);
			}
		}

		public override Statement_Label findLabelByIdentifier(string ident)
		{
			return null;
		}

		public override CodePiece generateCode(bool reverse)
		{
			throw new NotImplementedException(); //TODO Implement
		}
	}

	#endregion Constructs
}