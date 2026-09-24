using WashingMachine.Enums;

namespace WashingMachine.Models.Entities;

/// <summary>
/// Represents the live state of an active or completed wash cycle.
/// Updated by the cycle service as the cycle progresses.
/// </summary>
public class WashCycle
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime StartTime { get; init; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public CycleStage CurrentStage { get; set; } = CycleStage.Preparing;
    public double ProgressPercent { get; set; } = 0;
    public int RemainingSeconds { get; set; }
    public int ClothesCount { get; set; }
    public CycleStatus? FinalStatus { get; set; }

    /// <summary>Elapsed time of the cycle.</summary>
    public TimeSpan Elapsed => (EndTime ?? DateTime.Now) - StartTime;
}
