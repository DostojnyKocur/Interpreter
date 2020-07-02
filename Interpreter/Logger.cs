using System;
using System.Linq;
using Serilog;
using Serilog.Events;

namespace Interpreter
{
    public static class Logger
    {
        private static bool LogScope = false;
        private static bool LogMemory = false;

        public static void CreateLogger(string[] args)
        {
            ParseArguments(args);

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        }

        public static void Debug(string message)
        {
            Log.Logger.Debug(message);
        }

        public static void DebugScope(string message)
        {
            if (LogScope)
            {
                Log.Logger.Debug(message);
            }
        }

        public static void DebugMemory(string message)
        {
            if (LogMemory)
            {
                Log.Logger.Debug(message);
            }
        }

        private static void ParseArguments(string[] args)
        {
            if (args.Contains("--logscope"))
            {
                LogScope = true;
            }

            if (args.Contains("--logmemory"))
            {
                LogMemory = true;
            }
        }
    }
}
