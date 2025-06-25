using PocketLint.Core.Logging;

namespace PocketLint.Runner;
public class FileLogger : ILogger
{
    #region Properties and Fields

    private readonly string _filePath;

    #endregion

    #region ctor

    public FileLogger(string filePath)
    {
        _filePath = filePath;
        RotateFiles(filePath);
    }

    #endregion

    #region Public Methods

    public void Log(string message) => WriteLog($"[LOG] {message}");
    public void Warn(string message) => WriteLog($"[WARN] {message}");
    public void Error(string message) => WriteLog($"[ERROR] {message}");

    #endregion

    #region Private Methods

    private void WriteLog(string message)
    {
        try
        {
            File.AppendAllText(_filePath, $"[{DateTime.Now:dd-MM-yyyy hh\\:mm\\:ss.fff}]: {message}\n");
        }
        catch { /* Logging should not break the system */ }
    }

    #endregion

    #region Private Methods

    private static void RotateFiles(string filePath)
    {
        // Keep one copy of the log
        if (!File.Exists(filePath)) return;
        var oldFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"prev_{Path.GetFileName(filePath)}");
        if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
        File.Move(filePath, oldFilePath);
    }

    #endregion
}

