using System;
using System.Linq;
using WashingMachine.Models.Entities;
using WashingMachine.Enums;
using WashingMachine.Models.Common;

namespace WashingMachine.Views.Screens;

public static class ConfigurationView
{
    public static WashSettings? Show(WashSettings current)
    {
        var settings = current.Clone();

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("========================================");
            Console.WriteLine("        CONFIGURE WASHING SETTINGS      ");
            Console.WriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine($"1. Program:     {settings.Program.Name} ({settings.Program.Description})");
            Console.WriteLine($"2. Temperature: {settings.Temperature}");
            Console.WriteLine($"3. Spin Speed:  {(int)settings.SpinSpeed} RPM");
            Console.WriteLine($"4. Water Level: {settings.WaterLevel}");
            Console.WriteLine($"5. Pre-Wash:    {(settings.PreWash ? "ON" : "OFF")}");
            Console.WriteLine($"6. Extra Rinse: {(settings.ExtraRinse ? "ON" : "OFF")}");
            Console.WriteLine($"7. Quick Mode:  {(settings.QuickMode ? "ON" : "OFF")}");
            Console.WriteLine();
            Console.WriteLine("8. Save & Return");
            Console.WriteLine("0. Cancel");
            Console.WriteLine();
            Console.Write("Select an option to change: ");

            var input = Console.ReadLine()?.Trim();
            switch (input)
            {
                case "1": SelectProgram(settings);     break;
                case "2": SelectTemperature(settings); break;
                case "3": SelectSpinSpeed(settings);   break;
                case "4": SelectWaterLevel(settings);  break;
                case "5": settings.PreWash    = !settings.PreWash;    break;
                case "6": settings.ExtraRinse = !settings.ExtraRinse; break;
                case "7": settings.QuickMode  = !settings.QuickMode;  break;
                case "8": return settings;
                case "0": return null;
            }
        }
    }

    private static void SelectProgram(WashSettings settings)
    {
        Console.WriteLine("\n--- SELECT PROGRAM ---");
        var programs = ProgramRegistry.All.ToList();
        for (int i = 0; i < programs.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {programs[i].Name}");
        }
        Console.Write("Choice: ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= programs.Count)
        {
            settings.Program = programs[idx - 1];
        }
    }

    private static void SelectTemperature(WashSettings settings)
    {
        Console.WriteLine("\n--- SELECT TEMPERATURE ---");
        var options = Enum.GetValues<Temperature>();
        for (int i = 0; i < options.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {options[i]}");
        }
        Console.Write("Choice: ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= options.Length)
        {
            settings.Temperature = options[idx - 1];
        }
    }

    private static void SelectSpinSpeed(WashSettings settings)
    {
        Console.WriteLine("\n--- SELECT SPIN SPEED ---");
        var speeds = Enum.GetValues<SpinSpeed>();
        for (int i = 0; i < speeds.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {(int)speeds[i]} RPM");
        }
        Console.Write("Choice: ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= speeds.Length)
        {
            settings.SpinSpeed = speeds[idx - 1];
        }
    }

    private static void SelectWaterLevel(WashSettings settings)
    {
        Console.WriteLine("\n--- SELECT WATER LEVEL ---");
        var options = Enum.GetValues<WaterLevel>();
        for (int i = 0; i < options.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {options[i]}");
        }
        Console.Write("Choice: ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= options.Length)
        {
            settings.WaterLevel = options[idx - 1];
        }
    }
}
