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

    /// <summary>
    /// Initializes a new instance of the <see cref="WashingMachineController"/> class.
    /// </summary>
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

    /// <inheritdoc/>
    public async Task StartAsync()
    {
        _logger.Log("App", "Starting application");

        // Subscribe to machine events
        _machineService.ProgressChanged  += OnProgressChanged;
        _machineService.CycleCompleted   += OnCycleCompleted;
        _machineService.CycleCancelled   += OnCycleCancelled;

        // Restore previous state from file (file I/O — async is correct here)
        WashingMachineModel? saved = await _machineStateRepository.LoadAsync();
        if (saved != null)
        {
            _machineService.RestoreState(saved);
            Console.WriteLine("Previous machine state restored.");

            if (saved.State == MachineState.Running)
            {
                _machineService.ResumeSavedCycle(); // fires in background, no await needed
                Console.WriteLine("Resuming interrupted washing cycle in background...");
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        // Main application loop
        while (_isRunning)
        {
            MenuOption option = _dashboardView.Show(_machineService.Machine);
            await HandleMenuOptionAsync(option);
        }

        _logger.Log("App", "Application exiting");
    }

    private async Task HandleMenuOptionAsync(MenuOption option)
    {
        switch (option)
        {
            case MenuOption.StartWash:
                StartWash();
                break;

            case MenuOption.ConfigureSettings:
                ConfigureMachine();
                break;

            case MenuOption.AddClothes:
                AddClothes();
                break;

            case MenuOption.RemoveClothes:
                RemoveClothes();
                break;

            case MenuOption.ManageFavourites:
                await ManageFavouritesAsync();
                break;

            case MenuOption.ViewHistory:
                await ViewHistoryAsync();
                break;

            case MenuOption.ViewProgress:
                ShowProgress();
                break;

            case MenuOption.PauseCycle:
                HandlePause();
                break;

            case MenuOption.Exit:
                await ExitAsync();
                break;

            default:
                _dashboardView.DisplayError("Invalid choice. Please try again.");
                break;
        }
    }

    // ── Menu handlers ────────────────────────────────────────────────────────

    private void StartWash()
    {
        try
        {
            _machineService.StartCycle(); // no await — runs in background
            _logger.Log("Controller", "Cycle started");
            _dashboardView.DisplayMessage("Washing cycle started! Use option 7 to check progress.");
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
            WashSettings settings = _configurationView.GetSettings();
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

    private async Task ViewHistoryAsync()
    {
        _logger.Log("Controller", "Viewing history");
        List<WashHistory> history = await _historyService.GetAllAsync();
        _historyView.DisplayHistory(history);
    }

    private async Task ManageFavouritesAsync()
    {
        FavouriteMenuOption option = _favouriteView.ShowMenu();
        switch (option)
        {
            case FavouriteMenuOption.Apply:
                await ApplyFavouriteAsync();
                break;

            case FavouriteMenuOption.SaveCurrent:
                await SaveFavouriteAsync();
                break;

            case FavouriteMenuOption.Delete:
                await DeleteFavouriteAsync();
                break;

            default:
                break; // Back
        }
    }

    private async Task ApplyFavouriteAsync()
    {
        List<Favourite> favourites = await _favouriteService.GetAllAsync();
        if (favourites.Count == 0)
        {
            _dashboardView.DisplayError("No favourites saved yet.");
            return;
        }

        Guid id = _favouriteView.SelectFavourite(favourites);
        Favourite? fav = await _favouriteService.GetByIdAsync(id);
        if (fav != null)
        {
            _machineService.ApplyFavourite(fav);
            _logger.Log("Controller", $"Applied favourite '{fav.Name}'");
            _dashboardView.DisplayMessage($"Favourite '{fav.Name}' applied.");
        }
    }

    private async Task SaveFavouriteAsync()
    {
        Favourite fav = _favouriteView.CreateFavourite(_machineService.Machine.Settings);
        await _favouriteService.AddAsync(fav);
        _logger.Log("Controller", $"Saved favourite '{fav.Name}'");
        _dashboardView.DisplaySuccess(CommonMessages.FavouriteSaved);
    }

    private async Task DeleteFavouriteAsync()
    {
        List<Favourite> favourites = await _favouriteService.GetAllAsync();
        if (favourites.Count == 0)
        {
            _dashboardView.DisplayError("No favourites saved yet.");
            return;
        }

        Guid id = _favouriteView.SelectFavourite(favourites);
        await _favouriteService.DeleteAsync(id);
        _logger.Log("Controller", "Deleted a favourite");
        _dashboardView.DisplaySuccess(CommonMessages.FavouriteDeleted);
    }

    private void ShowProgress()
    {
        WashCycle? cycle = _machineService.Machine.CurrentCycle;
        if (cycle == null)
        {
            _dashboardView.DisplayMessage("No active washing cycle.");
            return;
        }

        _washingView.DisplayProgress(new WashProgressEventArgs
        {
            Stage              = cycle.Stage,
            ProgressPercentage = cycle.ProgressPercentage,
            RemainingSeconds   = cycle.RemainingSeconds,
        });

        Console.WriteLine("Press Enter to go back...");
        Console.ReadLine();
    }

    private void HandlePause()
    {
        try
        {
            _machineService.PauseCycle();
            _logger.Log("Controller", "Cycle paused, showing pause menu");

            PauseMenuOption option = _pauseView.Show();
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
                    // User hit back — resume automatically
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

    private async Task ExitAsync()
    {
        _logger.Log("Controller", "Saving machine state before exit");
        await _machineStateRepository.SaveAsync(_machineService.Machine);
        _isRunning = false;
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    /// <summary>
    /// Called every second while washing. Just stores progress — doesn't touch Console.
    /// User sees it on demand via option 7.
    /// </summary>
    private void OnProgressChanged(object? sender, WashProgressEventArgs e)
    {
        // Progress is already stored in Machine.CurrentCycle — nothing to do here.
        // We deliberately do NOT clear/redraw the console here because that would
        // interrupt whatever the user is currently doing on the main menu.
        _logger.Log("Progress", $"Stage={e.Stage}, {e.ProgressPercentage:F1}%, {e.RemainingSeconds}s left");
    }

    private void OnCycleCompleted(object? sender, EventArgs e)
    {
        _logger.Log("Cycle", "Completed event received");
        // Write directly to console (won't interrupt a ReadLine prompt awkwardly,
        // it just appears above the next menu render)
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[WashMate] Washing cycle completed!");
        Console.ResetColor();
    }

    private void OnCycleCancelled(object? sender, EventArgs e)
    {
        _logger.Log("Cycle", "Cancelled event received");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[WashMate] Washing cycle was cancelled.");
        Console.ResetColor();
    }
}