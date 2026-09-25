namespace WashingMachine.Models.Programs;

public class WoolProgram : WashProgram
{
    public override string Name => "Wool";
    public override string Description => "Gentle cycle for woollen garments.";
    protected override int DurationSeconds => 45;
}
