using WashingMachine.Enums;

namespace WashingMachine.Models.Programs;

/// <summary>Delicate program — extra-gentle cycle for delicate fabrics.</summary>
public sealed class DelicateProgram : WashProgram
{
    public override string Name => "Delicate";
    public override int TotalDurationSeconds => 50;
    public override Temperature RecommendedTemperature => Temperature.Cold;
    public override SpinSpeed DefaultSpinSpeed => SpinSpeed.Rpm400;
    public override WaterLevel DefaultWaterLevel => WaterLevel.Medium;
    public override bool SupportsPreWash => false;
    public override string Description => "Extra-gentle cycle for delicate fabrics";

    public override IReadOnlyList<(CycleStage Stage, int DurationSeconds)> GetStages() =>
    [
        (CycleStage.Preparing,  3),
        (CycleStage.Filling,    5),
        (CycleStage.Washing,   15),
        (CycleStage.Draining,   4),
        (CycleStage.Rinsing,   15),
        (CycleStage.Draining,   4),
        (CycleStage.Spinning,   4)
    ];
}
