namespace Basic
{
    public record Token(TokenType Type, string Text, int Line, int Column)
    {
        public static TokenType? LookupKeyword(string text)
        {
            switch (text)
            {
                case "let": return TokenType.Let;
                case "print": return TokenType.Print;
                case "input": return TokenType.Input;
                case "for": return TokenType.For;
                case "to": return TokenType.To;
                case "next": return TokenType.Next;
                case "if": return TokenType.If;
                case "then": return TokenType.Then;
                case "goto": return TokenType.Goto;
                case "gosub": return TokenType.Gosub;
                case "return": return TokenType.Return;
                case "end": return TokenType.End;
                default: return null;
            }
        }

        public static TokenType? LookupSymbol(char c)
        {
            switch (c)
            {
                case '=': return TokenType.Eq;
                case ',': return TokenType.Comma;
                case ';': return TokenType.Semicolon;
                case ':': return TokenType.Colon;
                case '+': return TokenType.Plus;
                case '-': return TokenType.Minus;
                case '*': return TokenType.Mult;
                case '/': return TokenType.Div;
                case '%': return TokenType.Mod;
                case '<': return TokenType.Lt;
                case '>': return TokenType.Gt;
                case '(': return TokenType.ParenL;
                case ')': return TokenType.ParenR;
                default: return null;
            }
        }
    }
    public enum TokenType
    {
        Identifier,
        Eq,
        Let,
        Print,
        Comma,
        Semicolon,
        Input,
        For,
        To,
        Next,
        If,
        Then,
        Goto,
        Gosub,
        Return,
        End,
        StringLiteral,
        IntegerLiteral,
        Plus,
        Minus,
        Mult,
        Div,
        Mod,
        Lt,
        Gt,
        ParenL,
        ParenR,
        Colon,
        Eol
    }
}