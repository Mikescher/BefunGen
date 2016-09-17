using BefunGen.AST.CodeGen;
using BefunGen.AST.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BefunGen.AST
{
	public static class GrammarTableMap
	{
		public static ASTObject CreateNewASTObject(GOLD.Reduction r, GOLD.Position goldpos)
		{
			ASTObject result = null;

			SourceCodePosition p = new SourceCodePosition(goldpos);

			switch ((ProductionIndex)r.Parent.TableIndex()) // Regex for empty cases:     ^\s+//[^\r]+\r\n\s+break;
			{
				case ProductionIndex.Program:
					// <Program> ::= <Program> ::= <Header> <Constants> <GlobalVars> <MainStatements> <MethodList> <Footer>
					result = new Program(p, (ProgramHeader)r.get_Data(0), ((ListVarDeclarations)r.get_Data(1)).List, ((ListVarDeclarations)r.get_Data(2)).List, (Method)r.get_Data(3), ((ListMethods)r.get_Data(4)).List);
					break;

				case ProductionIndex.HeaderProgramIdentifier:
					// <Header> ::= program Identifier
					result = new ProgramHeader(p, GetStrData(1, r));
					break;

				case ProductionIndex.HeaderProgramIdentifierColonDisplayLbracketCommaRbracket:
					//  <Header> ::= program Identifier ':' display '[' <Literal_Int> ',' <Literal_Int> ']'
					result = new ProgramHeader(p, GetStrData(1, r), (int)((LiteralInt)r.get_Data(5)).Value, (int)((LiteralInt)r.get_Data(7)).Value);
					break;

				case ProductionIndex.ConstantsConst:
					// <Constants> ::= const <VarList>
					result = (ListVarDeclarations)r.get_Data(1);
					break;

				case ProductionIndex.Constants:
					// <Constants> ::= 
					result = new ListVarDeclarations(p);
					break;

				case ProductionIndex.GlobalvarsGlobal:
					// <GlobalVars> ::= global <VarList>
					result = (ListVarDeclarations)r.get_Data(1);
					break;

				case ProductionIndex.Globalvars:
					// <GlobalVars> ::= 
					result = new ListVarDeclarations(p);
					break;

				case ProductionIndex.FooterEnd:
					// <Footer> ::= end
					result = new ProgramFooter(p);
					break;

				case ProductionIndex.Methodlist:
					// <MethodList> ::= <MethodList> <Method>
					result = ((ListMethods)r.get_Data(0)).Append((Method)r.get_Data(1));
					break;

				case ProductionIndex.Methodlist2:
					// <MethodList> ::=
					result = new ListMethods(p);
					break;

				case ProductionIndex.Mainstatements:
					// <MainStatements> ::= <MethodBody>
					result = new Method(p, new MethodHeader(p, new BTypeVoid(p), "main", new List<VarDeclaration>()), (MethodBody)r.get_Data(0));
					break;

				case ProductionIndex.Method:
					// <Method> ::= <MethodHeader> <MethodBody>
					result = new Method(p, (MethodHeader)r.get_Data(0), (MethodBody)r.get_Data(1));
					break;

				case ProductionIndex.Methodbody:
					// <MethodBody> ::= <VarDeclBody> <Statement>
					result = new MethodBody(p, ((ListVarDeclarations)r.get_Data(0)).List, StmtToStmtList((Statement)r.get_Data(1)));
					break;

				case ProductionIndex.MethodheaderIdentifierLparenRparen:
					// <MethodHeader> ::= <Type> Identifier '(' <ParamDecl> ')'
					result = new MethodHeader(p, (BType)r.get_Data(0), GetStrData(1, r), ((ListVarDeclarations)r.get_Data(3)).List);
					break;

				case ProductionIndex.VardeclbodyVar:
					// <VarDeclBody> ::= var <VarList>
					result = (ListVarDeclarations)r.get_Data(1);
					break;

				case ProductionIndex.Vardeclbody:
					// <VarDeclBody> ::=
					result = new ListVarDeclarations(p);
					break;

				case ProductionIndex.Paramdecl:
					// <ParamDecl> ::= <ParamList>
					result = (ListVarDeclarations)r.get_Data(0);
					break;

				case ProductionIndex.Paramdecl2:
					// <ParamDecl> ::=
					result = new ListVarDeclarations(p);
					break;

				case ProductionIndex.ParamlistComma:
					// <ParamList> ::= <ParamList> ',' <Param>
					result = ((ListVarDeclarations)r.get_Data(0)).Append((VarDeclaration)r.get_Data(2));
					break;

				case ProductionIndex.Paramlist:
					// <ParamList> ::= <Param>
					result = new ListVarDeclarations(p, (VarDeclaration)r.get_Data(0));
					break;

				case ProductionIndex.ParamIdentifier:
					// <Param> ::= <Type> Identifier
					result = CreateAstDeclarationFromReduction(r, false, p);
					break;

				case ProductionIndex.VarlistSemi:
					// <VarList> ::= <VarList> <VarDecl> ';'
					result = ((ListVarDeclarations)r.get_Data(0)).Append((ListVarDeclarations)r.get_Data(1));
					break;

				case ProductionIndex.VarlistSemi2:
					// <VarList> ::= <VarDecl> ';'
					result = (ListVarDeclarations)r.get_Data(0);
					break;

				case ProductionIndex.Vardecl:
					// <VarDecl> ::= <Type> <IdentifierList>
					result = new ListVarDeclarations(p);
					((ListIdentifier)r.get_Data(1)).List.ForEach(lp => ((ListVarDeclarations)result).Append(CreateAstDeclarationFromValues((BType)r.get_Data(0), lp, null, p)));
					break;

				case ProductionIndex.VardeclIdentifierColoneq:
					// <VarDecl> ::= <Type> Identifier ':=' <Literal>
					result = new ListVarDeclarations(p, CreateAstDeclarationFromReduction(r, true, p));
					break;

				case ProductionIndex.IdentifierlistCommaIdentifier:
					// <IdentifierList> ::= <IdentifierList> ',' Identifier
					result = ((ListIdentifier)r.get_Data(0)).Append(GetStrData(2, r));
					break;

				case ProductionIndex.IdentifierlistIdentifier:
					// <IdentifierList> ::= Identifier
					result = new ListIdentifier(p, GetStrData(0, r));
					break;

				case ProductionIndex.Optionalsimstatement:
					// <OptionalSimStatement> ::= <SimpleStatement>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Optionalsimstatement2:
					// <OptionalSimStatement> ::= 
					result = new StatementNOP(p);
					break;

				case ProductionIndex.StatementSemi:
					// <Statement> ::= <Statement> ::= <SimpleStatement> ';'
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.StatementBeginEnd:
					// <Statement> ::= begin <StatementList> end
					result = GetStmtListAsStatement(p, r, 1);
					break;

				case ProductionIndex.Statement:
					// <Statement> ::= <Stmt_If>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Statement2:
					// <Statement> ::= <Stmt_While>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Statement3:
					// <Statement> ::= <Stmt_For>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Statement4:
					// <Statement> ::= <Stmt_Repeat>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.StatementSemi2:
					// <Statement> ::= <Stmt_Goto> ';'
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Statement5:
					// <Statement> ::= <Stmt_Label>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Statement6:
					// <Statement> ::= <Stmt_Switch>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement:
					// <SimpleStatement> ::= <Stmt_Quit>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement2:
					// <SimpleStatement> ::= <Stmt_Return>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement3:
					// <SimpleStatement> ::= <Stmt_Out>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement4:
					// <SimpleStatement> ::= <Stmt_In>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement5:
					// <SimpleStatement> ::= <Stmt_Inc>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement6:
					// <SimpleStatement> ::= <Stmt_Assignment>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement7:
					// <SimpleStatement> ::= <Stmt_Call>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement8:
					// <SimpleStatement> ::= <Stmt_ModAssignment>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Simplestatement9:
					// <SimpleStatement> ::= <Stmt_Outf>
					result = (Statement)r.get_Data(0);
					break;

				case ProductionIndex.Statementlist:
					// <StatementList> ::= <StatementList> <Statement>
					result = ((ListStatements)r.get_Data(0)).Append((Statement)r.get_Data(1));
					break;

				case ProductionIndex.Statementlist2:
					// <StatementList> ::=
					result = new ListStatements(p);
					break;

				case ProductionIndex.StmtIncPlusplus:
					// <Stmt_Inc> ::= <ValuePointer> '++'
					result = new StatementInc(p, (ExpressionValuePointer)r.get_Data(0));
					break;

				case ProductionIndex.StmtIncMinusminus:
					// <Stmt_Inc> ::= <ValuePointer> '--'
					result = new StatementDec(p, (ExpressionValuePointer)r.get_Data(0));
					break;

				case ProductionIndex.StmtQuitQuit:
					// <Stmt_Quit> ::= quit
					result = new StatementQuit(p);
					break;

				case ProductionIndex.StmtQuitStop:
					// <Stmt_Quit> ::= stop
					result = new StatementQuit(p);
					break;

				case ProductionIndex.StmtQuitClose:
					// <Stmt_Quit> ::= close
					result = new StatementQuit(p);
					break;

				case ProductionIndex.StmtOutOut:
					// <Stmt_Out> ::= out <Expression>
					result = new StatementOut(p, (Expression)r.get_Data(1));
					break;

				case ProductionIndex.StmtOutOut2:
					// <Stmt_Out> ::= out <Literal_String>
					result = new StatementOutCharArrLiteral(p, (LiteralCharArr)r.get_Data(1));
					break;

				case ProductionIndex.StmtInIn:
					// <Stmt_In> ::= in <ValuePointer>
					result = new StatementIn(p, (ExpressionValuePointer)r.get_Data(1));
					break;

				case ProductionIndex.StmtAssignmentEq:
					// <Stmt_Assignment> ::= <ValuePointer> '=' <Expression>
					result = new StatementAssignment(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtModassignmentPluseq:
					// <Stmt_ModAssignment> ::= <ValuePointer> '+=' <Expression>
					result = ExpressionAdd.CreateAugmentedStatement(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtModassignmentMinuseq:
					// <Stmt_ModAssignment> ::= <ValuePointer> '-=' <Expression>
					result = ExpressionSub.CreateAugmentedStatement(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtModassignmentTimeseq:
					// <Stmt_ModAssignment> ::= <ValuePointer> '*=' <Expression>
					result = ExpressionMult.CreateAugmentedStatement(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtModassignmentDiveq:
					// <Stmt_ModAssignment> ::= <ValuePointer> '/=' <Expression>
					result = ExpressionDiv.CreateAugmentedStatement(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtModassignmentPercenteq:
					// <Stmt_ModAssignment> ::= <ValuePointer> '%=' <Expression>
					result = ExpressionMod.CreateAugmentedStatement(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtModassignmentAmpeq:
					// <Stmt_ModAssignment> ::= <ValuePointer> '&=' <Expression>
					result = ExpressionAnd.CreateAugmentedStatement(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtModassignmentPipeeq:
					// <Stmt_ModAssignment> ::= <ValuePointer> '|=' <Expression>
					result = ExpressionOr.CreateAugmentedStatement(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtModassignmentCareteq:
					// <Stmt_ModAssignment> ::= <ValuePointer> '^=' <Expression>
					result = ExpressionXor.CreateAugmentedStatement(p, (ExpressionValuePointer)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.StmtReturnReturn:
					// <Stmt_Return> ::= return <Expression>
					result = new StatementReturn(p, (Expression)r.get_Data(1));
					break;

				case ProductionIndex.StmtReturnReturn2:
					// <Stmt_Return> ::= return
					result = new StatementReturn(p);
					break;

				case ProductionIndex.StmtCallIdentifierLparenRparen:
					// <Stmt_Call> ::= Identifier '(' <ExpressionList> ')'
					result = new StatementMethodCall(p, GetStrData(r), ((ListExpressions)r.get_Data(2)).List);
					break;

				case ProductionIndex.StmtCallIdentifierLparenRparen2:
					// <Stmt_Call> ::= Identifier '(' ')'
					result = new StatementMethodCall(p, GetStrData(r));
					break;

				case ProductionIndex.StmtOutfOutf:
					// <Stmt_Outf> ::= outf <OutfList>
					result = OutfListToStmtList(p, (ListOutfElements)r.get_Data(1));
					break;

				case ProductionIndex.Outflist:
					// <OutfList> ::= <Expression>
					result = new ListOutfElements(p, (Expression)r.get_Data(0));
					break;

				case ProductionIndex.Outflist2:
					// <OutfList> ::= <Literal_String>
					result = new ListOutfElements(p, (LiteralCharArr)r.get_Data(0));
					break;

				case ProductionIndex.OutflistComma:
					// <OutfList> ::= <OutfList> ',' <Expression>
					result = ((ListOutfElements)r.get_Data(0)).Append((Expression)r.get_Data(2));
					break;

				case ProductionIndex.OutflistComma2:
					// <OutfList> ::= <OutfList> ',' <Literal_String>
					result = ((ListOutfElements)r.get_Data(0)).Append((LiteralCharArr)r.get_Data(2));
					break;

				case ProductionIndex.StmtIfIfThenEnd:
					// <Stmt_If> ::= if <Expression> then <StatementList> <Stmt_ElseIfList> end
					result = new StatementIf(p, (Expression)r.get_Data(1), GetStmtListAsStatement(p, r, 3), (Statement)r.get_Data(4));
					break;

				case ProductionIndex.StmtElseiflistElsifThen:
					// <Stmt_ElseIfList> ::= elsif <Expression> then <StatementList> <Stmt_ElseIfList>
					result = new StatementIf(p, (Expression)r.get_Data(1), GetStmtListAsStatement(p, r, 3), (Statement)r.get_Data(4));
					break;

				case ProductionIndex.StmtElseiflistElse:
					// <Stmt_ElseIfList> ::= else <StatementList>
					result = GetStmtListAsStatement(p, r, 1);
					break;

				case ProductionIndex.StmtElseiflist:
					// <Stmt_ElseIfList> ::=
					result = new StatementNOP(p);
					break;

				case ProductionIndex.StmtWhileWhileDoEnd:
					// <Stmt_While> ::= while <Expression> do <StatementList> end
					result = new StatementWhile(p, (Expression)r.get_Data(1), GetStmtListAsStatement(p, r, 3));
					break;

				case ProductionIndex.StmtForForLparenSemiSemiRparenDoEnd:
					// <Stmt_For> ::= for '(' <OptionalSimStatement> ';' <OptionalExpression> ';' <OptionalSimStatement> ')' do <StatementList> end
					result = StatementWhile.GenerateForLoop(p, (Statement)r.get_Data(2), (Expression)r.get_Data(4), (Statement)r.get_Data(6), GetStmtListAsStatement(p, r, 9));
					break;

				case ProductionIndex.StmtRepeatRepeatUntilLparenRparen:
					// <Stmt_Repeat> ::= repeat <StatementList> until '(' <Expression> ')'
					result = new StatementRepeatUntil(p, (Expression)r.get_Data(4), GetStmtListAsStatement(p, r, 1));
					break;

				case ProductionIndex.StmtSwitchSwitchBeginEnd:
					// <Stmt_Switch> ::= switch <Expression> begin <Stmt_Switch_CaseList> end
					result = new StatementSwitch(p, (Expression)r.get_Data(1), (ListSwitchs)r.get_Data(3));
					break;

				case ProductionIndex.StmtSwitchCaselistCaseColonEnd:
					// <Stmt_Switch_CaseList> ::= case <Value_Literal> ':' <StatementList> end <Stmt_Switch_CaseList>
					result = ((ListSwitchs)r.get_Data(5)).Prepend((LiteralValue)r.get_Data(1), GetStmtListAsStatement(p, r, 3));
					break;

				case ProductionIndex.StmtSwitchCaselistDefaultColonEnd:
					// <Stmt_Switch_CaseList> ::= default ':' <StatementList> end
					result = new ListSwitchs(p, null, GetStmtListAsStatement(p, r, 2));
					break;

				case ProductionIndex.StmtSwitchCaselist:
					// <Stmt_Switch_CaseList> ::= 
					result = new ListSwitchs(p);
					break;

				case ProductionIndex.StmtGotoGotoIdentifier:
					// <Stmt_Goto> ::= goto Identifier
					result = new StatementGoto(p, GetStrData(1, r));
					break;

				case ProductionIndex.StmtLabelIdentifierColon:
					// <Stmt_Label> ::= Identifier ':'
					result = new StatementLabel(p, GetStrData(r));
					break;

				case ProductionIndex.Type:
					// <Type> ::= <Type_Int>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.Type2:
					// <Type> ::= <Type_Digit>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.Type3:
					// <Type> ::= <Type_Char>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.Type4:
					// <Type> ::= <Type_Bool>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.Type5:
					// <Type> ::= <Type_Void>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.Type6:
					// <Type> ::= <Type_IntArr>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.Type7:
					// <Type> ::= <Type_String>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.Type8:
					// <Type> ::= <Type_DigitArr>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.Type9:
					// <Type> ::= <Type_BoolArr>
					result = (BType)r.get_Data(0);
					break;

				case ProductionIndex.TypeIntInt:
					// <Type_Int> ::= int
					result = new BTypeInt(p);
					break;

				case ProductionIndex.TypeIntInteger:
					// <Type_Int> ::= integer
					result = new BTypeInt(p);
					break;

				case ProductionIndex.TypeCharChar:
					// <Type_Char> ::= char
					result = new BTypeChar(p);
					break;

				case ProductionIndex.TypeCharCharacter:
					// <Type_Char> ::= Character
					result = new BTypeChar(p);
					break;

				case ProductionIndex.TypeDigitDigit:
					// <Type_Digit> ::= digit
					result = new BTypeDigit(p);
					break;

				case ProductionIndex.TypeBoolBool:
					// <Type_Bool> ::= bool
					result = new BTypeBool(p);
					break;

				case ProductionIndex.TypeBoolBoolean:
					// <Type_Bool> ::= boolean
					result = new BTypeBool(p);
					break;

				case ProductionIndex.TypeVoidVoid:
					// <Type_Void> ::= void
					result = new BTypeVoid(p);
					break;

				case ProductionIndex.TypeIntarrLbracketRbracket:
					// <Type_IntArr> ::= <Type_Int> '[' <Literal_Int> ']'
					result = new BTypeIntArr(p, (int)((LiteralInt)r.get_Data(2)).Value);
					break;

				case ProductionIndex.TypeStringLbracketRbracket:
					// <Type_String> ::= <Type_Char> '[' <Literal_Int> ']'
					result = new BTypeCharArr(p, (int)((LiteralInt)r.get_Data(2)).Value);
					break;

				case ProductionIndex.TypeDigitarrLbracketRbracket:
					// <Type_DigitArr> ::= <Type_Digit> '[' <Literal_Int> ']'
					result = new BTypeDigitArr(p, (int)((LiteralInt)r.get_Data(2)).Value);
					break;

				case ProductionIndex.TypeBoolarrLbracketRbracket:
					// <Type_BoolArr> ::= <Type_Bool> '[' <Literal_Int> ']'
					result = new BTypeBoolArr(p, (int)((LiteralInt)r.get_Data(2)).Value);
					break;

				case ProductionIndex.Literal:
					// <Literal> ::= <Array_Literal>
					result = (Literal)r.get_Data(0);
					break;

				case ProductionIndex.Literal2:
					// <Literal> ::= <Value_Literal>
					result = (Literal)r.get_Data(0);
					break;

				case ProductionIndex.ArrayLiteral:
					// <Array_Literal> ::= <Literal_IntArr>
					result = (LiteralArray)r.get_Data(0);
					break;

				case ProductionIndex.ArrayLiteral2:
					// <Array_Literal> ::= <Literal_String>
					result = (LiteralArray)r.get_Data(0);
					break;

				case ProductionIndex.ArrayLiteral3:
					// <Array_Literal> ::= <Literal_DigitArr>
					result = (LiteralArray)r.get_Data(0);
					break;

				case ProductionIndex.ArrayLiteral4:
					// <Array_Literal> ::= <Literal_BoolArr>
					result = (LiteralArray)r.get_Data(0);
					break;

				case ProductionIndex.ValueLiteral:
					// <Value_Literal> ::= <Literal_Int>
					result = (LiteralValue)r.get_Data(0);
					break;

				case ProductionIndex.ValueLiteral2:
					// <Value_Literal> ::= <Literal_Char>
					result = (LiteralValue)r.get_Data(0);
					break;

				case ProductionIndex.ValueLiteral3:
					// <Value_Literal> ::= <Literal_Bool>
					result = (LiteralValue)r.get_Data(0);
					break;

				case ProductionIndex.ValueLiteral4:
					// <Value_Literal> ::= <Literal_Digit>
					result = (LiteralValue)r.get_Data(0);
					break;

				case ProductionIndex.LiteralIntDecliteral:
					// <Literal_Int> ::= DecLiteral
					result = new LiteralInt(p, Convert.ToInt64(GetStrData(r), 10));
					break;

				case ProductionIndex.LiteralIntHexliteral:
					// <Literal_Int> ::= HexLiteral
					result = new LiteralInt(p, Convert.ToInt64(GetStrData(r), 16));
					break;

				case ProductionIndex.LiteralCharCharliteral:
					// <Literal_Char> ::= CharLiteral
					result = new LiteralChar(p, UnescapeChr(p, GetStrTrimData(r)));
					break;

				case ProductionIndex.LiteralBoolTrue:
					// <Literal_Bool> ::= true
					result = new LiteralBool(p, true);
					break;

				case ProductionIndex.LiteralBoolFalse:
					// <Literal_Bool> ::= false
					result = new LiteralBool(p, false);
					break;

				case ProductionIndex.LiteralDigitDigitliteral:
					// <Literal_Digit> ::= DigitLiteral
					result = new LiteralDigit(p, Convert.ToByte(GetStrData(r).Substring(1), 10));
					break;

				case ProductionIndex.LiteralIntarrLbraceRbrace:
					// <Literal_IntArr> ::= '{' <Literal_Int_List> '}'
					result = new LiteralIntArr(p, ((ListLiteralInts)r.get_Data(1)).List.Select(c => c.Value).ToList());
					break;

				case ProductionIndex.LiteralStringLbraceRbrace:
					// <Literal_String> ::= '{' <Literal_Char_List> '}'
					result = new LiteralCharArr(p, ((ListLiteralChars)r.get_Data(1)).List.Select(c => c.Value).ToList());
					break;

				case ProductionIndex.LiteralStringStringliteral:
					// <Literal_String> ::= StringLiteral
					result = new LiteralCharArr(p, UnescapeStr(p, GetStrTrimData(r)));
					break;

				case ProductionIndex.LiteralDigitarrLbraceRbrace:
					// <Literal_DigitArr> ::= '{' <Literal_Digit_List> '}'
					result = new LiteralDigitArr(p, ((ListLiteralDigits)r.get_Data(1)).List.Select(c => c.Value).ToList());
					break;

				case ProductionIndex.LiteralBoolarrLbraceRbrace:
					// <Literal_BoolArr> ::= '{' <Literal_Bool_List> '}'
					result = new LiteralBoolArr(p, ((ListLiteralBools)r.get_Data(1)).List.Select(c => c.Value).ToList());
					break;

				case ProductionIndex.LiteralIntListComma:
					// <Literal_Int_List> ::= <Literal_Int_List> ',' <Literal_Int>
					result = ((ListLiteralInts)r.get_Data(0)).Append((LiteralInt)r.get_Data(2));
					break;

				case ProductionIndex.LiteralIntList:
					// <Literal_Int_List> ::= <Literal_Int>
					result = new ListLiteralInts(p, (LiteralInt)r.get_Data(0));
					break;

				case ProductionIndex.LiteralCharListComma:
					// <Literal_Char_List> ::= <Literal_Char_List> ',' <Literal_Char>
					result = ((ListLiteralChars)r.get_Data(0)).Append((LiteralChar)r.get_Data(2));
					break;

				case ProductionIndex.LiteralCharList:
					// <Literal_Char_List> ::= <Literal_Char>
					result = new ListLiteralChars(p, (LiteralChar)r.get_Data(0));
					break;

				case ProductionIndex.LiteralDigitListComma:
					// <Literal_Digit_List> ::= <Literal_Digit_List> ',' <Literal_Digit>
					result = ((ListLiteralDigits)r.get_Data(0)).Append((LiteralDigit)r.get_Data(2));
					break;

				case ProductionIndex.LiteralDigitList:
					// <Literal_Digit_List> ::= <Literal_Digit>
					result = new ListLiteralDigits(p, (LiteralDigit)r.get_Data(0));
					break;

				case ProductionIndex.LiteralBoolListComma:
					// <Literal_Bool_List> ::= <Literal_Bool_List> ',' <Literal_Bool>
					result = ((ListLiteralBools)r.get_Data(0)).Append((LiteralBool)r.get_Data(2));
					break;

				case ProductionIndex.LiteralBoolList:
					// <Literal_Bool_List> ::= <Literal_Bool>
					result = new ListLiteralBools(p, (LiteralBool)r.get_Data(0));
					break;

				case ProductionIndex.Optionalexpression:
					// <OptionalExpression> ::= <Expression>
					result = (Expression)r.get_Data(0);
					break;

				case ProductionIndex.Optionalexpression2:
					// <OptionalExpression> ::= 
					result = new ExpressionLiteral(p, new LiteralBool(p, true));
					break;

				case ProductionIndex.Expression:
					// <Expression> ::= <Expr Bool>
					result = (Expression)r.get_Data(0);
					break;

				case ProductionIndex.ExprboolAmpamp:
					// <Expr Bool> ::= <Expr Bool> '&&' <Expr Eq>
					result = new ExpressionAnd(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExprboolPipepipe:
					// <Expr Bool> ::= <Expr Bool> '||' <Expr Eq>
					result = new ExpressionOr(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExprboolCaret:
					// <Expr Bool> ::= <Expr Bool> '^' <Expr Eq>
					result = new ExpressionXor(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.Exprbool:
					// <Expr Bool> ::= <Expr Eq>
					result = (Expression)r.get_Data(0);
					break;

				case ProductionIndex.ExpreqEqeq:
					// <Expr Eq> ::= <Expr Eq> '==' <Exp Comp>
					result = new ExpressionEquals(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExpreqExclameq:
					// <Expr Eq> ::= <Expr Eq> '!=' <Exp Comp>
					result = new ExpressionUnequals(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.Expreq:
					// <Expr Eq> ::= <Exp Comp>
					result = (Expression)r.get_Data(0);
					break;

				case ProductionIndex.ExpcompLt:
					// <Exp Comp> ::= <Exp Comp> '<' <Exp Add>
					result = new ExpressionLesser(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExpcompGt:
					// <Exp Comp> ::= <Exp Comp> '>' <Exp Add>
					result = new ExpressionGreater(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExpcompLteq:
					// <Exp Comp> ::= <Exp Comp> '<=' <Exp Add>
					result = new ExpressionLesserEquals(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExpcompGteq:
					// <Exp Comp> ::= <Exp Comp> '>=' <Exp Add>
					result = new ExpressionGreaterEquals(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.Expcomp:
					// <Exp Comp> ::= <Exp Add>
					result = (Expression)r.get_Data(0);
					break;

				case ProductionIndex.ExpaddPlus:
					// <Exp Add> ::= <Exp Add> '+' <Exp Mult>
					result = new ExpressionAdd(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExpaddMinus:
					// <Exp Add> ::= <Exp Add> '-' <Exp Mult>
					result = new ExpressionSub(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.Expadd:
					// <Exp Add> ::= <Exp Mult>
					result = (Expression)r.get_Data(0);
					break;

				case ProductionIndex.ExpmultTimes:
					// <Exp Mult> ::= <Exp Mult> '*' <Exp Unary>
					result = new ExpressionMult(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExpmultDiv:
					// <Exp Mult> ::= <Exp Mult> '/' <Exp Unary>
					result = new ExpressionDiv(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExpmultPercent:
					// <Exp Mult> ::= <Exp Mult> '%' <Exp Unary>
					result = new ExpressionMod(p, (Expression)r.get_Data(0), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.Expmult:
					// <Exp Mult> ::= <Exp Unary>
					result = (Expression)r.get_Data(0);
					break;

				case ProductionIndex.ExpunaryExclam:
					// <Exp Unary> ::= '!' <Value>
					result = new ExpressionNot(p, (Expression)r.get_Data(1));
					break;

				case ProductionIndex.ExpunaryMinus:
					// <Exp Unary> ::= '-' <Value>
					result = new ExpressionNegate(p, (Expression)r.get_Data(1));
					break;

				case ProductionIndex.ExpunaryLparenRparen:
					// <Exp Unary> ::= '(' <Type> ')' <Exp Unary>
					result = new ExpressionCast(p, (BType)r.get_Data(1), (Expression)r.get_Data(3));
					break;

				case ProductionIndex.Expunary:
					// <Exp Unary> ::= <Value>
					result = (Expression)r.get_Data(0);
					break;

				case ProductionIndex.Value:
					// <Value> ::= <Value_Literal>
					result = new ExpressionLiteral(p, (LiteralValue)r.get_Data(0));
					break;

				case ProductionIndex.Value2:
					// <Value> ::= <Exp Rand>
					result = (ExpressionRand)r.get_Data(0);
					break;

				case ProductionIndex.Value3:
					// <Value> ::= <ValuePointer>
					result = (ExpressionValuePointer)r.get_Data(0);
					break;

				case ProductionIndex.ValueIdentifierLparenRparen:
					// <Value> ::= Identifier '(' <ExpressionList> ')'
					result = new ExpressionFunctionCall(p, new StatementMethodCall(p, GetStrData(r), ((ListExpressions)r.get_Data(2)).List));
					break;

				case ProductionIndex.ValueIdentifierLparenRparen2:
					// <Value> ::= Identifier '(' ')'
					result = new ExpressionFunctionCall(p, new StatementMethodCall(p, GetStrData(r)));
					break;

				case ProductionIndex.ValueLparenRparen:
					// <Value> ::= '(' <Expression> ')'
					result = (Expression)r.get_Data(1);
					break;

				case ProductionIndex.ValuePlusplus:
					// <Value> ::= <ValuePointer> '++'
					result = new ExpressionPostIncrement(p, (ExpressionValuePointer)r.get_Data(0));
					break;

				case ProductionIndex.ValueMinusminus:
					// <Value> ::= <ValuePointer> '--'
					result = new ExpressionPostDecrement(p, (ExpressionValuePointer)r.get_Data(0));
					break;

				case ProductionIndex.ValuePlusplus2:
					// <Value> ::= '++' <ValuePointer>
					result = new ExpressionPreIncrement(p, (ExpressionValuePointer)r.get_Data(1));
					break;

				case ProductionIndex.ValueMinusminus2:
					// <Value> ::= '--' <ValuePointer>
					result = new ExpressionPreDecrement(p, (ExpressionValuePointer)r.get_Data(1));
					break;

				case ProductionIndex.ExprandRand:
					// <Exp Rand> ::= rand
					result = new ExpressionBooleanRand(p);
					break;

				case ProductionIndex.ExprandRandLbracketRbracket:
					// <Exp Rand> ::= rand '[' <Expression> ']'
					result = new ExpressionBase4Rand(p, (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ExpressionlistComma:
					// <ExpressionList> ::= <ExpressionList> ',' <Expression>
					result = ((ListExpressions)r.get_Data(0)).Append((Expression)r.get_Data(2));
					break;

				case ProductionIndex.Expressionlist:
					// <ExpressionList> ::= <Expression>
					result = new ListExpressions(p, (Expression)r.get_Data(0));
					break;

				case ProductionIndex.ValuepointerIdentifier:
					// <ValuePointer> ::= Identifier
					result = new ExpressionDirectValuePointer(p, GetStrData(r));
					break;

				case ProductionIndex.ValuepointerIdentifierLbracketRbracket:
					// <ValuePointer> ::= Identifier '[' <Expression> ']'
					result = new ExpressionArrayValuePointer(p, GetStrData(r), (Expression)r.get_Data(2));
					break;

				case ProductionIndex.ValuepointerDisplayLbracketCommaRbracket:
					// <ValuePointer> ::= display '[' <Expression> ',' <Expression> ']'
					result = new ExpressionDisplayValuePointer(p, (Expression)r.get_Data(2), (Expression)r.get_Data(4));
					break;

			}  //switch

			if (result == null)
			{
				throw new MissingReductionRuleException(r.Parent.ToString());
			}

			return result;
		}

		private static VarDeclaration CreateAstDeclarationFromReduction(GOLD.Reduction r, bool hasInit, SourceCodePosition p)
		{
			return CreateAstDeclarationFromValues((BType)r.get_Data(0), GetStrData(1, r), hasInit ? (Literal)r.get_Data(3) : null, p);
		}

		private static VarDeclaration CreateAstDeclarationFromValues(BType type, string ident, Literal initArr, SourceCodePosition p)
		{
			if (initArr != null)
			{
				if (type is BTypeArray)
					return new VarDeclarationArray(p, (BTypeArray)type, ident, (LiteralArray)initArr);
				else if (type is BTypeValue)
					return new VarDeclarationValue(p, (BTypeValue)type, ident, (LiteralValue)initArr);
				else
					return null;
			}
			else
			{
				if (type is BTypeArray)
					return new VarDeclarationArray(p, (BTypeArray)type, ident);
				else if (type is BTypeValue)
					return new VarDeclarationValue(p, (BTypeValue)type, ident);
				else
					return null;
			}
		}

		private static string GetStrData(GOLD.Reduction r)
		{
			return GetStrData(0, r);
		}

		private static string GetStrData(int p, GOLD.Reduction r)
		{
			if (r.get_Data(p) == null)
			{
				Console.Beep();
			}
			return (string)r.get_Data(p);
		}

		private static string UnescapeStr(SourceCodePosition p, string s)
		{
			StringBuilder outstr = new StringBuilder();

			bool esc = false;
			foreach (char chr in s)
			{
				if (esc)
				{
					outstr.Append(UnescapeChr(p, "\\" + chr));

					esc = false;
				}
				else
				{
					if (chr == '\\')
						esc = true;
					else
						outstr.Append(chr);
				}
			}

			return outstr.ToString();
		}

		private static char UnescapeChr(SourceCodePosition p, string s)
		{
			if (s.Length == 1)
			{
				return s[0];
			}
			else if (s.Length == 2 && s[0] == '\\')
			{
				switch (s[1])
				{
					case 'r':
						return '\r';
					case 'n':
						return '\n';
					case '0':
						return '\0';
					case 'b':
						return '\b';
					case 'v':
						return '\v';
					case 'f':
						return '\f';
					case 'a':
						return '\a';
					case '\\':
						return '\\';
					default:
						throw new InvalidFormatSpecifierException(s, p);
				}
			}
			else
			{
				throw new InvalidFormatSpecifierException(s, p);
			}
		}

		private static string GetStrTrimData(GOLD.Reduction r)
		{
			string s = GetStrData(r);
			return s.Substring(1, s.Length - 2);
		}

		private static StatementStatementList GetStmtListAsStatement(SourceCodePosition p, GOLD.Reduction r, int pos)
		{
			return new StatementStatementList(p, ((ListStatements)r.get_Data(pos)).List);
		}

		private static StatementStatementList StmtToStmtList(Statement s)
		{
			if (s is StatementStatementList)
				return s as StatementStatementList;
			else
				return new StatementStatementList(s.Position, new List<Statement>() { s });
		}

		private static StatementStatementList OutfListToStmtList(SourceCodePosition p, ListOutfElements s)
		{
			return new StatementStatementList(p, s.List.Select(lp => lp.CreateStatement()).ToList());
		}
	}
}