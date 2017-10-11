using BefunGen.AST;
using BefunGen.AST.CodeGen;
using BefunGen.AST.DirectRun;
using BefunGen.AST.Exceptions;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BefunGen.Commandline
{
	class Runner
	{
		private static string _title = "BefunGen";

		// https://www.debuggex.com/r/e51Awdj5tCpvrP3E
		private static Regex _rexFilename = new Regex(@"^(?:[a-zA-Z]\:|\\\\[\w\.\-\(\)]+\\[\w.$\-\(\)]+)\\(?:[\w\-\(\)]+\\)*\w([\w\-\(\)\.])+$");

		private readonly CommandLineArguments cmda;

		public Runner(string[] args)
		{
			if (args.Length > 0 && File.Exists(args[0]))
			{
				args[0] = "-file=" + args[0];
			}

			if (args.Length > 1 && _rexFilename.IsMatch(args[1]))
			{
				args[1] = "-out=" + args[1];
			}

			cmda = new CommandLineArguments(args, false);
		}

		public int Run()
		{
			if (cmda.IsEmpty() || cmda.Contains("help"))
			{
				return RunHelp();
			}
			else if (cmda.Contains("directrun"))
			{
				return RunDirect();
			}
			else
			{
				return RunCompile();
			}
		}
		
		private int RunDirect()
		{
			var input = cmda.GetStringDefault("file", null);
			if (string.IsNullOrWhiteSpace(input) || !File.Exists(input)) return Fail("Please specify a valid input file");

			try
			{
				string inputCode = File.ReadAllText(input);
				var parser = new TextFungeParser();

				var prog = parser.GenerateAst(inputCode);
				var env = new RunnerEnvironment();

				prog.RunDirect(env, "");
				
				return 0;
			}
			catch (BefunGenException ex)
			{
				return Fail("Error while compiling:\n\n" + ex);
			}
			catch (Exception ex)
			{
				return Fail("Internal error:\n\n" + ex);
			}
		}

		private int RunCompile()
		{
			Console.WriteLine();
			Console.WriteLine("" + _title + " (c) Mike Schwoerer @ mikescher.com");

			var input = cmda.GetStringDefault("file", null);
			if (string.IsNullOrWhiteSpace(input) || !File.Exists(input)) return Fail("Please specify a valid input file");

			var output = cmda.GetStringDefault("out", Path.ChangeExtension(input, "b93"));
			if (string.IsNullOrWhiteSpace(input)) return Fail("Please specify a valid output file");

			ASTObject.CGO = new CodeGenOptions
			{
				NumberLiteralRepresentation = cmda.GetEnumDefault("numrep", NumberRep.Best),
				SetNOPCellsToCustom = cmda.GetBoolDefault("specnop", false),

				DefaultNumeralValue = (byte)cmda.GetUIntDefaultRange("init_number", 0, 0, 255),
				DefaultCharacterValue = (char)cmda.GetUIntDefaultRange("init_char", ' ', 0, 255),
				DefaultBooleanValue = cmda.GetBoolDefault("init_bool", false),

				StripDoubleStringmodeToogle = cmda.GetBoolDefault("o_stringmode", true),
				CompressHorizontalCombining = cmda.GetBoolDefault("o_compresshorz", true),
				CompressVerticalCombining = cmda.GetBoolDefault("o_compressvert", true),
				CompileTimeEvaluateExpressions = cmda.GetBoolDefault("o_staticexpr", true),
				RemUnreferencedMethods = cmda.GetBoolDefault("o_unused", true),

				ExtendedBooleanCast = cmda.GetBoolDefault("safe_boolcast", false),
				DisplayModuloAccess = cmda.GetBoolDefault("safe_displacc", false),

				DefaultVarDeclarationWidth = cmda.GetIntDefaultRange("varwidthmin", 16, 0, 2048),

				DefaultDisplayValue = (char)cmda.GetUIntDefaultRange("displ_char", ' ', 0, 255),
				DisplayBorder = (char)cmda.GetUIntDefaultRange("displ_borderchar", '#', 0, 255),
				DisplayBorderThickness = cmda.GetIntDefaultRange("displ_borderwidth", 1, 0, 128),

				DefaultVarDeclarationSymbol = (char)cmda.GetUIntDefaultRange("chr_vardecl", ' ', 0, 255),
				DefaultTempSymbol = (char)cmda.GetUIntDefaultRange("chr_tmpdecl", ' ', 0, 255),
				DefaultResultTempSymbol = (char)cmda.GetUIntDefaultRange("chr_tempresult", ' ', 0, 255),
				CustomNOPSymbol = (char)cmda.GetUIntDefaultRange("chr_nop", '@', 0, 255)
			};

			try
			{
				Console.Out.WriteLine("Reading from " + Path.GetFullPath(input));
				string inputCode = File.ReadAllText(input);

				var parser = new TextFungeParser();

				string outputCode = parser.GenerateCode(inputCode, TextFungeParser.ExtractDisplayFromTFFormat(inputCode), cmda.IsSet("debug"));

				Console.Out.WriteLine("Writing to " + Path.GetFullPath(output));
				File.WriteAllText(output, outputCode, Encoding.UTF8);

				return 0;
			}
			catch (BefunGenException ex)
			{
				return Fail("Error while compiling:\n\n" + ex);
			}
			catch (Exception ex)
			{
				return Fail("Internal error:\n\n" + ex);
			}
		}

		private int Fail(string fail)
		{
			Console.Error.WriteLine(fail);
			return -1;
		}

		private int RunHelp()
		{
			Console.WriteLine();
			Console.WriteLine("BefunGen sourcefile [outputfile] [parameter] ");
			Console.WriteLine();
			Console.WriteLine("########## Parameter ##########");
			Console.WriteLine();
			Console.WriteLine("file              : The input sourcecode file in *.tf format");
			Console.WriteLine("out               : The output file");
			Console.WriteLine();
			Console.WriteLine("numrep            : The Numberliteral representation (fq name or index)");
			Console.WriteLine("specnop           : Set NOP Cells to #164 character");
			Console.WriteLine();
			Console.WriteLine("init_number       : Initial value of local variables of type number");
			Console.WriteLine("init_char         : Initial value of local variables of type char (ord-value)");
			Console.WriteLine("init_bool         : Initial value of local variables of type bool");
			Console.WriteLine();
			Console.WriteLine("o_stringmode      : Optimize Double Stringmode toggle");
			Console.WriteLine("o_compresshorz    : Optimize horizontal compression");
			Console.WriteLine("o_compressvert    : Optimize vertical compression");
			Console.WriteLine("o_staticexpr      : Optimize static expressions");
			Console.WriteLine("o_unused          : Optimize unused methods");
			Console.WriteLine();
			Console.WriteLine("safe_boolcast     : Use safer bool-cast implementation");
			Console.WriteLine("safe_displacc     : Use safer display access implementation");
			Console.WriteLine();
			Console.WriteLine("varwidthmin       : Minimum variable declaration width");
			Console.WriteLine();
			Console.WriteLine("displ_char        : Initial value of display cells");
			Console.WriteLine("displ_borderchar  : Character to build display border");
			Console.WriteLine("displ_borderwidth : Width of display border");
			Console.WriteLine();
			Console.WriteLine("chr_vardecl       : Default variable declaration char (ord-value)");
			Console.WriteLine("chr_tmpdecl       : Default temp declaration char (ord-value)");
			Console.WriteLine("chr_tempresult    : Default temp result char (ord-value)");
			Console.WriteLine("chr_nop           : Default char for NOP cells (needs -specnop) (ord-value)");
			Console.WriteLine();
			Console.WriteLine("debug             : include debug informations in output");

			return 0;
		}
	}
}
