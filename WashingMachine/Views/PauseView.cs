using WashingMachine.Enums;

namespace WashingMachine.Views;

/// <summary>
/// Provides pause screen operations.
/// </summary>
public class PauseView
{
    public PauseMenuOption Show()
    {
        Console.Clear();
        Console.WriteLine("--- Cycle Paused ---");
        Console.WriteLine("1. Resume");
        Console.WriteLine("2. Cancel");

        Console.Write("\nChoice : ");

        int.TryParse(Console.ReadLine(), out int option);
        return (PauseMenuOption)option;
    }
}