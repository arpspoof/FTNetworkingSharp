using System;
using System.Collections.Generic;

namespace Infra
{
    public static class Logger
    {
        public static bool Enabled { get; set; } = true;
        public static HashSet<string> Filter { get; } = new HashSet<string>();

        public enum LogType
        {
            INFO, WARNING, ERROR
        }

        private static ConsoleColor GetConsoleColor(LogType type)
        {
            switch (type)
            {
                case LogType.ERROR: return ConsoleColor.Red;
                case LogType.WARNING: return ConsoleColor.Yellow;
                default: return ConsoleColor.White;
            }
        }

        public static void Log(string msg, string module, LogType type = LogType.INFO, ConsoleColor? specialColor = null)
        {
            if (!Enabled) return;
            if (Filter.Contains(module)) return;
            lock (Console.Out)
            {
                ConsoleColor color = specialColor ?? GetConsoleColor(type);
                Console.Error.Write(DateTime.Now.ToString("G"));
                Console.ForegroundColor = color;
                Console.Error.WriteLine($" [{type.ToString()}][{module}] {msg}");
                Console.ResetColor();
            }
        }
    }
}
