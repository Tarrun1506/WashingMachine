using WashingMachine.Models.Common;
using WashingMachine.Models.Entities;
using WashingMachine.Enums;

namespace WashingMachine.Services;

/// <summary>Service for querying and filtering wash history using LINQ.</summary>
public interface IWashHistoryService
{
    Task<List<WashHistory>> GetAllAsync();
    Task<List<WashHistory>> GetRecentAsync(int count = 10);
    Task<List<WashHistory>> GetByProgramAsync(string programName);
    Task<List<WashHistory>> GetByStatusAsync(CycleStatus status);
    Task<List<WashHistory>> GetSortedByDateAsync(bool descending = true);
    Task<List<WashHistory>> GetSortedByDurationAsync(bool descending = true);
    Task RecordAsync(WashingMachineModel machine, WashCycle cycle, CycleStatus status);
}
