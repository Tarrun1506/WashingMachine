using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides history operations.
/// </summary>
public class HistoryView
{
    private readonly DashboardRenderer? _renderer;

    public HistoryView(DashboardRenderer? renderer = null)
    {
        _renderer = renderer;
    }

    public void DisplayHistory(List<WashHistory> history)
    {
        _renderer?.ClearContentArea();
        _renderer?.MoveToContentArea();

        Console.WriteLine();
        Console.WriteLine("--- Wash History ---");
        Console.WriteLine();
        if (history.Count == 0)
        {
            Console.WriteLine("No wash history yet.");
        }
        else
        {
            foreach (WashHistory item in history)
            {
                Console.WriteLine($"{item.ProgramName,-15}" + $"{item.Status,-15}" + $"{item.StartTime:g}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter To Continue...");
        Console.ReadLine();
    }
}