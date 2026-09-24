using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides configuration operations.
/// </summary>
public class ConfigurationView
{
    public WashSettings GetSettings()
    {
        WashSettings settings = new();

        Console.Clear();
        Console.WriteLine("--- Configure Washing Machine ---");
        Console.WriteLine();
        Console.WriteLine("Available Programs: Cotton, Quick Wash, Synthetic, Wool, Heavy Wash");
        Console.Write("Program Name : ");
        settings.ProgramName = Console.ReadLine()?.Trim() ?? "Cotton";

        Console.WriteLine("Temperature  : 0=Cold  1=Warm  2=Hot");
        Console.Write("Choice : ");
        settings.Temperature = int.TryParse(Console.ReadLine(), out int temp) && Enum.IsDefined(typeof(Temperature), temp)
            ? (Temperature)temp
            : Temperature.Warm;

        Console.WriteLine("Water Level  : 0=Low  1=Medium  2=High");
        Console.Write("Choice : ");
        settings.WaterLevel = int.TryParse(Console.ReadLine(), out int wl) && Enum.IsDefined(typeof(WaterLevel), wl)
            ? (WaterLevel)wl
            : WaterLevel.Medium;

        Console.WriteLine("Spin Speed   : 1=400rpm  2=800rpm  3=1000rpm  4=1200rpm  5=1400rpm");
        Console.Write("Choice : ");
        settings.SpinSpeed = int.TryParse(Console.ReadLine(), out int spin)
            ? spin switch
            {
                1 => SpinSpeed.Rpm400,
                2 => SpinSpeed.Rpm800,
                3 => SpinSpeed.Rpm1000,
                4 => SpinSpeed.Rpm1200,
                5 => SpinSpeed.Rpm1400,
                _ => SpinSpeed.Rpm800,
            }
            : SpinSpeed.Rpm800;

        Console.Write("Pre-Wash? (y/n) : ");
        settings.IsPreWashEnabled = Console.ReadLine()?.Trim().ToLower() == "y";

        Console.Write("Extra Rinse? (y/n) : ");
        settings.IsExtraRinseEnabled = Console.ReadLine()?.Trim().ToLower() == "y";

        Console.Write("Quick Wash? (y/n) : ");
        settings.IsQuickWashEnabled = Console.ReadLine()?.Trim().ToLower() == "y";

        return settings;
    }
}