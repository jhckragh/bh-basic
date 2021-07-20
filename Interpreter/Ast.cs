using System.Collections.Generic;

#nullable enable

namespace Basic
{
    public abstract record Expression
    {
        public sealed record Binary(Expression Left, Operator Operator, Expression Right) : Expression;
        public sealed record Identifier(string VariableName) : Expression;
        public sealed record IntegerLiteral(int Value) : Expression;
        public sealed record StringLiteral(string Value) : Expression;
    }

    public enum Operator { Lt, Eq, Gt, Plus, Minus, Mult, Div, Mod }

    public abstract record Command(int LineNumber)
    {
        public sealed record Let(int LineNumber, string VariableName, Expression Init) : Command(LineNumber);
        public sealed record Print(int LineNumber, List<Expression> Values) : Command(LineNumber);
        public sealed record Input(int LineNumber, string? Prompt, List<string> VariableNames) : Command(LineNumber);
        public sealed record For(int LineNumber, string VariableName, Expression From, Expression To) : Command(LineNumber);
        public sealed record RealizedFor(int LineNumber, string VariableName, int From, int To) : Command(LineNumber);
        public sealed record Next(int LineNumber, string VariableName) : Command(LineNumber);
        public sealed record If(int LineNumber, Expression Test, Command Then) : Command(LineNumber);
        public sealed record Goto(int LineNumber, int TargetLine) : Command(LineNumber);
        public sealed record Gosub(int LineNumber, int TargetLine) : Command(LineNumber);
        public sealed record Return(int LineNumber) : Command(LineNumber);
        public sealed record End(int LineNumber) : Command(LineNumber);
        public sealed record Empty(int LineNumber) : Command(LineNumber);
    }
}