using System;
using WashingMachine.Models.Entities;

namespace WashingMachine.Views.Screens;

public static class PauseView
{
    public static string Show(WashingMachineModel machine)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("========================================");
        Console.WriteLine("             WASHING PAUSED             ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();

        var cycle = machine.CurrentCycle;
        Console.WriteLine($"Current Load: {machine.ClothesCount} clothes");
        Console.WriteLine($"Progress: {cycle?.ProgressPercent:F1}%");
        Console.WriteLine();

        if (!machine.DoorLocked)
        {
            Console.WriteLine("You can safely add clothes now.");
            Console.WriteLine("1. Resume Washing");
            Console.WriteLine("2. Add Clothes");
            Console.WriteLine("3. Cancel Cycle");
            Console.WriteLine("0. Back to Dashboard");
        }
        else
        {
            Console.WriteLine("1. Resume Washing");
            Console.WriteLine("2. Pause & Add Clothes");
            Console.WriteLine("3. Cancel Cycle");
            Console.WriteLine("0. Back to Dashboard");
        }

        Console.WriteLine();
        Console.Write("Your choice: ");
        return Console.ReadLine()?.Trim() ?? "";
    }
}
