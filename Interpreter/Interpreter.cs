using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Unit = System.ValueTuple;

namespace Basic
{
    public class Interpreter
    {
        private static readonly Command End = new Command.End(-1);

        private readonly TextReader _input;
        private readonly TextWriter _output;
        private readonly List<Command> _program;
        private readonly TextReader _intSource;
        private readonly IDictionary<int, int> _lineIndices; // written line number -> index in _program
        private readonly IDictionary<string, int> _variables;
        private readonly Stack<Command.RealizedFor> _forCmds;
        private readonly Stack<Command.Gosub> _gosubCmds;

        private int _pc; // Program counter (_program[_pc] is current command)

        public Interpreter(TextReader input, TextWriter output) : this(input, output, new StreamReader(Console.OpenStandardInput())) {}

        public Interpreter(TextReader input, TextWriter output, TextReader intSource)
        {
            _input = input;
            _output = output;
            _program = new List<Command>();
            _intSource = intSource;
            _lineIndices = new Dictionary<int, int>();
            _variables = new Dictionary<string, int>();
            _forCmds = new Stack<Command.RealizedFor>();
            _gosubCmds = new Stack<Command.Gosub>();
            _pc = 0;
        }

        public void Run()
        {
            Parse();

            while (CurrentCommand is not Command.End)
            {
                Run(CurrentCommand);
            }
        }

        // --------

        private void Parse()
        {
            var progLines = new SortedDictionary<int, Command>();

            int lineNumber = 0;

            string? line;
            while ((line = _input.ReadLine()) is not null)
            {
                lineNumber++;
                var lexer = new LineLexer(line, lineNumber);
                var tokens = new List<Token>();
                while (true)
                {
                    var token = lexer.NextToken();
                    tokens.Add(token);
                    if (token.Type == TokenType.Eol)
                    {
                        break;
                    }
                }

                var cmd = new LineParser(tokens).Parse();
                progLines[cmd.LineNumber] = cmd;
            }

            _program.AddRange(progLines.Values);

            for (int i = 0; i < _program.Count; i++)
            {
                var cmd = _program[i];
                _lineIndices[cmd.LineNumber] = i;
            }
        }

        // --------

        private Unit Run(Command cmd) => cmd switch
        {
            Command.Let letCmd => Run(letCmd),
            Command.Print printCmd => Run(printCmd),
            Command.Input inputCmd => Run(inputCmd),
            Command.For forCmd => Run(forCmd),
            Command.Next nextCmd => Run(nextCmd),
            Command.If ifCmd => Run(ifCmd),
            Command.Goto gotoCmd => Run(gotoCmd),
            Command.Gosub gosubCmd => Run(gosubCmd),
            Command.Return returnCmd => Run(returnCmd),
            Command.Empty emptyCmd => Run(emptyCmd),
            Command.End _ => default,
            _ => throw new ArgumentException("unsupported command: " + cmd),
        };

        private Unit Run(Command.Let cmd)
        {
            int value = Eval(cmd.Init);
            _variables[cmd.VariableName] = value;
            Step();
            return default;
        }

        private Unit Run(Command.Print cmd)
        {
            foreach (var exp in cmd.Values)
            {
                if (exp is Expression.StringLiteral strLit)
                {
                    _output.Write(strLit.Value);
                }
                else if (exp is Expression.IntegerLiteral intLit)
                {
                    _output.Write(intLit.Value);
                }
                else
                {
                    _output.Write(Eval(exp));
                }
            }

            _output.WriteLine();
            Step();
            return default;
        }

        private Unit Run(Command.Input cmd)
        {
            if (cmd.Prompt != null)
            {
                _output.Write(cmd.Prompt);
            }

            foreach (string variableName in cmd.VariableNames)
            {
                int value = ReadInt();
                _variables[variableName] = value;
            }

            Step();
            return default;
        }

        private Unit Run(Command.For cmd)
        {
            int from = Eval(cmd.From);
            int to = Eval(cmd.To);
            _variables[cmd.VariableName] = from;
            _forCmds.Push(new Command.RealizedFor(cmd.LineNumber, cmd.VariableName, from, to));

            // Find matching "next" in case this for should be skipped
            int nextIdx = _pc + 1;
            int balance = 1;
            while (nextIdx < _program.Count && balance > 0)
            {
                if (_program[nextIdx] is Command.For) balance++;
                if (_program[nextIdx] is Command.Next) balance--;
                nextIdx++;
            }
            if (balance != 0)
            {
                throw new SemanticErrorException(
                    "this for is not matched by a next command",
                    cmd.LineNumber
                );
            }

            if (from > to)
            {
                _pc = nextIdx - 1;
            }
            else
            {
                Step();
            }

            return default;
        }

        private Unit Run(Command.Next cmd)
        {
            if (_forCmds.Count == 0)
            {
                throw new SemanticErrorException("next has no matching for", cmd.LineNumber);
            }

            var matchingFor = _forCmds.Peek();
            if (matchingFor.VariableName != cmd.VariableName)
            {
                throw new SemanticErrorException(
                    "next uses wrong variable name or is nested incorrectly",
                    cmd.LineNumber
                );
            }

            int counterValue = _variables[matchingFor.VariableName];
            if (counterValue >= matchingFor.To)
            {
                _forCmds.Pop();
            }
            else
            {
                _variables[matchingFor.VariableName] = counterValue + 1;
                _pc = _lineIndices[matchingFor.LineNumber];
            }

            Step();
            return default;
        }

        private Unit Run(Command.If cmd)
        {
            bool runThen = (Eval(cmd.Test) != 0);
            if (runThen)
            {
                Run(cmd.Then);
            }
            else
            {
                Step();
            }
            return default;
        }

        private Unit Run(Command.Goto cmd)
        {
            _pc = _lineIndices[cmd.TargetLine];
            return default;
        }

        private Unit Run(Command.Gosub cmd)
        {
            _gosubCmds.Push(cmd);
            _pc = _lineIndices[cmd.TargetLine];
            return default;
        }

        private Unit Run(Command.Return cmd)
        {
            if (_gosubCmds.Count == 0)
            {
                throw new SemanticErrorException(
                    "return has no matching gosub",
                    cmd.LineNumber
                );
            }

            var parent = _gosubCmds.Pop();
            _pc = _lineIndices[parent.LineNumber];
            Step();
            return default;
        }

        private Unit Run(Command.Empty cmd)
        {
            Step();
            return default;
        }

        private int Eval(Expression exp)
        {
            if (exp is Expression.Identifier ident)
            {
                if (!_variables.ContainsKey(ident.VariableName))
                {
                    throw new SemanticErrorException(
                        $"name '{ident.VariableName}' is not defined",
                        CurrentCommand.LineNumber
                    );
                }

                return _variables[ident.VariableName];
            }
            else if (exp is Expression.IntegerLiteral intLit)
            {
                return intLit.Value;
            }
            else if (exp is Expression.StringLiteral)
            {
                throw new SemanticErrorException(
                    "expected integer value but found string",
                    CurrentCommand.LineNumber
                );
            }
            else if (exp is Expression.Binary binExp)
            {
                int left = Eval(binExp.Left);
                int right = Eval(binExp.Right);
                return binExp.Operator switch
                {
                    Operator.Plus => left + right,
                    Operator.Minus => left - right,
                    Operator.Mult => left * right,
                    Operator.Div => left / right,
                    Operator.Mod => left % right,
                    Operator.Lt => (left < right) ? 1 : 0,
                    Operator.Eq => (left == right) ? 1 : 0,
                    Operator.Gt => (left > right) ? 1 : 0,
                    _ => throw new SemanticErrorException(
                        "unsupported operator " + binExp.Operator,
                        CurrentCommand.LineNumber
                    )
                };
            }

            throw new SemanticErrorException("internal error", CurrentCommand.LineNumber);
        }

        // --------

        private Command CurrentCommand => (_pc < _program.Count) ? _program[_pc] : End;

        private void Step()
        {
            _pc++;
        }

        private int ReadInt()
        {
            char c = ReadChar();
            while (char.IsWhiteSpace(c))
            {
                c = ReadChar();
            }
            
            var numeral = new StringBuilder();

            if (c == '-')
            {
                numeral.Append('-');
                c = ReadChar();
            }

            while (char.IsDigit(c))
            {
                numeral.Append(c);
                c = ReadChar();
            }

            return int.Parse(numeral.ToString());
        }

        private char ReadChar()
        {
            int x = _intSource.Read();
            return (x != -1) ? (char) x : '\0';
        }
    }
}