using WashingMachine.Enums;

namespace WashingMachine.Models.Programs;

/// <summary>Heavy Wash program — intensive cycle for heavily soiled items.</summary>
public sealed class HeavyWashProgram : WashProgram
{
    public override string Name => "Heavy Wash";
    public override int TotalDurationSeconds => 90;
    public override Temperature RecommendedTemperature => Temperature.Hot;
    public override SpinSpeed DefaultSpinSpeed => SpinSpeed.Rpm1400;
    public override WaterLevel DefaultWaterLevel => WaterLevel.High;
    public override bool SupportsPreWash => true;
    public override string Description => "Intensive cycle for heavily soiled items";

    public override IReadOnlyList<(CycleStage Stage, int DurationSeconds)> GetStages() =>
    [
        (CycleStage.Preparing,  3),
        (CycleStage.Filling,    7),
        (CycleStage.Washing,   35),
        (CycleStage.Draining,   6),
        (CycleStage.Rinsing,   22),
        (CycleStage.Draining,   6),
        (CycleStage.Spinning,  11)
    ];
}
