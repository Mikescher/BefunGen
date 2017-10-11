using BefunGen.AST.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BefunGen.AST.DirectRun
{
	public class RunnerEnvironment
	{
		public class RunnerEnvironmentFrame
		{
			public string Name;
			public Dictionary<string, long>       Variables1 = new Dictionary<string, long>();
			public Dictionary<string, List<long>> Variables2 = new Dictionary<string, List<long>>();

			public long? Result;
		}

		public readonly Random Random = new Random();

		public StringBuilder Output = new StringBuilder();

		public long DisplayWidth = 0;
		public long DisplayHeight = 0;
		public long[,] Display = new long[0, 0];
		public readonly Dictionary<string, long>       GlobalVariables1 = new Dictionary<string, long>();
		public readonly Dictionary<string, List<long>> GlobalVariables2 = new Dictionary<string, List<long>>();
		public readonly List<Method> Methods = new List<Method>();

		public readonly Stack<RunnerEnvironmentFrame> StackFrames = new Stack<RunnerEnvironmentFrame>();
		public RunnerEnvironmentFrame CurrentFrame => StackFrames.Peek();

		public StatementLabel JumpTarget = null;

		public bool HasQuit = false;

		public void RegisterMethod(Method ml)
		{
			Methods.Add(ml);
		}

		public void RegisterGlobalVariable(VarDeclaration cc)
		{
			if (cc is VarDeclarationValue)
				GlobalVariables1[cc.Identifier] = cc.Initial.AsNumber();
			else
				GlobalVariables2[cc.Identifier] = cc.Initial.AsNumberList().ToList();
		}

		public void RegisterVariable(VarDeclaration cc, long v)
		{
			CurrentFrame.Variables1[cc.Identifier] = v;
		}

		public void RegisterVariable(VarDeclaration cc, List<long> v)
		{
			CurrentFrame.Variables2[cc.Identifier] = v.ToList();
		}

		public void RegisterVariable(VarDeclaration cc)
		{
			if (cc is VarDeclarationValue)
				CurrentFrame.Variables1[cc.Identifier] = cc.Initial.AsNumber();
			else
				CurrentFrame.Variables2[cc.Identifier] = cc.Initial.AsNumberList().ToList();
		}

		public void StackFrameDown(string name)
		{
			StackFrames.Push(new RunnerEnvironmentFrame() { Name = name });
		}

		public long? StackFrameUp()
		{
			var r = CurrentFrame.Result;

			StackFrames.Pop();

			return r;
		}

		public long GetVariableNumber(VarDeclaration target)
		{
			if (CurrentFrame.Variables1.ContainsKey(target.Identifier)) return CurrentFrame.Variables1[target.Identifier];
			if (GlobalVariables1.ContainsKey(target.Identifier)) return GlobalVariables1[target.Identifier];

			throw new InternalCodeRunException("Variable " + target.Identifier + "  not found in " + CurrentFrame.Name);
		}

		public List<long> GetVariableNumberList(VarDeclaration target)
		{
			if (CurrentFrame.Variables2.ContainsKey(target.Identifier)) return CurrentFrame.Variables2[target.Identifier];
			if (GlobalVariables2.ContainsKey(target.Identifier)) return GlobalVariables2[target.Identifier];

			throw new InternalCodeRunException("Variable " + target.Identifier + "  not found in " + CurrentFrame.Name);
		}

		public void SetVariable(VarDeclaration target, long value)
		{
			if (CurrentFrame.Variables1.ContainsKey(target.Identifier))
			{
				CurrentFrame.Variables1[target.Identifier] = value;
				return;
			}
			if (GlobalVariables1.ContainsKey(target.Identifier))
			{
				GlobalVariables1[target.Identifier] = value;
				return;
			}

			throw new InternalCodeRunException("Variable " + target.Identifier + "  not found in " + CurrentFrame.Name);
		}

		public void SetVariable(VarDeclaration target, List<long> value)
		{
			if (CurrentFrame.Variables2.ContainsKey(target.Identifier))
			{
				CurrentFrame.Variables2[target.Identifier] = value;
				return;
			}
			if (GlobalVariables2.ContainsKey(target.Identifier))
			{
				GlobalVariables2[target.Identifier] = value;
				return;
			}

			throw new InternalCodeRunException("Variable " + target.Identifier + "  not found in " + CurrentFrame.Name);
		}

		public void ResetDisplay(int displayWidth, int displayHeight, string initialDisp, long defaultValue)
		{
			Display = new long[displayWidth, displayHeight];
			DisplayWidth = displayWidth;
			DisplayHeight = displayHeight;

			string[] split = initialDisp.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

			for (int y = 0; y < displayHeight; y++)
			{
				for (int x = 0; x < displayWidth; x++)
				{
					Display[x, y] = (y < split.Length && x < split[y].Length) ? (split[y][x]) : (defaultValue);
				}
			}
		}

		public long GetDisplay(long x, long y)
		{
			if (x < 0 || y < 0 || x >= DisplayWidth || y >= DisplayHeight) throw new InternalCodeRunException("Display access out of bounds");

			return Display[x, y];
		}

		public void SetResult(long v)
		{
			CurrentFrame.Result = v;
		}

		public void WriteOut(char v)
		{
			Output.Append(v);
			Console.Out.Write(v);
		}

		public void WriteOut(long v)
		{
			Output.Append(v + " ");
			Console.Out.Write(v + " ");
		}

		public void WriteOut(char[] v)
		{
			foreach (var c in v)
			{
				Output.Append(c);
				Console.Out.Write(c);
			}
		}
	}
}