using System;
using System.IO;
using Interpreter.Symbols;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 1)
            {
                InterpretFile(args[0]);
            }
            else
            {
                InteractiveMode();
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

        private static void InteractiveMode()
        {
            Console.WriteLine("To exit type 'exit' or 'quit'");

            while (true)
            {
                try
                {
                    Console.Write("interpreter> ");
                    var text = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    if (text.Length == 4 && (text.ToLower() == "exit" || text.ToLower() == "quit"))
                    {
                        break;
                    }

                    Process(text);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Exception: {exception.Message}");
                }
            }
        }

        private static void Process(string text)
        {
            var lexer = new Lexer(text);
            var parser = new Parser(lexer);

            var tree = parser.Parse();

            var symbolTableBuilder = new SymbolTableBuilder();
            symbolTableBuilder.Prepare(tree);

            var interpreter = new Interpreter();
            interpreter.Run(tree);

            interpreter.DebugPrintGlobalScope();
            symbolTableBuilder.DebugPrintSymbolTable();
        }
    }
}
