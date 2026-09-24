using System;
using System.IO;
using System.Threading.Tasks;
using WashingMachine.Controllers;
using WashingMachine.Repository.Implementations;
using WashingMachine.Services;
using WashingMachine.Storage;

namespace WashingMachine;

class Program
{
    static async Task Main()
    {
        Console.Title = "WashMate - Smart Washing Machine";
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WashingMachine", "Data");
        dataDirectory = Path.GetFullPath(dataDirectory);

        var storage = new JsonFileStorage(dataDirectory);
        var favouriteRepository = new FavouriteRepository(storage);
        var historyRepository   = new WashHistoryRepository(storage);

        var historyService    = new WashHistoryService(historyRepository);
        var machineService    = new WashingMachineService(historyService);
        var favouriteService  = new FavouriteService(favouriteRepository);

        var controller = new WashingMachineController(machineService, favouriteService, historyService, storage);

        ShowSplash();

        try
        {
            await controller.RunAsync();
        }
        catch (Exception ex)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unexpected Error: " + ex.Message);
            await LogUnhandledAsync(dataDirectory, ex);
        }
    }

    static void ShowSplash()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("        WASHMATE v1.0");
        Console.WriteLine("     Smart Washing Machine System");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Loading favourites and history...");
        Console.ResetColor();
        Console.WriteLine();
        System.Threading.Thread.Sleep(800);
    }

    static async Task LogUnhandledAsync(string dir, Exception ex)
    {
        try
        {
            var path = Path.Combine(dir, "application-log.txt");
            await File.AppendAllTextAsync(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UNHANDLED: {ex}{Environment.NewLine}");
        }
        catch { }
    }
}
