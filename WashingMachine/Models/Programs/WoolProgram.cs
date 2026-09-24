using WashingMachine.Enums;

namespace WashingMachine.Models.Programs;

/// <summary>Wool program — low-agitation cycle for woollen items.</summary>
public sealed class WoolProgram : WashProgram
{
    public override string Name => "Wool";
    public override int TotalDurationSeconds => 48;
    public override Temperature RecommendedTemperature => Temperature.Cold;
    public override SpinSpeed DefaultSpinSpeed => SpinSpeed.Rpm400;
    public override WaterLevel DefaultWaterLevel => WaterLevel.Low;
    public override bool SupportsPreWash => false;
    public override string Description => "Low-agitation cycle for woollen items";

    public override IReadOnlyList<(CycleStage Stage, int DurationSeconds)> GetStages() =>
    [
        (CycleStage.Preparing,  3),
        (CycleStage.Filling,    5),
        (CycleStage.Washing,   14),
        (CycleStage.Draining,   4),
        (CycleStage.Rinsing,   14),
        (CycleStage.Draining,   4),
        (CycleStage.Spinning,   4)
    ];
}
