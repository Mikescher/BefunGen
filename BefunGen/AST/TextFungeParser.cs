using BefunGen.AST.CodeGen;
using BefunGen.AST.Exceptions;
using BefunGen.Properties;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BefunGen.AST
{
	public class TextFungeParser
	{
		private GOLD.Parser parser;

		public long ParseTime { get; private set; }
		public long GenerateTime { get; private set; }

		public TextFungeParser()
		{
			parser = new GOLD.Parser();

			LoadTables(new BinaryReader(new MemoryStream(GetGrammar())));
		}

		public bool LoadTables(BinaryReader r)
		{
			return parser.LoadTables(r);
		}

		public bool LoadTables(string p)
		{
			return LoadTables(new BinaryReader(new FileStream(p, FileMode.Open)));
		}

		public string GenerateCode(string txt, string initialDisplay, bool debug)
		{
			Program p;
			CodePiece c;
			return GenerateCode(txt, initialDisplay, debug, out p, out c);
		}

		public string GenerateCode(string txt, string initialDisplay, bool debug, out Program p, out CodePiece cp)
		{
			p = GenerateAst(txt) as Program;

			GenerateTime = Environment.TickCount;
			cp = p.GenerateCode(initialDisplay);
			GenerateTime = Environment.TickCount - GenerateTime;

			string result;

			if (debug)
				result = cp.ToString();
			else
				result = cp.ToSimpleString();

			return result;
		}

		public Program GenerateAst(string txt)
		{
			ParseTime = Environment.TickCount;

			Program result = null;

			result = (Program)Parse(txt);

			if (result == null)
				throw new Exception("Result == null");

			result.Prepare();

			ParseTime = Environment.TickCount - ParseTime;

			return result;
		}

		public bool TryParse(string txt, string disp, out BefunGenException err, out Program prog)
		{
			ParseTime = Environment.TickCount;

			Program result = null;

			try
			{
				result = (Program)Parse(txt);
			}
			catch (BefunGenException e)
			{
				err = e;
				prog = null;
				return false;
			}
			catch (Exception e)
			{
				err = new NativeException(e);
				prog = null;
				return false;
			}

			if (result == null)
			{
				err = new WTFException();
				prog = null;
				return false;
			}

			try
			{
				result.Prepare();
			}
			catch (BefunGenException e)
			{
				err = e;
				prog = null;
				return false;
			}
			catch (Exception e)
			{
				err = new NativeException(e);
				prog = null;
				return false;
			}

			try
			{
				result.GenerateCode(disp);
			}
			catch (BefunGenException e)
			{
				err = e;
				prog = null;
				return false;
			}
			catch (Exception e)
			{
				err = new NativeException(e);
				prog = null;
				return false;
			}

			ParseTime = Environment.TickCount - ParseTime;

			err = null;
			prog = result;
			return true;
		}

		private object Parse(string txt)
		{
			lock (this)
			{
				object result = null;

				txt = txt.Replace("\r\n", "\n") + "\n";

				parser.Open(ref txt);
				parser.TrimReductions = false;

				bool done = false;
				while (!done)
				{
					GOLD.ParseMessage response = parser.Parse();

					switch (response)
					{
						case GOLD.ParseMessage.LexicalError:
						case GOLD.ParseMessage.SyntaxError:
						case GOLD.ParseMessage.InternalError:
						case GOLD.ParseMessage.NotLoadedError:
						case GOLD.ParseMessage.GroupError:
							Fail(response);
							break;

						case GOLD.ParseMessage.Reduction: // Reduction
							parser.CurrentReduction = GrammarTableMap.CreateNewASTObject(parser.CurrentReduction as GOLD.Reduction, parser.CurrentPosition());
							break;

						case GOLD.ParseMessage.Accept: //Accepted!
							result = parser.CurrentReduction;
							done = true;
							break;

						case GOLD.ParseMessage.TokenRead: //You don't have to do anything here.
							break;
					}
				}

				return result;
			}
		}

		private void Fail(GOLD.ParseMessage msg)
		{
			switch (msg)
			{
				case GOLD.ParseMessage.LexicalError: //Cannot recognize token
					throw new LexicalErrorException(parser.CurrentToken().Data, new SourceCodePosition(parser));

				case GOLD.ParseMessage.SyntaxError: //Expecting a different token
					throw new SyntaxErrorException(parser.CurrentToken().Data, parser.ExpectedSymbols().Text(), new SourceCodePosition(parser));

				case GOLD.ParseMessage.InternalError: //INTERNAL ERROR! Something is horribly wrong.
					throw new InternalErrorException(new SourceCodePosition(parser));

				case GOLD.ParseMessage.NotLoadedError: //This error occurs if the CGT was not loaded.
					throw new NotLoadedErrorException(new SourceCodePosition(parser));

				case GOLD.ParseMessage.GroupError: //GROUP ERROR! Unexpected end of file
					throw new GroupErrorException(new SourceCodePosition(parser));
			}
		}

		public static string ExtractDisplayFromTFFormat(string sourcecode)
		{
			string[] lines = Regex.Split(sourcecode, @"\r?\n");

			var displayBuilder = new StringBuilder();
			var inDisplayDefinition = false;
			var first = true;
			foreach (var rawline in lines)
			{
				var line = rawline.Trim();

				if (line.StartsWith("///"))
				{
					string content = line.Substring(3);

					if (inDisplayDefinition)
					{
						if (content.Trim() == "</DISPLAY>")
						{
							return displayBuilder.ToString();
						}
						else
						{
							if (first)
								displayBuilder.Append(content);
							else
								displayBuilder.Append("\n" + content);

							first = false;
						}
					}
					else
					{
						if (content.Trim() == "<DISPLAY>")
						{
							inDisplayDefinition = true;
							displayBuilder = new StringBuilder();
							first = true;
						}
					}
				}
				else
				{
					inDisplayDefinition = false;
				}
			}
			return string.Empty;
		}

		public static bool UpdateDisplayInTFFormat(ref string sourcecode, string displayvalue)
		{
			if (string.IsNullOrWhiteSpace(displayvalue)) return RemoveDisplayInTFFormat(ref sourcecode);

			var inputLines = Regex.Split(sourcecode, @"\r?\n");
			var displayLines = Regex.Split(displayvalue, @"\r?\n");
			
			#region Patch existing <DISPLAY>
			{
				StringBuilder output = new StringBuilder();

				bool replaced = false;
				bool inDisplayDefinition = false;
				string displayindent = "";
				foreach (var rawline in inputLines)
				{
					var line = rawline.TrimStart();

					if (line.StartsWith("///"))
					{
						string content = line.Substring(3);

						if (inDisplayDefinition)
						{
							if (content.Trim() == "</DISPLAY>")
							{
								// skip
								inDisplayDefinition = false;
								output.AppendLine(rawline);
								continue;
							}
							else
							{
								// skip
								continue;
							}
						}
						else
						{
							if (content.Trim() == "<DISPLAY>" && !replaced)
							{
								// add display
								output.AppendLine(rawline);
								displayindent = line.Substring(0, line.IndexOf("///"));
								foreach (var dl in displayLines) output.AppendLine(displayindent + "///" + dl);
								inDisplayDefinition = true;
								replaced = true;
							}
							else
							{
								// output
								output.AppendLine(rawline);
							}
						}
					}
					else
					{
						// output
						inDisplayDefinition = false;
						output.AppendLine(rawline);
					}
				}

				if (replaced)
				{
					StringBuilderToStringWithoutDanglingNewline(output);
					return true;
				}
			}
			#endregion

			#region Create new <DISPLAY>
			{
				StringBuilder output = new StringBuilder();

				bool replaced = false;
				string displayindent = "";
				foreach (var rawline in inputLines)
				{
					var line = rawline.TrimStart();

					if (line.Trim().ToLower().StartsWith("program ") && !replaced)
					{
						// add display
						displayindent = rawline.Substring(0, rawline.IndexOf("program"));
						output.AppendLine(displayindent + "///<DISPLAY>");
						foreach (var dl in displayLines) output.AppendLine(displayindent + "///" + dl);
						output.AppendLine(displayindent + "///</DISPLAY>");
						replaced = true;
					}

					// output
					output.AppendLine(rawline);
				}

				if (replaced)
				{
					StringBuilderToStringWithoutDanglingNewline(output);
					return true;
				}
			}
			#endregion

			return false;
		}

		private static bool RemoveDisplayInTFFormat(ref string sourcecode)
		{
			var inputLines = Regex.Split(sourcecode, @"\r?\n");

			StringBuilder output = new StringBuilder();

			bool inDisplayDefinition = false;
			foreach (var rawline in inputLines)
			{
				var line = rawline.TrimStart();

				if (line.StartsWith("///"))
				{
					string content = line.Substring(3);

					if (inDisplayDefinition)
					{
						if (content.Trim() == "</DISPLAY>")
						{
							// skip
							inDisplayDefinition = false;
							continue;
						}
						else
						{
							// skip
							continue;
						}
					}
					else
					{
						if (content.Trim() == "<DISPLAY>")
						{
							// skip
							inDisplayDefinition = true;
						}
						else
						{
							// output
							output.AppendLine(rawline);
						}
					}
				}
				else
				{
					// output
					inDisplayDefinition = false;
					output.AppendLine(rawline);
				}
			}

			sourcecode = StringBuilderToStringWithoutDanglingNewline(output);

			return true;
		}

		private static string StringBuilderToStringWithoutDanglingNewline(StringBuilder b)
		{
			var str = b.ToString();

			if (str.EndsWith("\r\n")) return str.Substring(0, str.Length - 2);

			if (str.EndsWith("\n")) return str.Substring(0, str.Length - 1);

			return str;
		} 

		public string GetGrammarDefinition()
		{
			return Resources.TextFunge_grm;
		}

		public byte[] GetGrammar()
		{
			return Resources.TextFunge_egt;
		}
	}
}