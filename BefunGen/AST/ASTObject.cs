using BefunGen.AST.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BefunGen.AST
{
	public abstract class ASTObject
	{
		public static string[] Keywords = 
		{ 
			"begin", "close", "const", "display", "do", 
			"else", "elsif", "end", "false", "for", 
			"global", "goto", "if", "in", "out", 
			"outf", "program", "quit", "rand", "repeat", 
			"return", "stop", "then", "true", "until", 
			"var", "while", "switch", "case", "default",
			
			"bool", "boolean", "char", "character", "digit", 
			"int", "integer", "void"
		};

		public readonly SourceCodePosition Position;

		public static CodeGenOptions CGO = CodeGenOptions.getCGO_Debug();

		public ASTObject(SourceCodePosition pos)
		{
			this.Position = pos;
		}

		protected string GetDebugCommaStringForList<T>(List<T> ls) where T : ASTObject
		{
			return string.Join(", ", ls.Select(p => p.GetDebugString()));
		}

		protected string GetDebugStringForList<T>(List<T> ls) where T : ASTObject
		{
			return string.Join("\n", ls.Select(p => p.GetDebugString()));
		}

		protected string Indent(string s)
		{
			return string.Join("\n", s.Split(new string[] { "\n" }, StringSplitOptions.None).Select(p => "    " + p));
		}

		public abstract string GetDebugString();

		public static bool IsKeyword(string s)
		{
			return Keywords.Contains(s.ToLower());
		}
	}
}