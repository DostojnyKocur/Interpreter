using System;
using System.IO;
using System.Threading.Tasks;
using Interpreter.AnalyzerService;
using Interpreter.LexerService;
using Interpreter.ParserService;

namespace Interpreter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Logger.CreateLogger(args);

            if (args.Length > 0)
            {
                await InterpretFile(args[0]);
            }
        }

        private static async Task InterpretFile(string file)
        {
            if(File.Exists(file))
            {
                var fileContent = await File.ReadAllTextAsync(file);

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
            semanticAnalyzer.Analyze(tree);

            var interpreter = new Interpreter();
            interpreter.Run(tree);  
        }
    }
}
