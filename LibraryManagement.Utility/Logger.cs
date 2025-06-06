using System;
using System.IO;
using System.Threading;

namespace LibraryManagement.Utility;
public static class Logger
{
    private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiLogs.txt");
    private static readonly object _lock = new object();

    public static void Log(string message)
    {
        try
        {
            lock (_lock)
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // If logging fails, silently fail or handle accordingly
        }
    }
}
