namespace WashingMachine.Models.Programs;

public class QuickWashProgram : WashProgram
{
    public override string Name => "Quick Wash";
    public override string Description => "Fast cycle for lightly soiled clothes.";
    protected override int DurationSeconds => 30;
    
    public override int GetDuration()
    {
        return base.GetDuration(); // Just to demonstrate override
    }
}
