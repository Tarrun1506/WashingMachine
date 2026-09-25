using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides history operations.
/// </summary>
public class HistoryView
{
    public void DisplayHistory(List<WashHistory> history)
    {
        Console.Clear();
        Console.WriteLine("--- Wash History ---");
        Console.WriteLine();
        foreach (WashHistory item in history)
        {
            Console.WriteLine($"{item.ProgramName,-15}" + $"{item.Status,-15}" + $"{item.StartTime:g}");
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter To Continue...");
        Console.ReadLine();
    }
}