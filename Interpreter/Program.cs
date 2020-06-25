using System;
using System.IO;

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

                var lexer = new Lexer(fileContent);
                var parser = new Parser(lexer);
                var interpreter = new Interpreter(parser);
                interpreter.Run();

                interpreter.DebugPrintGlobalScope();
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

                    var lexer = new Lexer(text);
                    var parser = new Parser(lexer);
                    var interpreter = new Interpreter(parser);
                    interpreter.Run();

                    interpreter.DebugPrintGlobalScope();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Exception: {exception.Message}");
                }
            }
        }
    }
}
