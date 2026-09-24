using System;
using WashingMachine.Models.Entities;
using WashingMachine.Enums;
using WashingMachine.Services;
using WashingMachine.Storage;
using System.Threading.Tasks;

namespace WashingMachine.Controllers;

public sealed class WashingMachineController
{
    private readonly IWashingMachineService _machineService;
    private readonly IFavouriteService      _favouriteService;
    private readonly IWashHistoryService    _historyService;
    private readonly IStorage               _storage;

    private bool _running = true;

    public WashingMachineController(
        IWashingMachineService machineService,
        IFavouriteService      favouriteService,
        IWashHistoryService    historyService,
        IStorage               storage)
    {
        _machineService   = machineService;
        _favouriteService = favouriteService;
        _historyService   = historyService;
        _storage          = storage;
    }

    public async Task RunAsync()
    {
        // Restore previous state if exists
        var savedState = await _storage.LoadMachineStateAsync();
        if (savedState != null)
        {
            _machineService.RestoreState(savedState);
            Console.WriteLine("Restored machine state from previous session.");
            await Task.Delay(1000);
        }

        while (_running)
        {
            var machine = _machineService.Machine;

            // Save state on every dashboard loop just in case
            await _storage.SaveMachineStateAsync(machine);

            if (machine.IsCycleActive || machine.IsCyclePaused)
            {
                await HandleWashingScreenAsync();
                continue;
            }

            if (machine.State is MachineState.Completed or MachineState.Cancelled)
            {
                Console.ForegroundColor = machine.State == MachineState.Completed ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(machine.State == MachineState.Completed
                    ? "\nWashing complete! Cycle recorded in history."
                    : "\nCycle cancelled. Recorded in history.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                // Acknowledged, so move to Idle
                _machineService.AcknowledgeCompletion(); 
                continue;
            }

            var input = Views.Screens.DashboardView.Show(machine);
            await HandleDashboardInputAsync(input);
        }

        // Save on exit
        await _storage.SaveMachineStateAsync(_machineService.Machine);
    }

    private async Task HandleDashboardInputAsync(string input)
    {
        switch (input)
        {
            case "1": HandleStartWashing();    break;
            case "2": HandleConfigure();       break;
            case "3": HandleAddClothes();      break;
            case "4": HandleRemoveClothes();   break;
            case "5": await HandleFavouritesAsync(); break;
            case "6": await HandleHistoryAsync();    break;
            case "7": await HandleWashingScreenAsync(); break;
            case "0": _running = false;        break;
            default:
                Console.WriteLine("Invalid option. Press Enter to try again.");
                Console.ReadLine();
                break;
        }
    }

    private void HandleStartWashing()
    {
        var result = _machineService.StartCycle();
        if (!result.Success)
        {
            ShowError(result.ErrorMessage!);
            return;
        }
        Console.WriteLine("Washing cycle started!");
        System.Threading.Thread.Sleep(1000);
    }

    private void HandleConfigure()
    {
        var newSettings = Views.Screens.ConfigurationView.Show(_machineService.Machine.Settings);
        if (newSettings == null) return; 

        var result = _machineService.ApplySettings(newSettings);
        if (!result.Success) ShowError(result.ErrorMessage!);
    }

    private void HandleAddClothes()
    {
        var (count, confirmed) = Views.Screens.AddClothesView.ShowAdd(_machineService.Machine);
        if (!confirmed) return;

        var result = _machineService.AddClothes(count);
        if (!result.Success) ShowError(result.ErrorMessage!);
    }

    private void HandleRemoveClothes()
    {
        var (count, confirmed) = Views.Screens.AddClothesView.ShowRemove(_machineService.Machine);
        if (!confirmed) return;

        var result = _machineService.RemoveClothes(count);
        if (!result.Success) ShowError(result.ErrorMessage!);
    }

    private async Task HandleWashingScreenAsync()
    {
        while (true)
        {
            await _storage.SaveMachineStateAsync(_machineService.Machine);
            var machine = _machineService.Machine;

            if (machine.State is MachineState.Idle or MachineState.Ready or
                MachineState.Completed or MachineState.Cancelled)
                return;

            if (machine.IsCyclePaused)
            {
                await HandlePauseScreenAsync();
                continue;
            }

            var input = Views.Screens.WashingView.Show(machine);

            switch (input.ToUpper())
            {
                case "1": 
                    var pRes = _machineService.PauseCycle();
                    if (!pRes.Success) ShowError(pRes.ErrorMessage!);
                    break;
                case "2": 
                    var aRes = _machineService.PauseForAddingClothes();
                    if (!aRes.Success) ShowError(aRes.ErrorMessage!);
                    break;
                case "3": 
                    _machineService.CancelCycle();
                    Console.WriteLine("Cycle cancelled.");
                    System.Threading.Thread.Sleep(1000);
                    return;
                case "0":
                    _running = false;
                    return;
                case "D": // done naturally
                    return; 
            }
        }
    }

    private async Task HandlePauseScreenAsync()
    {
        var input = Views.Screens.PauseView.Show(_machineService.Machine);

        switch (input.ToUpper())
        {
            case "1":
                var rRes = _machineService.ResumeCycle();
                if (!rRes.Success) ShowError(rRes.ErrorMessage!);
                break;
            case "2":
                if (_machineService.Machine.State == MachineState.Paused)
                {
                    _machineService.PauseForAddingClothes();
                }
                var (count, confirmed) = Views.Screens.AddClothesView.ShowAdd(_machineService.Machine);
                if (confirmed)
                {
                    var aRes = _machineService.AddClothes(count);
                    if (!aRes.Success) ShowError(aRes.ErrorMessage!);
                }
                break;
            case "3":
                _machineService.CancelCycle();
                return;
            case "0":
                _running = false;
                return;
        }
    }

    private async Task HandleFavouritesAsync()
    {
        while (true)
        {
            var favourites = await _favouriteService.GetAllAsync();
            var (action, selectedId) = Views.Screens.FavouritesView.Show(favourites);

            switch (action)
            {
                case Views.Screens.FavouritesView.FavouriteAction.UseFavourite:
                    var fav = await _favouriteService.GetByIdAsync(selectedId);
                    if (fav != null)
                    {
                        var result = _machineService.ApplyFavourite(fav);
                        if (!result.Success) ShowError(result.ErrorMessage!);
                    }
                    return;

                case Views.Screens.FavouritesView.FavouriteAction.AddFavourite:
                    var name = Views.Screens.FavouritesView.PromptFavouriteName();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var addResult = await _favouriteService.AddAsync(name, _machineService.Machine.Settings);
                        if (!addResult.Success) ShowError(addResult.ErrorMessage!);
                    }
                    break;

                case Views.Screens.FavouritesView.FavouriteAction.DeleteFavourite:
                    var delResult = await _favouriteService.DeleteAsync(selectedId);
                    if (!delResult.Success) ShowError(delResult.ErrorMessage!);
                    break;

                case Views.Screens.FavouritesView.FavouriteAction.Back:
                default:
                    return;
            }
        }
    }

    private async Task HandleHistoryAsync()
    {
        var request = Views.Screens.HistoryView.ShowMenu();
        if (request == null) return;

        var records = (request.Filter, request.Sort) switch
        {
            (Views.Screens.HistoryView.HistoryFilter.Recent,    _) => await _historyService.GetRecentAsync(10),
            (Views.Screens.HistoryView.HistoryFilter.Completed, _) => await _historyService.GetByStatusAsync(CycleStatus.Completed),
            (Views.Screens.HistoryView.HistoryFilter.Cancelled, _) => await _historyService.GetByStatusAsync(CycleStatus.Cancelled),
            (Views.Screens.HistoryView.HistoryFilter.ByProgram, _) when request.ProgramName != null => await _historyService.GetByProgramAsync(request.ProgramName),
            _ => request.Sort switch
            {
                Views.Screens.HistoryView.HistorySort.DateAsc      => await _historyService.GetSortedByDateAsync(false),
                Views.Screens.HistoryView.HistorySort.DurationDesc => await _historyService.GetSortedByDurationAsync(true),
                Views.Screens.HistoryView.HistorySort.DurationAsc  => await _historyService.GetSortedByDurationAsync(false),
                _                                                  => await _historyService.GetSortedByDateAsync(true)
            }
        };

        Views.Screens.HistoryView.ShowTable(records);
    }

    private void ShowError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nError: {msg}");
        Console.ResetColor();
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
}
