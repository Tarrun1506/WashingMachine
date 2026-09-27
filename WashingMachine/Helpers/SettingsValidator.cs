using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Helpers;

/// <summary>
/// Shared validation rules for wash settings and related input.
/// </summary>
public static class SettingsValidator
{
    public const int MaxFavouriteNameLength = 30;

    public static readonly string[] ValidPrograms =
    {
        "Cotton", "Quick Wash", "Synthetic", "Wool", "Heavy Wash"
    };

    public static bool ValidateProgramName(string programName, out string normalizedName)
    {
        foreach (string program in ValidPrograms)
        {
            if (string.Equals(program, programName?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                normalizedName = program;
                return true;
            }
        }

        normalizedName = "Cotton";
        return false;
    }

    public static bool TryParseProgramChoice(string input, out string programName)
    {
        programName = "Cotton";
        if (!int.TryParse(input, out int choice) || choice < 1 || choice > ValidPrograms.Length)
            return false;

        programName = ValidPrograms[choice - 1];
        return true;
    }

    public static bool TryParseSpinChoice(string input, out SpinSpeed spinSpeed)
    {
        spinSpeed = SpinSpeed.Rpm800;
        if (!int.TryParse(input, out int choice))
            return false;

        spinSpeed = choice switch
        {
            1 => SpinSpeed.Rpm400,
            2 => SpinSpeed.Rpm800,
            3 => SpinSpeed.Rpm1000,
            4 => SpinSpeed.Rpm1200,
            5 => SpinSpeed.Rpm1400,
            _ => SpinSpeed.Rpm800
        };

        return choice is >= 1 and <= 5;
    }

    public static bool TryParseYesNo(string input, out bool value)
    {
        value = false;
        string normalized = input.Trim().ToLowerInvariant();
        if (normalized is "y" or "yes")
        {
            value = true;
            return true;
        }

        if (normalized is "n" or "no")
        {
            value = false;
            return true;
        }

        return false;
    }

    public static bool TryValidateSettings(WashSettings? settings, out string error)
    {
        if (settings == null)
        {
            error = ErrorMessages.SettingsRequired;
            return false;
        }

        if (!ValidateProgramName(settings.ProgramName, out string normalized))
        {
            error = ErrorMessages.InvalidProgram;
            return false;
        }

        settings.ProgramName = normalized;

        if (!Enum.IsDefined(typeof(Temperature), settings.Temperature))
        {
            error = ErrorMessages.InvalidTemperature;
            return false;
        }

        if (!Enum.IsDefined(typeof(WaterLevel), settings.WaterLevel))
        {
            error = ErrorMessages.InvalidWaterLevel;
            return false;
        }

        if (!Enum.IsDefined(typeof(SpinSpeed), settings.SpinSpeed))
        {
            error = ErrorMessages.InvalidSpinSpeed;
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TryValidateFavouriteName(string? name, out string normalized, out string error)
    {
        normalized = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            error = ErrorMessages.FavouriteNameRequired;
            return false;
        }

        if (normalized.Length > MaxFavouriteNameLength)
        {
            error = ErrorMessages.FavouriteNameTooLong;
            return false;
        }

        error = string.Empty;
        return true;
    }
}
