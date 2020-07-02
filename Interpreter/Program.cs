using System;
using System.IO;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.CreateLogger(args);

            if (args.Length > 0)
            {
                InterpretFile(args[0]);
            }
        }

        private static void InterpretFile(string file)
        {
            if(File.Exists(file))
            {
                var fileContent = File.ReadAllText(file);

                Process(fileContent);
            }
            else
            {
                Console.WriteLine($"File {file} does not exist. Exiting...");
            }
        }

        private static void Process(string text)
        {
            var lexer = new Lexer(text);
            var parser = new Parser(lexer);

            var tree = parser.Parse();

            var semanticAnalyzer = new SemanticAnalyzer();
            semanticAnalyzer.Prepare(tree);

            var interpreter = new Interpreter();
            interpreter.Run(tree);  
        }
    }
}
