using BefunGen.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BefunGenTest
{
	[TestClass]
	public class Test_Programs
	{

		[TestMethod]
		public void codeGenTest_Program_MethodCalls()
		{
			BFTestHelper.debugProgram_Terminate(@"
			program testprog
				VAR
					int i;
				BEGIN

					OUT ''\r\nSTART\r\n'';

					ma();
					mb();
					mc();

					OUT ''\r\nFIN\r\n'';

					QUIT;
				END

				VOID ma()
				BEGIN
				
					OUT ''A1'';
					OUT ''A2'';
					OUT ''A3'';
					OUT ''\r\n'';

					RETURN;

				END

				VOID mb()
				BEGIN

					OUT ''B1'';
					OUT ''B2'';
					OUT ''B3'';
					OUT ''\r\n'';

					RETURN;

				END

				VOID mc()
				BEGIN

					OUT ''C1'';
					OUT ''C2'';
					OUT ''C3'';
					OUT ''\r\n'';

					RETURN;

				END
			END
			");
		}

		[TestMethod]
		public void codeGenTest_Program_ParameterMethodCalls()
		{
			BFTestHelper.debugProgram_Terminate(@"
			program example
				begin
					out euclid(44, 12);
				end

				int euclid(int a, int b) 
				begin
					OUT a;
					OUT ''  '';
					OUT b;
					OUT ''  '';
					return 1337;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_ArrayReturn()
		{
			BFTestHelper.debugProgram_Output("Hello", @"
			program example
				begin
					out blub();
				end
			
				char[5] blub()
				var
					char[5] result;
				begin
					result[0] = 'H';
					result[1] = 'e';
					result[2] = 'l';
					result[3] = 'l';
					result[4] = 'o';
					return result;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_ArrayReturn_2()
		{
			BFTestHelper.debugProgram_Output("Hello", @"
			program example
				begin
					out blub();
				end
			
				char[5] blub()
				var
					char[5] result;
				begin
					result[0] = 'H';
					result[1] = 'e';
					result[2] = 'l';
					result[3] = 'l';
					result[4] = 'o';
					OUT '''';
					return result;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_Euklid()
		{
			BFTestHelper.debugProgram_Output("4 ", @"
			program example
				begin
					out euclid(44, 12);
				end

				int euclid(int a, int b) 
				begin
					if (a == 0) then
						return b;
					else 
						if (b == 0) then
							return a;
						else 
							if (a > b) then
								return euclid(a - b, b);
							else
								return euclid(a, b - a);
							end
						end
					end
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_GlobalVar()
		{
			BFTestHelper.debugProgram_Output("99 ", @"
			program example
				global
				 int i;
				begin
					i = 0;
		
					doodle();
		
					OUT i;
				end

				void doodle() 
				begin
					i = 10;
		
					doodle2();
				end
	 
				void doodle2() 
				begin
					i = i * 10;
		
					doodle3();
				end
	 
				void doodle3() 
				begin
					i--;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_Constants()
		{
			BFTestHelper.debugProgram_Output("99 ", @"
			program example
				const
					int FALSCH := 0;
					int WAHR   := 1;
				global
					int i;
				begin
					i = FALSCH;
		
					doodle();
		
					OUT i;
				end

				void doodle() 
				begin
					i = 10;
		
					doodle2();
				end
	 
				void doodle2() 
				begin
					i = i * 10;
		
					doodle3();
				end
	 
				void doodle3() 
				begin
					i = i - WAHR;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_Modulo_Display_Access()
		{
			ASTObject.CGO.DisplayModuloAccess = true;

			BFTestHelper.debugProgram(@"
			program example : display[64, 16]
				begin
					FOR(;;) DO
						paintR();
					END
				end

				void paintR() 
				var
				 int x;
				 int y;
				begin
					x = ((((((((int)RAND)*2) + ((int)RAND))*2 + ((int)RAND) ) * 2 + ((int)RAND)*2) + ((int)RAND))*2 + ((int)RAND) ) * 2 + ((int)RAND);
					y = ((((((((int)RAND)*2) + ((int)RAND))*2 + ((int)RAND) ) * 2 + ((int)RAND)*2) + ((int)RAND))*2 + ((int)RAND) ) * 2 + ((int)RAND);

					OUT x;
					OUT '','';		
					OUT y;
					OUT ''\r\n'';		

					display[x, y] = '#';

					OUT ''\r\n'';

				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_switch_Access()
		{
			ASTObject.CGO.DisplayModuloAccess = true;

			BFTestHelper.debugProgram_Output("WIN", @"
			program example
				begin
					switch (9 * 9 / 9 / 9)
					begin
						case 1: 
							OUT ''WIN'';
						end
						case 4:
							OUT ''FAIL'';
						end
						case 0:
							OUT ''FAIL'';
						end
						case -9:
							OUT ''FAIL'';
						end
						case 111:
							OUT ''FAIL'';
						end
					end
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_switch_Common()
		{
			ASTObject.CGO.DisplayModuloAccess = true;

			BFTestHelper.debugProgram(@"
			program p 
				begin
					for (;;) do
						switch RAND[1]
						begin
							case 0:
								OUT ''0 '';
							end
							case 1:
								OUT ''1 '';
							end
							case 2:
								OUT ''2 '';
							end
							case 3:
								OUT ''3 '';
							end
						end
					end

				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_switch_Common_Reversed()
		{
			ASTObject.CGO.DisplayModuloAccess = true;

			BFTestHelper.debugProgram(@"
			program p 
				begin
					OUT '''';
					for (;;) do
						switch RAND[1]
						begin
							case 0:
								OUT ''0 '';
							end
							case 1:
								OUT ''1 '';
							end
							case 2:
								OUT ''2 '';
							end
							case 3:
								OUT ''3 '';
							end
						end
					end

				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_recursive_calls()
		{
			BFTestHelper.debugProgram_Terminate(@"
			program example : display[0, 0]
				begin
					push(peek());
				end
	
				void push(int v)
				begin
					out ''<in_push>'';
				end

				int peek()
				begin
					out ''<in_peek>'';
					return 42;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_call_order()
		{
			BFTestHelper.debugProgram_Output("ab", @"
			program example : display[0, 0]
				begin
					b(a());
				end
	
				void b(int v)
				begin
					out ''b'';
				end

				int a()
				begin
					out ''a'';
					return 0;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_call_order_2()
		{
			BFTestHelper.debugProgram_Output("aab", @"
			program example
				var 
					int x;
				begin
					x = a() * b(a());
				end
	
				int b(int v)
				begin
					out ''b'';
					return 0;
				end

				int a()
				begin
					out ''a'';
					return 0;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_CompileTimeEvaluation()
		{
			ASTObject.CGO.CompileTimeEvaluateExpressions = true;

			BFTestHelper.debugProgram_Output("", @"
			program example
				var 
					int x;
				begin
					x = 0 * a();
					x = a() * 0;
				end

				int a()
				begin
					out ''a'';
					return 0;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_ArrayParameter()
		{
			BFTestHelper.debugProgram_Output("110308", @"
			program p0
				var 
					digit[6] x := {#1, #1, #2, #3, #5, #8};
				begin
					x[2] = #0;
					x[4] = #0;
					a(x);
				end

				void a(digit[6] px)
				begin
					out b(px, 0);
					out b(px, 1);
					out b(px, 2);
					out b(px, 3);
					out b(px, 4);
					out b(px, 5);
				end

				digit b(digit[6] a, int i)
				var 
					digit[6] tmp;
				begin
					tmp = a;
					return tmp[i];
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_StackIdentity()
		{
			BFTestHelper.debugProgram_Output("4 8 15 16 23 42 ", @"
			program p0
				var 
					stack<int>[16] s;
				begin
					s.Push(42);
					s.Push(identity(16) + identity(8) - 1);
					s.Push(identity(16));
					s.Push(identity(identity(identity(15))));
					s.Push(identity(identity(8)));
					s.Push(identity(4));

					while (!s.empty()) do
						out s.peek();
						s.pop();
					end
				end

				int identity(int i)
				begin
					return i;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_VariableSpace1()
		{
			BFTestHelper.debugProgram_Output("8060 ", @"
			program p0
				var 
					int ii;
					int a, b, c ,d, e, f, g, h, i, j;
					int[100] s1;
					int k, l, m, n, o, p, q, r, s, t;
					int sum := 0;
				begin
					FOR(ii = 0; ii < 100; ii++) DO
						s1[ii] = ii;
					END
					a=101;b=102;c=103;d=104;e=105;f=106;g=107;h=108;i=109;j=110;
					k=201;l=202;m=203;n=204;o=205;p=206;q=207;r=208;s=209;t=210;
					
					FOR(ii = 0; ii < 100; ii++) DO
						sum += s1[ii];
					END
					sum += a+b+c+d+e+f+g+h+i+j;
					sum += k+l+m+n+o+p+q+r+s+t;

					out sum;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_VariableSpace2()
		{
			BFTestHelper.debugProgram_Output("113010 ", @"
			program p0
				var 
					int ii;
					int a, b, c ,d, e, f, g, h, i, j;
					int[100] s1;
					int k, l, m, n, o, p, q, r, s, t;
					int[100] s2;
					int sum := 0;
				begin
					FOR(ii = 0; ii < 100; ii++) DO
						s1[ii] = ii;
					END
					a=101;b=102;c=103;d=104;e=105;f=106;g=107;h=108;i=109;j=110;
					k=201;l=202;m=203;n=204;o=205;p=206;q=207;r=208;s=209;t=210;
					FOR(ii = 0; ii < 100; ii++) DO
						s2[ii] = 1000 + ii;
					END
					
					FOR(ii = 0; ii < 100; ii++) DO
						sum += s1[ii] + s2[ii];
					END
					sum += a+b+c+d+e+f+g+h+i+j;
					sum += k+l+m+n+o+p+q+r+s+t;

					out sum;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_VariableSpace3()
		{
			BFTestHelper.debugProgram_Output("113010 ", @"
			program p0
				var 
					int a, b, c ,d, e, f, g, h, i, j;
					int[100] s1;
					int k, l, m, n, o, p, q, r, s, t;
					int[100] s2;

					int ii;
				begin
					FOR(ii = 0; ii < 100; ii++) DO
						s1[ii] = ii;
					END
					FOR(ii = 0; ii < 100; ii++) DO
						s2[ii] = 1000 + ii;
					END
					

					out sumfunc(s1, s2);
				end

				int sumfunc(int[100] sp1, int[100] sp2)
				var
					int a, b, c ,d, e, f, g, h, i, j;
					int[100] s1;
					int k, l, m, n, o, p, q, r, s, t;
					int[100] s2;

					int sum := 0;
					int ii;
				begin
					a=101;b=102;c=103;d=104;e=105;f=106;g=107;h=108;i=109;j=110;
					k=201;l=202;m=203;n=204;o=205;p=206;q=207;r=208;s=209;t=210;

					s1 = sp1;
					s2 = sp2;

					FOR(ii = 0; ii < 100; ii++) DO
						sum += s1[ii] + s2[ii];
					END
					sum += a+b+c+d+e+f+g+h+i+j;
					sum += k+l+m+n+o+p+q+r+s+t;

					return sum;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_VariableSpace4()
		{
			BFTestHelper.debugProgram_Output("113010 ", @"
			program p0
				var 
					int a, b, c ,d, e, f, g, h, i, j;
					int[100] s1;
					int k, l, m, n, o, p, q, r, s, t;
					int[100] s2;

					int ii;
				begin
					s1 = arrinit(s1, 0000);
					s2 = arrinit(s2, 1000);

					out sumfunc(s1, s2);
				end

				int[100] arrinit(int[100] base, int offset)
				var
					int ii;
					int[100] ss;
				begin
					FOR(ii = 0; ii < 100; ii++) DO
						ss[ii] = base[ii] + offset + ii;
					END
					return ss;
				end

				int sumfunc(int[100] sp1, int[100] sp2)
				var
					int a, b, c ,d, e, f, g, h, i, j;
					int[100] s1;
					int k, l, m, n, o, p, q, r, s, t;
					int[100] s2;

					int sum := 0;
					int ii;
				begin
					a=101;b=102;c=103;d=104;e=105;f=106;g=107;h=108;i=109;j=110;
					k=201;l=202;m=203;n=204;o=205;p=206;q=207;r=208;s=209;t=210;

					s1 = sp1;
					s2 = sp2;

					FOR(ii = 0; ii < 100; ii++) DO
						sum += s1[ii] + s2[ii];
					END
					sum += a+b+c+d+e+f+g+h+i+j;
					sum += k+l+m+n+o+p+q+r+s+t;

					return sum;
				end
			end
			");
		}

		[TestMethod]
		public void codeGenTest_Program_BigArrayLiteral()
		{
			BFTestHelper.debugProgram_Output("576 ", @"
			program p0
				var 
					int i, s;
					int[128] a := 
					{
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,

						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,

						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,

						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8,
						1,2,3,4,5,6,7,8
					};
				begin
					s=0; 
					for (i=0;i<128;s += a[i++]) do end
					out s;
				end
			end
			");
		}
	}
}
