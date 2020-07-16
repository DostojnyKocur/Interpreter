using System;
using System.Linq;
using Serilog;
using Serilog.Events;

namespace Interpreter.Common
{
    public static class Logger
    {
        private static bool LogScope = false;
        private static bool LogMemory = false;
        private static LogEventLevel LogLevel = LogEventLevel.Information;

        public static void CreateLogger(string[] args)
        {
            ParseArguments(args);

            var configuration = new LoggerConfiguration()
                .MinimumLevel.Is(LogLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            Log.Logger = configuration.CreateLogger();
        }

        public static void Debug(string message)
        {
            Log.Logger.Debug(message);
        }

        public static void DebugScope(string message)
        {
            if (LogScope)
            {
                Log.Logger.Information(message);
            }
        }

        public static void DebugMemory(string message)
        {
            if (LogMemory)
            {
                Log.Logger.Information(message);
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

            var loglevel = args.FirstOrDefault(value => value.Contains("--loglevel="));
            if (loglevel != null)
            {
                loglevel = loglevel.Replace("--loglevel=", string.Empty);
                LogLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), loglevel);
            }

        }
    }
}
