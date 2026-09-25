namespace WashingMachine.Storage;

/// <summary>
/// Provides storage operations.
/// </summary>
public interface IStorage
{
    /// <summary>
    /// Loads a list of objects from storage asynchronously.
    /// </summary>
    Task<List<T>> LoadAsync<T>(string filePath);

    /// <summary>
    /// Saves a list of objects to storage asynchronously.
    /// </summary>
    Task SaveAsync<T>(string filePath, List<T> data);

    /// <summary>
    /// Saves a list of objects to storage synchronously — safe to call during shutdown.
    /// </summary>
    void Save<T>(string filePath, List<T> data);

    /// <summary>
    /// Loads a single object from storage asynchronously.
    /// </summary>
    Task<T?> LoadSingleAsync<T>(string filePath);

    /// <summary>
    /// Saves a single object to storage asynchronously.
    /// </summary>
    Task SaveSingleAsync<T>(string filePath, T data);

    /// <summary>
    /// Saves a single object to storage synchronously — safe to call during shutdown.
    /// </summary>
    void SaveSingle<T>(string filePath, T data);
}