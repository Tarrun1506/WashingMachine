using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Helpers;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides configuration operations.
/// </summary>
public class ConfigurationView
{
    private WashSettings _currentSettings = new();
    public ConfigurationView(WashSettings? currentSettings = null)
    {
        if (currentSettings != null)
        {
            _currentSettings = currentSettings;
        }
    }

    public WashSettings GetSettings()
    {
        WashSettings settings = new();

        Console.WriteLine();
        Console.WriteLine("--- Configure Washing Machine ---");
        Console.WriteLine();
        Console.WriteLine($"Available Programs: {string.Join(", ", SettingsValidator.ValidPrograms)}");
        Console.WriteLine("Press Enter to keep the current value.");
        Console.WriteLine();

        settings.ProgramName = ReadProgramName();
        settings.Temperature = ReadTemperature();
        settings.WaterLevel = ReadWaterLevel();
        settings.SpinSpeed = ReadSpinSpeed();
        settings.IsPreWashEnabled = ReadYesNo($"Pre-Wash? (y/n) [{(_currentSettings.IsPreWashEnabled ? "y" : "n")}]: ", _currentSettings.IsPreWashEnabled);
        settings.IsExtraRinseEnabled = ReadYesNo($"Extra Rinse? (y/n) [{(_currentSettings.IsExtraRinseEnabled ? "y" : "n")}]: ", _currentSettings.IsExtraRinseEnabled);
        settings.IsQuickWashEnabled = ReadYesNo($"Quick Wash? (y/n) [{(_currentSettings.IsQuickWashEnabled ? "y" : "n")}]: ", _currentSettings.IsQuickWashEnabled);

        return settings;
    }

    private string ReadProgramName()
    {
        Console.WriteLine("Available Programs:");
        for (int i = 0; i < SettingsValidator.ValidPrograms.Length; i++)
        {
            string currentMarker = SettingsValidator.ValidPrograms[i] == _currentSettings.ProgramName ? " [Current]" : "";
            Console.WriteLine($"  {i + 1}. {SettingsValidator.ValidPrograms[i]}{currentMarker}");
        }
        Console.WriteLine();

        int? choice = ConsoleInput.ReadOptionalIntInRange($"Program Selection [{GetCurrentProgramIndex()}]: ", 1, SettingsValidator.ValidPrograms.Length, ErrorMessages.InvalidProgram);
        return choice.HasValue ? SettingsValidator.ValidPrograms[choice.Value - 1] : _currentSettings.ProgramName;
    }

    private int GetCurrentProgramIndex()
    {
        for (int i = 0; i < SettingsValidator.ValidPrograms.Length; i++)
        {
            if (SettingsValidator.ValidPrograms[i] == _currentSettings.ProgramName)
                return i + 1;
        }
        return 1;
    }

    private Temperature ReadTemperature()
    {
        Console.WriteLine($"Temperature  : 0=Cold  1=Warm  2=Hot [{_currentSettings.Temperature}]");
        int? choice = ConsoleInput.ReadOptionalIntInRange("Choice : ", 0, 2, ErrorMessages.InvalidTemperature);
        return choice.HasValue ? (Temperature)choice.Value : _currentSettings.Temperature;
    }

    private WaterLevel ReadWaterLevel()
    {
        Console.WriteLine($"Water Level  : 0=Low  1=Medium  2=High [{_currentSettings.WaterLevel}]");
        int? choice = ConsoleInput.ReadOptionalIntInRange("Choice : ", 0, 2, ErrorMessages.InvalidWaterLevel);
        return choice.HasValue ? (WaterLevel)choice.Value : _currentSettings.WaterLevel;
    }

    private SpinSpeed ReadSpinSpeed()
    {
        Console.WriteLine($"Spin Speed   : 1=400rpm  2=800rpm  3=1000rpm  4=1200rpm  5=1400rpm [{_currentSettings.SpinSpeed}]");
        while (true)
        {
            Console.Write("Choice : ");
            string input = ConsoleInput.ReadTrimmed();
            if (string.IsNullOrEmpty(input))
                return _currentSettings.SpinSpeed;

            if (SettingsValidator.TryParseSpinChoice(input, out SpinSpeed spinSpeed))
                return spinSpeed;

            ConsoleInput.PrintError(ErrorMessages.InvalidSpinSpeed);
        }
    }

    private static bool ReadYesNo(string prompt, bool current)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = ConsoleInput.ReadTrimmed();
            if (string.IsNullOrEmpty(input))
                return current;

            if (SettingsValidator.TryParseYesNo(input, out bool value))
                return value;

            ConsoleInput.PrintError(ErrorMessages.InvalidYesNo);
        }
    }
}
