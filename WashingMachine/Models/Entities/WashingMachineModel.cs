using WashingMachine.Enums;

namespace WashingMachine.Models.Entities;

/// <summary>
/// Represents the overall state of the washing machine.
/// This is the central domain aggregate — persisted to JSON on shutdown.
/// </summary>
public class WashingMachineModel
{
    public const int MaxCapacity = 10;

    public MachineState State { get; set; } = MachineState.Idle;
    public int ClothesCount { get; set; } = 0;
    public bool DoorLocked { get; set; } = false;
    public WashSettings Settings { get; set; } = new();
    public WashCycle? CurrentCycle { get; set; }

    /// <summary>True when a cycle is actively in progress (not paused, not idle).</summary>
    public bool IsCycleActive =>
        State is MachineState.Washing or MachineState.Rinsing or MachineState.Spinning;

    /// <summary>True when a cycle exists but is waiting (paused or adding clothes).</summary>
    public bool IsCyclePaused =>
        State is MachineState.Paused or MachineState.AddingClothes;

    /// <summary>True when the machine has at least one item loaded.</summary>
    public bool HasClothes => ClothesCount > 0;

    /// <summary>Remaining capacity.</summary>
    public int AvailableCapacity => MaxCapacity - ClothesCount;
}
