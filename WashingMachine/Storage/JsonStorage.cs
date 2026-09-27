using System.Text.Json;
using WashingMachine.Constants;
using WashingMachine.Exceptions;

namespace WashingMachine.Storage;

/// <summary>
/// Provides JSON storage operations.
/// </summary>
public class JsonStorage : IStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <inheritdoc/>
    public async Task<List<T>> LoadAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        try
        {
            await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions) ?? [];
        }
        catch (JsonException exception)
        {
            throw new StorageException(ErrorMessages.DeserializeFailed, exception);
        }
        catch (IOException exception)
        {
            throw new StorageException(ErrorMessages.ReadFailed, exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new StorageException(ErrorMessages.AccessDenied, exception);
        }
        catch (NotSupportedException exception)
        {
            throw new StorageException(ErrorMessages.InvalidFilePath, exception);
        }
    }

    /// <inheritdoc/>
    public void Save<T>(string filePath, List<T> data)
    {
        try
        {
            WriteAtomically(filePath, data);
        }
        catch (Exception exception)
        {
            throw new StorageException(ErrorMessages.WriteFailed, exception);
        }
    }

    /// <inheritdoc/>
    public T? LoadSingle<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new StorageException(ErrorMessages.DeserializeFailed, exception);
        }
        catch (IOException exception)
        {
            throw new StorageException(ErrorMessages.ReadFailed, exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new StorageException(ErrorMessages.AccessDenied, exception);
        }
        catch (NotSupportedException exception)
        {
            throw new StorageException(ErrorMessages.InvalidFilePath, exception);
        }
    }

    /// <inheritdoc/>
    public void SaveSingle<T>(string filePath, T data)
    {
        try
        {
            WriteAtomically(filePath, data);
        }
        catch (Exception exception)
        {
            throw new StorageException(ErrorMessages.WriteFailed, exception);
        }
    }

    private static void EnsureDirectory(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    private static void WriteAtomically<T>(string filePath, T data)
    {
        EnsureDirectory(filePath);
        string temporaryPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            using (FileStream stream = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, data, JsonOptions);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
