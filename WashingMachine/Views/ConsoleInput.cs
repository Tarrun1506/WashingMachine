using WashingMachine.Constants;

namespace WashingMachine.Views;

/// <summary>
/// Shared validated console input helpers for the view layer.
/// </summary>
public static class ConsoleInput
{
    public static string ReadTrimmed() => Console.ReadLine()?.Trim() ?? string.Empty;

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static int ReadIntInRange(string prompt, int min, int max, string errorMessage)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = ReadTrimmed();
            if (int.TryParse(input, out int value) && value >= min && value <= max)
                return value;

            PrintError(errorMessage);
        }
    }

    public static int? ReadOptionalIntInRange(string prompt, int min, int max, string errorMessage)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = ReadTrimmed();
            if (string.IsNullOrEmpty(input))
                return null;

            if (int.TryParse(input, out int value) && value >= min && value <= max)
                return value;

            PrintError(errorMessage);
        }
    }

    public static string ReadRequiredText(string prompt, Func<string, string?> validate)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = ReadTrimmed();
            string? error = validate(input);
            if (error == null)
                return input;

            PrintError(error);
        }
    }
}
