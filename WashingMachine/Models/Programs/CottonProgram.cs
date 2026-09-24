using WashingMachine.Enums;

namespace WashingMachine.Models.Programs;

/// <summary>Cotton program — standard full cycle for everyday cotton garments.</summary>
public sealed class CottonProgram : WashProgram
{
    public override string Name => "Cotton";
    public override int TotalDurationSeconds => 70;
    public override Temperature RecommendedTemperature => Temperature.Warm;
    public override SpinSpeed DefaultSpinSpeed => SpinSpeed.Rpm1000;
    public override WaterLevel DefaultWaterLevel => WaterLevel.High;
    public override bool SupportsPreWash => true;
    public override string Description => "Full cycle for everyday cotton garments";

    public override IReadOnlyList<(CycleStage Stage, int DurationSeconds)> GetStages() =>
    [
        (CycleStage.Preparing,  3),
        (CycleStage.Filling,    5),
        (CycleStage.Washing,   25),
        (CycleStage.Draining,   5),
        (CycleStage.Rinsing,   17),
        (CycleStage.Draining,   5),
        (CycleStage.Spinning,  10)
    ];
}
