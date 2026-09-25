using WashingMachine.Enums;
using WashingMachine.Models;
using WashingMachine.Helpers;

namespace WashingMachine.Views;

/// <summary>
/// Provides configuration operations.
/// </summary>
public class ConfigurationView
{
    private WashSettings _currentSettings = new();

    public ConfigurationView(WashSettings? currentSettings = null, DashboardRenderer? renderer = null)
    {
        if (currentSettings != null)
        {
            _currentSettings = currentSettings;
        }
        // Renderer parameter ignored for simple implementation
    }

    public WashSettings GetSettings()
    {
        WashSettings settings = new();

        Console.Clear();
        Console.WriteLine("--- Configure Washing Machine ---");
        Console.WriteLine();
        Console.WriteLine("Available Programs: Cotton, Quick Wash, Synthetic, Wool, Heavy Wash");
        Console.WriteLine($"Program Name [{_currentSettings.ProgramName}]: ");
        string inputProg = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(inputProg)) inputProg = _currentSettings.ProgramName;

        if (SettingsValidator.ValidateProgramName(inputProg, out string validatedProg))
        {
            settings.ProgramName = validatedProg;
        }
        else
        {
            Console.WriteLine("Invalid program, defaulting to Cotton.");
            settings.ProgramName = "Cotton";
        }

        Console.WriteLine($"Temperature  : 0=Cold  1=Warm  2=Hot [{_currentSettings.Temperature}]");
        Console.Write("Choice : ");
        string inputTemp = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(inputTemp))
            settings.Temperature = _currentSettings.Temperature;
        else
            settings.Temperature = int.TryParse(inputTemp, out int temp) && Enum.IsDefined(typeof(Temperature), temp)
                ? (Temperature)temp
                : Temperature.Warm;


        Console.WriteLine($"Water Level  : 0=Low  1=Medium  2=High [{_currentSettings.WaterLevel}]");
        Console.Write("Choice : ");
        string inputWl = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(inputWl))
            settings.WaterLevel = _currentSettings.WaterLevel;
        else
            settings.WaterLevel = int.TryParse(inputWl, out int wl) && Enum.IsDefined(typeof(WaterLevel), wl)
                ? (WaterLevel)wl
                : WaterLevel.Medium;


        Console.WriteLine($"Spin Speed   : 1=400rpm  2=800rpm  3=1000rpm  4=1200rpm  5=1400rpm [{_currentSettings.SpinSpeed}]");
        Console.Write("Choice : ");
        string inputSpin = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(inputSpin))
            settings.SpinSpeed = _currentSettings.SpinSpeed;
        else
            settings.SpinSpeed = int.TryParse(inputSpin, out int spin)
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

        Console.Write($"Pre-Wash? (y/n) [{(_currentSettings.IsPreWashEnabled ? "y" : "n")}]: ");
        string inputPre = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
        settings.IsPreWashEnabled = string.IsNullOrEmpty(inputPre) ? _currentSettings.IsPreWashEnabled : (inputPre == "y");

        Console.Write($"Extra Rinse? (y/n) [{(_currentSettings.IsExtraRinseEnabled ? "y" : "n")}]: ");
        string inputExtra = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
        settings.IsExtraRinseEnabled = string.IsNullOrEmpty(inputExtra) ? _currentSettings.IsExtraRinseEnabled : (inputExtra == "y");

        Console.Write($"Quick Wash? (y/n) [{(_currentSettings.IsQuickWashEnabled ? "y" : "n")}]: ");
        string inputQuick = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
        settings.IsQuickWashEnabled = string.IsNullOrEmpty(inputQuick) ? _currentSettings.IsQuickWashEnabled : (inputQuick == "y");

        return settings;
    }
}