namespace WashingMachine.Storage;

/// <summary>
/// Provides storage operations.
/// </summary>
public interface IStorage
{
    Task<List<T>> LoadAsync<T>(string filePath);
    void Save<T>(string filePath, List<T> data);
    T? LoadSingle<T>(string filePath);
    void SaveSingle<T>(string filePath, T data);
}
