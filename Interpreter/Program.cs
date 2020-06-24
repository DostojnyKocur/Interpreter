using System;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
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
                    var result = interpreter.Run();

                    Console.WriteLine(result);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Exception: {exception.Message}");
                }
            }
        }
    }
}
