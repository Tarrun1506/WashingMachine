using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides dashboard operations with real-time progress display.
/// </summary>
public class DashboardView
{

    public MenuOption Show(WashingMachineModel machine)
    {
        // Use simple split-screen approach without complex cursor operations
        Console.Clear();

        // Display dashboard at top
        DisplayHeader(machine);
        DisplayProgressSection(machine);
        Console.WriteLine();

        // Display menu below
        DisplayMenu();
        Console.Write("Enter Choice : ");
        int.TryParse(Console.ReadLine(), out int choice);
        return (MenuOption)choice;
    }

    private void DisplayHeader(WashingMachineModel machine)
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

    public void DisplayOnly(WashingMachineModel machine)
    {
        Console.Clear();

        // Display dashboard at top
        DisplayHeader(machine);
        DisplayProgressSection(machine);
        Console.WriteLine();

        // Display menu below
        DisplayMenu();
        Console.WriteLine("Press any key to access menu...");
    }

    /// <summary>
    /// Updates only the dashboard section without clearing the content area.
    /// Note: This simple implementation clears the whole screen due to console limitations.
    /// </summary>
    public void UpdateDashboard(WashingMachineModel machine)
    {
        Console.Clear();
        DisplayHeader(machine);
        DisplayProgressSection(machine);
        Console.WriteLine();
    }

    private void DisplayMenu()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ " + PadRight("1. Start Washing", 54) + "│");
        Console.WriteLine("│ " + PadRight("2. Configure Settings", 54) + "│");
        Console.WriteLine("│ " + PadRight("3. Add Clothes", 54) + "│");
        Console.WriteLine("│ " + PadRight("4. Remove Clothes", 54) + "│");
        Console.WriteLine("│ " + PadRight("5. Favourites", 54) + "│");
        Console.WriteLine("│ " + PadRight("6. View History", 54) + "│");
        Console.WriteLine("│ " + PadRight("7. Pause Cycle", 54) + "│");
        Console.WriteLine("│ " + PadRight("0. Exit", 54) + "│");
        Console.WriteLine("└─────────────────────────────────────────────────────────┘");
    }

    public int ReadClothesCount()
    {
        Console.Write("Enter Clothes Count : ");
        int.TryParse(Console.ReadLine(), out int count);
        return count;
    }

    public void DisplayMessage(string message)
    {
        Console.Clear();
        Console.WriteLine(message);
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }

    public void DisplayError(string message)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }

    public void DisplaySuccess(string message)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }

    private string CenterText(string text, int width)
    {
        if (text.Length >= width) return text.Substring(0, width);
        int padding = (width - text.Length) / 2;
        return new string(' ', padding) + text + new string(' ', width - text.Length - padding);
    }

    private string PadRight(string text, int width)
    {
        return text.Length >= width ? text.Substring(0, width) : text + new string(' ', width - text.Length);
    }
}