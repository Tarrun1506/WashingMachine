using WashingMachine.Models;
using WashingMachine.Repository;

namespace WashingMachine.Services;

/// <summary>
/// Provides wash history operations.
/// </summary>
public class WashHistoryService : IWashHistoryService
{
    private readonly IWashHistoryRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="WashHistoryService"/> class.
    /// </summary>
    /// <param name="repository">History repository.</param>
    public WashHistoryService(IWashHistoryRepository repository)
    {
        this._repository = repository;
    }

    /// <inheritdoc/>
    public async Task<List<WashHistory>> GetAllAsync()
    {
        return await this._repository.GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(WashHistory history)
    {
        await this._repository.AddAsync(history);
    }
}