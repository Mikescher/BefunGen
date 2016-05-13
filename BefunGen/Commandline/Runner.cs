using BefunGen.AST;
using BefunGen.AST.CodeGen;
using BefunGen.AST.Exceptions;
using System;
using System.IO;
using System.Text;

namespace BefunGen.Commandline
{
	class Runner
	{
		private static string TITLE = "BefunGen";
		
		private readonly CommandLineArguments cmda;

		public Runner(string[] args)
		{
			if (args.Length > 0 && File.Exists(args[0]))
			{
				args[0] = "-file=\"" + args[0] + "\"";
			}

			if (args.Length > 1 && File.Exists(args[1]))
			{
				args[1] = "-out=\"" + args[1] + "\"";
			}

			cmda = new CommandLineArguments(args, false);
		}

		public int Run()
		{
			Console.WriteLine();
			Console.WriteLine("" + TITLE + " (c) Mike Schwoerer @ mikescher.com");

			if (cmda.IsEmpty() || cmda.Contains("help"))
			{
				ShowHelp();
				return 0;
			}

			var input = cmda.GetStringDefault("file", null);
			if (string.IsNullOrWhiteSpace(input) || !File.Exists("file")) return Fail("Please specify a valid file");

			var output = cmda.GetStringDefault("out", Path.ChangeExtension(input, "b93"));
			if (string.IsNullOrWhiteSpace(input)) return Fail("Please specify a valid file");

			ASTObject.CGO = new CodeGenOptions
			{
				NumberLiteralRepresentation = cmda.GetEnumDefault("numrep", NumberRep.Best),
				SetNOPCellsToCustom = cmda.GetBoolDefault("specnop", false),

				DefaultNumeralValue            = (byte) cmda.GetUIntDefaultRange("init_number", 0, 0, 255),
				DefaultCharacterValue          = (char) cmda.GetUIntDefaultRange("init_char", 0, 0, 255),
				DefaultBooleanValue            = cmda.GetBoolDefault("init_bool", false),

				StripDoubleStringmodeToogle    = cmda.GetBoolDefault("o_stringmode", true),
				CompressHorizontalCombining    = cmda.GetBoolDefault("o_compresshorz", true),
				CompressVerticalCombining      = cmda.GetBoolDefault("o_compressvert", true),
				CompileTimeEvaluateExpressions = cmda.GetBoolDefault("o_staticexpr", true),
				RemUnreferencedMethods         = cmda.GetBoolDefault("o_unused", true),

				ExtendedBooleanCast            = cmda.GetBoolDefault("safe_boolcast", false),
				DisplayModuloAccess            = cmda.GetBoolDefault("safe_displacc", false),

				DefaultVarDeclarationWidth     = cmda.GetIntDefaultRange("varwidthmin", 16, 0, 2048),

				DefaultDisplayValue            = (char) cmda.GetUIntDefaultRange("displ_char", 0, 0, 255),
				DisplayBorder                  = (char) cmda.GetUIntDefaultRange("displ_borderchar", 0, 0, 255),
				DisplayBorderThickness         = cmda.GetIntDefaultRange("displ_borderwidth", 16, 0, 128),

				DefaultVarDeclarationSymbol    = (char) cmda.GetUIntDefaultRange("chr_vardecl", 0, 0, 255),
				DefaultTempSymbol              = (char) cmda.GetUIntDefaultRange("chr_tmpdecl", 0, 0, 255),
				DefaultResultTempSymbol        = (char) cmda.GetUIntDefaultRange("chr_tempresult", 0, 0, 255),
				CustomNOPSymbol                = (char) cmda.GetUIntDefaultRange("chr_nop", 0, 0, 255)
			};
			
			try
			{
				string inputCode = File.ReadAllText(input);

				var parser = new TextFungeParser();

				string outputCode = parser.generateCode(inputCode, parser.ExtractDisplayFromTFFormat(inputCode), cmda.IsSet("debug"));

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

		private void ShowHelp()
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
		}
	}
}
