using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Events;
using WashingMachine.Helpers;
using WashingMachine.Models;
using WashingMachine.Repository;
using WashingMachine.Services;
using WashingMachine.Views;

namespace WashingMachine.Controllers;

/// <summary>
/// Controls the washing machine application flow.
/// </summary>
public class WashingMachineController : IWashingMachineController
{
    private readonly DashboardView       _dashboardView;
    private readonly ConfigurationView   _configurationView;
    private readonly FavouriteView       _favouriteView;
    private readonly HistoryView         _historyView;
    private readonly PauseView           _pauseView;
    private readonly WashingView         _washingView;

    private readonly IWashingMachineService  _machineService;
    private readonly IFavouriteService       _favouriteService;
    private readonly IWashHistoryService     _historyService;
    private readonly IMachineStateRepository _machineStateRepository;
    private readonly Logger                  _logger;

    private bool _isRunning = true;
    private bool _changesSaved = false;
    private bool _showUnloadScreen = false;
    private bool _showCompletionScreen = false;
    private bool _showCancellationScreen = false;
    private readonly object _saveLock = new();

    public WashingMachineController(
        IWashingMachineService  machineService,
        IFavouriteService       favouriteService,
        IWashHistoryService     historyService,
        IMachineStateRepository machineStateRepository,
        Logger                  logger)
    {
        _machineService         = machineService;
        _favouriteService       = favouriteService;
        _historyService         = historyService;
        _machineStateRepository = machineStateRepository;
        _logger                 = logger;

        _dashboardView     = new DashboardView();
        _configurationView = new ConfigurationView();
        _favouriteView     = new FavouriteView();
        _historyView       = new HistoryView();
        _pauseView         = new PauseView();
        _washingView       = new WashingView();
    }

    public void Start()
    {
        _logger.Log("App", "Starting application");

        AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveChanges();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            SaveChanges();
            eventArgs.Cancel = false;
            Environment.Exit(0);
        };

        _machineService.ProgressChanged     += OnProgressChanged;
        _machineService.CycleCompleted      += OnCycleCompleted;
        _machineService.CycleCancelled      += OnCycleCancelled;
        _machineService.ClothesUnloadRequired += OnClothesUnloadRequired;

        _historyService.Initialize();
        _favouriteService.Initialize();
        // Skip state restoration to avoid console handle issues
        /*
        WashingMachineModel? saved = _machineStateRepository.Load();
        if (saved != null)
        {
            _machineService.RestoreState(saved);
            Console.WriteLine("Previous machine state restored.");

            if (saved.State == MachineState.Running)
            {
                _machineService.ResumeSavedCycle();
                Console.WriteLine("Resuming interrupted washing cycle in background...");
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        */

        while (_isRunning)
        {
            // Check for special screens to show
            if (_showUnloadScreen)
            {
                ShowUnloadScreen();
                _showUnloadScreen = false;
            }
            else if (_showCompletionScreen)
            {
                ShowCompletionScreen();
                _showCompletionScreen = false;
            }
            else if (_showCancellationScreen)
            {
                ShowCancellationScreen();
                _showCancellationScreen = false;
            }
            else
            {
                MenuOption option = _dashboardView.Show(_machineService.Machine);
                HandleMenuOption(option);
            }
        }

        SaveChanges();
        _logger.Log("App", "Application exiting");
    }

    private void HandleMenuOption(MenuOption option)
    {
        switch (option)
        {
            case MenuOption.StartWash:
                StartWash();
                // Show auto-refresh dashboard after starting wash
                if (_machineService.Machine.State == MachineState.Running)
                {
                    ShowAutoRefreshDashboard();
                }
                break;
            case MenuOption.ConfigureSettings: ConfigureMachine(); break;
            case MenuOption.AddClothes: AddClothes(); break;
            case MenuOption.RemoveClothes: RemoveClothes(); break;
            case MenuOption.ManageFavourites: ManageFavourites(); break;
            case MenuOption.ViewHistory: ViewHistory(); break;
            case MenuOption.PauseCycle: HandlePause(); break;
            case MenuOption.Exit:
                _isRunning = false;
                break;
            default:
                _dashboardView.DisplayError("Invalid choice. Please try again.");
                break;
        }
    }

    private void ShowAutoRefreshDashboard()
    {
        // Manual refresh loop for the dashboard
        while (_isRunning)
        {
            if (_machineService.Machine.State == MachineState.Running ||
                _machineService.Machine.State == MachineState.Paused)
            {
                _dashboardView.DisplayOnly(_machineService.Machine);

                // Check for key press to return to menu
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true); // Clear the key
                    break;
                }
            }
            else
            {
                // Machine is idle, return to normal menu
                break;
            }

            Thread.Sleep(1000); // Refresh every 1 second
        }
    }

    private void StartWash()
    {
        try
        {
            _machineService.StartCycle();
            _logger.Log("Controller", "Cycle started");
            _dashboardView.DisplayMessage("Washing cycle started! Progress will be shown on dashboard.");
        }
        catch (Exception ex)
        {
            _logger.LogError("StartWash", ex.Message);
            _dashboardView.DisplayError(ex.Message);
        }
    }

    private void ConfigureMachine()
    {
        try
        {
            var configView = new ConfigurationView(_machineService.Machine.Settings);
            WashSettings settings = configView.GetSettings();
            _machineService.ApplySettings(settings);
            _dashboardView.DisplayMessage("Settings applied.");
        }
        catch (Exception ex)
        {
            _logger.LogError("ConfigureMachine", ex.Message);
            _dashboardView.DisplayError(ex.Message);
        }
    }

    private void AddClothes()
    {
        try
        {
            int count = _dashboardView.ReadClothesCount();
            _machineService.AddClothes(count);
            _dashboardView.DisplayMessage($"Added {count} clothes. Total: {_machineService.Machine.ClothesCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError("AddClothes", ex.Message);
            _dashboardView.DisplayError(ex.Message);
        }
    }

    private void RemoveClothes()
    {
        try
        {
            int count = _dashboardView.ReadClothesCount();
            _machineService.RemoveClothes(count);
            _dashboardView.DisplayMessage($"Removed. Remaining: {_machineService.Machine.ClothesCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError("RemoveClothes", ex.Message);
            _dashboardView.DisplayError(ex.Message);
        }
    }

    private void ViewHistory()
    {
        _logger.Log("Controller", "Viewing history");
        List<WashHistory> history = _historyService.GetAll();
        var historyView = new HistoryView();
        historyView.DisplayHistory(history);
    }

    private void ManageFavourites()
    {
        var favouriteView = new FavouriteView();
        FavouriteMenuOption option = favouriteView.ShowMenu();
        switch (option)
        {
            case FavouriteMenuOption.Apply: ApplyFavourite(); break;
            case FavouriteMenuOption.SaveCurrent: SaveFavourite(); break;
            case FavouriteMenuOption.Delete: DeleteFavourite(); break;
            default: break;
        }
    }

    private void ApplyFavourite()
    {
        List<Favourite> favourites = _favouriteService.GetAll();
        if (favourites.Count == 0)
        {
            _dashboardView.DisplayError("No favourites saved yet.");
            return;
        }

        var favouriteView = new FavouriteView();
        Guid id = favouriteView.SelectFavourite(favourites);
        Favourite? fav = _favouriteService.GetById(id);
        if (fav != null)
        {
            _machineService.ApplyFavourite(fav);
            _logger.Log("Controller", $"Applied favourite '{fav.Name}'");
            _dashboardView.DisplayMessage($"Favourite '{fav.Name}' applied.");
        }
    }

    private void SaveFavourite()
    {
        var favouriteView = new FavouriteView();
        Favourite fav = favouriteView.CreateFavourite(_machineService.Machine.Settings);
        _favouriteService.Add(fav);
        _logger.Log("Controller", $"Saved favourite '{fav.Name}'");
        _dashboardView.DisplaySuccess(CommonMessages.FavouriteSaved);
    }

    private void DeleteFavourite()
    {
        List<Favourite> favourites = _favouriteService.GetAll();
        if (favourites.Count == 0)
        {
            _dashboardView.DisplayError("No favourites saved yet.");
            return;
        }

        var favouriteView = new FavouriteView();
        Guid id = favouriteView.SelectFavourite(favourites);
        _favouriteService.Delete(id);
        _logger.Log("Controller", "Deleted a favourite");
        _dashboardView.DisplaySuccess(CommonMessages.FavouriteDeleted);
    }

    private void HandlePause()
    {
        try
        {
            _machineService.PauseCycle();
            _logger.Log("Controller", "Cycle paused, showing pause menu");

            var pauseView = new PauseView();
            PauseMenuOption option = pauseView.Show();
            switch (option)
            {
                case PauseMenuOption.Resume:
                    _machineService.ResumeCycle();
                    _dashboardView.DisplayMessage("Cycle resumed.");
                    break;

                case PauseMenuOption.Cancel:
                    _machineService.CancelCycle();
                    _dashboardView.DisplayMessage("Cycle cancelled.");
                    break;

                default:
                    _machineService.ResumeCycle();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("HandlePause", ex.Message);
            _dashboardView.DisplayError(ex.Message);
        }
    }

    private void SaveChanges()
    {
        lock (_saveLock)
        {
            if (_changesSaved) return;
            _changesSaved = true;
        }
        
        try
        {
            _logger.Log("Controller", "Saving all machine states and data before exit");
            _machineStateRepository.Save(_machineService.Machine);
            _historyService.SaveChanges();
            _favouriteService.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError("SaveChanges", ex.Message);
            Console.WriteLine($"\n[WashMate] Warning: Could not save all data. {ex.Message}");
        }
    }

    private void OnProgressChanged(object? sender, WashProgressEventArgs e)
    {
        _logger.Log("Progress", $"Stage={e.Stage}, {e.ProgressPercentage:F1}%, {e.RemainingSeconds}s left");
    }

    private void OnClothesUnloadRequired(object? sender, EventArgs e)
    {
        _logger.Log("Unload", "Clothes unload required");
        _showUnloadScreen = true;
    }

    private void OnCycleCompleted(object? sender, EventArgs e)
    {
        _logger.Log("Cycle", "Completed event received");
        // Note: ClothesUnloadRequired is called before this, so clothes are already reset
        _showCompletionScreen = true;
    }

    private string CenterText(string text, int width)
    {
        if (text.Length >= width) return text.Substring(0, width);
        int padding = (width - text.Length) / 2;
        return new string(' ', padding) + text + new string(' ', width - text.Length - padding);
    }

    private void OnCycleCancelled(object? sender, EventArgs e)
    {
        _logger.Log("Cycle", "Cancelled event received");
        _showCancellationScreen = true;
    }

    private void ShowUnloadScreen()
    {
        _logger.Log("Unload", "Showing unload screen");

        // Show unloading message
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║" + CenterText("WASH CYCLE COMPLETED - UNLOADING CLOTHES", 56) + "║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        // Show countdown for 3 seconds
        for (int i = 3; i > 0; i--)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nUnloading clothes... {i} second{(i > 1 ? "s" : "")} remaining");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        // Reset clothes count after unloading
        _machineService.Machine.ClothesCount = 0;
        _logger.Log("Unload", "Clothes count reset to 0");
    }

    private void ShowCompletionScreen()
    {
        _logger.Log("Cycle", "Showing completion screen");

        // Show final completion message
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║" + CenterText("✓ READY FOR NEXT CYCLE", 56) + "║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine("\nMachine has been reset. Add clothes and start a new cycle.");
        Console.WriteLine("Press Enter to continue...");
        Console.ResetColor();
        Console.ReadLine();
    }

    private void ShowCancellationScreen()
    {
        _logger.Log("Cycle", "Showing cancellation screen");

        // Reset clothes count immediately on cancellation
        _machineService.Machine.ClothesCount = 0;
        _logger.Log("Cancel", "Clothes count reset to 0");

        // Show cancellation message on dashboard
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║" + CenterText("✗ WASH CYCLE CANCELLED", 56) + "║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine("\nMachine has been reset to default state.");
        Console.WriteLine("Press Enter to continue...");
        Console.ResetColor();
        Console.ReadLine();
    }
}
