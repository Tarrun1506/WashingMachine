using WashingMachine.Models.Common;
using WashingMachine.Models.Entities;
using WashingMachine.Models.Events;

namespace WashingMachine.Services;

/// <summary>
/// Core service interface for all washing machine operations.
/// Controllers depend on this abstraction, not the concrete implementation.
/// </summary>
public interface IWashingMachineService
{
    /// <summary>The current machine state snapshot.</summary>
    WashingMachineModel Machine { get; }
    void RestoreState(WashingMachineModel state);
    void AcknowledgeCompletion();

    // ── Clothes management ───────────────────────────────────────────────
    ServiceResult AddClothes(int count);
    ServiceResult RemoveClothes(int count);

    // ── Settings ─────────────────────────────────────────────────────────
    ServiceResult ApplySettings(WashSettings settings);
    ServiceResult ApplyFavourite(Favourite favourite);

    // ── Cycle lifecycle ──────────────────────────────────────────────────
    /// <summary>Starts the washing cycle asynchronously. Does NOT block.</summary>
    ServiceResult StartCycle();
    ServiceResult PauseCycle();
    ServiceResult ResumeCycle();
    ServiceResult CancelCycle();

    /// <summary>Pauses the cycle and unlocks the door so clothes can be added.</summary>
    ServiceResult PauseForAddingClothes();
    ServiceResult FinishAddingClothes();

    // ── Events ───────────────────────────────────────────────────────────
    event EventHandler<WashProgressEventArgs> ProgressChanged;
    event EventHandler<MachineStateChangedEventArgs> StateChanged;
    event EventHandler<StageChangedEventArgs> StageChanged;
    event EventHandler CycleCompleted;
    event EventHandler CycleCancelled;
}
