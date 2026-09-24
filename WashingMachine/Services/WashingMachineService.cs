using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Events;
using WashingMachine.Exceptions;
using WashingMachine.Helpers;
using WashingMachine.Models;

namespace WashingMachine.Services;

/// <summary>
/// Provides washing machine operations.
/// </summary>
public class WashingMachineService : IWashingMachineService
{
    private readonly IWashHistoryService _historyService;
    private readonly Logger _logger;

    // Pause gate: starts open (1). When paused, we drain it to 0 so the cycle waits.
    private readonly SemaphoreSlim _pauseGate = new(1, 1);
    private CancellationTokenSource? _cts;

    public WashingMachineModel Machine { get; } = new();

    public event EventHandler<WashProgressEventArgs>? ProgressChanged;
    public event EventHandler? CycleCompleted;
    public event EventHandler? CycleCancelled;

    /// <summary>
    /// Initializes a new instance of the <see cref="WashingMachineService"/> class.
    /// </summary>
    public WashingMachineService(IWashHistoryService historyService, Logger logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void ApplySettings(WashSettings settings)
    {
        Machine.Settings = settings;
        _logger.Log("ApplySettings", $"Program={settings.ProgramName}, Temp={settings.Temperature}, Spin={settings.SpinSpeed}");
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    /// <remarks>
    /// Returns immediately — the actual cycle runs in the background via Task.Run.
    /// Call this without await from the controller so the UI stays responsive.
    /// </remarks>
    public void StartCycle()
    {
        if (Machine.ClothesCount == 0)
            throw new MachineOperationException("Please add clothes before starting the cycle.");

        if (Machine.State == MachineState.Running)
            throw new MachineOperationException(ErrorMessages.CycleAlreadyRunning);

        // Make sure the pause gate is open before starting
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

        // Run in background — fire-and-forget style (controller does NOT await this)
        Task.Run(() => RunCycleAsync(Machine.CurrentCycle.RemainingSeconds, _cts.Token));
    }

    /// <inheritdoc/>
    public void PauseCycle()
    {
        if (Machine.State != MachineState.Running)
            throw new MachineOperationException("Machine is not running.");

        // Drain the semaphore so the cycle loop blocks at WaitAsync
        _pauseGate.Wait(0);
        Machine.State = MachineState.Paused;
        _logger.Log("PauseCycle", $"Paused at {Machine.CurrentCycle?.RemainingSeconds}s remaining");
    }

    /// <inheritdoc/>
    public void ResumeCycle()
    {
        if (Machine.State != MachineState.Paused)
            throw new MachineOperationException("Machine is not paused.");

        Machine.State = MachineState.Running;
        // Release the gate so the cycle loop can continue
        _pauseGate.Release();
        _logger.Log("ResumeCycle", "Resumed");
    }

    /// <inheritdoc/>
    public void CancelCycle()
    {
        if (Machine.State != MachineState.Running && Machine.State != MachineState.Paused)
            throw new MachineOperationException("No running cycle found.");

        // If paused, release the gate first so the task can observe cancellation
        if (_pauseGate.CurrentCount == 0)
            _pauseGate.Release();

        _cts?.Cancel();
        _logger.Log("CancelCycle", "Cycle cancellation requested");
    }

    /// <inheritdoc/>
    public void RestoreState(WashingMachineModel saved)
    {
        Machine.State        = saved.State;
        Machine.ClothesCount = saved.ClothesCount;
        Machine.IsDoorLocked = saved.IsDoorLocked;
        Machine.Settings     = saved.Settings;
        Machine.CurrentCycle = saved.CurrentCycle;
        _logger.Log("RestoreState", $"State={saved.State}, Clothes={saved.ClothesCount}");
    }

    /// <inheritdoc/>
    public void ResumeSavedCycle()
    {
        if (Machine.State != MachineState.Running || Machine.CurrentCycle == null)
            return;

        // Ensure gate is open
        if (_pauseGate.CurrentCount == 0)
            _pauseGate.Release();

        _cts = new CancellationTokenSource();
        int remaining = Machine.CurrentCycle.RemainingSeconds;
        _logger.Log("ResumeSavedCycle", $"Resuming with {remaining}s remaining");
        Task.Run(() => RunCycleAsync(remaining, _cts.Token));
    }

    // ── Background wash loop ────────────────────────────────────────────────

    private async Task RunCycleAsync(int remainingSeconds, CancellationToken token)
    {
        int totalSeconds = remainingSeconds; // use saved value as total for progress %

        try
        {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

            while (remainingSeconds > 0)
            {
                // Check cancellation before waiting
                if (token.IsCancellationRequested)
                    break;

                // Block here while paused — releases when ResumeCycle() calls Release()
                await _pauseGate.WaitAsync(token);
                _pauseGate.Release(); // immediately re-release so next tick can enter

                // Wait exactly 1 second
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
                await RecordHistoryAsync(CycleStatus.Cancelled);
                Machine.State        = MachineState.Cancelled;
                Machine.IsDoorLocked = false;
                Machine.CurrentCycle = null;
                CycleCancelled?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.Log("Cycle", "Completed successfully");
                await RecordHistoryAsync(CycleStatus.Completed);
                Machine.State        = MachineState.Completed;
                Machine.IsDoorLocked = false;
                Machine.CurrentCycle = null;
                CycleCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Log("Cycle", "Cancelled (OperationCanceledException)");
            await RecordHistoryAsync(CycleStatus.Cancelled);
            Machine.State        = MachineState.Cancelled;
            Machine.IsDoorLocked = false;
            Machine.CurrentCycle = null;
            CycleCancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateStage(int remainingSeconds, int totalSeconds)
    {
        // Split the total into 4 equal quarters for stages
        double quarter = totalSeconds / 4.0;

        CycleStage stage;
        if (remainingSeconds > quarter * 3)
            stage = CycleStage.Filling;
        else if (remainingSeconds > quarter * 2)
            stage = CycleStage.Washing;
        else if (remainingSeconds > quarter)
            stage = CycleStage.Rinsing;
        else
            stage = CycleStage.Spinning;

        Machine.CurrentCycle!.Stage = stage;
    }

    private async Task RecordHistoryAsync(CycleStatus status)
    {
        WashHistory history = new()
        {
            ProgramName  = Machine.Settings.ProgramName,
            ClothesCount = Machine.ClothesCount,
            StartTime    = Machine.CurrentCycle?.StartTime ?? DateTime.Now,
            EndTime      = DateTime.Now,
            Status       = status,
        };

        await _historyService.AddAsync(history);
        _logger.Log("RecordHistory", $"Program={history.ProgramName}, Status={status}");
    }

    private int GetProgramDuration()
    {
        return Machine.Settings.ProgramName.Trim().ToUpperInvariant() switch
        {
            "QUICK WASH" => 30,
            "WOOL"       => 45,
            "SYNTHETIC"  => 60,
            "COTTON"     => 90,
            "HEAVY WASH" => 120,
            _            => 60,
        };
    }
}