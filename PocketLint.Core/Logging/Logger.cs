using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace PocketLint.Core.Logging;
public class Logger
{
    #region Properties and Fields

    private static readonly List<ILogger> _loggers = new();

    #endregion

    #region Public Methods

    public static void Register(ILogger logger)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (!_loggers.Contains(logger)) _loggers.Add(logger);
    }

    public static void Unregister(ILogger logger)
    {
        if (logger != null) _loggers.Remove(logger);
    }

    public static void Log(string message, [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0, [CallerFilePath] string callerPath = "")
    {
        callerPath = Path.GetFileName(callerPath);
        foreach (var logger in _loggers)
            logger.Log($"({callerPath}:{caller}:{line}) - {message}");
    }

    public static void Warn(string message, [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0, [CallerFilePath] string callerPath = "")
    {
        callerPath = Path.GetFileName(callerPath);
        foreach (var logger in _loggers)
            logger.Warn($"({callerPath}:{caller}:{line}) - {message}");
    }

    public static void Error(string message, [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0, [CallerFilePath] string callerPath = "")
    {
        callerPath = Path.GetFileName(callerPath);
        foreach (var logger in _loggers)
            logger.Error($"({callerPath}:{caller}:{line}) - {message}");
    }

    #endregion
}
