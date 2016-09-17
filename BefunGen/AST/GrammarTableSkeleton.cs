
namespace BefunGen.AST
{
	enum SymbolIndex
	{
		@Eof = 0,                                  // (EOF)
		@Error = 1,                                // (Error)
		@Comment = 2,                              // Comment
		@Newline = 3,                              // NewLine
		@Whitespace = 4,                           // Whitespace
		@Timesdiv = 5,                             // '*/'
		@Divtimes = 6,                             // '/*'
		@Divdiv = 7,                               // '//'
		@Minus = 8,                                // '-'
		@Minusminus = 9,                           // '--'
		@Exclam = 10,                              // '!'
		@Exclameq = 11,                            // '!='
		@Percent = 12,                             // '%'
		@Percenteq = 13,                           // '%='
		@Ampamp = 14,                              // '&&'
		@Ampeq = 15,                               // '&='
		@Lparen = 16,                              // '('
		@Rparen = 17,                              // ')'
		@Times = 18,                               // '*'
		@Timeseq = 19,                             // '*='
		@Comma = 20,                               // ','
		@Div = 21,                                 // '/'
		@Diveq = 22,                               // '/='
		@Colon = 23,                               // ':'
		@Coloneq = 24,                             // ':='
		@Semi = 25,                                // ';'
		@Lbracket = 26,                            // '['
		@Rbracket = 27,                            // ']'
		@Caret = 28,                               // '^'
		@Careteq = 29,                             // '^='
		@Lbrace = 30,                              // '{'
		@Pipepipe = 31,                            // '||'
		@Pipeeq = 32,                              // '|='
		@Rbrace = 33,                              // '}'
		@Plus = 34,                                // '+'
		@Plusplus = 35,                            // '++'
		@Pluseq = 36,                              // '+='
		@Lt = 37,                                  // '<'
		@Lteq = 38,                                // '<='
		@Eq = 39,                                  // '='
		@Minuseq = 40,                             // '-='
		@Eqeq = 41,                                // '=='
		@Gt = 42,                                  // '>'
		@Gteq = 43,                                // '>='
		@Begin = 44,                               // begin
		@Bool = 45,                                // bool
		@Boolean = 46,                             // boolean
		@Case = 47,                                // case
		@Char = 48,                                // char
		@Character = 49,                           // Character
		@Charliteral = 50,                         // CharLiteral
		@Close = 51,                               // close
		@Const = 52,                               // const
		@Decliteral = 53,                          // DecLiteral
		@Default = 54,                             // default
		@Digit = 55,                               // digit
		@Digitliteral = 56,                        // DigitLiteral
		@Display = 57,                             // display
		@Do = 58,                                  // do
		@Else = 59,                                // else
		@Elsif = 60,                               // elsif
		@End = 61,                                 // end
		@False = 62,                               // false
		@For = 63,                                 // for
		@Global = 64,                              // global
		@Goto = 65,                                // goto
		@Hexliteral = 66,                          // HexLiteral
		@Identifier = 67,                          // Identifier
		@If = 68,                                  // if
		@In = 69,                                  // in
		@Int = 70,                                 // int
		@Integer = 71,                             // integer
		@Out = 72,                                 // out
		@Outf = 73,                                // outf
		@Program = 74,                             // program
		@Quit = 75,                                // quit
		@Rand = 76,                                // rand
		@Repeat = 77,                              // repeat
		@Return = 78,                              // return
		@Stop = 79,                                // stop
		@Stringliteral = 80,                       // StringLiteral
		@Switch = 81,                              // switch
		@Then = 82,                                // then
		@True = 83,                                // true
		@Until = 84,                               // until
		@Var = 85,                                 // var
		@Void = 86,                                // void
		@While = 87,                               // while
		ArrayLiteral = 88,                       // <Array_Literal>
		@Constants = 89,                           // <Constants>
		@Expadd = 90,                              // <Exp Add>
		@Expcomp = 91,                             // <Exp Comp>
		@Expmult = 92,                             // <Exp Mult>
		@Exprand = 93,                             // <Exp Rand>
		@Expunary = 94,                            // <Exp Unary>
		@Exprbool = 95,                            // <Expr Bool>
		@Expreq = 96,                              // <Expr Eq>
		@Expression = 97,                          // <Expression>
		@Expressionlist = 98,                      // <ExpressionList>
		@Footer = 99,                              // <Footer>
		@Globalvars = 100,                         // <GlobalVars>
		@Header = 101,                             // <Header>
		@Identifierlist = 102,                     // <IdentifierList>
		@Literal = 103,                            // <Literal>
		LiteralBool = 104,                       // <Literal_Bool>
		LiteralBoolList = 105,                  // <Literal_Bool_List>
		LiteralBoolarr = 106,                    // <Literal_BoolArr>
		LiteralChar = 107,                       // <Literal_Char>
		LiteralCharList = 108,                  // <Literal_Char_List>
		LiteralDigit = 109,                      // <Literal_Digit>
		LiteralDigitList = 110,                 // <Literal_Digit_List>
		LiteralDigitarr = 111,                   // <Literal_DigitArr>
		LiteralInt = 112,                        // <Literal_Int>
		LiteralIntList = 113,                   // <Literal_Int_List>
		LiteralIntarr = 114,                     // <Literal_IntArr>
		LiteralString = 115,                     // <Literal_String>
		@Mainstatements = 116,                     // <MainStatements>
		@Method = 117,                             // <Method>
		@Methodbody = 118,                         // <MethodBody>
		@Methodheader = 119,                       // <MethodHeader>
		@Methodlist = 120,                         // <MethodList>
		@Optionalexpression = 121,                 // <OptionalExpression>
		@Optionalsimstatement = 122,               // <OptionalSimStatement>
		@Outflist = 123,                           // <OutfList>
		@Param = 124,                              // <Param>
		@Paramdecl = 125,                          // <ParamDecl>
		@Paramlist = 126,                          // <ParamList>
		@Program2 = 127,                           // <Program>
		@Simplestatement = 128,                    // <SimpleStatement>
		@Statement = 129,                          // <Statement>
		@Statementlist = 130,                      // <StatementList>
		StmtAssignment = 131,                    // <Stmt_Assignment>
		StmtCall = 132,                          // <Stmt_Call>
		StmtElseiflist = 133,                    // <Stmt_ElseIfList>
		StmtFor = 134,                           // <Stmt_For>
		StmtGoto = 135,                          // <Stmt_Goto>
		StmtIf = 136,                            // <Stmt_If>
		StmtIn = 137,                            // <Stmt_In>
		StmtInc = 138,                           // <Stmt_Inc>
		StmtLabel = 139,                         // <Stmt_Label>
		StmtModassignment = 140,                 // <Stmt_ModAssignment>
		StmtOut = 141,                           // <Stmt_Out>
		StmtOutf = 142,                          // <Stmt_Outf>
		StmtQuit = 143,                          // <Stmt_Quit>
		StmtRepeat = 144,                        // <Stmt_Repeat>
		StmtReturn = 145,                        // <Stmt_Return>
		StmtSwitch = 146,                        // <Stmt_Switch>
		StmtSwitchCaselist = 147,               // <Stmt_Switch_CaseList>
		StmtWhile = 148,                         // <Stmt_While>
		@Type = 149,                               // <Type>
		TypeBool = 150,                          // <Type_Bool>
		TypeBoolarr = 151,                       // <Type_BoolArr>
		TypeChar = 152,                          // <Type_Char>
		TypeDigit = 153,                         // <Type_Digit>
		TypeDigitarr = 154,                      // <Type_DigitArr>
		TypeInt = 155,                           // <Type_Int>
		TypeIntarr = 156,                        // <Type_IntArr>
		TypeString = 157,                        // <Type_String>
		TypeVoid = 158,                          // <Type_Void>
		@Value = 159,                              // <Value>
		ValueLiteral = 160,                      // <Value_Literal>
		@Valuepointer = 161,                       // <ValuePointer>
		@Vardecl = 162,                            // <VarDecl>
		@Vardeclbody = 163,                        // <VarDeclBody>
		@Varlist = 164                             // <VarList>
	}

	enum ProductionIndex
	{
		@Program = 0,                              // <Program> ::= <Header> <Constants> <GlobalVars> <MainStatements> <MethodList> <Footer>
		HeaderProgramIdentifier = 1,            // <Header> ::= program Identifier
		HeaderProgramIdentifierColonDisplayLbracketCommaRbracket = 2,  // <Header> ::= program Identifier ':' display '[' <Literal_Int> ',' <Literal_Int> ']'
		FooterEnd = 3,                           // <Footer> ::= end
		ConstantsConst = 4,                      // <Constants> ::= const <VarList>
		@Constants = 5,                            // <Constants> ::= 
		GlobalvarsGlobal = 6,                    // <GlobalVars> ::= global <VarList>
		@Globalvars = 7,                           // <GlobalVars> ::= 
		@Methodlist = 8,                           // <MethodList> ::= <MethodList> <Method>
		@Methodlist2 = 9,                          // <MethodList> ::= 
		@Mainstatements = 10,                      // <MainStatements> ::= <MethodBody>
		@Method = 11,                              // <Method> ::= <MethodHeader> <MethodBody>
		@Methodbody = 12,                          // <MethodBody> ::= <VarDeclBody> <Statement>
		MethodheaderIdentifierLparenRparen = 13,  // <MethodHeader> ::= <Type> Identifier '(' <ParamDecl> ')'
		VardeclbodyVar = 14,                     // <VarDeclBody> ::= var <VarList>
		@Vardeclbody = 15,                         // <VarDeclBody> ::= 
		@Paramdecl = 16,                           // <ParamDecl> ::= <ParamList>
		@Paramdecl2 = 17,                          // <ParamDecl> ::= 
		ParamlistComma = 18,                     // <ParamList> ::= <ParamList> ',' <Param>
		@Paramlist = 19,                           // <ParamList> ::= <Param>
		ParamIdentifier = 20,                    // <Param> ::= <Type> Identifier
		VarlistSemi = 21,                        // <VarList> ::= <VarList> <VarDecl> ';'
		VarlistSemi2 = 22,                       // <VarList> ::= <VarDecl> ';'
		@Vardecl = 23,                             // <VarDecl> ::= <Type> <IdentifierList>
		VardeclIdentifierColoneq = 24,          // <VarDecl> ::= <Type> Identifier ':=' <Literal>
		IdentifierlistCommaIdentifier = 25,     // <IdentifierList> ::= <IdentifierList> ',' Identifier
		IdentifierlistIdentifier = 26,           // <IdentifierList> ::= Identifier
		@Optionalsimstatement = 27,                // <OptionalSimStatement> ::= <SimpleStatement>
		@Optionalsimstatement2 = 28,               // <OptionalSimStatement> ::= 
		StatementSemi = 29,                      // <Statement> ::= <SimpleStatement> ';'
		StatementBeginEnd = 30,                 // <Statement> ::= begin <StatementList> end
		@Statement = 31,                           // <Statement> ::= <Stmt_If>
		@Statement2 = 32,                          // <Statement> ::= <Stmt_While>
		@Statement3 = 33,                          // <Statement> ::= <Stmt_For>
		@Statement4 = 34,                          // <Statement> ::= <Stmt_Repeat>
		StatementSemi2 = 35,                     // <Statement> ::= <Stmt_Goto> ';'
		@Statement5 = 36,                          // <Statement> ::= <Stmt_Label>
		@Statement6 = 37,                          // <Statement> ::= <Stmt_Switch>
		@Simplestatement = 38,                     // <SimpleStatement> ::= <Stmt_Quit>
		@Simplestatement2 = 39,                    // <SimpleStatement> ::= <Stmt_Return>
		@Simplestatement3 = 40,                    // <SimpleStatement> ::= <Stmt_Out>
		@Simplestatement4 = 41,                    // <SimpleStatement> ::= <Stmt_In>
		@Simplestatement5 = 42,                    // <SimpleStatement> ::= <Stmt_Inc>
		@Simplestatement6 = 43,                    // <SimpleStatement> ::= <Stmt_Assignment>
		@Simplestatement7 = 44,                    // <SimpleStatement> ::= <Stmt_Call>
		@Simplestatement8 = 45,                    // <SimpleStatement> ::= <Stmt_ModAssignment>
		@Simplestatement9 = 46,                    // <SimpleStatement> ::= <Stmt_Outf>
		@Statementlist = 47,                       // <StatementList> ::= <StatementList> <Statement>
		@Statementlist2 = 48,                      // <StatementList> ::= 
		StmtIncPlusplus = 49,                   // <Stmt_Inc> ::= <ValuePointer> '++'
		StmtIncMinusminus = 50,                 // <Stmt_Inc> ::= <ValuePointer> '--'
		StmtQuitQuit = 51,                      // <Stmt_Quit> ::= quit
		StmtQuitStop = 52,                      // <Stmt_Quit> ::= stop
		StmtQuitClose = 53,                     // <Stmt_Quit> ::= close
		StmtOutOut = 54,                        // <Stmt_Out> ::= out <Expression>
		StmtOutOut2 = 55,                       // <Stmt_Out> ::= out <Literal_String>
		StmtInIn = 56,                          // <Stmt_In> ::= in <ValuePointer>
		StmtAssignmentEq = 57,                  // <Stmt_Assignment> ::= <ValuePointer> '=' <Expression>
		StmtModassignmentPluseq = 58,           // <Stmt_ModAssignment> ::= <ValuePointer> '+=' <Expression>
		StmtModassignmentMinuseq = 59,          // <Stmt_ModAssignment> ::= <ValuePointer> '-=' <Expression>
		StmtModassignmentTimeseq = 60,          // <Stmt_ModAssignment> ::= <ValuePointer> '*=' <Expression>
		StmtModassignmentDiveq = 61,            // <Stmt_ModAssignment> ::= <ValuePointer> '/=' <Expression>
		StmtModassignmentPercenteq = 62,        // <Stmt_ModAssignment> ::= <ValuePointer> '%=' <Expression>
		StmtModassignmentAmpeq = 63,            // <Stmt_ModAssignment> ::= <ValuePointer> '&=' <Expression>
		StmtModassignmentPipeeq = 64,           // <Stmt_ModAssignment> ::= <ValuePointer> '|=' <Expression>
		StmtModassignmentCareteq = 65,          // <Stmt_ModAssignment> ::= <ValuePointer> '^=' <Expression>
		StmtReturnReturn = 66,                  // <Stmt_Return> ::= return <Expression>
		StmtReturnReturn2 = 67,                 // <Stmt_Return> ::= return
		StmtCallIdentifierLparenRparen = 68,  // <Stmt_Call> ::= Identifier '(' <ExpressionList> ')'
		StmtCallIdentifierLparenRparen2 = 69,  // <Stmt_Call> ::= Identifier '(' ')'
		StmtOutfOutf = 70,                      // <Stmt_Outf> ::= outf <OutfList>
		@Outflist = 71,                            // <OutfList> ::= <Expression>
		@Outflist2 = 72,                           // <OutfList> ::= <Literal_String>
		OutflistComma = 73,                      // <OutfList> ::= <OutfList> ',' <Expression>
		OutflistComma2 = 74,                     // <OutfList> ::= <OutfList> ',' <Literal_String>
		StmtIfIfThenEnd = 75,                 // <Stmt_If> ::= if <Expression> then <StatementList> <Stmt_ElseIfList> end
		StmtElseiflistElsifThen = 76,          // <Stmt_ElseIfList> ::= elsif <Expression> then <StatementList> <Stmt_ElseIfList>
		StmtElseiflistElse = 77,                // <Stmt_ElseIfList> ::= else <StatementList>
		StmtElseiflist = 78,                     // <Stmt_ElseIfList> ::= 
		StmtWhileWhileDoEnd = 79,             // <Stmt_While> ::= while <Expression> do <StatementList> end
		StmtForForLparenSemiSemiRparenDoEnd = 80,  // <Stmt_For> ::= for '(' <OptionalSimStatement> ';' <OptionalExpression> ';' <OptionalSimStatement> ')' do <StatementList> end
		StmtRepeatRepeatUntilLparenRparen = 81,  // <Stmt_Repeat> ::= repeat <StatementList> until '(' <Expression> ')'
		StmtSwitchSwitchBeginEnd = 82,        // <Stmt_Switch> ::= switch <Expression> begin <Stmt_Switch_CaseList> end
		StmtSwitchCaselistCaseColonEnd = 83,  // <Stmt_Switch_CaseList> ::= case <Value_Literal> ':' <StatementList> end <Stmt_Switch_CaseList>
		StmtSwitchCaselistDefaultColonEnd = 84,  // <Stmt_Switch_CaseList> ::= default ':' <StatementList> end
		StmtSwitchCaselist = 85,                // <Stmt_Switch_CaseList> ::= 
		StmtGotoGotoIdentifier = 86,           // <Stmt_Goto> ::= goto Identifier
		StmtLabelIdentifierColon = 87,         // <Stmt_Label> ::= Identifier ':'
		@Type = 88,                                // <Type> ::= <Type_Int>
		@Type2 = 89,                               // <Type> ::= <Type_Digit>
		@Type3 = 90,                               // <Type> ::= <Type_Char>
		@Type4 = 91,                               // <Type> ::= <Type_Bool>
		@Type5 = 92,                               // <Type> ::= <Type_Void>
		@Type6 = 93,                               // <Type> ::= <Type_IntArr>
		@Type7 = 94,                               // <Type> ::= <Type_String>
		@Type8 = 95,                               // <Type> ::= <Type_DigitArr>
		@Type9 = 96,                               // <Type> ::= <Type_BoolArr>
		TypeIntInt = 97,                        // <Type_Int> ::= int
		TypeIntInteger = 98,                    // <Type_Int> ::= integer
		TypeCharChar = 99,                      // <Type_Char> ::= char
		TypeCharCharacter = 100,                // <Type_Char> ::= Character
		TypeDigitDigit = 101,                   // <Type_Digit> ::= digit
		TypeBoolBool = 102,                     // <Type_Bool> ::= bool
		TypeBoolBoolean = 103,                  // <Type_Bool> ::= boolean
		TypeVoidVoid = 104,                     // <Type_Void> ::= void
		TypeIntarrLbracketRbracket = 105,      // <Type_IntArr> ::= <Type_Int> '[' <Literal_Int> ']'
		TypeStringLbracketRbracket = 106,      // <Type_String> ::= <Type_Char> '[' <Literal_Int> ']'
		TypeDigitarrLbracketRbracket = 107,    // <Type_DigitArr> ::= <Type_Digit> '[' <Literal_Int> ']'
		TypeBoolarrLbracketRbracket = 108,     // <Type_BoolArr> ::= <Type_Bool> '[' <Literal_Int> ']'
		@Literal = 109,                            // <Literal> ::= <Array_Literal>
		@Literal2 = 110,                           // <Literal> ::= <Value_Literal>
		ArrayLiteral = 111,                      // <Array_Literal> ::= <Literal_IntArr>
		ArrayLiteral2 = 112,                     // <Array_Literal> ::= <Literal_String>
		ArrayLiteral3 = 113,                     // <Array_Literal> ::= <Literal_DigitArr>
		ArrayLiteral4 = 114,                     // <Array_Literal> ::= <Literal_BoolArr>
		ValueLiteral = 115,                      // <Value_Literal> ::= <Literal_Int>
		ValueLiteral2 = 116,                     // <Value_Literal> ::= <Literal_Char>
		ValueLiteral3 = 117,                     // <Value_Literal> ::= <Literal_Bool>
		ValueLiteral4 = 118,                     // <Value_Literal> ::= <Literal_Digit>
		LiteralIntDecliteral = 119,             // <Literal_Int> ::= DecLiteral
		LiteralIntHexliteral = 120,             // <Literal_Int> ::= HexLiteral
		LiteralCharCharliteral = 121,           // <Literal_Char> ::= CharLiteral
		LiteralBoolTrue = 122,                  // <Literal_Bool> ::= true
		LiteralBoolFalse = 123,                 // <Literal_Bool> ::= false
		LiteralDigitDigitliteral = 124,         // <Literal_Digit> ::= DigitLiteral
		LiteralIntarrLbraceRbrace = 125,       // <Literal_IntArr> ::= '{' <Literal_Int_List> '}'
		LiteralStringLbraceRbrace = 126,       // <Literal_String> ::= '{' <Literal_Char_List> '}'
		LiteralStringStringliteral = 127,       // <Literal_String> ::= StringLiteral
		LiteralDigitarrLbraceRbrace = 128,     // <Literal_DigitArr> ::= '{' <Literal_Digit_List> '}'
		LiteralBoolarrLbraceRbrace = 129,      // <Literal_BoolArr> ::= '{' <Literal_Bool_List> '}'
		LiteralIntListComma = 130,             // <Literal_Int_List> ::= <Literal_Int_List> ',' <Literal_Int>
		LiteralIntList = 131,                   // <Literal_Int_List> ::= <Literal_Int>
		LiteralCharListComma = 132,            // <Literal_Char_List> ::= <Literal_Char_List> ',' <Literal_Char>
		LiteralCharList = 133,                  // <Literal_Char_List> ::= <Literal_Char>
		LiteralDigitListComma = 134,           // <Literal_Digit_List> ::= <Literal_Digit_List> ',' <Literal_Digit>
		LiteralDigitList = 135,                 // <Literal_Digit_List> ::= <Literal_Digit>
		LiteralBoolListComma = 136,            // <Literal_Bool_List> ::= <Literal_Bool_List> ',' <Literal_Bool>
		LiteralBoolList = 137,                  // <Literal_Bool_List> ::= <Literal_Bool>
		@Optionalexpression = 138,                 // <OptionalExpression> ::= <Expression>
		@Optionalexpression2 = 139,                // <OptionalExpression> ::= 
		@Expression = 140,                         // <Expression> ::= <Expr Bool>
		ExprboolAmpamp = 141,                    // <Expr Bool> ::= <Expr Bool> '&&' <Expr Eq>
		ExprboolPipepipe = 142,                  // <Expr Bool> ::= <Expr Bool> '||' <Expr Eq>
		ExprboolCaret = 143,                     // <Expr Bool> ::= <Expr Bool> '^' <Expr Eq>
		@Exprbool = 144,                           // <Expr Bool> ::= <Expr Eq>
		ExpreqEqeq = 145,                        // <Expr Eq> ::= <Expr Eq> '==' <Exp Comp>
		ExpreqExclameq = 146,                    // <Expr Eq> ::= <Expr Eq> '!=' <Exp Comp>
		@Expreq = 147,                             // <Expr Eq> ::= <Exp Comp>
		ExpcompLt = 148,                         // <Exp Comp> ::= <Exp Comp> '<' <Exp Add>
		ExpcompGt = 149,                         // <Exp Comp> ::= <Exp Comp> '>' <Exp Add>
		ExpcompLteq = 150,                       // <Exp Comp> ::= <Exp Comp> '<=' <Exp Add>
		ExpcompGteq = 151,                       // <Exp Comp> ::= <Exp Comp> '>=' <Exp Add>
		@Expcomp = 152,                            // <Exp Comp> ::= <Exp Add>
		ExpaddPlus = 153,                        // <Exp Add> ::= <Exp Add> '+' <Exp Mult>
		ExpaddMinus = 154,                       // <Exp Add> ::= <Exp Add> '-' <Exp Mult>
		@Expadd = 155,                             // <Exp Add> ::= <Exp Mult>
		ExpmultTimes = 156,                      // <Exp Mult> ::= <Exp Mult> '*' <Exp Unary>
		ExpmultDiv = 157,                        // <Exp Mult> ::= <Exp Mult> '/' <Exp Unary>
		ExpmultPercent = 158,                    // <Exp Mult> ::= <Exp Mult> '%' <Exp Unary>
		@Expmult = 159,                            // <Exp Mult> ::= <Exp Unary>
		ExpunaryExclam = 160,                    // <Exp Unary> ::= '!' <Value>
		ExpunaryMinus = 161,                     // <Exp Unary> ::= '-' <Value>
		ExpunaryLparenRparen = 162,             // <Exp Unary> ::= '(' <Type> ')' <Exp Unary>
		@Expunary = 163,                           // <Exp Unary> ::= <Value>
		@Value = 164,                              // <Value> ::= <Value_Literal>
		@Value2 = 165,                             // <Value> ::= <Exp Rand>
		@Value3 = 166,                             // <Value> ::= <ValuePointer>
		ValueIdentifierLparenRparen = 167,     // <Value> ::= Identifier '(' <ExpressionList> ')'
		ValueIdentifierLparenRparen2 = 168,    // <Value> ::= Identifier '(' ')'
		ValueLparenRparen = 169,                // <Value> ::= '(' <Expression> ')'
		ValuePlusplus = 170,                     // <Value> ::= <ValuePointer> '++'
		ValueMinusminus = 171,                   // <Value> ::= <ValuePointer> '--'
		ValuePlusplus2 = 172,                    // <Value> ::= '++' <ValuePointer>
		ValueMinusminus2 = 173,                  // <Value> ::= '--' <ValuePointer>
		ExprandRand = 174,                       // <Exp Rand> ::= rand
		ExprandRandLbracketRbracket = 175,     // <Exp Rand> ::= rand '[' <Expression> ']'
		ExpressionlistComma = 176,               // <ExpressionList> ::= <ExpressionList> ',' <Expression>
		@Expressionlist = 177,                     // <ExpressionList> ::= <Expression>
		ValuepointerIdentifier = 178,            // <ValuePointer> ::= Identifier
		ValuepointerIdentifierLbracketRbracket = 179,  // <ValuePointer> ::= Identifier '[' <Expression> ']'
		ValuepointerDisplayLbracketCommaRbracket = 180   // <ValuePointer> ::= display '[' <Expression> ',' <Expression> ']'
	}
}
