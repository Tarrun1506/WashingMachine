using WashingMachine.Events;

namespace WashingMachine.Views;

/// <summary>
/// Displays washing progress to the console.
/// </summary>
public class WashingView
{
    /// <summary>
    /// Displays a snapshot of current washing progress.
    /// Called by the controller when the user explicitly selects "View Progress".
    /// Does NOT clear the screen every tick.
    /// </summary>
    public void DisplayProgress(WashProgressEventArgs eventArgs)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("--- Washing Progress ---");
        Console.ResetColor();
        Console.WriteLine($"  Stage     : {eventArgs.Stage}");
        Console.WriteLine($"  Progress  : {eventArgs.ProgressPercentage:F1}%");
        Console.WriteLine($"  Remaining : {eventArgs.RemainingSeconds / 60}m {eventArgs.RemainingSeconds % 60}s");
        Console.WriteLine();
    }
}