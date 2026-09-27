using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Handles split-screen rendering with a fixed dashboard at the top.
/// </summary>
public class DashboardRenderer
{
    private const int DashboardHeight = 10; // Height of the dashboard section
    private WashingMachineModel _machine;

    public DashboardRenderer(WashingMachineModel machine)
    {
        _machine = machine;
    }

    /// <summary>
    /// Gets the starting line for the content area (below the dashboard).
    /// </summary>
    public int ContentStartLine => DashboardHeight + 1;

    /// <summary>
    /// Updates the machine reference for dashboard rendering.
    /// </summary>
    public void UpdateMachine(WashingMachineModel machine)
    {
        _machine = machine;
    }

    /// <summary>
    /// Renders the dashboard at the top of the console without clearing the entire screen.
    /// </summary>
    public void RenderDashboard()
    {
        try
        {
            // Save current cursor position
            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            // Move to top of console
            Console.SetCursorPosition(0, 0);

            // Clear only the dashboard area line by line with proper spacing
            for (int i = 0; i < DashboardHeight; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            // Render dashboard content
            Console.SetCursorPosition(0, 0);
            DisplayHeader();
            DisplayProgressSection();

            // Restore cursor position to content area if we were there
            if (originalTop >= ContentStartLine)
            {
                Console.SetCursorPosition(originalLeft, originalTop);
            }
            else
            {
                Console.SetCursorPosition(0, ContentStartLine);
            }
        }
        catch
        {
            // If console operations fail, fall back to simple rendering
            Console.Clear();
            DisplayHeader();
            DisplayProgressSection();
            Console.WriteLine();
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

            Console.SetCursorPosition(0, ContentStartLine);
        }
        catch
        {
            Console.Clear();
            Console.SetCursorPosition(0, ContentStartLine);
        }
    }

    /// <summary>
    /// Forces a full screen redraw (clears everything and redraws dashboard).
    /// </summary>
    public void ForceFullRedraw()
    {
        try
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            DisplayHeader();
            DisplayProgressSection();
            Console.SetCursorPosition(0, ContentStartLine);
        }
        catch
        {
            Console.Clear();
            DisplayHeader();
            DisplayProgressSection();
            Console.WriteLine();
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
        catch
        {
            // If cursor positioning fails, just write newlines
            for (int i = 0; i < ContentStartLine; i++)
            {
                Console.WriteLine();
            }
        }
    }

    private void DisplayHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("+============================================================+");
        Console.WriteLine("|             WASHMATE - SMART WASHING MACHINE                |");
        Console.WriteLine("+============================================================+");
        Console.ResetColor();
    }

    private void DisplayProgressSection()
    {
        // Status indicator
        Console.Write("Status: ");
        ConsoleColor statusColor = _machine.State switch
        {
            MachineState.Running => ConsoleColor.Green,
            MachineState.Paused => ConsoleColor.Yellow,
            MachineState.Idle => ConsoleColor.Gray,
            MachineState.Ready => ConsoleColor.Blue,
            _ => ConsoleColor.White
        };
        Console.ForegroundColor = statusColor;
        string statusText = _machine.State.ToString().ToUpper();
        Console.Write($"[{statusText}]");
        Console.ResetColor();
        Console.WriteLine();

        // Machine info
        Console.WriteLine($"+- Clothes: {_machine.ClothesCount}/{WashingMachineModel.MaximumCapacity}");
        Console.WriteLine($"|- Program: {_machine.Settings.ProgramName}");
        Console.WriteLine($"+- Door: {(_machine.IsDoorLocked ? "LOCKED" : "UNLOCKED")}");

        // Real-time progress if running
        if (_machine.State == MachineState.Running && _machine.CurrentCycle != null)
        {
            DisplayProgressBar(_machine.CurrentCycle);
        }
        else if (_machine.State == MachineState.Paused && _machine.CurrentCycle != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⏸ Cycle PAUSED");
            Console.ResetColor();
            DisplayProgressBar(_machine.CurrentCycle);
        }
        else if (_machine.State == MachineState.Idle && _machine.ClothesCount == 0)
        {
            // Show completion message when idle with no clothes
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Ready for next cycle");
            Console.ResetColor();
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
        Console.Write(new string('#', filled));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('-', empty));
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
}
