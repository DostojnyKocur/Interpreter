using System;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("To exit type 'exit' or 'quit'");

            while(true)
            {
                var text = string.Empty;
                try
                {
                    Console.Write("interpreter> ");
                    text = Console.ReadLine();
                }
                catch(Exception exception)
                {
                    Console.WriteLine($"Exception: {exception.Message}");
                }

                if(string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if(text.Length == 4 && (text.ToLower() == "exit" || text.ToLower() == "quit"))
                {
                    break;
                }

                var interpreter = new Interpreter(text);
                var result = interpreter.Run();

                Console.WriteLine(result);
            }
        }
    }
}
