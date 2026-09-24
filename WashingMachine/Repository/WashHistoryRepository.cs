using WashingMachine.Constants;
using WashingMachine.Models;
using WashingMachine.Storage;

namespace WashingMachine.Repository;

/// <summary>
/// Provides wash history repository operations.
/// </summary>
public class WashHistoryRepository : IWashHistoryRepository
{
    private readonly IStorage _storage;

    /// <summary>
    /// Initializes a new instance of the <see cref="WashHistoryRepository"/> class.
    /// </summary>
    /// <param name="storage">Storage implementation.</param>
    public WashHistoryRepository(IStorage storage)
    {
        this._storage = storage;
    }

    /// <inheritdoc/>
    public async Task<List<WashHistory>> GetAllAsync()
    {
        return await this._storage.LoadAsync<WashHistory>(Configurables.WashHistoryFilePath);
    }

    /// <inheritdoc/>
    public async Task AddAsync(WashHistory history)
    {
        List<WashHistory> historyRecords = await this.GetAllAsync();
        historyRecords.Add(history);
        await this._storage.SaveAsync(Configurables.WashHistoryFilePath, historyRecords);
    }
}