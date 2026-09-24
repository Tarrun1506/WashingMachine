using WashingMachine.Enums;

namespace WashingMachine.Models.Programs;

/// <summary>Quick Wash program — fast 30-second cycle for lightly soiled items.</summary>
public sealed class QuickWashProgram : WashProgram
{
    public override string Name => "Quick Wash";
    public override int TotalDurationSeconds => 35;
    public override Temperature RecommendedTemperature => Temperature.Cold;
    public override SpinSpeed DefaultSpinSpeed => SpinSpeed.Rpm800;
    public override WaterLevel DefaultWaterLevel => WaterLevel.Low;
    public override bool SupportsPreWash => false;
    public override string Description => "Fast cycle for lightly soiled items";

    public override IReadOnlyList<(CycleStage Stage, int DurationSeconds)> GetStages() =>
    [
        (CycleStage.Preparing,  2),
        (CycleStage.Filling,    3),
        (CycleStage.Washing,   12),
        (CycleStage.Draining,   3),
        (CycleStage.Rinsing,    9),
        (CycleStage.Draining,   3),
        (CycleStage.Spinning,   3)
    ];
}
