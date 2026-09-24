using System;
using System.Threading;
using WashingMachine.Models.Entities;
using WashingMachine.Enums;

namespace WashingMachine.Views.Screens;

public static class WashingView
{
    private static readonly object _renderLock = new();

    public static string Show(WashingMachineModel machine)
    {
        Console.Clear();
        Render(machine);

        while (true)
        {
            if (Console.KeyAvailable)
            {
                var input = Console.ReadLine()?.Trim() ?? "";
                if (input == "1" || input == "2" || input == "3" || input == "0")
                {
                    return input;
                }
            }

            Thread.Sleep(500);
            lock (_renderLock)
            {
                Console.SetCursorPosition(0, 0);
                Render(machine);
            }

            if (machine.State is MachineState.Completed or MachineState.Cancelled or MachineState.Idle)
            {
                return "D";
            }
        }
    }

    public static void Render(WashingMachineModel machine)
    {
        var cycle = machine.CurrentCycle;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("          WASHING IN PROGRESS           ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();

        if (cycle != null)
        {
            var stateLabel = machine.State switch
            {
                MachineState.Rinsing  => "Rinsing",
                MachineState.Spinning => "Spinning",
                _                     => "Washing"
            };
            Console.WriteLine($"Stage: {cycle.CurrentStage} - {stateLabel}");
        }
        Console.WriteLine();
        Console.WriteLine($"Program: {machine.Settings.Program.Name}");
        Console.WriteLine($"Progress: {cycle?.ProgressPercent ?? 0:F1}%");
        Console.WriteLine($"Remaining: {FormatRemaining(cycle?.RemainingSeconds ?? 0)}");
        Console.WriteLine($"Door: {(machine.DoorLocked ? "Locked" : "Unlocked")}");
        Console.WriteLine();
        Console.WriteLine("1. Pause");
        Console.WriteLine("2. Pause & Add Clothes");
        Console.WriteLine("3. Cancel Cycle");
        Console.WriteLine("0. Back to Dashboard (Keep running in background)");
        Console.WriteLine();
        Console.WriteLine("Type choice and press Enter:");
        // Keep some blank lines to overwrite old inputs if any
        Console.WriteLine(new string(' ', 40));
        Console.WriteLine(new string(' ', 40));
        Console.SetCursorPosition(0, Console.CursorTop - 2);
    }

    private static string FormatRemaining(int seconds) =>
        $"{seconds / 60:D2}:{seconds % 60:D2}";
}
