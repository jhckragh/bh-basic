using NUnit.Framework;
using Basic;
using System.IO;
using System;

namespace Tests
{
    public class InterpreterTests
    {
        private static readonly string Endl = Environment.NewLine;

        [Test]
        public void TestEmpty()
        {
            var input = @"10 ";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("", output.ToString());
        }

        [Test]
        public void TestPrintEmpty()
        {
            var input = @"10 print";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("" + Endl, output.ToString());
        }

        [Test]
        public void TestPrintIntegerLiteral()
        {
            var input = @"10 print 123";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("123" + Endl, output.ToString());
        }

        [Test]
        public void TestPrintIntegerExpSimple()
        {
            var input = @"10 print 2 + 2";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("4" + Endl, output.ToString());
        }

        [Test]
        public void TestPrintIntegerExpMod()
        {
            var input = @"10 print 15 % 2";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("1" + Endl, output.ToString());
        }

        [Test]
        public void TestPrintIntegerLiteralsComma()
        {
            var input = @"10 print 123, 456, 789";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("123\t456\t789" + Endl, output.ToString());
        }

        [Test]
        public void TestPrintIntegerLiteralsSemicolon()
        {
            var input = @"10 print 123;456;789";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("123456789" + Endl, output.ToString());
        }

        [Test]
        public void TestPrintIntegerLiteralsMixed()
        {
            var input = @"10 print 123,456;789";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("123\t456789" + Endl, output.ToString());
        }

        [Test]
        public void TestPrintStringLiteral()
        {
            var input = @"10 print ""hello World!"" ";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("hello World!" + Endl, output.ToString());
        }

        [Test]
        public void TestPrintExpNeg()
        {
            var input = @"10 print -5 * (-5)";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("25" + Endl, output.ToString());
        }

        [Test]
        public void TestLetSimple()
        {
            var input = @"10 let x = 42
20 print x";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("42" + Endl, output.ToString());
        }

        [Test]
        public void TestLetSimpleExp()
        {
            var input = @"10 let x = 2 + 2
20 print x";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("4" + Endl, output.ToString());
        }

        [Test]
        public void TestLetDouble()
        {
            var input = @"10 let x = 10
20 let y = 20
30 print x, y";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("10\t20" + Endl, output.ToString());
        }

        [Test]
        public void TestLetDoubleExpSimple()
        {
            var input = @"10 let x = 10
20 let y = 2 * x + 10
30 print y";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("30" + Endl, output.ToString());
        }

        [Test]
        public void TestInputSimple()
        {
            var input = @"10 input x
20 print x";
            var intSource = new StringReader("999");
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output, intSource);
            interpreter.Run();
            Assert.AreEqual("999" + Endl, output.ToString());
        }

        [Test]
        public void TestInputSimple2()
        {
            var input = @"10 input x, y
20 print x, y";
            var intSource = new StringReader("  123    321");
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output, intSource);
            interpreter.Run();
            Assert.AreEqual("123\t321" + Endl, output.ToString());
        }

        [Test]
        public void TestInputSimpleMult()
        {
            var input = @"10 input qty
20 input price
30 print qty * price";
            var intSource = new StringReader(@"
3


    20");
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output, intSource);
            interpreter.Run();
            Assert.AreEqual("60" + Endl, output.ToString());
        }

        [Test]
        public void TestForConstToConst1()
        {
            var input = @"10 for i=1 to 3
20 print ""hello""
30 next i
40 end";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("hello" + Endl
                          + "hello" + Endl
                          + "hello" + Endl, output.ToString());
        }

        [Test]
        public void TestForConstToConst2()
        {
            var input = @"10 for i=1 to 3
20 print i
30 next i
40 end";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("1" + Endl
                          + "2" + Endl
                          + "3" + Endl, output.ToString());
        }

        [Test]
        public void TestForConstToConst3()
        {
            var input = @"10 for i=1 to 1
20 print i
30 next i
40 end";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual($"1{Endl}", output.ToString());
        }

        [Test]
        public void TestForSkip1()
        {
            var input = @"
10 for i=5 to 3
20   print i
30   next i
40 end";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("", output.ToString());
        }

        [Test]
        public void TestComment1()
        {
            var input = @"10 print 100
// this is a comment
20 print 200";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual($"100{Endl}200{Endl}", output.ToString());
        }

        [Test]
        public void TestComment2()
        {
            var input = @"1 print 100
// 2 print 200
3 print 300";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual($"100{Endl}300{Endl}", output.ToString());
        }

        [Test]
        public void TestComment3()
        {
            var input = @"
    // this is a comment

1 print 100
// 2 print 200

3 print 300";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual($"100{Endl}300{Endl}", output.ToString());
        }

        [Test]
        public void TestProgram1()
        {
            var input = @"10 print ""Table of Squares""
20 print
30 print ""How many values would you like?""
40 input num
50 for i=1 to num
60 print i, i*i
70 next i
80 end";
            var intSource = new StringReader("5");
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output, intSource);
            interpreter.Run();
            Assert.AreEqual("Table of Squares" + Endl
                          + Endl
                          + "How many values would you like?" + Endl
                          + "1\t1" + Endl
                          + "2\t4" + Endl
                          + "3\t9" + Endl
                          + "4\t16" + Endl
                          + "5\t25" + Endl,
                          output.ToString());
        }

        [Test]
        public void TestProgram2()
        {
            var input = @"10 let x=7
20 gosub 100
30 let x=9
40 gosub 100
50 goto 200
100 print x, x*x
110 return
200 end
";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("7\t49" + Endl
                          + "9\t81" + Endl,
                          output.ToString());
        }

        [Test]
        public void TestProgram3A()
        {
            var input = @"10 input x
20 if x > 0 then goto 100
30 print ""x is not positive""
40 print ""x ="" ; x
50 goto 200
100 print ""x is positive""
200 end
";
            var intSource = new StringReader("5");
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output, intSource);
            interpreter.Run();
            Assert.AreEqual("x is positive" + Endl, output.ToString());
        }

        [Test]
        public void TestProgram3B()
        {
            var input = @"10 input x
20 if x > 0 then goto 100
30 print ""x is not positive""
40 print ""x = "" ; x
50 goto 200
100 print ""x is positive""
200 end
";
            var intSource = new StringReader("-5");
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output, intSource);
            interpreter.Run();
            Assert.AreEqual("x is not positive" + Endl
                          + "x = -5" + Endl, output.ToString());
        }

        [Test]
        public void TestProgramPrimesUnder50()
        {
            var input = @"10 let N = 50
20 for n = 2 to N
30   let is_prime = 1
40   for d = 2 to n - 1
50     if n % d = 0 then let is_prime = 0
60     next d
70   if is_prime then print n
80   next n
90 end
";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            var expected = "2 3 5 7 11 13 17 19 23 29 31 37 41 43 47".Replace(" ", Endl);
            Assert.AreEqual(expected + Endl, output.ToString());
        }

        [Test]
        public void TestProgramNested()
        {
            var input = @"
10 for i=1 to 3
20   for j=1 to 2
30     print i ; "","" ; j
40   next j
50 next i
60 end
";
            var output = new StringWriter();
            var interpreter = new Interpreter(new StringReader(input), output);
            interpreter.Run();
            Assert.AreEqual("1,1" + Endl
                          + "1,2" + Endl
                          + "2,1" + Endl
                          + "2,2" + Endl
                          + "3,1" + Endl
                          + "3,2" + Endl, output.ToString());
        }
    }
}