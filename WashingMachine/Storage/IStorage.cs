namespace WashingMachine.Storage;

/// <summary>
/// Storage abstraction for persisting and loading typed data collections.
/// Repositories depend on this interface (Dependency Inversion Principle),
/// allowing the underlying format to be swapped without touching the repository.
/// </summary>
public interface IStorage
{
    /// <summary>Loads all items from a named store. Returns an empty list if the store does not exist.</summary>
    Task<List<T>> LoadAllAsync<T>(string storeName);

    Task SaveAllAsync<T>(string storeName, IEnumerable<T> items);

    Task SaveMachineStateAsync(WashingMachine.Models.Entities.WashingMachineModel machine);
    Task<WashingMachine.Models.Entities.WashingMachineModel?> LoadMachineStateAsync();
}
