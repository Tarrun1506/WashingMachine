using WashingMachine.Enums;

namespace WashingMachine.Models;

/// <summary>
/// Represents a favourite wash configuration.
/// </summary>
public class Favourite
{
    /// <summary>
    /// Gets or sets the favourite identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the favourite name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the program name.
    /// </summary>
    public string ProgramName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temperature.
    /// </summary>
    public Temperature Temperature { get; set; }

    /// <summary>
    /// Gets or sets the spin speed.
    /// </summary>
    public SpinSpeed SpinSpeed { get; set; }

    /// <summary>
    /// Gets or sets the water level.
    /// </summary>
    public WaterLevel WaterLevel { get; set; }

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
