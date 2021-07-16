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
}