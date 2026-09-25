using WashingMachine.Constants;

namespace WashingMachine.Helpers;

/// <summary>
/// Simple file-based logger. Appends log entries to a text file asynchronously.
/// The calling code stays fully synchronous — file writes are fire-and-forget.
/// </summary>
public class Logger
{
    private readonly string _logFilePath;
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    public Logger()
    {
        _logFilePath = Configurables.LogFilePath;
        string? dir = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    /// <summary>
    /// Logs an informational entry to the log file only (not to the console).
    /// </summary>
    public void Log(string operation, string details = "")
    {
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{operation}] {details}";
        _ = WriteToFileAsync(entry);
    }

    /// <summary>
    /// Logs an error entry to the log file only (not to the console).
    /// </summary>
    public void LogError(string operation, string error)
    {
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR:{operation}] {error}";
        _ = WriteToFileAsync(entry);
    }

    private async Task WriteToFileAsync(string entry)
    {
        await _fileLock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(_logFilePath, entry + Environment.NewLine);
        }
        catch
        {
            // Swallow log errors — logging should never crash the app
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
