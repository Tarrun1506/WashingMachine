using WashingMachine.Models.Entities;
using WashingMachine.Repository.Abstractions;
using WashingMachine.Storage;

namespace WashingMachine.Repository.Implementations;

/// <summary>
/// Wash history repository implementation.
/// Persists history records via IStorage and uses LINQ for all queries
/// exposed to the service layer.
/// </summary>
public sealed class WashHistoryRepository : IWashHistoryRepository
{
    private const string StoreName = "wash-history";
    private readonly IStorage _storage;

    public WashHistoryRepository(IStorage storage)
    {
        _storage = storage;
    }

    public Task<List<WashHistory>> GetAllAsync() =>
        _storage.LoadAllAsync<WashHistory>(StoreName);

    public async Task<WashHistory?> GetByIdAsync(Guid id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(h => h.Id == id);
    }

    public async Task AddAsync(WashHistory history)
    {
        var all = await GetAllAsync();
        all.Add(history);
        await _storage.SaveAllAsync(StoreName, all);
    }

    public async Task UpdateAsync(WashHistory history)
    {
        var all = await GetAllAsync();
        var index = all.FindIndex(h => h.Id == history.Id);
        if (index >= 0) all[index] = history;
        await _storage.SaveAllAsync(StoreName, all);
    }

    public async Task DeleteAsync(Guid id)
    {
        var all = await GetAllAsync();
        all.RemoveAll(h => h.Id == id);
        await _storage.SaveAllAsync(StoreName, all);
    }
}
