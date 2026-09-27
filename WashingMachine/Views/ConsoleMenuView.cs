using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

public sealed class ConsoleMenuView
{
    private static readonly object ConsoleGate = new();
    public MenuOption Show(WashingMachineModel machine)
    {
        Console.WriteLine("\n===== WASHMATE =====");
        Console.WriteLine($"Status: {machine.State} | Clothes: {machine.ClothesCount}/{WashingMachineModel.MaximumCapacity} | Program: {machine.Settings.ProgramName}");
        if (machine.CurrentCycle is not null)
            Console.WriteLine($"{machine.CurrentCycle.Stage} — {machine.CurrentCycle.ProgressPercentage:F0}% ({machine.CurrentCycle.RemainingSeconds}s remaining)");
        Console.WriteLine("1. Start wash\n2. Configure settings\n3. Add clothes\n4. Remove clothes\n5. Favourites\n6. View history\n7. Pause cycle\n0. Exit");
        return (MenuOption)ConsoleInput.ReadIntInRange("Enter choice: ", 0, 7, ErrorMessages.InvalidMenuChoice);
    }

    public int ReadClothesCount(int min, int max) => ConsoleInput.ReadIntInRange(
        "Enter clothes count: ", min, max, $"Please enter a number between {min} and {max}.");

    public void DisplayError(string message) => DisplayMessage(message, true);
    public void DisplayMessage(string message) => DisplayMessage(message, false);
    public void DisplaySuccess(string message) => DisplayMessage(message, false);

    public void DisplayError(WashingMachineModel machine, string message) => DisplayMessage(message, true);
    public void DisplayMessage(WashingMachineModel machine, string message) => DisplayMessage(message, false);
    public void DisplaySuccess(WashingMachineModel machine, string message) => DisplayMessage(message, false);

    public void Notify(string message)
    {
        lock (ConsoleGate) Console.WriteLine($"\n{message}");
    }

    private static void DisplayMessage(string message, bool isError)
    {
        if (isError) ConsoleInput.PrintError(message);
        else Console.WriteLine(message);
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }
}
