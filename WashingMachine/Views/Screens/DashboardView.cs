using System;
using WashingMachine.Models.Entities;

namespace WashingMachine.Views.Screens;

public static class DashboardView
{
    public static string Show(WashingMachineModel machine)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("    WASHMATE - SMART WASHING MACHINE    ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"STATUS: {machine.State}");
        Console.WriteLine($"LOAD: {machine.ClothesCount} / {WashingMachineModel.MaxCapacity} clothes");
        Console.WriteLine($"PROGRAM: {machine.Settings.Program.Name} | TEMP: {machine.Settings.Temperature} | SPIN: {(int)machine.Settings.SpinSpeed} RPM");
        Console.WriteLine($"DOOR: {(machine.DoorLocked ? "Locked" : "Closed")}");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("Please select an option:");
        Console.WriteLine("1. Start Washing");
        Console.WriteLine("2. Configure Settings");
        Console.WriteLine("3. Add Clothes");
        Console.WriteLine("4. Remove Clothes");
        Console.WriteLine("5. Favourites");
        Console.WriteLine("6. View History");
        if (machine.IsCycleActive || machine.IsCyclePaused)
        {
            Console.WriteLine("7. View Washing Progress");
        }
        Console.WriteLine("0. Exit Application");
        Console.WriteLine();
        Console.Write("Your choice: ");

        return Console.ReadLine()?.Trim() ?? "";
    }
}
