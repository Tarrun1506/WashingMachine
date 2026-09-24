namespace WashingMachine.Enums;

/// <summary>The operational state of the washing machine.</summary>
public enum MachineState
{
    Idle,
    Ready,
    Washing,
    Paused,
    AddingClothes,
    Rinsing,
    Spinning,
    Completed,
    Cancelled
}
