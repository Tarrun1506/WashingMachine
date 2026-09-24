namespace WashingMachine.Enums;

/// <summary>Stages within a single wash cycle.</summary>
public enum CycleStage
{
    Preparing,
    Filling,
    Washing,
    Draining,
    Rinsing,
    Spinning,
    Completed
}
