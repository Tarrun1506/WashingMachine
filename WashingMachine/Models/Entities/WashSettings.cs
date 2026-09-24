using WashingMachine.Enums;
using WashingMachine.Models.Programs;

namespace WashingMachine.Models.Entities;

/// <summary>
/// Represents the current washing settings configured by the user.
/// This is a plain data model — no UI or business-rule logic here.
/// </summary>
public class WashSettings
{
    public WashProgram Program { get; set; } = new CottonProgram();
    public Temperature Temperature { get; set; } = Temperature.Warm;
    public SpinSpeed SpinSpeed { get; set; } = SpinSpeed.Rpm1000;
    public WaterLevel WaterLevel { get; set; } = WaterLevel.Medium;
    public bool PreWash { get; set; } = false;
    public bool ExtraRinse { get; set; } = false;
    public bool QuickMode { get; set; } = false;

    /// <summary>Creates a deep copy of the current settings.</summary>
    public WashSettings Clone() => new()
    {
        Program     = Program,
        Temperature = Temperature,
        SpinSpeed   = SpinSpeed,
        WaterLevel  = WaterLevel,
        PreWash     = PreWash,
        ExtraRinse  = ExtraRinse,
        QuickMode   = QuickMode
    };
}
