namespace WashingMachine.Storage;

/// <summary>
/// Provides storage operations.
/// </summary>
public interface IStorage
{
    /// <summary>
    /// Loads data from storage.
    /// </summary>
    /// <typeparam name="T">Type of data.</typeparam>
    /// <param name="filePath">File path.</param>
    /// <returns>Loaded data.</returns>
    Task<List<T>> LoadAsync<T>(string filePath);

    /// <summary>
    /// Saves data to storage.
    /// </summary>
    /// <typeparam name="T">Type of data.</typeparam>
    /// <param name="filePath">File path.</param>
    /// <param name="data">Data to save.</param>
    /// <returns>A task representing the operation.</returns>
    Task SaveAsync<T>(string filePath, List<T> data);

    /// <summary>
    /// Loads a single object from storage.
    /// </summary>
    /// <typeparam name="T">Type of data.</typeparam>
    /// <param name="filePath">File path.</param>
    /// <returns>Loaded object.</returns>
    Task<T?> LoadSingleAsync<T>(string filePath);

    /// <summary>
    /// Saves a single object to storage.
    /// </summary>
    /// <typeparam name="T">Type of data.</typeparam>
    /// <param name="filePath">File path.</param>
    /// <param name="data">Data to save.</param>
    /// <returns>A task representing the operation.</returns>
    Task SaveSingleAsync<T>(string filePath, T data);
}