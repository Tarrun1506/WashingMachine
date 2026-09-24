using WashingMachine.Enums;

namespace WashingMachine.Models.Programs;

/// <summary>
/// Abstract base class for all washing programs.
/// Demonstrates inheritance and polymorphism: each program defines its own
/// characteristics that affect cycle behaviour without changing the service layer.
/// </summary>
public abstract class WashProgram
{
    /// <summary>Display name of the program.</summary>
    public abstract string Name { get; }

    /// <summary>Total simulated duration of the cycle in seconds.</summary>
    public abstract int TotalDurationSeconds { get; }

    /// <summary>Recommended temperature for this program.</summary>
    public abstract Temperature RecommendedTemperature { get; }

    /// <summary>Default spin speed for this program.</summary>
    public abstract SpinSpeed DefaultSpinSpeed { get; }

    /// <summary>Default water level for this program.</summary>
    public abstract WaterLevel DefaultWaterLevel { get; }

    /// <summary>Whether this program supports pre-wash.</summary>
    public abstract bool SupportsPreWash { get; }

    /// <summary>Short description shown in the configuration screen.</summary>
    public abstract string Description { get; }

    /// <summary>
    /// Returns the ordered list of stages with their durations in seconds.
    /// Derived classes override this to define their own stage timings.
    /// </summary>
    public abstract IReadOnlyList<(CycleStage Stage, int DurationSeconds)> GetStages();

    public override string ToString() => Name;
}
