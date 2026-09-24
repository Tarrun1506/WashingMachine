using WashingMachine.Constants;

namespace WashingMachine.Helpers;

/// <summary>
/// Simple file-based logger. Appends log entries to a text file.
/// File I/O uses async; everything else is plain synchronous code.
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

    public void Log(string operation, string details = "")
    {
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{operation}] {details}";
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(entry);
        Console.ResetColor();
        // Fire-and-forget the async file write so calling code stays sync
        _ = WriteToFileAsync(entry);
    }

    public void LogError(string operation, string error)
    {
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR:{operation}] {error}";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(entry);
        Console.ResetColor();
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
