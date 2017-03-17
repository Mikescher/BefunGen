
namespace BefunGen.AST
{
	public enum SymbolIndex
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
		@Dot = 21,                                 // '.'
		@Div = 22,                                 // '/'
		@Diveq = 23,                               // '/='
		@Colon = 24,                               // ':'
		@Coloneq = 25,                             // ':='
		@Semi = 26,                                // ';'
		@Lbracket = 27,                            // '['
		@Rbracket = 28,                            // ']'
		@Caret = 29,                               // '^'
		@Careteq = 30,                             // '^='
		@Lbrace = 31,                              // '{'
		@Pipepipe = 32,                            // '||'
		@Pipeeq = 33,                              // '|='
		@Rbrace = 34,                              // '}'
		@Plus = 35,                                // '+'
		@Plusplus = 36,                            // '++'
		@Pluseq = 37,                              // '+='
		@Lt = 38,                                  // '<'
		@Lteq = 39,                                // '<='
		@Eq = 40,                                  // '='
		@Minuseq = 41,                             // '-='
		@Eqeq = 42,                                // '=='
		@Gt = 43,                                  // '>'
		@Gteq = 44,                                // '>='
		@Begin = 45,                               // begin
		@Bool = 46,                                // bool
		@Boolean = 47,                             // boolean
		@Case = 48,                                // case
		@Char = 49,                                // char
		@Character = 50,                           // Character
		@Charliteral = 51,                         // CharLiteral
		@Close = 52,                               // close
		@Const = 53,                               // const
		@Decliteral = 54,                          // DecLiteral
		@Default = 55,                             // default
		@Digit = 56,                               // digit
		@Digitliteral = 57,                        // DigitLiteral
		@Display = 58,                             // display
		@Do = 59,                                  // do
		@Else = 60,                                // else
		@Elsif = 61,                               // elsif
		@End = 62,                                 // end
		@False = 63,                               // false
		@For = 64,                                 // for
		@Global = 65,                              // global
		@Goto = 66,                                // goto
		@Hexliteral = 67,                          // HexLiteral
		@Identifier = 68,                          // Identifier
		@If = 69,                                  // if
		@In = 70,                                  // in
		@Int = 71,                                 // int
		@Integer = 72,                             // integer
		@Out = 73,                                 // out
		@Outf = 74,                                // outf
		@Program = 75,                             // program
		@Quit = 76,                                // quit
		@Rand = 77,                                // rand
		@Repeat = 78,                              // repeat
		@Return = 79,                              // return
		@Stack = 80,                               // stack
		@Stop = 81,                                // stop
		@Stringliteral = 82,                       // StringLiteral
		@Switch = 83,                              // switch
		@Then = 84,                                // then
		@True = 85,                                // true
		@Until = 86,                               // until
		@Var = 87,                                 // var
		@Void = 88,                                // void
		@While = 89,                               // while
		@Array_literal = 90,                       // <Array_Literal>
		@Constants = 91,                           // <Constants>
		@Expadd = 92,                              // <Exp Add>
		@Expcomp = 93,                             // <Exp Comp>
		@Expmult = 94,                             // <Exp Mult>
		@Exprand = 95,                             // <Exp Rand>
		@Expunary = 96,                            // <Exp Unary>
		@Exprbool = 97,                            // <Expr Bool>
		@Expreq = 98,                              // <Expr Eq>
		@Expression = 99,                          // <Expression>
		@Expressionlist = 100,                     // <ExpressionList>
		@Footer = 101,                             // <Footer>
		@Globalvars = 102,                         // <GlobalVars>
		@Header = 103,                             // <Header>
		@Identifierlist = 104,                     // <IdentifierList>
		@Literal = 105,                            // <Literal>
		@Literal_bool = 106,                       // <Literal_Bool>
		@Literal_bool_list = 107,                  // <Literal_Bool_List>
		@Literal_boolarr = 108,                    // <Literal_BoolArr>
		@Literal_char = 109,                       // <Literal_Char>
		@Literal_char_list = 110,                  // <Literal_Char_List>
		@Literal_digit = 111,                      // <Literal_Digit>
		@Literal_digit_list = 112,                 // <Literal_Digit_List>
		@Literal_digitarr = 113,                   // <Literal_DigitArr>
		@Literal_int = 114,                        // <Literal_Int>
		@Literal_int_list = 115,                   // <Literal_Int_List>
		@Literal_intarr = 116,                     // <Literal_IntArr>
		@Literal_string = 117,                     // <Literal_String>
		@Mainstatements = 118,                     // <MainStatements>
		@Method = 119,                             // <Method>
		@Methodbody = 120,                         // <MethodBody>
		@Methodheader = 121,                       // <MethodHeader>
		@Methodlist = 122,                         // <MethodList>
		@Optionalexpression = 123,                 // <OptionalExpression>
		@Optionalsimstatement = 124,               // <OptionalSimStatement>
		@Outflist = 125,                           // <OutfList>
		@Param = 126,                              // <Param>
		@Paramdecl = 127,                          // <ParamDecl>
		@Paramlist = 128,                          // <ParamList>
		@Program2 = 129,                           // <Program>
		@Simplestatement = 130,                    // <SimpleStatement>
		@Statement = 131,                          // <Statement>
		@Statementlist = 132,                      // <StatementList>
		@Stmt_assignment = 133,                    // <Stmt_Assignment>
		@Stmt_call = 134,                          // <Stmt_Call>
		@Stmt_elseiflist = 135,                    // <Stmt_ElseIfList>
		@Stmt_for = 136,                           // <Stmt_For>
		@Stmt_goto = 137,                          // <Stmt_Goto>
		@Stmt_if = 138,                            // <Stmt_If>
		@Stmt_in = 139,                            // <Stmt_In>
		@Stmt_inc = 140,                           // <Stmt_Inc>
		@Stmt_label = 141,                         // <Stmt_Label>
		@Stmt_modassignment = 142,                 // <Stmt_ModAssignment>
		@Stmt_out = 143,                           // <Stmt_Out>
		@Stmt_outf = 144,                          // <Stmt_Outf>
		@Stmt_quit = 145,                          // <Stmt_Quit>
		@Stmt_repeat = 146,                        // <Stmt_Repeat>
		@Stmt_return = 147,                        // <Stmt_Return>
		@Stmt_subcall = 148,                       // <Stmt_Subcall>
		@Stmt_switch = 149,                        // <Stmt_Switch>
		@Stmt_switch_caselist = 150,               // <Stmt_Switch_CaseList>
		@Stmt_while = 151,                         // <Stmt_While>
		@Type = 152,                               // <Type>
		@Type_bool = 153,                          // <Type_Bool>
		@Type_boolarr = 154,                       // <Type_BoolArr>
		@Type_boolstack = 155,                     // <Type_BoolStack>
		@Type_char = 156,                          // <Type_Char>
		@Type_charstack = 157,                     // <Type_CharStack>
		@Type_digit = 158,                         // <Type_Digit>
		@Type_digitarr = 159,                      // <Type_DigitArr>
		@Type_digitstack = 160,                    // <Type_DigitStack>
		@Type_int = 161,                           // <Type_Int>
		@Type_intarr = 162,                        // <Type_IntArr>
		@Type_intstack = 163,                      // <Type_IntStack>
		@Type_string = 164,                        // <Type_String>
		@Type_void = 165,                          // <Type_Void>
		@Value = 166,                              // <Value>
		@Value_literal = 167,                      // <Value_Literal>
		@Valuepointer = 168,                       // <ValuePointer>
		@Vardecl = 169,                            // <VarDecl>
		@Vardeclbody = 170,                        // <VarDeclBody>
		@Varlist = 171                             // <VarList>
	}

	public enum ProductionIndex
	{
		@Program = 0,                              // <Program> ::= <Header> <Constants> <GlobalVars> <MainStatements> <MethodList> <Footer>
		@Header_Program_Identifier = 1,            // <Header> ::= program Identifier
		@Header_Program_Identifier_Colon_Display_Lbracket_Comma_Rbracket = 2,  // <Header> ::= program Identifier ':' display '[' <Literal_Int> ',' <Literal_Int> ']'
		@Footer_End = 3,                           // <Footer> ::= end
		@Constants_Const = 4,                      // <Constants> ::= const <VarList>
		@Constants = 5,                            // <Constants> ::= 
		@Globalvars_Global = 6,                    // <GlobalVars> ::= global <VarList>
		@Globalvars = 7,                           // <GlobalVars> ::= 
		@Methodlist = 8,                           // <MethodList> ::= <MethodList> <Method>
		@Methodlist2 = 9,                          // <MethodList> ::= 
		@Mainstatements = 10,                      // <MainStatements> ::= <MethodBody>
		@Method = 11,                              // <Method> ::= <MethodHeader> <MethodBody>
		@Methodbody = 12,                          // <MethodBody> ::= <VarDeclBody> <Statement>
		@Methodheader_Identifier_Lparen_Rparen = 13,  // <MethodHeader> ::= <Type> Identifier '(' <ParamDecl> ')'
		@Vardeclbody_Var = 14,                     // <VarDeclBody> ::= var <VarList>
		@Vardeclbody = 15,                         // <VarDeclBody> ::= 
		@Paramdecl = 16,                           // <ParamDecl> ::= <ParamList>
		@Paramdecl2 = 17,                          // <ParamDecl> ::= 
		@Paramlist_Comma = 18,                     // <ParamList> ::= <ParamList> ',' <Param>
		@Paramlist = 19,                           // <ParamList> ::= <Param>
		@Param_Identifier = 20,                    // <Param> ::= <Type> Identifier
		@Varlist_Semi = 21,                        // <VarList> ::= <VarList> <VarDecl> ';'
		@Varlist_Semi2 = 22,                       // <VarList> ::= <VarDecl> ';'
		@Vardecl = 23,                             // <VarDecl> ::= <Type> <IdentifierList>
		@Vardecl_Identifier_Coloneq = 24,          // <VarDecl> ::= <Type> Identifier ':=' <Literal>
		@Identifierlist_Comma_Identifier = 25,     // <IdentifierList> ::= <IdentifierList> ',' Identifier
		@Identifierlist_Identifier = 26,           // <IdentifierList> ::= Identifier
		@Optionalsimstatement = 27,                // <OptionalSimStatement> ::= <SimpleStatement>
		@Optionalsimstatement2 = 28,               // <OptionalSimStatement> ::= 
		@Statement_Semi = 29,                      // <Statement> ::= <SimpleStatement> ';'
		@Statement_Begin_End = 30,                 // <Statement> ::= begin <StatementList> end
		@Statement = 31,                           // <Statement> ::= <Stmt_If>
		@Statement2 = 32,                          // <Statement> ::= <Stmt_While>
		@Statement3 = 33,                          // <Statement> ::= <Stmt_For>
		@Statement4 = 34,                          // <Statement> ::= <Stmt_Repeat>
		@Statement_Semi2 = 35,                     // <Statement> ::= <Stmt_Goto> ';'
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
		@Simplestatement10 = 47,                   // <SimpleStatement> ::= <Stmt_Subcall>
		@Statementlist = 48,                       // <StatementList> ::= <StatementList> <Statement>
		@Statementlist2 = 49,                      // <StatementList> ::= 
		@Stmt_inc_Plusplus = 50,                   // <Stmt_Inc> ::= <ValuePointer> '++'
		@Stmt_inc_Minusminus = 51,                 // <Stmt_Inc> ::= <ValuePointer> '--'
		@Stmt_quit_Quit = 52,                      // <Stmt_Quit> ::= quit
		@Stmt_quit_Stop = 53,                      // <Stmt_Quit> ::= stop
		@Stmt_quit_Close = 54,                     // <Stmt_Quit> ::= close
		@Stmt_out_Out = 55,                        // <Stmt_Out> ::= out <Expression>
		@Stmt_out_Out2 = 56,                       // <Stmt_Out> ::= out <Literal_String>
		@Stmt_in_In = 57,                          // <Stmt_In> ::= in <ValuePointer>
		@Stmt_assignment_Eq = 58,                  // <Stmt_Assignment> ::= <ValuePointer> '=' <Expression>
		@Stmt_modassignment_Pluseq = 59,           // <Stmt_ModAssignment> ::= <ValuePointer> '+=' <Expression>
		@Stmt_modassignment_Minuseq = 60,          // <Stmt_ModAssignment> ::= <ValuePointer> '-=' <Expression>
		@Stmt_modassignment_Timeseq = 61,          // <Stmt_ModAssignment> ::= <ValuePointer> '*=' <Expression>
		@Stmt_modassignment_Diveq = 62,            // <Stmt_ModAssignment> ::= <ValuePointer> '/=' <Expression>
		@Stmt_modassignment_Percenteq = 63,        // <Stmt_ModAssignment> ::= <ValuePointer> '%=' <Expression>
		@Stmt_modassignment_Ampeq = 64,            // <Stmt_ModAssignment> ::= <ValuePointer> '&=' <Expression>
		@Stmt_modassignment_Pipeeq = 65,           // <Stmt_ModAssignment> ::= <ValuePointer> '|=' <Expression>
		@Stmt_modassignment_Careteq = 66,          // <Stmt_ModAssignment> ::= <ValuePointer> '^=' <Expression>
		@Stmt_return_Return = 67,                  // <Stmt_Return> ::= return <Expression>
		@Stmt_return_Return2 = 68,                 // <Stmt_Return> ::= return
		@Stmt_call_Identifier_Lparen_Rparen = 69,  // <Stmt_Call> ::= Identifier '(' <ExpressionList> ')'
		@Stmt_call_Identifier_Lparen_Rparen2 = 70,  // <Stmt_Call> ::= Identifier '(' ')'
		@Stmt_outf_Outf = 71,                      // <Stmt_Outf> ::= outf <OutfList>
		@Stmt_subcall_Identifier_Dot_Identifier_Lparen_Rparen = 72,  // <Stmt_Subcall> ::= Identifier '.' Identifier '(' <ExpressionList> ')'
		@Stmt_subcall_Identifier_Dot_Identifier_Lparen_Rparen2 = 73,  // <Stmt_Subcall> ::= Identifier '.' Identifier '(' ')'
		@Outflist = 74,                            // <OutfList> ::= <Expression>
		@Outflist2 = 75,                           // <OutfList> ::= <Literal_String>
		@Outflist_Comma = 76,                      // <OutfList> ::= <OutfList> ',' <Expression>
		@Outflist_Comma2 = 77,                     // <OutfList> ::= <OutfList> ',' <Literal_String>
		@Stmt_if_If_Then_End = 78,                 // <Stmt_If> ::= if <Expression> then <StatementList> <Stmt_ElseIfList> end
		@Stmt_elseiflist_Elsif_Then = 79,          // <Stmt_ElseIfList> ::= elsif <Expression> then <StatementList> <Stmt_ElseIfList>
		@Stmt_elseiflist_Else = 80,                // <Stmt_ElseIfList> ::= else <StatementList>
		@Stmt_elseiflist = 81,                     // <Stmt_ElseIfList> ::= 
		@Stmt_while_While_Do_End = 82,             // <Stmt_While> ::= while <Expression> do <StatementList> end
		@Stmt_for_For_Lparen_Semi_Semi_Rparen_Do_End = 83,  // <Stmt_For> ::= for '(' <OptionalSimStatement> ';' <OptionalExpression> ';' <OptionalSimStatement> ')' do <StatementList> end
		@Stmt_repeat_Repeat_Until_Lparen_Rparen = 84,  // <Stmt_Repeat> ::= repeat <StatementList> until '(' <Expression> ')'
		@Stmt_switch_Switch_Begin_End = 85,        // <Stmt_Switch> ::= switch <Expression> begin <Stmt_Switch_CaseList> end
		@Stmt_switch_caselist_Case_Colon_End = 86,  // <Stmt_Switch_CaseList> ::= case <Value_Literal> ':' <StatementList> end <Stmt_Switch_CaseList>
		@Stmt_switch_caselist_Default_Colon_End = 87,  // <Stmt_Switch_CaseList> ::= default ':' <StatementList> end
		@Stmt_switch_caselist = 88,                // <Stmt_Switch_CaseList> ::= 
		@Stmt_goto_Goto_Identifier = 89,           // <Stmt_Goto> ::= goto Identifier
		@Stmt_label_Identifier_Colon = 90,         // <Stmt_Label> ::= Identifier ':'
		@Type = 91,                                // <Type> ::= <Type_Int>
		@Type2 = 92,                               // <Type> ::= <Type_Digit>
		@Type3 = 93,                               // <Type> ::= <Type_Char>
		@Type4 = 94,                               // <Type> ::= <Type_Bool>
		@Type5 = 95,                               // <Type> ::= <Type_Void>
		@Type6 = 96,                               // <Type> ::= <Type_IntArr>
		@Type7 = 97,                               // <Type> ::= <Type_String>
		@Type8 = 98,                               // <Type> ::= <Type_DigitArr>
		@Type9 = 99,                               // <Type> ::= <Type_BoolArr>
		@Type10 = 100,                             // <Type> ::= <Type_IntStack>
		@Type11 = 101,                             // <Type> ::= <Type_DigitStack>
		@Type12 = 102,                             // <Type> ::= <Type_CharStack>
		@Type13 = 103,                             // <Type> ::= <Type_BoolStack>
		@Type_int_Int = 104,                       // <Type_Int> ::= int
		@Type_int_Integer = 105,                   // <Type_Int> ::= integer
		@Type_char_Char = 106,                     // <Type_Char> ::= char
		@Type_char_Character = 107,                // <Type_Char> ::= Character
		@Type_digit_Digit = 108,                   // <Type_Digit> ::= digit
		@Type_bool_Bool = 109,                     // <Type_Bool> ::= bool
		@Type_bool_Boolean = 110,                  // <Type_Bool> ::= boolean
		@Type_void_Void = 111,                     // <Type_Void> ::= void
		@Type_intarr_Lbracket_Rbracket = 112,      // <Type_IntArr> ::= <Type_Int> '[' <Literal_Int> ']'
		@Type_string_Lbracket_Rbracket = 113,      // <Type_String> ::= <Type_Char> '[' <Literal_Int> ']'
		@Type_digitarr_Lbracket_Rbracket = 114,    // <Type_DigitArr> ::= <Type_Digit> '[' <Literal_Int> ']'
		@Type_boolarr_Lbracket_Rbracket = 115,     // <Type_BoolArr> ::= <Type_Bool> '[' <Literal_Int> ']'
		@Type_intstack_Stack_Lt_Gt_Lbracket_Rbracket = 116,  // <Type_IntStack> ::= stack '<' <Type_Int> '>' '[' <Literal_Int> ']'
		@Type_charstack_Stack_Lt_Gt_Lbracket_Rbracket = 117,  // <Type_CharStack> ::= stack '<' <Type_CharStack> '>' '[' <Literal_Int> ']'
		@Type_digitstack_Stack_Lt_Gt_Lbracket_Rbracket = 118,  // <Type_DigitStack> ::= stack '<' <Type_Digit> '>' '[' <Literal_Int> ']'
		@Type_boolstack_Stack_Lt_Gt_Lbracket_Rbracket = 119,  // <Type_BoolStack> ::= stack '<' <Type_Bool> '>' '[' <Literal_Int> ']'
		@Literal = 120,                            // <Literal> ::= <Array_Literal>
		@Literal2 = 121,                           // <Literal> ::= <Value_Literal>
		@Array_literal = 122,                      // <Array_Literal> ::= <Literal_IntArr>
		@Array_literal2 = 123,                     // <Array_Literal> ::= <Literal_String>
		@Array_literal3 = 124,                     // <Array_Literal> ::= <Literal_DigitArr>
		@Array_literal4 = 125,                     // <Array_Literal> ::= <Literal_BoolArr>
		@Value_literal = 126,                      // <Value_Literal> ::= <Literal_Int>
		@Value_literal2 = 127,                     // <Value_Literal> ::= <Literal_Char>
		@Value_literal3 = 128,                     // <Value_Literal> ::= <Literal_Bool>
		@Value_literal4 = 129,                     // <Value_Literal> ::= <Literal_Digit>
		@Literal_int_Decliteral = 130,             // <Literal_Int> ::= DecLiteral
		@Literal_int_Hexliteral = 131,             // <Literal_Int> ::= HexLiteral
		@Literal_char_Charliteral = 132,           // <Literal_Char> ::= CharLiteral
		@Literal_bool_True = 133,                  // <Literal_Bool> ::= true
		@Literal_bool_False = 134,                 // <Literal_Bool> ::= false
		@Literal_digit_Digitliteral = 135,         // <Literal_Digit> ::= DigitLiteral
		@Literal_intarr_Lbrace_Rbrace = 136,       // <Literal_IntArr> ::= '{' <Literal_Int_List> '}'
		@Literal_string_Lbrace_Rbrace = 137,       // <Literal_String> ::= '{' <Literal_Char_List> '}'
		@Literal_string_Stringliteral = 138,       // <Literal_String> ::= StringLiteral
		@Literal_digitarr_Lbrace_Rbrace = 139,     // <Literal_DigitArr> ::= '{' <Literal_Digit_List> '}'
		@Literal_boolarr_Lbrace_Rbrace = 140,      // <Literal_BoolArr> ::= '{' <Literal_Bool_List> '}'
		@Literal_int_list_Comma = 141,             // <Literal_Int_List> ::= <Literal_Int_List> ',' <Literal_Int>
		@Literal_int_list = 142,                   // <Literal_Int_List> ::= <Literal_Int>
		@Literal_char_list_Comma = 143,            // <Literal_Char_List> ::= <Literal_Char_List> ',' <Literal_Char>
		@Literal_char_list = 144,                  // <Literal_Char_List> ::= <Literal_Char>
		@Literal_digit_list_Comma = 145,           // <Literal_Digit_List> ::= <Literal_Digit_List> ',' <Literal_Digit>
		@Literal_digit_list = 146,                 // <Literal_Digit_List> ::= <Literal_Digit>
		@Literal_bool_list_Comma = 147,            // <Literal_Bool_List> ::= <Literal_Bool_List> ',' <Literal_Bool>
		@Literal_bool_list = 148,                  // <Literal_Bool_List> ::= <Literal_Bool>
		@Optionalexpression = 149,                 // <OptionalExpression> ::= <Expression>
		@Optionalexpression2 = 150,                // <OptionalExpression> ::= 
		@Expression = 151,                         // <Expression> ::= <Expr Bool>
		@Exprbool_Ampamp = 152,                    // <Expr Bool> ::= <Expr Bool> '&&' <Expr Eq>
		@Exprbool_Pipepipe = 153,                  // <Expr Bool> ::= <Expr Bool> '||' <Expr Eq>
		@Exprbool_Caret = 154,                     // <Expr Bool> ::= <Expr Bool> '^' <Expr Eq>
		@Exprbool = 155,                           // <Expr Bool> ::= <Expr Eq>
		@Expreq_Eqeq = 156,                        // <Expr Eq> ::= <Expr Eq> '==' <Exp Comp>
		@Expreq_Exclameq = 157,                    // <Expr Eq> ::= <Expr Eq> '!=' <Exp Comp>
		@Expreq = 158,                             // <Expr Eq> ::= <Exp Comp>
		@Expcomp_Lt = 159,                         // <Exp Comp> ::= <Exp Comp> '<' <Exp Add>
		@Expcomp_Gt = 160,                         // <Exp Comp> ::= <Exp Comp> '>' <Exp Add>
		@Expcomp_Lteq = 161,                       // <Exp Comp> ::= <Exp Comp> '<=' <Exp Add>
		@Expcomp_Gteq = 162,                       // <Exp Comp> ::= <Exp Comp> '>=' <Exp Add>
		@Expcomp = 163,                            // <Exp Comp> ::= <Exp Add>
		@Expadd_Plus = 164,                        // <Exp Add> ::= <Exp Add> '+' <Exp Mult>
		@Expadd_Minus = 165,                       // <Exp Add> ::= <Exp Add> '-' <Exp Mult>
		@Expadd = 166,                             // <Exp Add> ::= <Exp Mult>
		@Expmult_Times = 167,                      // <Exp Mult> ::= <Exp Mult> '*' <Exp Unary>
		@Expmult_Div = 168,                        // <Exp Mult> ::= <Exp Mult> '/' <Exp Unary>
		@Expmult_Percent = 169,                    // <Exp Mult> ::= <Exp Mult> '%' <Exp Unary>
		@Expmult = 170,                            // <Exp Mult> ::= <Exp Unary>
		@Expunary_Exclam = 171,                    // <Exp Unary> ::= '!' <Value>
		@Expunary_Minus = 172,                     // <Exp Unary> ::= '-' <Value>
		@Expunary_Lparen_Rparen = 173,             // <Exp Unary> ::= '(' <Type> ')' <Exp Unary>
		@Expunary = 174,                           // <Exp Unary> ::= <Value>
		@Value = 175,                              // <Value> ::= <Value_Literal>
		@Value2 = 176,                             // <Value> ::= <ValuePointer>
		@Value_Identifier_Lparen_Rparen = 177,     // <Value> ::= Identifier '(' <ExpressionList> ')'
		@Value_Identifier_Lparen_Rparen2 = 178,    // <Value> ::= Identifier '(' ')'
		@Value_Lparen_Rparen = 179,                // <Value> ::= '(' <Expression> ')'
		@Value_Plusplus = 180,                     // <Value> ::= <ValuePointer> '++'
		@Value_Minusminus = 181,                   // <Value> ::= <ValuePointer> '--'
		@Value_Plusplus2 = 182,                    // <Value> ::= '++' <ValuePointer>
		@Value_Minusminus2 = 183,                  // <Value> ::= '--' <ValuePointer>
		@Value_Identifier_Dot_Identifier_Lparen_Rparen = 184,  // <Value> ::= Identifier '.' Identifier '(' <ExpressionList> ')'
		@Value_Identifier_Dot_Identifier_Lparen_Rparen2 = 185,  // <Value> ::= Identifier '.' Identifier '(' ')'
		@Value3 = 186,                             // <Value> ::= <Exp Rand>
		@Exprand_Rand = 187,                       // <Exp Rand> ::= rand
		@Exprand_Rand_Lbracket_Rbracket = 188,     // <Exp Rand> ::= rand '[' <Expression> ']'
		@Expressionlist_Comma = 189,               // <ExpressionList> ::= <ExpressionList> ',' <Expression>
		@Expressionlist = 190,                     // <ExpressionList> ::= <Expression>
		@Valuepointer_Identifier = 191,            // <ValuePointer> ::= Identifier
		@Valuepointer_Identifier_Lbracket_Rbracket = 192,  // <ValuePointer> ::= Identifier '[' <Expression> ']'
		@Valuepointer_Display_Lbracket_Comma_Rbracket = 193   // <ValuePointer> ::= display '[' <Expression> ',' <Expression> ']'
	}
}
