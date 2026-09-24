using WashingMachine.Models.Programs;

namespace WashingMachine.Models.Common;

/// <summary>
/// Central registry of all available wash programs.
/// Adding a new program only requires registering it here — the rest of the
/// system picks it up automatically, demonstrating the Open/Closed Principle.
/// </summary>
public static class ProgramRegistry
{
    private static readonly List<WashProgram> _programs =
    [
        new CottonProgram(),
        new SyntheticProgram(),
        new DelicateProgram(),
        new WoolProgram(),
        new QuickWashProgram(),
        new HeavyWashProgram()
    ];

    public static IReadOnlyList<WashProgram> All => _programs;

    /// <summary>Finds a program by name (case-insensitive).</summary>
    public static WashProgram? FindByName(string name) =>
        _programs.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns the program by name or the first program as a default.</summary>
    public static WashProgram GetByNameOrDefault(string name) =>
        FindByName(name) ?? _programs[0];
}
