using WashingMachine.Models;

namespace WashingMachine.Repository;

/// <summary>
/// Provides wash history repository operations.
/// </summary>
public interface IWashHistoryRepository
{
    /// <summary>
    /// Gets all history records.
    /// </summary>
    /// <returns>Collection of history records.</returns>
    Task<List<WashHistory>> GetAllAsync();

    /// <summary>
    /// Adds a history record.
    /// </summary>
    /// <param name="history">History record.</param>
    Task AddAsync(WashHistory history);
}