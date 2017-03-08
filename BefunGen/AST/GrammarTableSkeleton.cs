
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
		@Stack = 79,                               // stack
		@Stop = 80,                                // stop
		@Stringliteral = 81,                       // StringLiteral
		@Switch = 82,                              // switch
		@Then = 83,                                // then
		@True = 84,                                // true
		@Until = 85,                               // until
		@Var = 86,                                 // var
		@Void = 87,                                // void
		@While = 88,                               // while
		@Array_literal = 89,                       // <Array_Literal>
		@Constants = 90,                           // <Constants>
		@Expadd = 91,                              // <Exp Add>
		@Expcomp = 92,                             // <Exp Comp>
		@Expmult = 93,                             // <Exp Mult>
		@Exprand = 94,                             // <Exp Rand>
		@Expunary = 95,                            // <Exp Unary>
		@Exprbool = 96,                            // <Expr Bool>
		@Expreq = 97,                              // <Expr Eq>
		@Expression = 98,                          // <Expression>
		@Expressionlist = 99,                      // <ExpressionList>
		@Footer = 100,                             // <Footer>
		@Globalvars = 101,                         // <GlobalVars>
		@Header = 102,                             // <Header>
		@Identifierlist = 103,                     // <IdentifierList>
		@Literal = 104,                            // <Literal>
		@Literal_bool = 105,                       // <Literal_Bool>
		@Literal_bool_list = 106,                  // <Literal_Bool_List>
		@Literal_boolarr = 107,                    // <Literal_BoolArr>
		@Literal_char = 108,                       // <Literal_Char>
		@Literal_char_list = 109,                  // <Literal_Char_List>
		@Literal_digit = 110,                      // <Literal_Digit>
		@Literal_digit_list = 111,                 // <Literal_Digit_List>
		@Literal_digitarr = 112,                   // <Literal_DigitArr>
		@Literal_int = 113,                        // <Literal_Int>
		@Literal_int_list = 114,                   // <Literal_Int_List>
		@Literal_intarr = 115,                     // <Literal_IntArr>
		@Literal_string = 116,                     // <Literal_String>
		@Mainstatements = 117,                     // <MainStatements>
		@Method = 118,                             // <Method>
		@Methodbody = 119,                         // <MethodBody>
		@Methodheader = 120,                       // <MethodHeader>
		@Methodlist = 121,                         // <MethodList>
		@Optionalexpression = 122,                 // <OptionalExpression>
		@Optionalsimstatement = 123,               // <OptionalSimStatement>
		@Outflist = 124,                           // <OutfList>
		@Param = 125,                              // <Param>
		@Paramdecl = 126,                          // <ParamDecl>
		@Paramlist = 127,                          // <ParamList>
		@Program2 = 128,                           // <Program>
		@Simplestatement = 129,                    // <SimpleStatement>
		@Statement = 130,                          // <Statement>
		@Statementlist = 131,                      // <StatementList>
		@Stmt_assignment = 132,                    // <Stmt_Assignment>
		@Stmt_call = 133,                          // <Stmt_Call>
		@Stmt_elseiflist = 134,                    // <Stmt_ElseIfList>
		@Stmt_for = 135,                           // <Stmt_For>
		@Stmt_goto = 136,                          // <Stmt_Goto>
		@Stmt_if = 137,                            // <Stmt_If>
		@Stmt_in = 138,                            // <Stmt_In>
		@Stmt_inc = 139,                           // <Stmt_Inc>
		@Stmt_label = 140,                         // <Stmt_Label>
		@Stmt_modassignment = 141,                 // <Stmt_ModAssignment>
		@Stmt_out = 142,                           // <Stmt_Out>
		@Stmt_outf = 143,                          // <Stmt_Outf>
		@Stmt_quit = 144,                          // <Stmt_Quit>
		@Stmt_repeat = 145,                        // <Stmt_Repeat>
		@Stmt_return = 146,                        // <Stmt_Return>
		@Stmt_switch = 147,                        // <Stmt_Switch>
		@Stmt_switch_caselist = 148,               // <Stmt_Switch_CaseList>
		@Stmt_while = 149,                         // <Stmt_While>
		@Type = 150,                               // <Type>
		@Type_bool = 151,                          // <Type_Bool>
		@Type_boolarr = 152,                       // <Type_BoolArr>
		@Type_boolstack = 153,                     // <Type_BoolStack>
		@Type_char = 154,                          // <Type_Char>
		@Type_charstack = 155,                     // <Type_CharStack>
		@Type_digit = 156,                         // <Type_Digit>
		@Type_digitarr = 157,                      // <Type_DigitArr>
		@Type_digitstack = 158,                    // <Type_DigitStack>
		@Type_int = 159,                           // <Type_Int>
		@Type_intarr = 160,                        // <Type_IntArr>
		@Type_intstack = 161,                      // <Type_IntStack>
		@Type_string = 162,                        // <Type_String>
		@Type_void = 163,                          // <Type_Void>
		@Value = 164,                              // <Value>
		@Value_literal = 165,                      // <Value_Literal>
		@Valuepointer = 166,                       // <ValuePointer>
		@Vardecl = 167,                            // <VarDecl>
		@Vardeclbody = 168,                        // <VarDeclBody>
		@Varlist = 169                             // <VarList>
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
		@Statementlist = 47,                       // <StatementList> ::= <StatementList> <Statement>
		@Statementlist2 = 48,                      // <StatementList> ::= 
		@Stmt_inc_Plusplus = 49,                   // <Stmt_Inc> ::= <ValuePointer> '++'
		@Stmt_inc_Minusminus = 50,                 // <Stmt_Inc> ::= <ValuePointer> '--'
		@Stmt_quit_Quit = 51,                      // <Stmt_Quit> ::= quit
		@Stmt_quit_Stop = 52,                      // <Stmt_Quit> ::= stop
		@Stmt_quit_Close = 53,                     // <Stmt_Quit> ::= close
		@Stmt_out_Out = 54,                        // <Stmt_Out> ::= out <Expression>
		@Stmt_out_Out2 = 55,                       // <Stmt_Out> ::= out <Literal_String>
		@Stmt_in_In = 56,                          // <Stmt_In> ::= in <ValuePointer>
		@Stmt_assignment_Eq = 57,                  // <Stmt_Assignment> ::= <ValuePointer> '=' <Expression>
		@Stmt_modassignment_Pluseq = 58,           // <Stmt_ModAssignment> ::= <ValuePointer> '+=' <Expression>
		@Stmt_modassignment_Minuseq = 59,          // <Stmt_ModAssignment> ::= <ValuePointer> '-=' <Expression>
		@Stmt_modassignment_Timeseq = 60,          // <Stmt_ModAssignment> ::= <ValuePointer> '*=' <Expression>
		@Stmt_modassignment_Diveq = 61,            // <Stmt_ModAssignment> ::= <ValuePointer> '/=' <Expression>
		@Stmt_modassignment_Percenteq = 62,        // <Stmt_ModAssignment> ::= <ValuePointer> '%=' <Expression>
		@Stmt_modassignment_Ampeq = 63,            // <Stmt_ModAssignment> ::= <ValuePointer> '&=' <Expression>
		@Stmt_modassignment_Pipeeq = 64,           // <Stmt_ModAssignment> ::= <ValuePointer> '|=' <Expression>
		@Stmt_modassignment_Careteq = 65,          // <Stmt_ModAssignment> ::= <ValuePointer> '^=' <Expression>
		@Stmt_return_Return = 66,                  // <Stmt_Return> ::= return <Expression>
		@Stmt_return_Return2 = 67,                 // <Stmt_Return> ::= return
		@Stmt_call_Identifier_Lparen_Rparen = 68,  // <Stmt_Call> ::= Identifier '(' <ExpressionList> ')'
		@Stmt_call_Identifier_Lparen_Rparen2 = 69,  // <Stmt_Call> ::= Identifier '(' ')'
		@Stmt_outf_Outf = 70,                      // <Stmt_Outf> ::= outf <OutfList>
		@Outflist = 71,                            // <OutfList> ::= <Expression>
		@Outflist2 = 72,                           // <OutfList> ::= <Literal_String>
		@Outflist_Comma = 73,                      // <OutfList> ::= <OutfList> ',' <Expression>
		@Outflist_Comma2 = 74,                     // <OutfList> ::= <OutfList> ',' <Literal_String>
		@Stmt_if_If_Then_End = 75,                 // <Stmt_If> ::= if <Expression> then <StatementList> <Stmt_ElseIfList> end
		@Stmt_elseiflist_Elsif_Then = 76,          // <Stmt_ElseIfList> ::= elsif <Expression> then <StatementList> <Stmt_ElseIfList>
		@Stmt_elseiflist_Else = 77,                // <Stmt_ElseIfList> ::= else <StatementList>
		@Stmt_elseiflist = 78,                     // <Stmt_ElseIfList> ::= 
		@Stmt_while_While_Do_End = 79,             // <Stmt_While> ::= while <Expression> do <StatementList> end
		@Stmt_for_For_Lparen_Semi_Semi_Rparen_Do_End = 80,  // <Stmt_For> ::= for '(' <OptionalSimStatement> ';' <OptionalExpression> ';' <OptionalSimStatement> ')' do <StatementList> end
		@Stmt_repeat_Repeat_Until_Lparen_Rparen = 81,  // <Stmt_Repeat> ::= repeat <StatementList> until '(' <Expression> ')'
		@Stmt_switch_Switch_Begin_End = 82,        // <Stmt_Switch> ::= switch <Expression> begin <Stmt_Switch_CaseList> end
		@Stmt_switch_caselist_Case_Colon_End = 83,  // <Stmt_Switch_CaseList> ::= case <Value_Literal> ':' <StatementList> end <Stmt_Switch_CaseList>
		@Stmt_switch_caselist_Default_Colon_End = 84,  // <Stmt_Switch_CaseList> ::= default ':' <StatementList> end
		@Stmt_switch_caselist = 85,                // <Stmt_Switch_CaseList> ::= 
		@Stmt_goto_Goto_Identifier = 86,           // <Stmt_Goto> ::= goto Identifier
		@Stmt_label_Identifier_Colon = 87,         // <Stmt_Label> ::= Identifier ':'
		@Type = 88,                                // <Type> ::= <Type_Int>
		@Type2 = 89,                               // <Type> ::= <Type_Digit>
		@Type3 = 90,                               // <Type> ::= <Type_Char>
		@Type4 = 91,                               // <Type> ::= <Type_Bool>
		@Type5 = 92,                               // <Type> ::= <Type_Void>
		@Type6 = 93,                               // <Type> ::= <Type_IntArr>
		@Type7 = 94,                               // <Type> ::= <Type_String>
		@Type8 = 95,                               // <Type> ::= <Type_DigitArr>
		@Type9 = 96,                               // <Type> ::= <Type_BoolArr>
		@Type10 = 97,                              // <Type> ::= <Type_IntStack>
		@Type11 = 98,                              // <Type> ::= <Type_DigitStack>
		@Type12 = 99,                              // <Type> ::= <Type_CharStack>
		@Type13 = 100,                             // <Type> ::= <Type_BoolStack>
		@Type_int_Int = 101,                       // <Type_Int> ::= int
		@Type_int_Integer = 102,                   // <Type_Int> ::= integer
		@Type_char_Char = 103,                     // <Type_Char> ::= char
		@Type_char_Character = 104,                // <Type_Char> ::= Character
		@Type_digit_Digit = 105,                   // <Type_Digit> ::= digit
		@Type_bool_Bool = 106,                     // <Type_Bool> ::= bool
		@Type_bool_Boolean = 107,                  // <Type_Bool> ::= boolean
		@Type_void_Void = 108,                     // <Type_Void> ::= void
		@Type_intarr_Lbracket_Rbracket = 109,      // <Type_IntArr> ::= <Type_Int> '[' <Literal_Int> ']'
		@Type_string_Lbracket_Rbracket = 110,      // <Type_String> ::= <Type_Char> '[' <Literal_Int> ']'
		@Type_digitarr_Lbracket_Rbracket = 111,    // <Type_DigitArr> ::= <Type_Digit> '[' <Literal_Int> ']'
		@Type_boolarr_Lbracket_Rbracket = 112,     // <Type_BoolArr> ::= <Type_Bool> '[' <Literal_Int> ']'
		@Type_intstack_Stack_Lt_Gt_Lbracket_Rbracket = 113,  // <Type_IntStack> ::= stack '<' <Type_Int> '>' '[' <Literal_Int> ']'
		@Type_charstack_Stack_Lt_Gt_Lbracket_Rbracket = 114,  // <Type_CharStack> ::= stack '<' <Type_CharStack> '>' '[' <Literal_Int> ']'
		@Type_digitstack_Stack_Lt_Gt_Lbracket_Rbracket = 115,  // <Type_DigitStack> ::= stack '<' <Type_Digit> '>' '[' <Literal_Int> ']'
		@Type_boolstack_Stack_Lt_Gt_Lbracket_Rbracket = 116,  // <Type_BoolStack> ::= stack '<' <Type_Bool> '>' '[' <Literal_Int> ']'
		@Literal = 117,                            // <Literal> ::= <Array_Literal>
		@Literal2 = 118,                           // <Literal> ::= <Value_Literal>
		@Array_literal = 119,                      // <Array_Literal> ::= <Literal_IntArr>
		@Array_literal2 = 120,                     // <Array_Literal> ::= <Literal_String>
		@Array_literal3 = 121,                     // <Array_Literal> ::= <Literal_DigitArr>
		@Array_literal4 = 122,                     // <Array_Literal> ::= <Literal_BoolArr>
		@Value_literal = 123,                      // <Value_Literal> ::= <Literal_Int>
		@Value_literal2 = 124,                     // <Value_Literal> ::= <Literal_Char>
		@Value_literal3 = 125,                     // <Value_Literal> ::= <Literal_Bool>
		@Value_literal4 = 126,                     // <Value_Literal> ::= <Literal_Digit>
		@Literal_int_Decliteral = 127,             // <Literal_Int> ::= DecLiteral
		@Literal_int_Hexliteral = 128,             // <Literal_Int> ::= HexLiteral
		@Literal_char_Charliteral = 129,           // <Literal_Char> ::= CharLiteral
		@Literal_bool_True = 130,                  // <Literal_Bool> ::= true
		@Literal_bool_False = 131,                 // <Literal_Bool> ::= false
		@Literal_digit_Digitliteral = 132,         // <Literal_Digit> ::= DigitLiteral
		@Literal_intarr_Lbrace_Rbrace = 133,       // <Literal_IntArr> ::= '{' <Literal_Int_List> '}'
		@Literal_string_Lbrace_Rbrace = 134,       // <Literal_String> ::= '{' <Literal_Char_List> '}'
		@Literal_string_Stringliteral = 135,       // <Literal_String> ::= StringLiteral
		@Literal_digitarr_Lbrace_Rbrace = 136,     // <Literal_DigitArr> ::= '{' <Literal_Digit_List> '}'
		@Literal_boolarr_Lbrace_Rbrace = 137,      // <Literal_BoolArr> ::= '{' <Literal_Bool_List> '}'
		@Literal_int_list_Comma = 138,             // <Literal_Int_List> ::= <Literal_Int_List> ',' <Literal_Int>
		@Literal_int_list = 139,                   // <Literal_Int_List> ::= <Literal_Int>
		@Literal_char_list_Comma = 140,            // <Literal_Char_List> ::= <Literal_Char_List> ',' <Literal_Char>
		@Literal_char_list = 141,                  // <Literal_Char_List> ::= <Literal_Char>
		@Literal_digit_list_Comma = 142,           // <Literal_Digit_List> ::= <Literal_Digit_List> ',' <Literal_Digit>
		@Literal_digit_list = 143,                 // <Literal_Digit_List> ::= <Literal_Digit>
		@Literal_bool_list_Comma = 144,            // <Literal_Bool_List> ::= <Literal_Bool_List> ',' <Literal_Bool>
		@Literal_bool_list = 145,                  // <Literal_Bool_List> ::= <Literal_Bool>
		@Optionalexpression = 146,                 // <OptionalExpression> ::= <Expression>
		@Optionalexpression2 = 147,                // <OptionalExpression> ::= 
		@Expression = 148,                         // <Expression> ::= <Expr Bool>
		@Exprbool_Ampamp = 149,                    // <Expr Bool> ::= <Expr Bool> '&&' <Expr Eq>
		@Exprbool_Pipepipe = 150,                  // <Expr Bool> ::= <Expr Bool> '||' <Expr Eq>
		@Exprbool_Caret = 151,                     // <Expr Bool> ::= <Expr Bool> '^' <Expr Eq>
		@Exprbool = 152,                           // <Expr Bool> ::= <Expr Eq>
		@Expreq_Eqeq = 153,                        // <Expr Eq> ::= <Expr Eq> '==' <Exp Comp>
		@Expreq_Exclameq = 154,                    // <Expr Eq> ::= <Expr Eq> '!=' <Exp Comp>
		@Expreq = 155,                             // <Expr Eq> ::= <Exp Comp>
		@Expcomp_Lt = 156,                         // <Exp Comp> ::= <Exp Comp> '<' <Exp Add>
		@Expcomp_Gt = 157,                         // <Exp Comp> ::= <Exp Comp> '>' <Exp Add>
		@Expcomp_Lteq = 158,                       // <Exp Comp> ::= <Exp Comp> '<=' <Exp Add>
		@Expcomp_Gteq = 159,                       // <Exp Comp> ::= <Exp Comp> '>=' <Exp Add>
		@Expcomp = 160,                            // <Exp Comp> ::= <Exp Add>
		@Expadd_Plus = 161,                        // <Exp Add> ::= <Exp Add> '+' <Exp Mult>
		@Expadd_Minus = 162,                       // <Exp Add> ::= <Exp Add> '-' <Exp Mult>
		@Expadd = 163,                             // <Exp Add> ::= <Exp Mult>
		@Expmult_Times = 164,                      // <Exp Mult> ::= <Exp Mult> '*' <Exp Unary>
		@Expmult_Div = 165,                        // <Exp Mult> ::= <Exp Mult> '/' <Exp Unary>
		@Expmult_Percent = 166,                    // <Exp Mult> ::= <Exp Mult> '%' <Exp Unary>
		@Expmult = 167,                            // <Exp Mult> ::= <Exp Unary>
		@Expunary_Exclam = 168,                    // <Exp Unary> ::= '!' <Value>
		@Expunary_Minus = 169,                     // <Exp Unary> ::= '-' <Value>
		@Expunary_Lparen_Rparen = 170,             // <Exp Unary> ::= '(' <Type> ')' <Exp Unary>
		@Expunary = 171,                           // <Exp Unary> ::= <Value>
		@Value = 172,                              // <Value> ::= <Value_Literal>
		@Value2 = 173,                             // <Value> ::= <Exp Rand>
		@Value3 = 174,                             // <Value> ::= <ValuePointer>
		@Value_Identifier_Lparen_Rparen = 175,     // <Value> ::= Identifier '(' <ExpressionList> ')'
		@Value_Identifier_Lparen_Rparen2 = 176,    // <Value> ::= Identifier '(' ')'
		@Value_Lparen_Rparen = 177,                // <Value> ::= '(' <Expression> ')'
		@Value_Plusplus = 178,                     // <Value> ::= <ValuePointer> '++'
		@Value_Minusminus = 179,                   // <Value> ::= <ValuePointer> '--'
		@Value_Plusplus2 = 180,                    // <Value> ::= '++' <ValuePointer>
		@Value_Minusminus2 = 181,                  // <Value> ::= '--' <ValuePointer>
		@Exprand_Rand = 182,                       // <Exp Rand> ::= rand
		@Exprand_Rand_Lbracket_Rbracket = 183,     // <Exp Rand> ::= rand '[' <Expression> ']'
		@Expressionlist_Comma = 184,               // <ExpressionList> ::= <ExpressionList> ',' <Expression>
		@Expressionlist = 185,                     // <ExpressionList> ::= <Expression>
		@Valuepointer_Identifier = 186,            // <ValuePointer> ::= Identifier
		@Valuepointer_Identifier_Lbracket_Rbracket = 187,  // <ValuePointer> ::= Identifier '[' <Expression> ']'
		@Valuepointer_Display_Lbracket_Comma_Rbracket = 188   // <ValuePointer> ::= display '[' <Expression> ',' <Expression> ']'
	}
}
