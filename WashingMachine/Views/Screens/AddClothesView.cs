using System;
using WashingMachine.Models.Entities;

namespace WashingMachine.Views.Screens;

public static class AddClothesView
{
    public static (int count, bool confirmed) ShowAdd(WashingMachineModel machine)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("              ADD CLOTHES               ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.WriteLine($"Current Load: {machine.ClothesCount}");
        Console.WriteLine($"Available:    {machine.AvailableCapacity}");
        Console.WriteLine();
        Console.Write("How many clothes to add? (0 to cancel): ");
        
        if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
        {
            return (count, true);
        }
        return (0, false);
    }

    public static (int count, bool confirmed) ShowRemove(WashingMachineModel machine)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("========================================");
        Console.WriteLine("             REMOVE CLOTHES             ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.WriteLine($"Current Load: {machine.ClothesCount}");
        Console.WriteLine();
        Console.Write("How many clothes to remove? (0 to cancel): ");
        
        if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
        {
            return (count, true);
        }
        return (0, false);
    }
}
