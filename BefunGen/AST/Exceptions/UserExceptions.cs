﻿using BefunGen.AST.CodeGen;
using System;
using System.Linq;

namespace BefunGen.AST.Exceptions
{
	public class ArrayLiteralTooBigException : BefunGenUserException
	{
		public ArrayLiteralTooBigException(SourceCodePosition pos)
			: base("ArrayLiteral is too big for Variable", pos) { }
	}

	public class GroupErrorException : BefunGenUserException
	{
		public GroupErrorException(SourceCodePosition pos)
			: base("Unexpected end of file", pos) { }
	}

	public class ImplicitCastException : BefunGenUserException
	{
		public ImplicitCastException(SourceCodePosition pos, BType from, BType to)
			: base(String.Format("Cannot implicitly cast from {0} to {1}", from, to), pos) { }

		public ImplicitCastException(SourceCodePosition pos, BType from, params BType[] to)
			: base(String.Format("Cannot implicitly cast from {0} to ({1})", from, string.Join(" or ", ((BType[])to).ToList())), pos) { }
	}

	public class IndexOperatorNotDefiniedException : BefunGenUserException
	{
		public IndexOperatorNotDefiniedException(SourceCodePosition pos)
			: base("Cant perform index operation here", pos) { }
	}

	public class InvalidCompareException : BefunGenUserException
	{
		public InvalidCompareException(BType a, BType b, SourceCodePosition pos)
			: base(String.Format("Cannot compare Types of {0} and {1}", a, b), pos) { }
	}

	public class InvalidFormatSpecifierException : BefunGenUserException
	{
		public InvalidFormatSpecifierException(string fs, SourceCodePosition pos)
			: base(String.Format("Formatspecifier '{0}' is unknown", fs), pos) { }
	}

	public class NoConditionException : BefunGenUserException
	{
		public NoConditionException(SourceCodePosition pos)
			: base("Expression is not a valid Condition", pos) { }
	}

	public class NotAllPathsReturnException : BefunGenUserException
	{
		public NotAllPathsReturnException(Method m, SourceCodePosition pos)
			: base("Not all Paths of Method " + m.Identifier + "() return a value.", pos) { }
	}

	public class SyntaxErrorException : BefunGenUserException
	{
		public SyntaxErrorException(object data, string expect, SourceCodePosition pos)
			: base("Lexical Error on data='" + data + "', expected='" + expect + "'", pos) { }
	}

	public class UnresolvableReferenceException : BefunGenUserException
	{
		public UnresolvableReferenceException(string s, SourceCodePosition pos)
			: base("Could not resolve reference: \"" + s + "\"", pos) { }
	}

	public class VoidObjectCallException : BefunGenUserException
	{
		public VoidObjectCallException(SourceCodePosition pos)
			: base("Operation not possible on <void>", pos) { }
	}

	public class WrongParameterCountException : BefunGenUserException
	{
		public WrongParameterCountException(int actual, int expected, SourceCodePosition pos)
			: base(String.Format("Parametercount ({0}) differs from expected ({1})", actual, expected), pos) { }
	}

	public class WrongTypeException : BefunGenUserException
	{
		public WrongTypeException(SourceCodePosition pos, BType found, BType expec)
			: base(String.Format("Wrong Type found: Found = {0}. Expected = {1}", found, expec), pos) { }

		public WrongTypeException(SourceCodePosition pos, BType found, params BType[] expec)
			: base(String.Format("Wrong Type found: Found = {0}. Expected = {1}", found, string.Join(" or ", expec.ToList())), pos) { }
	}

	public class ArrayTooSmallException : BefunGenUserException
	{
		public ArrayTooSmallException(SourceCodePosition pos)
			: base(String.Format("The size of the array is too small"), pos) { }
	}

	public class InlineVoidMethodCallException : BefunGenUserException
	{
		public InlineVoidMethodCallException(SourceCodePosition pos)
			: base("Call of an void-Method inside of a expression", pos) { }
	}

	public class InitGlobalVariableException : BefunGenUserException
	{
		public InitGlobalVariableException(SourceCodePosition pos, string ident)
			: base("The global variable " + ident + " has an Initializator, this is not possible for global variables", pos) { }
	}

	public class NoValueInitConstantException : BefunGenUserException
	{
		public NoValueInitConstantException(SourceCodePosition pos, string ident)
			: base("The Constant " + ident + " has no value", pos) { }
	}

	public class UndefiniedValueInitConstantException : BefunGenUserException
	{
		public UndefiniedValueInitConstantException(SourceCodePosition pos, string ident)
			: base("The Constant " + ident + " has no well defined value", pos) { }
	}

	public class UndefiniedValueInitValueException : BefunGenUserException
	{
		public UndefiniedValueInitValueException(SourceCodePosition pos, string ident)
			: base("The Global variable " + ident + " has no well defined initial value", pos) { }
	}

	public class IllegalIdentifierException : BefunGenUserException
	{
		public IllegalIdentifierException(SourceCodePosition pos, string ident)
			: base("The Identifier " + ident + " is reserved", pos) { }
	}

	public class ConstantValueChangedException : BefunGenUserException
	{
		public ConstantValueChangedException(SourceCodePosition pos, string ident)
			: base("You cannot change the value of the Constant " + ident, pos) { }
	}

	public class EmptyDisplayAccessException : BefunGenUserException
	{
		public EmptyDisplayAccessException(SourceCodePosition pos)
			: base("Trying to access a display with size == 0 ", pos) { }
	}

	public class DuplicateSwitchCaseException : BefunGenUserException
	{
		public DuplicateSwitchCaseException(SourceCodePosition pos, LiteralValue val)
			: base(String.Format("There are more than one Case with the same Value ({0}) in the Switch Statement ", val.GetDebugString()), pos) { }
	}

	public class InitialDisplayValueTooBigException : BefunGenUserException
	{
		public InitialDisplayValueTooBigException(int minx, int miny, int realx, int realy)
			: base(String.Format("The Initial Displaysize ({0}|{1}) does not fit into the display ({2}|{3}).", realx, realy, minx, miny), new SourceCodePosition()) { }
	}

	public class IllegalReturnCallInMainException : BefunGenUserException
	{
		public IllegalReturnCallInMainException(SourceCodePosition pos)
			: base("Illegal call of RETURN in the Main-statement ", pos) { }
	}

	public class ConstantArrayException : BefunGenUserException
	{
		public ConstantArrayException(SourceCodePosition pos)
			: base("An Array cannot be declared as constant ", pos) { }
	}

	public class DuplicateIdentifierException : BefunGenUserException
	{
		public DuplicateIdentifierException(SourceCodePosition pos, string ident)
			: base(String.Format("The Identifier {0} is already in use", ident), pos) { }
	}

	public class ClassMethodNotFoundException : BefunGenUserException
	{
		public ClassMethodNotFoundException(SourceCodePosition pos, string ident)
			: base(string.Format("The class method {0} does not exist", ident), pos) { }
	}

	public class ClassMethodTypeNotValidException : BefunGenUserException
	{
		public ClassMethodTypeNotValidException(SourceCodePosition pos, AutoClassMethods ident, BType t)
			: base(string.Format("You cannot call the class method {0} on the type {1}", ident, t.ToString()), pos) { }
	}

	public class CannotInitStackException : BefunGenUserException
	{
		public CannotInitStackException(SourceCodePosition pos)
			: base(string.Format("A stack variable cannot be initialized wit a value"), pos) { }
	}
}
