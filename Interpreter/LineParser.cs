using System;
using System.Collections.Generic;

namespace Basic
{
    public class LineParser
    {
        private readonly IList<Token> _line;
        private readonly Token _eol;
        private int _cursor;
        private int _writtenLineNumber;

        public LineParser(IList<Token> line)
        {
            _line = AppendEol(line);
            _eol = _line[^1];
            _cursor = 0;
        }

        public Command Parse()
        {
            if (CurrentToken.Type == TokenType.Eol)
            {
                return new Command.Empty(-1); // Empty line gets phony line number
            }

            bool startsWithInt = (CurrentToken.Type == TokenType.IntegerLiteral)
                && int.TryParse(CurrentToken.Text, out _writtenLineNumber);
            if (!startsWithInt)
            {
                throw new SyntaxErrorException(
                    "line must start with an integer line number",
                    CurrentToken.Line,
                    CurrentToken.Column
                );
            }

            AcceptIt();
            Command cmd = ParseCommand();
            Accept(TokenType.Eol);
            return cmd;
        }

        private Command ParseCommand()
        {
            if (CurrentToken.Type == TokenType.Eol)
            {
                return new Command.Empty(_writtenLineNumber);
            }

            switch (CurrentToken.Type)
            {
                case TokenType.Let: return ParseLet();
                case TokenType.Print: return ParsePrint();
                case TokenType.Input: return ParseInput();
                case TokenType.For: return ParseFor();
                case TokenType.Next: return ParseNext();
                case TokenType.If: return ParseIf();
                case TokenType.Goto: return ParseGoto();
                case TokenType.Gosub: return ParseGosub();
                case TokenType.Return: return ParseReturn();
                case TokenType.End: return ParseEnd();
                default: break;
            }

            throw new SyntaxErrorException(
                $"expected a keyword (let, print, ...) but saw '{CurrentToken.Text}'",
                CurrentToken.Line,
                CurrentToken.Column
            );
        }

        private Command ParseLet()
        {
            AcceptIt(); // Consume "let"
            Token ident = Accept(TokenType.Identifier);
            Accept(TokenType.Eq);
            Expression exp = ParseExpression();
            Accept(TokenType.Eol);
            return new Command.Let(_writtenLineNumber, ident.Text, exp);
        }

        private Command ParsePrint()
        {
            AcceptIt(); // Consume "print"
            List<Expression> values = new List<Expression>();
            while (CurrentToken.Type != TokenType.Eol)
            {
                switch (CurrentToken.Type)
                {
                    case TokenType.Comma:
                        values.Add(new Expression.StringLiteral("\t"));
                        AcceptIt();
                        break;
                    case TokenType.Semicolon:
                        AcceptIt();
                        break;
                    default:
                        values.Add(ParseExpression());
                        break;
                }
            }
            return new Command.Print(_writtenLineNumber, values);
        }

        private Command ParseInput()
        {
            AcceptIt(); // Consume "input"

            string? prompt = null;
            if (CurrentToken.Type == TokenType.StringLiteral)
            {
                prompt = CurrentToken.Text;
                AcceptIt();
            }

            List<string> variableNames = new List<string>();
            variableNames.Add(Accept(TokenType.Identifier).Text);
            while (CurrentToken.Type == TokenType.Comma)
            {
                AcceptIt();
                variableNames.Add(Accept(TokenType.Identifier).Text);
            }

            return new Command.Input(_writtenLineNumber, prompt, variableNames);
        }

        private Command ParseFor()
        {
            AcceptIt(); // Consume "for"
            string variableName = Accept(TokenType.Identifier).Text;
            Accept(TokenType.Eq);
            Expression from = ParseExpression();
            Accept(TokenType.To);
            Expression to = ParseExpression();
            return new Command.For(_writtenLineNumber, variableName, from, to);
        }

        private Command ParseNext()
        {
            AcceptIt(); // Consume "next"
            Token ident = Accept(TokenType.Identifier);
            return new Command.Next(_writtenLineNumber, ident.Text);
        }

        private Command ParseIf()
        {
            AcceptIt(); // Consume "if"
            Expression test = ParseExpression();
            Accept(TokenType.Then);
            Command then = ParseCommand();
            return new Command.If(_writtenLineNumber, test, then);
        }

        private Command ParseGoto()
        {
            AcceptIt(); // Consume "goto"
            Token targetLine = Accept(TokenType.IntegerLiteral);
            return new Command.Goto(_writtenLineNumber, int.Parse(targetLine.Text));
        }

        private Command ParseGosub()
        {
            AcceptIt(); // Consume "gosub"
            Token targetLine = Accept(TokenType.IntegerLiteral);
            return new Command.Gosub(_writtenLineNumber, int.Parse(targetLine.Text));
        }

        private Command ParseReturn()
        {
            AcceptIt(); // Consume "return"
            return new Command.Return(_writtenLineNumber);
        }

        private Command ParseEnd()
        {
            AcceptIt(); // Consume "end"
            return new Command.End(_writtenLineNumber);
        }

        private Expression ParseExpression()
        {
            Expression left = ParseArithmeticExpression();

            if (IsRelationalOp(CurrentToken))
            {
                Operator op = GetOperator(AcceptIt());
                Expression right = ParseArithmeticExpression();
                return new Expression.Binary(left, op, right);
            }
            return left;
        }

        private Expression ParseArithmeticExpression()
        {
            Expression term1 = ParseTerm();
            while (CurrentToken.Type == TokenType.Plus ||
                   CurrentToken.Type == TokenType.Minus)
            {
                Operator op = GetOperator(AcceptIt());
                Expression term2 = ParseTerm();
                term1 = new Expression.Binary(term1, op, term2);
            }
            return term1;
        }

        private Expression ParseTerm()
        {
            Expression factor1 = ParseFactor();
            while (CurrentToken.Type == TokenType.Mult ||
                   CurrentToken.Type == TokenType.Div ||
                   CurrentToken.Type == TokenType.Mod)
            {
                Operator op = GetOperator(AcceptIt());
                Expression factor2 = ParseFactor();
                factor1 = new Expression.Binary(factor1, op, factor2);
            }
            return factor1;
        }

        private Expression ParseFactor()
        {
            switch (CurrentToken.Type)
            {
                case TokenType.Identifier:
                    string variableName = CurrentToken.Text;
                    AcceptIt();
                    return new Expression.Identifier(variableName);
                case TokenType.IntegerLiteral:
                    int value = int.Parse(CurrentToken.Text);
                    AcceptIt();
                    return new Expression.IntegerLiteral(value);
                case TokenType.StringLiteral:
                    string text = CurrentToken.Text;
                    AcceptIt();
                    return new Expression.StringLiteral(text);
                case TokenType.ParenL:
                    AcceptIt();
                    Expression exp = ParseExpression();
                    Accept(TokenType.ParenR);
                    return exp;
                case TokenType.Minus:
                    AcceptIt();
                    Expression factor = ParseFactor();
                    return new Expression.Binary(new Expression.IntegerLiteral(0), Operator.Minus, factor);
                default: break;
            }

            throw new SyntaxErrorException(
                $"expected variable, literal, '(' or '-', but saw '{CurrentToken.Text}'",
                CurrentToken.Line,
                CurrentToken.Column
            );
        }

        private Operator GetOperator(Token token)
        {
            switch (token.Type) {
                case TokenType.Lt: return Operator.Lt;
                case TokenType.Eq: return Operator.Eq;
                case TokenType.Gt: return Operator.Gt;
                case TokenType.Plus: return Operator.Plus;
                case TokenType.Minus: return Operator.Minus;
                case TokenType.Mult: return Operator.Mult;
                case TokenType.Div: return Operator.Div;
                case TokenType.Mod: return Operator.Mod;
                default: throw new NotImplementedException("operator: " + token.Text);
            }
        }

        private Token CurrentToken => (_cursor < _line.Count) ? _line[_cursor] : _eol;

        private static bool IsRelationalOp(Token token)
        {
            return token.Type == TokenType.Eq ||
               token.Type == TokenType.Gt ||
               token.Type == TokenType.Lt;
        }

        private static bool IsArithmeticOp(Token token)
        {
            return token.Type == TokenType.Plus || 
               token.Type == TokenType.Minus ||
               token.Type == TokenType.Mult ||
               token.Type == TokenType.Div ||
               token.Type == TokenType.Mod;
        }

        private static IList<Token> AppendEol(IList<Token> tokens)
        {
            IList<Token> prepped = new List<Token>(tokens);
            if (prepped.Count == 0)
            {
                prepped.Add(new Token(TokenType.Eol, "<eol>", 1, 0));
            }
            else
            {
                Token last = tokens[^1];
                if (last.Type != TokenType.Eol)
                {
                    int col = last.Column + last.Text.Length;
                    prepped.Add(new Token(TokenType.Eol, "<eol>", last.Line, col));
                }
            }
            return prepped;
        }

        private Token AcceptIt()
        {
            Token t = CurrentToken;
            _cursor++;
            return t;
        }

        private Token Accept(TokenType expectedType)
        {
            Token t = CurrentToken;
            if (CurrentToken.Type != expectedType)
            {
                throw new SyntaxErrorException(
                    $"expected {expectedType} but saw {CurrentToken.Type}",
                    CurrentToken.Line,
                    CurrentToken.Column
                );
            }
            _cursor++;
            return t;
        }
    }
}