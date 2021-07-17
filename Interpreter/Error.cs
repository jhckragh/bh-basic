using System;

namespace Basic
{
    public class SyntaxErrorException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public SyntaxErrorException() {}

        public SyntaxErrorException(string message) : base(message) {}

        public SyntaxErrorException(string message, Exception inner) : base(message, inner) {}

        public SyntaxErrorException(string message, int line, int column) : base(message)
        {
            Line = line;
            Column = column;
        }
    }

    public class SemanticErrorException : Exception
    {
        public int Line { get; }

        public SemanticErrorException() {}

        public SemanticErrorException(string message) : base(message) {}

        public SemanticErrorException(string message, Exception inner) : base(message, inner) {}

        public SemanticErrorException(string message, int line) : base(message)
        {
            Line = line;
        }
    }
}