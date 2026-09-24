using WashingMachine.Enums;

namespace WashingMachine.Models;

/// <summary>
/// Represents an active wash cycle.
/// </summary>
public class WashCycle
{
    /// <summary>
    /// Gets or sets the cycle identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the start time.
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the current stage.
    /// </summary>
    public CycleStage Stage { get; set; } = CycleStage.Filling;

    /// <summary>
    /// Gets or sets the progress percentage.
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Gets or sets the remaining seconds.
    /// </summary>
    public int RemainingSeconds { get; set; }
}