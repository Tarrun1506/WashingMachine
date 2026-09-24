using WashingMachine.Enums;

namespace WashingMachine.Models.Programs;

/// <summary>Synthetic program — gentle cycle for synthetic fabrics.</summary>
public sealed class SyntheticProgram : WashProgram
{
    public override string Name => "Synthetic";
    public override int TotalDurationSeconds => 55;
    public override Temperature RecommendedTemperature => Temperature.Cold;
    public override SpinSpeed DefaultSpinSpeed => SpinSpeed.Rpm800;
    public override WaterLevel DefaultWaterLevel => WaterLevel.Medium;
    public override bool SupportsPreWash => false;
    public override string Description => "Gentle cycle for synthetic fabrics";

    public override IReadOnlyList<(CycleStage Stage, int DurationSeconds)> GetStages() =>
    [
        (CycleStage.Preparing,  3),
        (CycleStage.Filling,    5),
        (CycleStage.Washing,   18),
        (CycleStage.Draining,   4),
        (CycleStage.Rinsing,   15),
        (CycleStage.Draining,   4),
        (CycleStage.Spinning,   6)
    ];
}
