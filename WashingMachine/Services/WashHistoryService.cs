using WashingMachine.Models.Entities;
using WashingMachine.Enums;
using WashingMachine.Repository.Abstractions;
using WashingMachine.Services;

namespace WashingMachine.Services;

/// <summary>
/// Wash history service — all queries use LINQ meaningfully.
/// Demonstrates: OrderByDescending, Where, Take, sorting.
/// </summary>
public sealed class WashHistoryService : IWashHistoryService
{
    private readonly IWashHistoryRepository _repository;

    public WashHistoryService(IWashHistoryRepository repository)
    {
        _repository = repository;
    }

    public Task<List<WashHistory>> GetAllAsync() =>
        _repository.GetAllAsync();

    /// <summary>LINQ: most recent N records, ordered by start time.</summary>
    public async Task<List<WashHistory>> GetRecentAsync(int count = 10)
    {
        var all = await _repository.GetAllAsync();
        return [.. all.OrderByDescending(h => h.StartTime).Take(count)];
    }

    /// <summary>LINQ: filter by program name (case-insensitive).</summary>
    public async Task<List<WashHistory>> GetByProgramAsync(string programName)
    {
        var all = await _repository.GetAllAsync();
        return [.. all.Where(h =>
            h.ProgramName.Equals(programName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(h => h.StartTime)];
    }

    /// <summary>LINQ: filter by final cycle status (Completed / Cancelled).</summary>
    public async Task<List<WashHistory>> GetByStatusAsync(CycleStatus status)
    {
        var all = await _repository.GetAllAsync();
        return [.. all.Where(h => h.Status == status)
                     .OrderByDescending(h => h.StartTime)];
    }

    /// <summary>LINQ: sort by start time.</summary>
    public async Task<List<WashHistory>> GetSortedByDateAsync(bool descending = true)
    {
        var all = await _repository.GetAllAsync();
        return descending
            ? [.. all.OrderByDescending(h => h.StartTime)]
            : [.. all.OrderBy(h => h.StartTime)];
    }

    /// <summary>LINQ: sort by cycle duration.</summary>
    public async Task<List<WashHistory>> GetSortedByDurationAsync(bool descending = true)
    {
        var all = await _repository.GetAllAsync();
        return descending
            ? [.. all.OrderByDescending(h => h.Duration)]
            : [.. all.OrderBy(h => h.Duration)];
    }

    /// <summary>Creates and persists a history record from the completed cycle.</summary>
    public async Task RecordAsync(WashingMachineModel machine, WashCycle cycle, CycleStatus status)
    {
        var record = new WashHistory
        {
            ProgramName  = machine.Settings.Program.Name,
            Temperature  = machine.Settings.Temperature,
            SpinSpeed    = machine.Settings.SpinSpeed,
            WaterLevel   = machine.Settings.WaterLevel,
            PreWash      = machine.Settings.PreWash,
            ExtraRinse   = machine.Settings.ExtraRinse,
            QuickMode    = machine.Settings.QuickMode,
            StartTime    = cycle.StartTime,
            EndTime      = cycle.EndTime ?? DateTime.Now,
            ClothesCount = cycle.ClothesCount,
            Status       = status
        };
        await _repository.AddAsync(record);
    }
}
