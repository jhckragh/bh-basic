using NUnit.Framework;
using Basic;
using System.Collections.Generic;

namespace Tests
{
    public class ParserTests
    {
        [Test]
        public void TestEmpty()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "10", 1, 0),
            };

            var expected = new Command.Empty(10);

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestLetLiteral()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "10", 1, 0),
                new Token(TokenType.Let, "let", 1, 3),
                new Token(TokenType.Identifier, "x", 1, 7),
                new Token(TokenType.Eq, "=", 1, 9),
                new Token(TokenType.IntegerLiteral, "42", 1, 11)
            };
            
            var expected = new Command.Let(10, "x", new Expression.IntegerLiteral(42));

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestPrintEmpty()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "20", 1, 0),
                new Token(TokenType.Print, "print", 1, 3)
            };

            var actual = new LineParser(input).Parse();
            Assert.AreEqual(20, actual.LineNumber);
            Assert.IsInstanceOf<Command.Print>(actual);
            Assert.That(((Command.Print) actual).Values.Count == 0, "print list should be empty");
        }

        [Test]
        public void TestPrintModExp()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "20", 1, 0),
                new Token(TokenType.Print, "print", 1, 3),
                new Token(TokenType.IntegerLiteral, "15", 1, 9),
                new Token(TokenType.Mod, "%", 1, 12),
                new Token(TokenType.IntegerLiteral, "3", 1, 14)
            };

            var actual = new LineParser(input).Parse();
            
            Assert.AreEqual(20, actual.LineNumber);
            Assert.IsInstanceOf<Command.Print>(actual);

            var print = (Command.Print) actual;

            Assert.That(print.Values.Count == 1, "print list should consist of 1 exp");

            var expectedExp = new Expression.Binary(
                new Expression.IntegerLiteral(15),
                Operator.Mod,
                new Expression.IntegerLiteral(3)
            );

            Assert.AreEqual(expectedExp, print.Values[0]);
        }

        [Test]
        public void TestInputSingle()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "40", 1, 0),
                new Token(TokenType.Input, "input", 1, 3),
                new Token(TokenType.Identifier, "num", 1, 9),
            };

            var actual = new LineParser(input).Parse();

            Assert.AreEqual(40, actual.LineNumber);
            Assert.IsInstanceOf<Command.Input>(actual);
            Command.Input actualInput = (Command.Input) actual;
            Assert.IsNull(actualInput.Prompt);
            Assert.AreEqual(1, actualInput.VariableNames.Count);
            Assert.AreEqual("num", actualInput.VariableNames[0]);
        }

        [Test]
        public void TestForLiterals()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "50", 1, 0),
                new Token(TokenType.For, "for", 1, 3),
                new Token(TokenType.Identifier, "i", 1, 7),
                new Token(TokenType.Eq, "=", 1, 8),
                new Token(TokenType.IntegerLiteral, "1", 1, 9),
                new Token(TokenType.To, "to", 1, 11),
                new Token(TokenType.IntegerLiteral, "10", 1, 14),
            };

            var from = new Expression.IntegerLiteral(1);
            var to = new Expression.IntegerLiteral(10);
            var expected = new Command.For(50, "i", from, to);

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestForFromExpToLiteral()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "50", 1, 0),
                new Token(TokenType.For, "for", 1, 3),
                new Token(TokenType.Identifier, "i", 1, 7),
                new Token(TokenType.Eq, "=", 1, 8),
                new Token(TokenType.IntegerLiteral, "2", 1, 10),
                new Token(TokenType.Mult, "*", 1, 12),
                new Token(TokenType.Identifier, "x", 1, 14),
                new Token(TokenType.To, "to", 1, 16),
                new Token(TokenType.IntegerLiteral, "999", 1, 19),
            };

            var from = new Expression.Binary(new Expression.IntegerLiteral(2), Operator.Mult, new Expression.Identifier("x"));
            var to = new Expression.IntegerLiteral(999);
            var expected = new Command.For(50, "i", from, to);

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestNextPass()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "70", 1, 0),
                new Token(TokenType.Next, "next", 1, 3),
                new Token(TokenType.Identifier, "i", 1, 8)
            };

            var expected = new Command.Next(70, "i");

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestIfEmpty()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "20", 1, 0),
                new Token(TokenType.If, "if", 1, 3),
                new Token(TokenType.Identifier, "x", 1, 6),
                new Token(TokenType.Gt, ">", 1, 8),
                new Token(TokenType.IntegerLiteral, "0", 1, 10),
                new Token(TokenType.Then, "then", 1, 12)
            };

            var test = new Expression.Binary(
                new Expression.Identifier("x"),
                Operator.Gt,
                new Expression.IntegerLiteral(0));
            var expected = new Command.If(20, test, new Command.Empty(20));

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestIfExpGoto()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "20", 1, 0),
                new Token(TokenType.If, "if", 1, 3),
                new Token(TokenType.Identifier, "x", 1, 6),
                new Token(TokenType.Plus, "+", 1, 8),
                new Token(TokenType.IntegerLiteral, "1", 1, 10),
                new Token(TokenType.Eq, "=", 1, 12),
                new Token(TokenType.IntegerLiteral, "100", 1, 14),
                new Token(TokenType.Then, "then", 1, 18),
                new Token(TokenType.Goto, "goto", 1, 20),
                new Token(TokenType.IntegerLiteral, "123", 1, 25)
            };

            var test = new Expression.Binary(
                new Expression.Binary(
                    new Expression.Identifier("x"),
                    Operator.Plus,
                    new Expression.IntegerLiteral(1)),
                Operator.Eq,
                new Expression.IntegerLiteral(100));
            var expected = new Command.If(20, test, new Command.Goto(20, 123));

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestGotoPass()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "50", 1, 0),
                new Token(TokenType.Goto, "goto", 1, 3),
                new Token(TokenType.IntegerLiteral, "200", 1, 8)
            };

            var expected = new Command.Goto(50, 200);

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestReturnPass()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "110", 1, 0),
                new Token(TokenType.Return, "return", 1, 4)
            };

            var expected = new Command.Return(110);

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }

        [Test]
        public void TestEndPass()
        {
            var input = new List<Token> {
                new Token(TokenType.IntegerLiteral, "200", 1, 0),
                new Token(TokenType.End, "end", 1, 4)
            };

            var expected = new Command.End(200);

            Assert.AreEqual(expected, new LineParser(input).Parse());
        }
    }
}