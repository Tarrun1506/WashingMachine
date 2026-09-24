using WashingMachine.Models;

namespace WashingMachine.Services;

/// <summary>
/// Provides wash history operations.
/// </summary>
public interface IWashHistoryService
{
    Task<List<WashHistory>> GetAllAsync();

    Task AddAsync(WashHistory history);
}