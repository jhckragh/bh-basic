using System.Text;

namespace Basic
{
    public class LineLexer
    {
        private const char Eol = '\0';

        private readonly string _line;
        private int _lineNumber;
        private int _columnNumber;

        public LineLexer(string line, int lineNumber)
        {
            _line = line;
            _lineNumber = lineNumber;
        }

        public Token NextToken()
        {
            while (char.IsWhiteSpace(CurrentChar))
            {
                Advance();
            }

            if (CurrentChar == '/')
            {
                Advance();
                if (CurrentChar == '/')
                {
                    _columnNumber = _line.Length;
                }
                else
                {
                    return new Token(TokenType.Div, "/", _lineNumber, _columnNumber - 1);
                }
            }

            if (CurrentChar == Eol)
            {
                return new Token(TokenType.Eol, "<eol>", _lineNumber, _line.Length);
            }

            if (IsIdentifierStart(CurrentChar))
            {
                return LexIdentifierOrKeyword();
            }

            if (char.IsDigit(CurrentChar))
            {
                return LexIntegerLiteral();
            }

            if (CurrentChar == '"')
            {
                return LexStringLiteral();
            }

            TokenType type = Token.LookupSymbol(CurrentChar) ?? TokenType.Error;
            string text = char.ToString(CurrentChar);
            
            if (type == TokenType.Error)
            {
                text = $"character '{text}' is not allowed here";
            }

            Advance();

            return new Token(type, text, _lineNumber, _columnNumber - 1);
        }

        private Token LexIdentifierOrKeyword()
        {
            int column = _columnNumber;
            var text = new StringBuilder();

            while (IsIdentifierPart(CurrentChar))
            {
                text.Append(CurrentChar);
                Advance();
            }

            TokenType type = Token.LookupKeyword(text.ToString()) ?? TokenType.Identifier;

            return new Token(type, text.ToString(), _lineNumber, column);
        }

        private Token LexIntegerLiteral()
        {
            int column = _columnNumber;
            var numeral = new StringBuilder();

            while (char.IsDigit(CurrentChar))
            {
                numeral.Append(CurrentChar);
                Advance();
            }

            return new Token(TokenType.IntegerLiteral, numeral.ToString(), _lineNumber, column);
        }

        private Token LexStringLiteral()
        {
            int column = _columnNumber;

            Advance(); // Skip opening quote

            var text = new StringBuilder();

            while (CurrentChar != '"' && CurrentChar != Eol)
            {
                if (CurrentChar == '\n')
                {
                    string err = "line end in string literal";
                    return new Token(TokenType.Error, err, _lineNumber, column);
                }

                text.Append(CurrentChar);
                Advance();
            }

            if (CurrentChar != '"')
            {
                string err = "unclosed string literal";
                return new Token(TokenType.Error, err, _lineNumber, column);
            }

            Advance(); // Skip closing quote

            return new Token(TokenType.StringLiteral, text.ToString(), _lineNumber, column);
        }

        private char CurrentChar
            => (_columnNumber < _line.Length) ? _line[_columnNumber] : Eol;
        
        private void Advance()
        {
            _columnNumber++;
        }

        private static bool IsIdentifierStart(char c)
            => char.IsLetter(c) || c == '_' || c == '$';
        
        private static bool IsIdentifierPart(char c)
            => IsIdentifierStart(c) || char.IsDigit(c);
    }
}