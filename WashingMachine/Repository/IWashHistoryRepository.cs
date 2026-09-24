using WashingMachine.Models.Entities;

namespace WashingMachine.Repository.Abstractions;

/// <summary>
/// CRUD repository for wash history records.
/// </summary>
public interface IWashHistoryRepository
{
    Task<List<WashHistory>> GetAllAsync();
    Task<WashHistory?> GetByIdAsync(Guid id);
    Task AddAsync(WashHistory history);
    Task UpdateAsync(WashHistory history);
    Task DeleteAsync(Guid id);
}
