namespace WashingMachine.Models.Programs;

public class CottonProgram : WashProgram
{
    public override string Name => "Cotton";
    public override string Description => "Standard program for normally soiled cottons.";
    protected override int DurationSeconds => 90;
}
