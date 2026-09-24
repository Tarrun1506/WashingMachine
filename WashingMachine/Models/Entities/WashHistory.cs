using WashingMachine.Enums;

namespace WashingMachine.Models.Entities;

/// <summary>
/// An immutable record of a completed or cancelled wash cycle.
/// Stored in wash-history.json via the WashHistoryRepository.
/// </summary>
public class WashHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProgramName { get; set; } = string.Empty;
    public Temperature Temperature { get; set; }
    public SpinSpeed SpinSpeed { get; set; }
    public WaterLevel WaterLevel { get; set; }
    public bool PreWash { get; set; }
    public bool ExtraRinse { get; set; }
    public bool QuickMode { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ClothesCount { get; set; }
    public CycleStatus Status { get; set; }

    /// <summary>Duration of the wash cycle.</summary>
    public TimeSpan Duration => EndTime - StartTime;
}
