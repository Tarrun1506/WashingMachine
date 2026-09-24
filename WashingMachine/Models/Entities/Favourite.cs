using WashingMachine.Enums;

namespace WashingMachine.Models.Entities;

/// <summary>
/// A named favourite wash configuration saved by the user.
/// Persisted to favourites.json.
/// </summary>
public class Favourite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public Temperature Temperature { get; set; }
    public SpinSpeed SpinSpeed { get; set; }
    public WaterLevel WaterLevel { get; set; }
    public bool PreWash { get; set; }
    public bool ExtraRinse { get; set; }
    public bool QuickMode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
