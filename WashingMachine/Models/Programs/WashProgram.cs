namespace WashingMachine.Models.Programs;

/// <summary>
/// Abstract base class for all washing programs.
/// Each program defines its own name, description, and duration.
/// Subclasses override these to provide program-specific behaviour.
/// </summary>
public abstract class WashProgram
{
    /// <summary>Gets the program name.</summary>
    public abstract string Name { get; }

    /// <summary>Gets a short description of what this program is for.</summary>
    public abstract string Description { get; }

    /// <summary>Gets the total wash cycle duration in seconds.</summary>
    protected abstract int DurationSeconds { get; }
    
    /// <summary>Returns the total duration.</summary>
    public virtual int GetDuration() => DurationSeconds;
}
