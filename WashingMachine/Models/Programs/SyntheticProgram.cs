namespace WashingMachine.Models.Programs;

public class SyntheticProgram : WashProgram
{
    public override string Name => "Synthetic";
    public override string Description => "For synthetic fabrics like polyester and polyamide.";
    protected override int DurationSeconds => 60;
}
