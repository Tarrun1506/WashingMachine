using WashingMachine.Models;

namespace WashingMachine.Helpers;

public static class SettingsValidator
{
    private static readonly string[] ValidPrograms = { "Cotton", "Quick Wash", "Synthetic", "Wool", "Heavy Wash" };

    public static bool ValidateProgramName(string programName, out string normalizedName)
    {
        foreach (var p in ValidPrograms)
        {
            if (string.Equals(p, programName, StringComparison.OrdinalIgnoreCase))
            {
                normalizedName = p;
                return true;
            }
        }
        normalizedName = "Cotton"; // Default fallback
        return false;
    }
}
