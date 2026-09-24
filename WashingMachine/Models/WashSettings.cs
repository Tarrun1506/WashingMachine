using WashingMachine.Enums;

namespace WashingMachine.Models;

/// <summary>
/// Represents wash settings.
/// </summary>
public class WashSettings
{
    /// <summary>
    /// Gets or sets the program name.
    /// </summary>
    public string ProgramName { get; set; } = "Cotton";

    /// <summary>
    /// Gets or sets the temperature.
    /// </summary>
    public Temperature Temperature { get; set; } = Temperature.Warm;

    /// <summary>
    /// Gets or sets the spin speed.
    /// </summary>
    public SpinSpeed SpinSpeed { get; set; } = SpinSpeed.Rpm1000;

    /// <summary>
    /// Gets or sets the water level.
    /// </summary>
    public WaterLevel WaterLevel { get; set; } = WaterLevel.Medium;

    /// <summary>
    /// Gets or sets a value indicating whether prewash is enabled.
    /// </summary>
    public bool IsPreWashEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether extra rinse is enabled.
    /// </summary>
    public bool IsExtraRinseEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether quick wash is enabled.
    /// </summary>
    public bool IsQuickWashEnabled { get; set; }
}