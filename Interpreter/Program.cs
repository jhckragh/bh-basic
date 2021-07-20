using System;
using System.IO;

namespace Basic
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: dotnet run -- SOURCEFILE");
                return 1;
            }

            using (var inputStream = File.OpenText(args[0]))
            using (var outputStream = new StreamWriter(Console.OpenStandardOutput()))
            {
                outputStream.AutoFlush = true;
                var interpreter = new Interpreter(inputStream, outputStream);
                try
                {
                    interpreter.Run();
                }
                catch (SyntaxErrorException e)
                {
                    Console.Error.WriteLine($"[{e.Line}:{e.Column}] syntax error: " + e.Message);
                    return 1;
                }
                catch (SemanticErrorException e)
                {
                    Console.Error.WriteLine($"error on line {e.Line}: " + e.Message);
                    return 1;
                }
            }

            return 0; 
        }
    }
}
