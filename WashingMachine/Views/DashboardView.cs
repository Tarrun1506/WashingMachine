using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides dashboard operations.
/// </summary>
public class DashboardView
{
    public MenuOption Show(WashingMachineModel machine)
    {
        Console.Clear();

        Console.WriteLine(MenuMessages.ApplicationTitle);
        Console.WriteLine();
        Console.WriteLine($"State           : {machine.State}");
        Console.WriteLine($"Clothes Count   : {machine.ClothesCount}");
        Console.WriteLine($"Program         : {machine.Settings.ProgramName}");
        Console.WriteLine();

        Console.WriteLine(MenuMessages.StartWash);
        Console.WriteLine(MenuMessages.ConfigureSettings);
        Console.WriteLine(MenuMessages.AddClothes);
        Console.WriteLine(MenuMessages.RemoveClothes);
        Console.WriteLine(MenuMessages.Favourites);
        Console.WriteLine(MenuMessages.History);
        Console.WriteLine(MenuMessages.Progress);
        Console.WriteLine(MenuMessages.PauseCycle);
        Console.WriteLine(MenuMessages.Exit);
        Console.WriteLine();

        Console.Write("Enter Choice : ");
        int.TryParse(Console.ReadLine(), out int choice);
        return (MenuOption)choice;
    }

    public int ReadClothesCount()
    {
        Console.Write("Enter Clothes Count : ");
        int.TryParse(Console.ReadLine(), out int count);
        return count;
    }

    public void DisplayMessage(string message)
    {
        Console.WriteLine(message);
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }

    public void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }

    public void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }
}