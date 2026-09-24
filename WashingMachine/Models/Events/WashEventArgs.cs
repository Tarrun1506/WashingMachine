using WashingMachine.Enums;

namespace WashingMachine.Models.Events;

/// <summary>Event args for progress updates during a wash cycle.</summary>
public class WashProgressEventArgs : EventArgs
{
    public double ProgressPercent { get; init; }
    public int RemainingSeconds { get; init; }
    public CycleStage CurrentStage { get; init; }
}

/// <summary>Event args for machine state changes.</summary>
public class MachineStateChangedEventArgs : EventArgs
{
    public MachineState OldState { get; init; }
    public MachineState NewState { get; init; }
}

/// <summary>Event args for cycle stage transitions.</summary>
public class StageChangedEventArgs : EventArgs
{
    public CycleStage OldStage { get; init; }
    public CycleStage NewStage { get; init; }
}
