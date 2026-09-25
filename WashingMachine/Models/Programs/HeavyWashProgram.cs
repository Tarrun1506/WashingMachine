namespace WashingMachine.Models.Programs;

public class HeavyWashProgram : WashProgram
{
    public override string Name => "Heavy Wash";
    public override string Description => "Intensive program for heavily soiled items.";
    protected override int DurationSeconds => 120;
}
