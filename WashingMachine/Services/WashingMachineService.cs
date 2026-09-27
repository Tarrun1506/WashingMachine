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
    private readonly object _machineLock = new();
    private readonly object _cycleLock = new();
    private readonly WashingMachineModel _machine = new();
    private CancellationTokenSource? _cts;
    private Task? _cycleTask;
    private bool _disposed;

    public WashingMachineModel Machine
    {
        get
        {
            lock (_machineLock) return CopyMachine(_machine);
        }
    }

    public event EventHandler<WashProgressEventArgs>? ProgressChanged;
    public event EventHandler? CycleCancelled;
    public event EventHandler? CycleCompleted;

    public WashingMachineService(IWashHistoryService historyService, Logger logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    private static WashingMachineModel CopyMachine(WashingMachineModel machine) => new()
    {
        State = machine.State,
        ClothesCount = machine.ClothesCount,
        IsDoorLocked = machine.IsDoorLocked,
        Settings = new WashSettings
        {
            ProgramName = machine.Settings.ProgramName,
            Temperature = machine.Settings.Temperature,
            SpinSpeed = machine.Settings.SpinSpeed,
            WaterLevel = machine.Settings.WaterLevel,
            IsPreWashEnabled = machine.Settings.IsPreWashEnabled,
            IsExtraRinseEnabled = machine.Settings.IsExtraRinseEnabled,
            IsQuickWashEnabled = machine.Settings.IsQuickWashEnabled,
        },
        CurrentCycle = machine.CurrentCycle is null ? null : new WashCycle
        {
            Id = machine.CurrentCycle.Id,
            StartTime = machine.CurrentCycle.StartTime,
            Stage = machine.CurrentCycle.Stage,
            ProgressPercentage = machine.CurrentCycle.ProgressPercentage,
            RemainingSeconds = machine.CurrentCycle.RemainingSeconds,
        },
    };

    private static WashSettings CopySettings(WashSettings settings) => new()
    {
        ProgramName = settings.ProgramName,
        Temperature = settings.Temperature,
        SpinSpeed = settings.SpinSpeed,
        WaterLevel = settings.WaterLevel,
        IsPreWashEnabled = settings.IsPreWashEnabled,
        IsExtraRinseEnabled = settings.IsExtraRinseEnabled,
        IsQuickWashEnabled = settings.IsQuickWashEnabled,
    };

    public void AddClothes(int count)
    {
        int total;
        lock (_machineLock)
        {
            if (_machine.State == MachineState.Running) throw new MachineOperationException(ErrorMessages.CannotAddWhileRunning);
            if (count <= 0) throw new MachineOperationException(ErrorMessages.InvalidClothesCount);
            if (_machine.ClothesCount >= Configurables.MaximumCapacity) throw new MachineOperationException(ErrorMessages.MachineFull);
            if (count > Configurables.MaximumCapacity || _machine.ClothesCount + count > Configurables.MaximumCapacity) throw new MachineOperationException(ErrorMessages.CapacityExceeded);
            _machine.ClothesCount += count;
            if (_machine.State == MachineState.Idle) _machine.State = MachineState.Ready;
            total = _machine.ClothesCount;
        }
        _logger.Log("AddClothes", $"Added {count} clothes. Total: {total}");
    }

    public void RemoveClothes(int count)
    {
        int remaining;
        lock (_machineLock)
        {
            if (_machine.State == MachineState.Running) throw new MachineOperationException(ErrorMessages.CannotRemoveWhileRunning);
            if (_machine.ClothesCount == 0) throw new MachineOperationException(ErrorMessages.NoClothesToRemove);
            if (count <= 0) throw new MachineOperationException(ErrorMessages.InvalidClothesCount);
            if (count > _machine.ClothesCount) throw new MachineOperationException(ErrorMessages.InvalidClothesRemoval);
            _machine.ClothesCount -= count;
            if (_machine.ClothesCount == 0) _machine.State = MachineState.Idle;
            remaining = _machine.ClothesCount;
        }
        _logger.Log("RemoveClothes", $"Removed {count} clothes. Remaining: {remaining}");
    }

    public void ApplySettings(WashSettings settings)
    {
        if (!SettingsValidator.TryValidateSettings(settings, out string error))
            throw new MachineOperationException(error);
        lock (_machineLock)
        {
            if (_machine.State == MachineState.Running || _machine.State == MachineState.Paused)
                throw new MachineOperationException(ErrorMessages.CannotModifyWhileRunning);
            _machine.Settings = CopySettings(settings);
        }
        _logger.Log("ApplySettings", $"Program={settings.ProgramName}, Temp={settings.Temperature}, Spin={settings.SpinSpeed}");
    }

    public void ApplyFavourite(Favourite favourite)
    {
        if (favourite == null)
            throw new MachineOperationException(ErrorMessages.FavouriteNotFound);

        ApplySettings(new WashSettings
        {
            ProgramName         = favourite.ProgramName,
            Temperature         = favourite.Temperature,
            SpinSpeed           = favourite.SpinSpeed,
            WaterLevel          = favourite.WaterLevel,
            IsPreWashEnabled    = favourite.IsPreWashEnabled,
            IsExtraRinseEnabled = favourite.IsExtraRinseEnabled,
            IsQuickWashEnabled  = favourite.IsQuickWashEnabled,
        });
        _logger.Log("ApplyFavourite", $"Applied '{favourite.Name}'");
    }

    public void StartCycle()
    {
        int duration;
        lock (_machineLock)
        {
            if (_machine.ClothesCount == 0) throw new MachineOperationException(ErrorMessages.ClothesRequiredToStart);
            if (_machine.State == MachineState.Running) throw new MachineOperationException(ErrorMessages.CycleAlreadyRunning);
            if (_machine.State == MachineState.Paused) throw new MachineOperationException(ErrorMessages.CycleIsPaused);
            if (!SettingsValidator.TryValidateSettings(_machine.Settings, out string settingsError)) throw new MachineOperationException(settingsError);
            duration = GetProgramDuration();
            _machine.State = MachineState.Running;
            _machine.IsDoorLocked = true;
            _machine.CurrentCycle = new WashCycle { StartTime = DateTime.Now, Stage = CycleStage.Filling, ProgressPercentage = 0, RemainingSeconds = duration };
        }
        StartCycleTask(duration);
        _logger.Log("StartCycle", $"Program={Machine.Settings.ProgramName}, Duration={duration}s");
    }

    private void StartCycleTask(int remainingSeconds)
    {
        lock (_cycleLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _cts = new CancellationTokenSource();
            CancellationTokenSource cycleCts = _cts;
            _cycleTask = Task.Run(() => RunCycleAsync(remainingSeconds, cycleCts));
        }
    }

    public void PauseCycle()
    {
        lock (_machineLock)
        {
            if (_machine.State != MachineState.Running) throw new MachineOperationException(ErrorMessages.CycleNotRunning);
            _pauseGate.Wait(0);
            _machine.State = MachineState.Paused;
        }
        _logger.Log("PauseCycle", $"Paused at {Machine.CurrentCycle?.RemainingSeconds}s remaining");
    }

    public void ResumeCycle()
    {
        lock (_machineLock)
        {
            if (_machine.State != MachineState.Paused) throw new MachineOperationException(ErrorMessages.MachineNotPaused);
            _machine.State = MachineState.Running;
            if (_pauseGate.CurrentCount == 0) _pauseGate.Release();
        }
        _logger.Log("ResumeCycle", "Resumed");
    }

    public void CancelCycle()
    {
        lock (_machineLock)
        {
            if (_machine.State != MachineState.Running && _machine.State != MachineState.Paused) throw new MachineOperationException(ErrorMessages.CycleNotRunning);
            if (_pauseGate.CurrentCount == 0) _pauseGate.Release();
        }
        lock (_cycleLock) _cts?.Cancel();
        _logger.Log("CancelCycle", "Cycle cancellation requested");
    }

    public void RestoreState(WashingMachineModel saved)
    {
        lock (_machineLock)
        {
            int clothes = Math.Clamp(saved.ClothesCount, 0, Configurables.MaximumCapacity);
            _machine.ClothesCount = clothes;
            _machine.CurrentCycle = saved.CurrentCycle is null ? null : CopyMachine(new WashingMachineModel { CurrentCycle = saved.CurrentCycle }).CurrentCycle;
            if (saved.Settings != null && SettingsValidator.TryValidateSettings(saved.Settings, out _)) _machine.Settings = CopySettings(saved.Settings);
            bool hasCycle = _machine.CurrentCycle is not null;
            _machine.State = hasCycle && saved.State is (MachineState.Running or MachineState.Paused) ? MachineState.Running : saved.State;
            if (!Enum.IsDefined(_machine.State) || (!hasCycle && _machine.State is (MachineState.Running or MachineState.Paused))) _machine.State = MachineState.Idle;
            if (clothes == 0 && _machine.State is not (MachineState.Running or MachineState.Paused)) _machine.State = MachineState.Idle;
            _machine.IsDoorLocked = _machine.State is MachineState.Running or MachineState.Paused;
        }
        _logger.Log("RestoreState", $"State={Machine.State}, Clothes={Machine.ClothesCount}");
    }

    public void ResetMachine()
    {
        lock (_machineLock)
        {
            _machine.State = MachineState.Idle;
            _machine.ClothesCount = 0;
            _machine.IsDoorLocked = false;
            _machine.CurrentCycle = null;
        }
    }

    public void ResumeSavedCycle()
    {
        int remaining;
        lock (_machineLock)
        {
            if (_machine.State != MachineState.Running || _machine.CurrentCycle == null) return;
            remaining = Math.Max(_machine.CurrentCycle.RemainingSeconds, 0);
            _machine.IsDoorLocked = true;
        }
        if (_pauseGate.CurrentCount == 0) _pauseGate.Release();
        _logger.Log("ResumeSavedCycle", $"Resuming with {remaining}s remaining");
        StartCycleTask(remaining);
    }

    private async Task RunCycleAsync(int remainingSeconds, CancellationTokenSource cycleCts)
    {
        CancellationToken token = cycleCts.Token;
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
                double progress = ((double)(totalSeconds - remainingSeconds) / totalSeconds) * 100;
                CycleStage stage;
                lock (_machineLock)
                {
                    if (_machine.CurrentCycle is null) return;
                    UpdateStage(remainingSeconds, totalSeconds);
                    _machine.CurrentCycle.ProgressPercentage = progress;
                    _machine.CurrentCycle.RemainingSeconds = remainingSeconds;
                    stage = _machine.CurrentCycle.Stage;
                }

                ProgressChanged?.Invoke(this, new WashProgressEventArgs
                {
                    Stage              = stage,
                    ProgressPercentage = progress,
                    RemainingSeconds   = remainingSeconds,
                });
            }

            if (token.IsCancellationRequested)
            {
                _logger.Log("Cycle", "Cancelled");
                RecordHistory(CycleStatus.Cancelled);
                lock (_machineLock)
                {
                    _machine.State = MachineState.Idle;
                    _machine.IsDoorLocked = false;
                    _machine.CurrentCycle = null;
                    _machine.ClothesCount = 0;
                }
                CycleCancelled?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.Log("Cycle", "Washing cycle completed, starting unloading");
                
                // Update to unloading stage
                lock (_machineLock)
                {
                    if (_machine.CurrentCycle is null) return;
                    _machine.CurrentCycle.Stage = CycleStage.Unloading;
                    _machine.CurrentCycle.RemainingSeconds = 3;
                    _machine.CurrentCycle.ProgressPercentage = 100;
                }
                ProgressChanged?.Invoke(this, new WashProgressEventArgs
                {
                    Stage              = CycleStage.Unloading,
                    ProgressPercentage = 100,
                    RemainingSeconds   = 3,
                });

                // Run unloading countdown
                for (int unloadSeconds = 3; unloadSeconds > 0; unloadSeconds--)
                {
                    if (token.IsCancellationRequested)
                        break;

                    await _pauseGate.WaitAsync(token);
                    _pauseGate.Release();

                    await timer.WaitForNextTickAsync(token);
                    
                    lock (_machineLock)
                    {
                        if (_machine.CurrentCycle is null) return;
                        _machine.CurrentCycle.RemainingSeconds = unloadSeconds - 1;
                    }
                    ProgressChanged?.Invoke(this, new WashProgressEventArgs
                    {
                        Stage              = CycleStage.Unloading,
                        ProgressPercentage = 100,
                        RemainingSeconds   = unloadSeconds - 1,
                    });
                }

                if (token.IsCancellationRequested) throw new OperationCanceledException(token);

                // Reset clothes count after unloading
                _logger.Log("Cycle", "Unloading completed, clothes reset to 0");
                RecordHistory(CycleStatus.Completed);
                lock (_machineLock)
                {
                    _machine.ClothesCount = 0;
                    _machine.State = MachineState.Idle;
                    _machine.IsDoorLocked = false;
                    _machine.CurrentCycle = null;
                }

                // Notify cycle completion
                CycleCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Log("Cycle", "Cancelled (OperationCanceledException)");
            RecordHistory(CycleStatus.Cancelled);
            lock (_machineLock)
            {
                _machine.State = MachineState.Idle;
                _machine.IsDoorLocked = false;
                _machine.CurrentCycle = null;
                _machine.ClothesCount = 0;
            }
            CycleCancelled?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError("Cycle", ex.ToString());
            lock (_machineLock)
            {
                _machine.State = MachineState.Idle;
                _machine.IsDoorLocked = false;
                _machine.CurrentCycle = null;
            }
        }
        finally
        {
            lock (_cycleLock) if (ReferenceEquals(_cts, cycleCts)) _cts = null;
            cycleCts.Dispose();
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
        _machine.CurrentCycle!.Stage = stage;
    }

    private void RecordHistory(CycleStatus status)
    {
        WashingMachineModel snapshot = Machine;
        WashHistory history = new()
        {
            ProgramName  = snapshot.Settings.ProgramName,
            ClothesCount = snapshot.ClothesCount,
            StartTime    = snapshot.CurrentCycle?.StartTime ?? DateTime.Now,
            EndTime      = DateTime.Now,
            Status       = status,
        };

        _historyService.Add(history);
        _logger.Log("RecordHistory", $"Program={history.ProgramName}, Status={status}");
    }

    private int GetProgramDuration()
    {
        string name = _machine.Settings.ProgramName.Trim();
        if (_programRegistry.TryGetValue(name, out WashProgram? program))
        {
            return program.GetDuration();
        }
        return new CottonProgram().GetDuration();
    }

    public void Dispose()
    {
        Task? cycleTask;
        CancellationTokenSource? cycleCts;
        lock (_cycleLock)
        {
            if (_disposed) return;
            _disposed = true;
            cycleTask = _cycleTask;
            cycleCts = _cts;
        }
        try { cycleCts?.Cancel(); }
        catch (ObjectDisposedException) { }
        if (_pauseGate.CurrentCount == 0) _pauseGate.Release();
        try { cycleTask?.GetAwaiter().GetResult(); }
        catch (Exception ex) { _logger.LogError("Shutdown", ex.ToString()); }
        _pauseGate.Dispose();
        GC.SuppressFinalize(this);
    }
}
