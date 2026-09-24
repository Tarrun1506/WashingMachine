using WashingMachine.Models.Common;
using WashingMachine.Models.Entities;
using WashingMachine.Enums;
using WashingMachine.Models.Events;
using WashingMachine.Exceptions;
using WashingMachine.Repository.Abstractions;
using WashingMachine.Services;

namespace WashingMachine.Services;

/// <summary>
/// Core washing machine service — contains ALL business logic.
/// Demonstrates:
///   - async/await + Task for a non-blocking wash cycle
///   - CancellationToken for cooperative cancellation
///   - Events/delegates to decouple UI from the engine
///   - SemaphoreSlim to prevent simultaneous cycle starts
///   - State machine: only valid transitions are permitted
/// </summary>
public sealed class WashingMachineService : IWashingMachineService
{
    // ── Dependencies ─────────────────────────────────────────────────────
    private readonly IWashHistoryService _historyService;

    // ── Machine state ────────────────────────────────────────────────────
    public WashingMachineModel Machine { get; private set; } = new();

    public void RestoreState(WashingMachineModel state)
    {
        Machine = state;
        if (Machine.IsCycleActive || Machine.IsCyclePaused)
        {
            _cycleLock.Wait(0);
            _cts = new CancellationTokenSource();
            if (Machine.IsCyclePaused || Machine.State == MachineState.AddingClothes)
            {
                _pauseGate.Wait(0);
            }
            _cycleTask = Task.Run(() => RunCycleAsync(Machine.CurrentCycle!, Machine.Settings.Clone(), _cts.Token, isResuming: true));
        }
    }

    public void AcknowledgeCompletion()
    {
        if (Machine.State == MachineState.Completed || Machine.State == MachineState.Cancelled)
        {
            Machine.State = MachineState.Idle;
        }
    }

    // ── Async cycle control ───────────────────────────────────────────────
    private Task? _cycleTask;
    private CancellationTokenSource? _cts;
    // Prevents two simultaneous StartCycle calls from racing
    private readonly SemaphoreSlim _cycleLock = new(1, 1);
    // ManualResetEvent used to pause/resume the cycle cleanly
    private readonly SemaphoreSlim _pauseGate = new(1, 1);

    // ── Events ───────────────────────────────────────────────────────────
    public event EventHandler<WashProgressEventArgs>? ProgressChanged;
    public event EventHandler<MachineStateChangedEventArgs>? StateChanged;
    public event EventHandler<StageChangedEventArgs>? StageChanged;
    public event EventHandler? CycleCompleted;
    public event EventHandler? CycleCancelled;

    public WashingMachineService(IWashHistoryService historyService)
    {
        _historyService = historyService;
    }

    // ── Clothes management ───────────────────────────────────────────────

    public ServiceResult AddClothes(int count)
    {
        if (count <= 0)
            return ServiceResult.Fail("Please enter a number greater than zero.");

        if (Machine.State is not (MachineState.Idle or MachineState.Ready or MachineState.Paused or MachineState.AddingClothes))
            return ServiceResult.Fail("Cannot add clothes while the machine is running.");

        if (count > Machine.AvailableCapacity)
            return ServiceResult.Fail(
                $"Cannot add {count} clothes.\n" +
                $"Current Load : {Machine.ClothesCount}\n" +
                $"Maximum Load : {WashingMachineModel.MaxCapacity}\n" +
                $"Available    : {Machine.AvailableCapacity}");

        Machine.ClothesCount += count;
        UpdateStateAfterClothesChange();
        return ServiceResult.Ok();
    }

    public ServiceResult RemoveClothes(int count)
    {
        if (count <= 0)
            return ServiceResult.Fail("Please enter a number greater than zero.");

        if (Machine.State is not (MachineState.Idle or MachineState.Ready))
            return ServiceResult.Fail("Cannot remove clothes while the machine is running.");

        if (count > Machine.ClothesCount)
            return ServiceResult.Fail(
                $"Cannot remove {count} clothes. Only {Machine.ClothesCount} loaded.");

        Machine.ClothesCount -= count;
        UpdateStateAfterClothesChange();
        return ServiceResult.Ok();
    }

    private void UpdateStateAfterClothesChange()
    {
        if (Machine.State is MachineState.AddingClothes or MachineState.Paused)
            return; // Keep current state during mid-cycle add

        var newState = Machine.ClothesCount > 0 ? MachineState.Ready : MachineState.Idle;
        ChangeState(newState);
    }

    // ── Settings ─────────────────────────────────────────────────────────

    public ServiceResult ApplySettings(WashSettings settings)
    {
        if (Machine.IsCycleActive)
            return ServiceResult.Fail("Cannot change settings while the machine is running.");

        Machine.Settings = settings;
        if (Machine.ClothesCount > 0 && Machine.State == MachineState.Idle)
            ChangeState(MachineState.Ready);

        return ServiceResult.Ok();
    }

    public ServiceResult ApplyFavourite(Favourite favourite)
    {
        if (Machine.IsCycleActive)
            return ServiceResult.Fail("Cannot apply a favourite while the machine is running.");

        var program = ProgramRegistry.GetByNameOrDefault(favourite.ProgramName);
        Machine.Settings = new WashSettings
        {
            Program     = program,
            Temperature = favourite.Temperature,
            SpinSpeed   = favourite.SpinSpeed,
            WaterLevel  = favourite.WaterLevel,
            PreWash     = favourite.PreWash,
            ExtraRinse  = favourite.ExtraRinse,
            QuickMode   = favourite.QuickMode
        };
        return ServiceResult.Ok();
    }

    // ── Cycle lifecycle ──────────────────────────────────────────────────

    public ServiceResult StartCycle()
    {
        // Guard: must have clothes
        if (!Machine.HasClothes)
            return ServiceResult.Fail("Please add clothes before starting the washing cycle.");

        // Guard: prevent two simultaneous cycles (non-blocking check)
        if (!_cycleLock.Wait(0))
            return ServiceResult.Fail(
                "The washing machine is already running.\n" +
                "Please wait until the current washing cycle is completed\n" +
                "or cancel the current cycle before starting a new one.");

        // Guard: already in an active or paused state
        if (Machine.IsCycleActive || Machine.IsCyclePaused)
        {
            _cycleLock.Release();
            return ServiceResult.Fail(
                "The washing machine is already running.\n" +
                "Please wait until the current washing cycle is completed\n" +
                "or cancel the current cycle before starting a new one.");
        }

        _cts = new CancellationTokenSource();
        var cycle = new WashCycle
        {
            ClothesCount = Machine.ClothesCount,
            RemainingSeconds = Machine.Settings.Program.TotalDurationSeconds
        };
        Machine.CurrentCycle = cycle;
        Machine.DoorLocked = true;
        ChangeState(MachineState.Washing);

        // Fire-and-forget the cycle task; lock is released inside RunCycleAsync when done
        _cycleTask = Task.Run(() => RunCycleAsync(cycle, Machine.Settings.Clone(), _cts.Token));

        return ServiceResult.Ok();
    }

    public ServiceResult PauseCycle()
    {
        if (!Machine.IsCycleActive)
            return ServiceResult.Fail("Cannot pause — no active washing cycle.");

        // Block the pause gate so the cycle loop waits
        _pauseGate.Wait(0);
        ChangeState(MachineState.Paused);
        return ServiceResult.Ok();
    }

    public ServiceResult ResumeCycle()
    {
        if (Machine.State is not (MachineState.Paused or MachineState.AddingClothes))
            return ServiceResult.Fail("Cannot resume — machine is not paused.");

        Machine.DoorLocked = true;
        ChangeState(MachineState.Washing);
        // Release the gate so the cycle loop continues
        _pauseGate.Release();
        return ServiceResult.Ok();
    }

    public ServiceResult CancelCycle()
    {
        if (!Machine.IsCycleActive && !Machine.IsCyclePaused)
            return ServiceResult.Fail("No active cycle to cancel.");

        // If paused, release the gate first so the task can observe cancellation
        if (_pauseGate.CurrentCount == 0)
            _pauseGate.Release();

        _cts?.Cancel();
        return ServiceResult.Ok();
    }

    public ServiceResult PauseForAddingClothes()
    {
        if (!Machine.IsCycleActive)
            return ServiceResult.Fail("Cannot pause — no active washing cycle.");

        _pauseGate.Wait(0);
        Machine.DoorLocked = false;
        ChangeState(MachineState.AddingClothes);
        return ServiceResult.Ok();
    }

    public ServiceResult FinishAddingClothes()
    {
        if (Machine.State != MachineState.AddingClothes)
            return ServiceResult.Fail("Not in adding-clothes mode.");

        if (Machine.CurrentCycle != null)
            Machine.CurrentCycle.ClothesCount = Machine.ClothesCount;

        Machine.DoorLocked = true;
        ChangeState(MachineState.Washing);
        _pauseGate.Release();
        return ServiceResult.Ok();
    }

    // ── The async washing engine ──────────────────────────────────────────

    /// <summary>
    /// Runs the entire wash cycle asynchronously through all stages.
    /// Uses CancellationToken for cooperative cancellation,
    /// SemaphoreSlim (_pauseGate) for pause/resume,
    /// and PeriodicTimer for accurate per-second progress updates.
    /// </summary>
    private async Task RunCycleAsync(WashCycle cycle, WashSettings settings, CancellationToken token, bool isResuming = false)
    {
        var stages = settings.Program.GetStages().ToList();

        if (settings.QuickMode)
            stages = stages.Select(s => (s.Stage, Math.Max(1, s.DurationSeconds / 2))).ToList();

        int totalSeconds = stages.Sum(s => s.DurationSeconds);
        int elapsedSeconds = isResuming ? Math.Max(0, totalSeconds - cycle.RemainingSeconds) : 0;

        try
        {
            foreach (var (stage, durationSeconds) in stages)
            {
                token.ThrowIfCancellationRequested();

                // Update visible stage (maps cycle stage to machine state)
                var machineState = StageToMachineState(stage);
                if (machineState != Machine.State && Machine.State != MachineState.Paused && Machine.State != MachineState.AddingClothes)
                {
                    RaiseStageChanged(cycle.CurrentStage, stage);
                    cycle.CurrentStage = stage;
                    ChangeState(machineState);
                }

                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                int stageElapsed = 0;

                while (stageElapsed < durationSeconds)
                {
                    // Check cancellation
                    token.ThrowIfCancellationRequested();

                    // Wait for the pause gate — if paused, this blocks until resumed
                    await _pauseGate.WaitAsync(token).ConfigureAwait(false);
                    _pauseGate.Release(); // immediately re-release; we just waited for "un-paused"

                    // Wait one real second
                    await timer.WaitForNextTickAsync(token).ConfigureAwait(false);

                    stageElapsed++;
                    elapsedSeconds++;

                    int remaining = totalSeconds - elapsedSeconds;
                    double progress = (double)elapsedSeconds / totalSeconds * 100.0;
                    cycle.ProgressPercent = progress;
                    cycle.RemainingSeconds = remaining;

                    // Raise progress event (UI subscribes to redraw)
                    ProgressChanged?.Invoke(this, new WashProgressEventArgs
                    {
                        ProgressPercent  = progress,
                        RemainingSeconds = remaining,
                        CurrentStage     = cycle.CurrentStage
                    });
                }
            }

            // ── Cycle completed ─────────────────────────────────────
            cycle.FinalStatus = CycleStatus.Completed;
            cycle.EndTime     = DateTime.Now;
            await RecordHistoryAsync(cycle, settings, CycleStatus.Completed);
            FinalizeCycle(MachineState.Completed);
            CycleCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException)
        {
            // ── Cycle cancelled ─────────────────────────────────────
            cycle.FinalStatus = CycleStatus.Cancelled;
            cycle.EndTime     = DateTime.Now;
            await RecordHistoryAsync(cycle, settings, CycleStatus.Cancelled);
            FinalizeCycle(MachineState.Cancelled);
            CycleCancelled?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _cycleLock.Release();
        }
    }

    private void FinalizeCycle(MachineState endState)
    {
        Machine.DoorLocked   = false;
        Machine.CurrentCycle = null;
        ChangeState(endState);

        // After a brief moment, return to idle
        Task.Delay(2000).ContinueWith(_ =>
        {
            Machine.ClothesCount = 0;
            ChangeState(MachineState.Idle);
        });
    }

    private async Task RecordHistoryAsync(WashCycle cycle, WashSettings settings, CycleStatus status)
    {
        try
        {
            await _historyService.RecordAsync(Machine, cycle, status).ConfigureAwait(false);
        }
        catch { /* history recording must not crash the cycle */ }
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static MachineState StageToMachineState(CycleStage stage) => stage switch
    {
        CycleStage.Rinsing  => MachineState.Rinsing,
        CycleStage.Spinning => MachineState.Spinning,
        _                   => MachineState.Washing
    };

    private void ChangeState(MachineState newState)
    {
        if (Machine.State == newState) return;
        var old = Machine.State;
        Machine.State = newState;
        StateChanged?.Invoke(this, new MachineStateChangedEventArgs { OldState = old, NewState = newState });
    }

    private void RaiseStageChanged(CycleStage old, CycleStage newStage)
    {
        if (old == newStage) return;
        StageChanged?.Invoke(this, new StageChangedEventArgs { OldStage = old, NewStage = newStage });
    }
}
