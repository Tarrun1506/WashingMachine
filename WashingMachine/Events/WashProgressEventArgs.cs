using WashingMachine.Enums;

namespace WashingMachine.Events;

/// <summary>
/// Represents progress information for a washing cycle.
/// </summary>
public class WashProgressEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the current cycle stage.
    /// </summary>
    public CycleStage Stage { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage.
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Gets or sets the remaining seconds.
    /// </summary>
    public int RemainingSeconds { get; set; }
}