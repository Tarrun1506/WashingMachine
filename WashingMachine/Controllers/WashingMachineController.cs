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

    private readonly IWashingMachineService  _machineService;
    private readonly IFavouriteService       _favouriteService;
    private readonly IWashHistoryService     _historyService;
    private readonly IMachineStateRepository _machineStateRepository;
    private readonly Logger                  _logger;

    private bool _isRunning = true;
    private bool _changesSaved = false;
    private bool _showCancellationScreen = false;
    private bool _dashboardRefreshEnabled = false;
    private bool _cycleJustCompleted = false;
    private readonly object _saveLock = new();
    private CancellationTokenSource? _dashboardRefreshCts;

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

        _machineService.ProgressChanged += OnProgressChanged;
        _machineService.CycleCompleted  += OnCycleCompleted;
        _machineService.CycleCancelled  += OnCycleCancelled;

        _historyService.Initialize();
        _favouriteService.Initialize();
        WashingMachineModel? saved = _machineStateRepository.Load();
        if (saved != null)
        {
            _machineService.RestoreState(saved);

            // Only show restoration screen if there's a running cycle to restore
            if (saved.State == MachineState.Running)
            {
                ShowCycleRestorationScreen(saved);
            }
        }

        // Start dashboard refresh loop
        StartDashboardRefresh();

        // Initialize dashboard renderer
        _dashboardView.EnsureRendererInitialized(_machineService.Machine);

        while (_isRunning)
        {
            // Check for special screens to show
            if (_showCancellationScreen)
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

        // Stop dashboard refresh loop
        StopDashboardRefresh();

        SaveChanges();
        _logger.Log("App", "Application exiting");
    }

    private void HandleMenuOption(MenuOption option)
    {
        switch (option)
        {
            case MenuOption.StartWash:
                StartWash();
                break;
            case MenuOption.ConfigureSettings: ConfigureMachine(); break;
            case MenuOption.AddClothes:
                if (_machineService.Machine.State == MachineState.Running)
                {
                    _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ErrorMessages.CannotAddWhileRunning);
                }
                else
                {
                    AddClothes();
                }
                break;
            case MenuOption.RemoveClothes:
                if (_machineService.Machine.State == MachineState.Running)
                {
                    _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ErrorMessages.CannotRemoveWhileRunning);
                }
                else
                {
                    RemoveClothes();
                }
                break;
            case MenuOption.ManageFavourites: ManageFavourites(); break;
            case MenuOption.ViewHistory: ViewHistory(); break;
            case MenuOption.PauseCycle: HandlePause(); break;
            case MenuOption.Exit:
                _isRunning = false;
                break;
            default:
                _dashboardView.DisplayError(ErrorMessages.InvalidMenuChoice);
                break;
        }
    }

    private void ShowAutoRefreshDashboard()
    {
        // This is now handled by the continuous dashboard refresh loop
        // Just show the dashboard and wait for user input
        _dashboardView.DisplayOnly(_machineService.Machine);
        Console.ReadKey(true);
    }

    private void StartDashboardRefresh()
    {
        _dashboardRefreshEnabled = true;
        _dashboardRefreshCts = new CancellationTokenSource();

        Task.Run(() =>
        {
            while (_dashboardRefreshEnabled && !_dashboardRefreshCts.Token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(1000); // Refresh every 1 second

                    if (_dashboardRefreshCts.Token.IsCancellationRequested)
                        break;

                    // Update dashboard if machine is running, paused, or just completed
                    if (_machineService.Machine.State == MachineState.Running ||
                        _machineService.Machine.State == MachineState.Paused ||
                        _cycleJustCompleted)
                    {
                        // Update dashboard without clearing content
                        _dashboardView.UpdateDashboard(_machineService.Machine);
                        
                        // Reset the flag after one update
                        if (_cycleJustCompleted)
                        {
                            _cycleJustCompleted = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("DashboardRefresh", ex.Message);
                    // Don't break on error, continue trying
                }
            }
        });
    }

    private void StopDashboardRefresh()
    {
        _dashboardRefreshEnabled = false;
        _dashboardRefreshCts?.Cancel();
        _dashboardRefreshCts?.Dispose();
    }

    private void StartWash()
    {
        try
        {
            _machineService.StartCycle();
            _logger.Log("Controller", "Cycle started");
            _dashboardView.DisplayMessageWithRedraw(_machineService.Machine, "Washing cycle started! Progress will be shown on dashboard.");
        }
        catch (Exception ex)
        {
            _logger.LogError("StartWash", ex.Message);
            _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ex.Message);
        }
    }

    private void ConfigureMachine()
    {
        try
        {
            var renderer = _dashboardView.GetRenderer();
            var configView = new ConfigurationView(_machineService.Machine.Settings, renderer);
            WashSettings settings = configView.GetSettings();
            _machineService.ApplySettings(settings);
            _dashboardView.DisplayMessageWithRedraw(_machineService.Machine, "Settings applied.");
        }
        catch (Exception ex)
        {
            _logger.LogError("ConfigureMachine", ex.Message);
            _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ex.Message);
        }
    }

    private int RemainingCapacity => Configurables.MaximumCapacity - _machineService.Machine.ClothesCount;

    private void AddClothes()
    {
        try
        {
            if (RemainingCapacity <= 0)
            {
                _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ErrorMessages.MachineFull);
                return;
            }

            int count = _dashboardView.ReadClothesCount(1, RemainingCapacity);
            _machineService.AddClothes(count);
            _dashboardView.DisplayMessageWithRedraw(_machineService.Machine, $"Added {count} clothes. Total: {_machineService.Machine.ClothesCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError("AddClothes", ex.Message);
            _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ex.Message);
        }
    }

    private void AddClothesWithoutRedraw()
    {
        try
        {
            if (RemainingCapacity <= 0)
            {
                ConsoleInput.PrintError(ErrorMessages.MachineFull);
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                return;
            }

            int count = _dashboardView.ReadClothesCount(1, RemainingCapacity);
            _machineService.AddClothes(count);
            Console.WriteLine($"Added {count} clothes. Total: {_machineService.Machine.ClothesCount}");
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            _logger.LogError("AddClothes", ex.Message);
            ConsoleInput.PrintError($"Error: {ex.Message}");
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }

    private void RemoveClothes()
    {
        try
        {
            int loaded = _machineService.Machine.ClothesCount;
            if (loaded == 0)
            {
                _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ErrorMessages.NoClothesToRemove);
                return;
            }

            int count = _dashboardView.ReadClothesCount(1, loaded);
            _machineService.RemoveClothes(count);
            _dashboardView.DisplayMessageWithRedraw(_machineService.Machine, $"Removed. Remaining: {_machineService.Machine.ClothesCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError("RemoveClothes", ex.Message);
            _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ex.Message);
        }
    }

    private void RemoveClothesWithoutRedraw()
    {
        try
        {
            int loaded = _machineService.Machine.ClothesCount;
            if (loaded == 0)
            {
                ConsoleInput.PrintError(ErrorMessages.NoClothesToRemove);
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                return;
            }

            int count = _dashboardView.ReadClothesCount(1, loaded);
            _machineService.RemoveClothes(count);
            Console.WriteLine($"Removed. Remaining: {_machineService.Machine.ClothesCount}");
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            _logger.LogError("RemoveClothes", ex.Message);
            ConsoleInput.PrintError($"Error: {ex.Message}");
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }

    private void ViewHistory()
    {
        _logger.Log("Controller", "Viewing history");
        List<WashHistory> history = _historyService.GetAll();
        var renderer = _dashboardView.GetRenderer();
        var historyView = new HistoryView(renderer);
        historyView.DisplayHistory(history);
        // Force full redraw after viewing history
        _dashboardView.ForceFullRedraw(_machineService.Machine);
    }

    private void ManageFavourites()
    {
        var renderer = _dashboardView.GetRenderer();
        var favouriteView = new FavouriteView(renderer);
        FavouriteMenuOption option = favouriteView.ShowMenu();
        switch (option)
        {
            case FavouriteMenuOption.Apply: ApplyFavourite(renderer); break;
            case FavouriteMenuOption.SaveCurrent: SaveFavourite(renderer); break;
            case FavouriteMenuOption.Delete: DeleteFavourite(renderer); break;
            default: break;
        }
    }

    private void ApplyFavourite(DashboardRenderer? renderer)
    {
        try
        {
            List<Favourite> favourites = _favouriteService.GetAll();
            if (favourites.Count == 0)
            {
                _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ErrorMessages.NoFavouritesSaved);
                return;
            }

            var favouriteView = new FavouriteView(renderer);
            Guid id = favouriteView.SelectFavourite(favourites);
            Favourite? fav = _favouriteService.GetById(id);
            if (fav == null)
            {
                _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ErrorMessages.FavouriteNotFound);
                return;
            }

            _machineService.ApplyFavourite(fav);
            _logger.Log("Controller", $"Applied favourite '{fav.Name}'");
            _dashboardView.DisplayMessageWithRedraw(_machineService.Machine, $"Favourite '{fav.Name}' applied.");
        }
        catch (Exception ex)
        {
            _logger.LogError("ApplyFavourite", ex.Message);
            _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ex.Message);
        }
    }

    private void SaveFavourite(DashboardRenderer? renderer)
    {
        try
        {
            var favouriteView = new FavouriteView(renderer);
            Favourite fav = favouriteView.CreateFavourite(_machineService.Machine.Settings);
            _favouriteService.Add(fav);
            _logger.Log("Controller", $"Saved favourite '{fav.Name}'");
            _dashboardView.DisplaySuccessWithRedraw(_machineService.Machine, CommonMessages.FavouriteSaved);
        }
        catch (Exception ex)
        {
            _logger.LogError("SaveFavourite", ex.Message);
            _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ex.Message);
        }
    }

    private void DeleteFavourite(DashboardRenderer? renderer)
    {
        try
        {
            List<Favourite> favourites = _favouriteService.GetAll();
            if (favourites.Count == 0)
            {
                _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ErrorMessages.NoFavouritesSaved);
                return;
            }

            var favouriteView = new FavouriteView(renderer);
            Guid id = favouriteView.SelectFavourite(favourites);
            _favouriteService.Delete(id);
            _logger.Log("Controller", "Deleted a favourite");
            _dashboardView.DisplaySuccessWithRedraw(_machineService.Machine, CommonMessages.FavouriteDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError("DeleteFavourite", ex.Message);
            _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ex.Message);
        }
    }

    private void HandlePause()
    {
        try
        {
            _machineService.PauseCycle();
            _logger.Log("Controller", "Cycle paused, showing pause menu");

            var renderer = _dashboardView.GetRenderer();
            var pauseView = new PauseView(renderer);

            // Loop to keep showing pause menu until user resumes or cancels
            bool stayInPauseMenu = true;
            while (stayInPauseMenu)
            {
                PauseMenuOption option = pauseView.Show(_machineService.Machine);
                switch (option)
                {
                    case PauseMenuOption.Resume:
                        _machineService.ResumeCycle();
                        _dashboardView.DisplayMessageWithRedraw(_machineService.Machine, "Cycle resumed.");
                        stayInPauseMenu = false;
                        break;

                    case PauseMenuOption.Cancel:
                        _machineService.CancelCycle();
                        _dashboardView.DisplayMessageWithRedraw(_machineService.Machine, "Cycle cancelled.");
                        stayInPauseMenu = false;
                        break;

                    case PauseMenuOption.AddClothes:
                        AddClothesWithoutRedraw();
                        // Stay in pause menu
                        break;

                    case PauseMenuOption.RemoveClothes:
                        RemoveClothesWithoutRedraw();
                        // Stay in pause menu
                        break;

                    default:
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("HandlePause", ex.Message);
            _dashboardView.DisplayErrorWithRedraw(_machineService.Machine, ex.Message);
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
            Console.WriteLine("\n[WashMate] All data saved successfully.");
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

    private void OnCycleCompleted(object? sender, EventArgs e)
    {
        _logger.Log("Cycle", "Completed event received");
        _cycleJustCompleted = true;
        // Force dashboard update to show completion state with menu
        _dashboardView.ShowMenuWithDashboard(_machineService.Machine);
    }

    private void ShowCycleRestorationScreen(WashingMachineModel saved)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("+============================================================+");
        Console.WriteLine("|           PREVIOUS CYCLE DETECTED                           |");
        Console.WriteLine("+============================================================+");
        Console.ResetColor();
        
        Console.WriteLine($"\nA previous washing cycle was interrupted:");
        Console.WriteLine($"  Program: {saved.Settings.ProgramName}");
        Console.WriteLine($"  Stage: {saved.CurrentCycle?.Stage}");
        Console.WriteLine($"  Progress: {saved.CurrentCycle?.ProgressPercentage:F1}%");
        Console.WriteLine($"  Time Remaining: {saved.CurrentCycle?.RemainingSeconds} seconds");
        Console.WriteLine($"  Clothes: {saved.ClothesCount}");
        
        Console.WriteLine("\nWould you like to continue this cycle?");
        Console.WriteLine("1. Yes - Continue the cycle");
        Console.WriteLine("2. No - Reset and start fresh");

        int choice = ConsoleInput.ReadIntInRange("\nEnter your choice (1 or 2): ", 1, 2, ErrorMessages.InvalidRestorationChoice);

        if (choice == 1)
        {
            _machineService.ResumeSavedCycle();
            Console.WriteLine("\nResuming interrupted washing cycle in background...");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        else
        {
            _machineService.Machine.State = MachineState.Idle;
            _machineService.Machine.IsDoorLocked = false;
            _machineService.Machine.CurrentCycle = null;
            _machineService.Machine.ClothesCount = 0;
            Console.WriteLine("\nMachine state has been reset.");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
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

    private void ShowCancellationScreen()
    {
        _logger.Log("Cycle", "Showing cancellation screen");

        // Reset clothes count immediately on cancellation
        _machineService.Machine.ClothesCount = 0;
        _logger.Log("Cancel", "Clothes count reset to 0");

        // Clear content area and show cancellation message
        _dashboardView.ClearContentArea();
        _dashboardView.MoveToContentArea();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("+============================================================+");
        Console.WriteLine("|                   ✗ WASH CYCLE CANCELLED                  |");
        Console.WriteLine("+============================================================+");
        Console.WriteLine("\nMachine has been reset to default state.");
        Console.WriteLine("Press Enter to continue...");
        Console.ResetColor();
        Console.ReadLine();

        // Force full redraw after cancellation
        _dashboardView.ForceFullRedraw(_machineService.Machine);
    }
}
