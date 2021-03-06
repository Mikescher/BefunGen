﻿using BefunGen.AST.CodeGen;
using System;

namespace BefunGen.AST.Exceptions
{
	public class AccessTemporaryASTObjectException : BefunGenInternalException
	{
		public AccessTemporaryASTObjectException(SourceCodePosition pos)
			: base("Trying to access temporary AST-object", pos) { }
	}

	public class InvalidReductionException : BefunGenInternalException
	{
		public InvalidReductionException()
			: base("Grammar reduction is not possible") { }
	}

	public class InvalidAstStateException : BefunGenInternalException
	{
		public InvalidAstStateException(SourceCodePosition pos)
			: base("AST is currently in an invalid state", pos) { }
	}

	public class InvalidBefungeCommandTypeException : BefunGenInternalException
	{
		public InvalidBefungeCommandTypeException(SourceCodePosition pos)
			: base("BefungeCMD-Enum is in an impossible State", pos) { }
	}

	public class LexicalErrorException : BefunGenInternalException
	{
		public LexicalErrorException(object data, SourceCodePosition pos)
			: base("Lexical Error on data='" + data + "'", pos) { }
	}

	public class NotLoadedErrorException : BefunGenInternalException
	{
		public NotLoadedErrorException(SourceCodePosition pos)
			: base("CGT not loaded", pos) { }
	}

	public class InternalErrorException : BefunGenInternalException
	{
		public InternalErrorException(SourceCodePosition pos)
			: base("Parser Error", pos) { }
	}

	public class CodePieceReverseException : BefunGenInternalException
	{
		public CodePieceReverseException(CodePiece p)
			: base("Cannot reverse Codepiece " + p + ".") { }

		public CodePieceReverseException(BefungeCommand p)
			: base("Cannot reverse Command " + p + ".") { }
	}

	public class CommandPathFindingFailureException : BefunGenInternalException
	{
		public CommandPathFindingFailureException(string msg)
			: base("Internal Pathfinding failure: " + msg) { }
	}

	public class InternalCodeGenException : BefunGenInternalException
	{
		public InternalCodeGenException()
			: base("Internal CodeGen Exception") { }
	}

	public class InternalCodeRunException : BefunGenInternalException
	{
		public InternalCodeRunException()
			: base("Internal CodeGen Runner Exception") { }

		public InternalCodeRunException(string msg)
			: base("Internal CodeGen Runner Exception: " + msg) { }
	}

	public class InvalidCodeManipulationException : BefunGenInternalException
	{
		public InvalidCodeManipulationException(string msg)
			: base("Invalid Codemanipulation: " + msg) { }
	}

	public class WTFException : BefunGenInternalException
	{
		public WTFException()
			: base("This should not be possible to happen ... I'm confused") { }
	}

	public class BgNotImplementedException : BefunGenInternalException
	{
		public BgNotImplementedException()
			: base("Method not yet implemented.") { }
	}

	public class MissingReductionRuleException : BefunGenInternalException
	{
		public MissingReductionRuleException(string r)
			: base("Reduction Rule for " + r + " is missing.") { }
	}

	public class NativeException : BefunGenInternalException
	{
		public NativeException(Exception e)
			: base("Native Exception:" + e.ToString()) { }
	}

	public class CodeAssertException : BefunGenInternalException
	{
		public CodeAssertException(string msg)
			: base("Code Assert failed:" + msg) { }
	}
}
