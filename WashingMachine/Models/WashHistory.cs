using WashingMachine.Enums;

namespace WashingMachine.Models;

/// <summary>
/// Represents wash history.
/// </summary>
public class WashHistory
{
    /// <summary>
    /// Gets or sets the history identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the program name.
    /// </summary>
    public string ProgramName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public CycleStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the clothes count.
    /// </summary>
    public int ClothesCount { get; set; }

    /// <summary>
    /// Gets the duration.
    /// </summary>
    public TimeSpan Duration =>
        this.EndTime - this.StartTime;
}