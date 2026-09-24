using WashingMachine.Enums;

namespace WashingMachine.Models;

/// <summary>
/// Represents the washing machine.
/// </summary>
public class WashingMachineModel
{
    /// <summary>
    /// Maximum clothes capacity.
    /// </summary>
    public const int MaximumCapacity = 10;

    /// <summary>
    /// Gets or sets the machine state.
    /// </summary>
    public MachineState State { get; set; } = MachineState.Idle;

    /// <summary>
    /// Gets or sets the clothes count.
    /// </summary>
    public int ClothesCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the door is locked.
    /// </summary>
    public bool IsDoorLocked { get; set; }

    /// <summary>
    /// Gets or sets the current settings.
    /// </summary>
    public WashSettings Settings { get; set; } = new WashSettings();

    /// <summary>
    /// Gets or sets the current wash cycle.
    /// </summary>
    public WashCycle? CurrentCycle { get; set; }
}