using System;
using System.Collections.Generic;
using System.Linq;
using WashingMachine.Models.Entities;
using WashingMachine.Enums;

namespace WashingMachine.Views.Screens;

public static class HistoryView
{
    public enum HistoryFilter { All, Recent, ByProgram, Completed, Cancelled }
    public enum HistorySort   { DateDesc, DateAsc, DurationDesc, DurationAsc }

    public record HistoryRequest(HistoryFilter Filter, HistorySort Sort, string? ProgramName);

    public static HistoryRequest? ShowMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("              WASH HISTORY              ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("1. All Records");
        Console.WriteLine("2. Recent (last 10)");
        Console.WriteLine("3. Completed Only");
        Console.WriteLine("4. Cancelled Only");
        Console.WriteLine("5. Filter by Program");
        Console.WriteLine("6. Sort Options");
        Console.WriteLine("0. Back");
        Console.WriteLine();
        Console.Write("Choice: ");

        var input = Console.ReadLine()?.Trim();
        return input switch
        {
            "1" => new HistoryRequest(HistoryFilter.All,       HistorySort.DateDesc, null),
            "2" => new HistoryRequest(HistoryFilter.Recent,    HistorySort.DateDesc, null),
            "3" => new HistoryRequest(HistoryFilter.Completed, HistorySort.DateDesc, null),
            "4" => new HistoryRequest(HistoryFilter.Cancelled, HistorySort.DateDesc, null),
            "5" => new HistoryRequest(HistoryFilter.ByProgram, HistorySort.DateDesc, PromptProgram()),
            "6" => new HistoryRequest(HistoryFilter.All,       PromptSort(), null),
            _   => null
        };
    }

    public static void ShowTable(IEnumerable<WashHistory> records)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("              WASH HISTORY              ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();

        var list = records.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine("No records found.");
        }
        else
        {
            Console.WriteLine($"{"Date",-11} {"Program",-13} {"Temp",-6} {"Status"}");
            Console.WriteLine(new string('-', 50));
            foreach (var h in list)
            {
                var dateStr    = h.StartTime.ToString("dd MMM");
                var programStr = h.ProgramName.Length > 12 ? h.ProgramName[..12] : h.ProgramName;
                var statusStr  = h.Status == CycleStatus.Completed ? "Completed" : "Cancelled";
                Console.WriteLine($"{dateStr,-11} {programStr,-13} {h.Temperature,-6} {statusStr}");
            }
        }
        
        Console.WriteLine();
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static string? PromptProgram()
    {
        Console.WriteLine("\n--- SELECT PROGRAM ---");
        var programs = WashingMachine.Models.Common.ProgramRegistry.All.ToList();
        for (int i = 0; i < programs.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {programs[i].Name}");
        }
        Console.Write("Choice: ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= programs.Count)
        {
            return programs[idx - 1].Name;
        }
        return null;
    }

    private static HistorySort PromptSort()
    {
        Console.WriteLine("\n--- SORT OPTIONS ---");
        Console.WriteLine("1. Date (Newest first)");
        Console.WriteLine("2. Date (Oldest first)");
        Console.WriteLine("3. Duration (Longest first)");
        Console.WriteLine("4. Duration (Shortest first)");
        Console.Write("Choice: ");
        
        return Console.ReadLine()?.Trim() switch
        {
            "2" => HistorySort.DateAsc,
            "3" => HistorySort.DurationDesc,
            "4" => HistorySort.DurationAsc,
            _ => HistorySort.DateDesc
        };
    }
}
