using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Handles split-screen rendering with a fixed dashboard at the top.
/// </summary>
public class DashboardRenderer
{
    private const int DashboardHeight = 12; // Height of the dashboard section

    public DashboardRenderer()
    {
    }

    /// <summary>
    /// Gets the starting line for the content area (below the dashboard).
    /// </summary>
    public int ContentStartLine => DashboardHeight + 1;

    /// <summary>
    /// Renders the dashboard at the top of the console.
    /// </summary>
    public void RenderDashboard(WashingMachineModel machine)
    {
        try
        {
            // Save current cursor position
            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            // Move to top of console
            Console.SetCursorPosition(0, 0);

            // Clear only the dashboard area
            for (int i = 0; i < DashboardHeight; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            // Render dashboard content
            Console.SetCursorPosition(0, 0);
            DisplayHeader();
            DisplayProgressSection(machine);

            // Restore cursor position
            Console.SetCursorPosition(originalLeft, originalTop);
        }
        catch (Exception ex)
        {
            // Fallback to simple rendering if console operations fail
            Console.WriteLine($"Dashboard render error: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears the content area (below the dashboard).
    /// </summary>
    public void ClearContentArea()
    {
        try
        {
            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            for (int i = ContentStartLine; i < Console.WindowHeight; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            Console.SetCursorPosition(originalLeft, originalTop);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Clear content area error: {ex.Message}");
        }
    }

    /// <summary>
    /// Moves cursor to the start of the content area.
    /// </summary>
    public void MoveToContentArea()
    {
        try
        {
            Console.SetCursorPosition(0, ContentStartLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Move to content area error: {ex.Message}");
        }
    }

    /// <summary>
    /// Moves cursor to a specific position within the content area.
    /// </summary>
    public void MoveToContentPosition(int left, int relativeTop)
    {
        try
        {
            Console.SetCursorPosition(left, ContentStartLine + relativeTop);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Move to content position error: {ex.Message}");
        }
    }

    private void DisplayHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║" + CenterText("WASHMATE - SMART WASHING MACHINE", 56) + "║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    private void DisplayProgressSection(WashingMachineModel machine)
    {
        // Status indicator
        Console.Write("Status: ");
        ConsoleColor statusColor = machine.State switch
        {
            MachineState.Running => ConsoleColor.Green,
            MachineState.Paused => ConsoleColor.Yellow,
            MachineState.Idle => ConsoleColor.Gray,
            MachineState.Ready => ConsoleColor.Blue,
            _ => ConsoleColor.White
        };
        Console.ForegroundColor = statusColor;
        string statusText = machine.State.ToString().ToUpper();
        Console.Write($"[{statusText}]");
        Console.ResetColor();
        Console.WriteLine();

        // Machine info
        Console.WriteLine($"┌─ Clothes: {machine.ClothesCount}/{WashingMachineModel.MaximumCapacity}");
        Console.WriteLine($"├─ Program: {machine.Settings.ProgramName}");
        Console.WriteLine($"└─ Door: {(machine.IsDoorLocked ? "LOCKED" : "UNLOCKED")}");

        // Real-time progress if running
        if (machine.State == MachineState.Running && machine.CurrentCycle != null)
        {
            DisplayProgressBar(machine.CurrentCycle);
        }
        else if (machine.State == MachineState.Paused && machine.CurrentCycle != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⏸ Cycle PAUSED");
            Console.ResetColor();
            DisplayProgressBar(machine.CurrentCycle);
        }
    }

    private void DisplayProgressBar(WashCycle cycle)
    {
        int barWidth = 40;
        int filled = (int)(cycle.ProgressPercentage / 100 * barWidth);
        int empty = barWidth - filled;

        Console.Write("Stage: ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(cycle.Stage.ToString());
        Console.ResetColor();

        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(new string('█', filled));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('░', empty));
        Console.ResetColor();
        Console.Write($"] {cycle.ProgressPercentage:F1}%");
        Console.WriteLine();

        Console.Write("Time: ");
        int minutes = cycle.RemainingSeconds / 60;
        int seconds = cycle.RemainingSeconds % 60;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{minutes:D2}:{seconds:D2} remaining");
        Console.ResetColor();
    }

    private string CenterText(string text, int width)
    {
        if (text.Length >= width) return text.Substring(0, width);
        int padding = (width - text.Length) / 2;
        return new string(' ', padding) + text + new string(' ', width - text.Length - padding);
    }
}
