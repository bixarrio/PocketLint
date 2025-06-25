using System;

namespace PocketLint.Core.Logging;

public class ConsoleLogger : ILogger
{
    #region Public Methods

    public void Log(string message) => WriteLine(ConsoleColor.Gray, $"[LOG] {message}");
    public void Warn(string message) => WriteLine(ConsoleColor.DarkYellow, $"[WARN] {message}");
    public void Error(string message) => WriteLine(ConsoleColor.Red, $"[ERROR] {message}");

    #endregion

    #region Private Methods

    private static void WriteLine(ConsoleColor color, string message)
    {
        var originalColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    #endregion
}
