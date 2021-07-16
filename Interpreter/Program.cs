using System;
using System.IO;

namespace Basic
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: dotnet run -- SOURCEFILE");
                return;
            }

            using (var inputStream = File.OpenText(args[0]))
            using (var outputStream = new StreamWriter(Console.OpenStandardOutput()))
            {
                outputStream.AutoFlush = true;
                var interpreter = new Interpreter(inputStream, outputStream);
                interpreter.Run();
            }      
        }
    }
}
