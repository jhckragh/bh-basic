using NUnit.Framework;
using Basic;

namespace Tests
{
    public class LexerTests
    {
        [Test]
        public void TestExpr1()
        {
            var lineNumber = 1;
            var input = @"2+2  2  +      2  +";
            var lexer = new LineLexer(input, lineNumber);

            var expected = new[] {
                new Token(TokenType.IntegerLiteral, "2", lineNumber, 0),
                new Token(TokenType.Plus, "+", lineNumber, 1),
                new Token(TokenType.IntegerLiteral, "2", lineNumber, 2),
                new Token(TokenType.IntegerLiteral, "2", lineNumber, 5),
                new Token(TokenType.Plus, "+", lineNumber, 8),
                new Token(TokenType.IntegerLiteral, "2", lineNumber, 15),
                new Token(TokenType.Plus, "+", lineNumber, 18)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestExpr2()
        {
            var lineNumber = 1;
            var input = @"15 % 3";
            var lexer = new LineLexer(input, lineNumber);

            var expected = new[] {
                new Token(TokenType.IntegerLiteral, "15", lineNumber, 0),
                new Token(TokenType.Mod, "%", lineNumber, 3),
                new Token(TokenType.IntegerLiteral, "3", lineNumber, 5)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestPrintEmpty()
        {
            var lineNumber = 1;
            var input = @"20 print";
            var lexer = new LineLexer(input, lineNumber);

            var expected = new[] {
                new Token(TokenType.IntegerLiteral, "20", lineNumber, 0),
                new Token(TokenType.Print, "print", lineNumber, 3)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestPrintString()
        {
            var lineNumber = 1;
            var input = @"10 print ""hello""";
            var lexer = new LineLexer(input, lineNumber);

            var expected = new[] {
                new Token(TokenType.IntegerLiteral, "10", lineNumber, 0),
                new Token(TokenType.Print, "print", lineNumber, 3),
                new Token(TokenType.StringLiteral, "hello", lineNumber, 9)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestPrintVar()
        {
            var lineNumber = 1;
            var input = @"100 print i";
            var lexer = new LineLexer(input, lineNumber);

            var expected = new[] {
                new Token(TokenType.IntegerLiteral, "100", lineNumber, 0),
                new Token(TokenType.Print, "print", lineNumber, 4),
                new Token(TokenType.Identifier, "i", lineNumber, 10)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestPrintVarList()
        {
            var lineNumber = 1;
            var input = @"101 print x, y";
            var lexer = new LineLexer(input, lineNumber);

            var expected = new[] {
                new Token(TokenType.IntegerLiteral, "101", lineNumber, 0),
                new Token(TokenType.Print, "print", lineNumber, 4),
                new Token(TokenType.Identifier, "x", lineNumber, 10),
                new Token(TokenType.Comma, ",", lineNumber, 11),
                new Token(TokenType.Identifier, "y", lineNumber, 13)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestString1()
        {
            var lineNumber = 1;
            var input = @"  ""abc""  ";
            var lexer = new LineLexer(input, lineNumber);
            var expected = new Token(TokenType.StringLiteral, "abc", lineNumber, 2);
            Assert.AreEqual(expected, lexer.NextToken());
            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestString2()
        {
            var lineNumber = 1;
            var input = @"  ""Hello world!""  ";
            var lexer = new LineLexer(input, lineNumber);
            var expected = new Token(TokenType.StringLiteral, "Hello world!", lineNumber, 2);
            Assert.AreEqual(expected, lexer.NextToken());
            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestString3()
        {
            var lineNumber = 1;
            var input = @"  ""abc""    ""DeF""  ";
            var lexer = new LineLexer(input, lineNumber);
            
            var expected = new[] {
                new Token(TokenType.StringLiteral, "abc", lineNumber, 2),
                new Token(TokenType.StringLiteral, "DeF", lineNumber, 11)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestString4()
        {
            var lineNumber = 1;
            var input = @"   ""3A""   ""!b""   ""h3H3_!#@""   ""1111""  ";
            var lexer = new LineLexer(input, lineNumber);
            
            var expected = new[] {
                new Token(TokenType.StringLiteral, "3A", lineNumber, 3),
                new Token(TokenType.StringLiteral, "!b", lineNumber, 10),
                new Token(TokenType.StringLiteral, "h3H3_!#@", lineNumber, 17),
                new Token(TokenType.StringLiteral, "1111", lineNumber, 30)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestSymbols()
        {
            var lineNumber = 1;
            var input = "=+-*/,;:><()%";
            var lexer = new LineLexer(input, lineNumber);
            
            var expected = new[] {
                new Token(TokenType.Eq, "=", lineNumber, 0),
                new Token(TokenType.Plus, "+", lineNumber, 1),
                new Token(TokenType.Minus, "-", lineNumber, 2),
                new Token(TokenType.Mult, "*", lineNumber, 3),
                new Token(TokenType.Div, "/", lineNumber, 4),
                new Token(TokenType.Comma, ",", lineNumber, 5),
                new Token(TokenType.Semicolon, ";", lineNumber, 6),
                new Token(TokenType.Colon, ":", lineNumber, 7),
                new Token(TokenType.Gt, ">", lineNumber, 8),
                new Token(TokenType.Lt, "<", lineNumber, 9),
                new Token(TokenType.ParenL, "(", lineNumber, 10),
                new Token(TokenType.ParenR, ")", lineNumber, 11),
                new Token(TokenType.Mod, "%", lineNumber, 12)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }

        [Test]
        public void TestKeywords()
        {
            var lineNumber = 1;
            var input = "let print input for to next "
                      + "if then goto gosub return end";
            var lexer = new LineLexer(input, lineNumber);

            var expected = new[] {
                new Token(TokenType.Let, "let", lineNumber, 0),
                new Token(TokenType.Print, "print", lineNumber, 4),
                new Token(TokenType.Input, "input", lineNumber, 10),
                new Token(TokenType.For, "for", lineNumber, 16),
                new Token(TokenType.To, "to", lineNumber, 20),
                new Token(TokenType.Next, "next", lineNumber, 23),
                new Token(TokenType.If, "if", lineNumber, 28),
                new Token(TokenType.Then, "then", lineNumber, 31),
                new Token(TokenType.Goto, "goto", lineNumber, 36),
                new Token(TokenType.Gosub, "gosub", lineNumber, 41),
                new Token(TokenType.Return, "return", lineNumber, 47),
                new Token(TokenType.End, "end", lineNumber, 54)
            };

            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.NextToken());
            }

            Assert.AreEqual(TokenType.Eol, lexer.NextToken().Type);
        }
    }
}