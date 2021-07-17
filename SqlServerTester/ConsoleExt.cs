using System;

namespace SqlServerTester
{
    public static class ConsoleExt
    {
        public static void Write(
            object message,
            ConsoleColor? foreground = null,
            ConsoleColor? background = null)
        {
            if (foreground.HasValue)
            {
                Console.ForegroundColor = foreground.Value;
            }

            if (background.HasValue)
            {
                Console.BackgroundColor = background.Value;
            }
            
            Console.Write(message);
            Console.ResetColor();
        }

        public static void WriteLine(
            object message,
            ConsoleColor? foreground = null,
            ConsoleColor? background = null)
        {
            Write(message + "\n", foreground, background);
        }
        
        public static void Error(object message)
        {
            WriteLine(message, ConsoleColor.Red);
        }

        public static void Success(object message)
        {
            WriteLine(message, ConsoleColor.Green);
        }
    }
}