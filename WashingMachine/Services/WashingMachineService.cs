using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Events;
using WashingMachine.Exceptions;
using WashingMachine.Helpers;
using WashingMachine.Models;
using WashingMachine.Models.Programs;

namespace WashingMachine.Services;

/// <summary>
/// Provides washing machine operations.
/// </summary>
public class WashingMachineService : IWashingMachineService, IDisposable
{
    private readonly IWashHistoryService _historyService;
    private readonly Logger _logger;

    private readonly Dictionary<string, WashProgram> _programRegistry = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Quick Wash", new QuickWashProgram() },
        { "Wool", new WoolProgram() },
        { "Synthetic", new SyntheticProgram() },
        { "Cotton", new CottonProgram() },
        { "Heavy Wash", new HeavyWashProgram() }
    };

    // Pause gate: starts open (1). When paused, we drain it to 0 so the cycle waits.
    private readonly SemaphoreSlim _pauseGate = new(1, 1);
    private CancellationTokenSource? _cts;

    public WashingMachineModel Machine { get; } = new();

    public event EventHandler<WashProgressEventArgs>? ProgressChanged;
    public event EventHandler? CycleCompleted;
    public event EventHandler? CycleCancelled;
    public event EventHandler? ClothesUnloadRequired;

    public WashingMachineService(IWashHistoryService historyService, Logger logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    public void AddClothes(int count)
    {
        if (count <= 0)
            throw new MachineOperationException(ErrorMessages.InvalidClothesCount);

        if (Machine.ClothesCount + count > Configurables.MaximumCapacity)
            throw new MachineOperationException(ErrorMessages.CapacityExceeded);

        Machine.ClothesCount += count;
        Machine.State = MachineState.Ready;
        _logger.Log("AddClothes", $"Added {count} clothes. Total: {Machine.ClothesCount}");
    }

    public void RemoveClothes(int count)
    {
        if (count <= 0)
            throw new MachineOperationException(ErrorMessages.InvalidClothesCount);

        if (count > Machine.ClothesCount)
            throw new MachineOperationException(ErrorMessages.InvalidClothesRemoval);

        Machine.ClothesCount -= count;
        if (Machine.ClothesCount == 0)
            Machine.State = MachineState.Idle;

        _logger.Log("RemoveClothes", $"Removed {count} clothes. Remaining: {Machine.ClothesCount}");
    }

    public void ApplySettings(WashSettings settings)
    {
        Machine.Settings = settings;
        _logger.Log("ApplySettings", $"Program={settings.ProgramName}, Temp={settings.Temperature}, Spin={settings.SpinSpeed}");
    }

    public void ApplyFavourite(Favourite favourite)
    {
        Machine.Settings = new WashSettings
        {
            ProgramName        = favourite.ProgramName,
            Temperature        = favourite.Temperature,
            SpinSpeed          = favourite.SpinSpeed,
            WaterLevel         = favourite.WaterLevel,
            IsPreWashEnabled   = favourite.IsPreWashEnabled,
            IsExtraRinseEnabled = favourite.IsExtraRinseEnabled,
            IsQuickWashEnabled = favourite.IsQuickWashEnabled,
        };
        _logger.Log("ApplyFavourite", $"Applied '{favourite.Name}'");
    }

    public void StartCycle()
    {
        if (Machine.ClothesCount == 0)
            throw new MachineOperationException("Please add clothes before starting the cycle.");

        if (Machine.State == MachineState.Running)
            throw new MachineOperationException(ErrorMessages.CycleAlreadyRunning);

        if (_pauseGate.CurrentCount == 0)
            _pauseGate.Release();

        _cts = new CancellationTokenSource();
        Machine.State = MachineState.Running;
        Machine.IsDoorLocked = true;
        Machine.CurrentCycle = new WashCycle
        {
            StartTime          = DateTime.Now,
            Stage              = CycleStage.Filling,
            ProgressPercentage = 0,
            RemainingSeconds   = GetProgramDuration(),
        };

        _logger.Log("StartCycle", $"Program={Machine.Settings.ProgramName}, Duration={Machine.CurrentCycle.RemainingSeconds}s");

        Task.Run(() => RunCycleAsync(Machine.CurrentCycle.RemainingSeconds, _cts.Token));
    }

    public void PauseCycle()
    {
        if (Machine.State != MachineState.Running)
            throw new MachineOperationException("Machine is not running.");

        _pauseGate.Wait(0);
        Machine.State = MachineState.Paused;
        _logger.Log("PauseCycle", $"Paused at {Machine.CurrentCycle?.RemainingSeconds}s remaining");
    }

    public void ResumeCycle()
    {
        if (Machine.State != MachineState.Paused)
            throw new MachineOperationException("Machine is not paused.");

        Machine.State = MachineState.Running;
        _pauseGate.Release();
        _logger.Log("ResumeCycle", "Resumed");
    }

    public void CancelCycle()
    {
        if (Machine.State != MachineState.Running && Machine.State != MachineState.Paused)
            throw new MachineOperationException("No running cycle found.");

        if (_pauseGate.CurrentCount == 0)
            _pauseGate.Release();

        _cts?.Cancel();
        _logger.Log("CancelCycle", "Cycle cancellation requested");
    }

    public void RestoreState(WashingMachineModel saved)
    {
        Machine.State        = saved.State;
        Machine.ClothesCount = saved.ClothesCount;
        Machine.IsDoorLocked = saved.IsDoorLocked;
        Machine.Settings     = saved.Settings;
        Machine.CurrentCycle = saved.CurrentCycle;
        _logger.Log("RestoreState", $"State={saved.State}, Clothes={saved.ClothesCount}");
    }

    public void ResumeSavedCycle()
    {
        if (Machine.State != MachineState.Running || Machine.CurrentCycle == null)
            return;

        if (_pauseGate.CurrentCount == 0)
            _pauseGate.Release();

        _cts = new CancellationTokenSource();
        int remaining = Machine.CurrentCycle.RemainingSeconds;
        _logger.Log("ResumeSavedCycle", $"Resuming with {remaining}s remaining");
        Task.Run(() => RunCycleAsync(remaining, _cts.Token));
    }

    private async Task RunCycleAsync(int remainingSeconds, CancellationToken token)
    {
        int totalSeconds = remainingSeconds; 

        try
        {
            // TIMER SHOWCASE - Using PeriodicTimer as requested
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

            while (remainingSeconds > 0)
            {
                if (token.IsCancellationRequested)
                    break;

                await _pauseGate.WaitAsync(token);
                _pauseGate.Release(); 

                bool ticked = await timer.WaitForNextTickAsync(token);
                if (!ticked)
                    break;

                remainingSeconds--;
                UpdateStage(remainingSeconds, totalSeconds);

                double progress = ((double)(totalSeconds - remainingSeconds) / totalSeconds) * 100;
                Machine.CurrentCycle!.ProgressPercentage = progress;
                Machine.CurrentCycle!.RemainingSeconds   = remainingSeconds;

                ProgressChanged?.Invoke(this, new WashProgressEventArgs
                {
                    Stage              = Machine.CurrentCycle.Stage,
                    ProgressPercentage = progress,
                    RemainingSeconds   = remainingSeconds,
                });
            }

            if (token.IsCancellationRequested)
            {
                _logger.Log("Cycle", "Cancelled");
                RecordHistory(CycleStatus.Cancelled);
                Machine.State        = MachineState.Idle;
                Machine.IsDoorLocked = false;
                Machine.CurrentCycle = null;
                CycleCancelled?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.Log("Cycle", "Completed successfully");
                RecordHistory(CycleStatus.Completed);
                Machine.State        = MachineState.Idle;
                Machine.IsDoorLocked = false;
                Machine.CurrentCycle = null;

                // Notify that clothes need to be unloaded (this will trigger the 3-second delay)
                ClothesUnloadRequired?.Invoke(this, EventArgs.Empty);

                // Reset clothes count after unloading
                Machine.ClothesCount = 0;

                // Then notify cycle completion
                CycleCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Log("Cycle", "Cancelled (OperationCanceledException)");
            RecordHistory(CycleStatus.Cancelled);
            Machine.State        = MachineState.Idle;
            Machine.IsDoorLocked = false;
            Machine.CurrentCycle = null;
            Machine.ClothesCount = 0;
            CycleCancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateStage(int remainingSeconds, int totalSeconds)
    {
        double quarter = totalSeconds / 4.0;
        CycleStage stage;
        if (remainingSeconds > quarter * 3) stage = CycleStage.Filling;
        else if (remainingSeconds > quarter * 2) stage = CycleStage.Washing;
        else if (remainingSeconds > quarter) stage = CycleStage.Rinsing;
        else stage = CycleStage.Spinning;
        Machine.CurrentCycle!.Stage = stage;
    }

    private void RecordHistory(CycleStatus status)
    {
        WashHistory history = new()
        {
            ProgramName  = Machine.Settings.ProgramName,
            ClothesCount = Machine.ClothesCount,
            StartTime    = Machine.CurrentCycle?.StartTime ?? DateTime.Now,
            EndTime      = DateTime.Now,
            Status       = status,
        };

        _historyService.Add(history);
        _logger.Log("RecordHistory", $"Program={history.ProgramName}, Status={status}");
    }

    private int GetProgramDuration()
    {
        string name = Machine.Settings.ProgramName.Trim();
        if (_programRegistry.TryGetValue(name, out WashProgram? program))
        {
            return program.GetDuration();
        }
        return new CottonProgram().GetDuration();
    }

    public void Dispose()
    {
        _cts?.Dispose();
        _pauseGate.Dispose();
        GC.SuppressFinalize(this);
    }
}
