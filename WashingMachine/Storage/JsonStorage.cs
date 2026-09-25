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
    public async Task SaveAsync<T>(string filePath, List<T> data)
    {
        try
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, data, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new StorageException(ErrorMessages.SerializeFailed, exception);
        }
        catch (IOException exception)
        {
            throw new StorageException(ErrorMessages.WriteFailed, exception);
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
            EnsureDirectory(filePath);
            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception exception)
        {
            throw new StorageException(ErrorMessages.WriteFailed, exception);
        }
    }

    /// <inheritdoc/>
    public async Task<T?> LoadSingleAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }

        try
        {
            await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
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
    public async Task SaveSingleAsync<T>(string filePath, T data)
    {
        try
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, data, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new StorageException(ErrorMessages.SerializeFailed, exception);
        }
        catch (IOException exception)
        {
            throw new StorageException(ErrorMessages.WriteFailed, exception);
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
            EnsureDirectory(filePath);
            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(filePath, json);
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
}