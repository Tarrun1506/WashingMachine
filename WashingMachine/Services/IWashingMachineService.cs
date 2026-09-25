using WashingMachine.Events;
using WashingMachine.Models;

namespace WashingMachine.Services;

/// <summary>
/// Provides washing machine operations.
/// </summary>
public interface IWashingMachineService
{
    WashingMachineModel Machine { get; }

    event EventHandler<WashProgressEventArgs>? ProgressChanged;
    event EventHandler? CycleCompleted;
    event EventHandler? CycleCancelled;
    event EventHandler? ClothesUnloadRequired;

    void AddClothes(int count);
    void RemoveClothes(int count);
    void ApplySettings(WashSettings settings);
    void ApplyFavourite(Favourite favourite);

    /// <summary>Starts the wash cycle in the background. Does NOT block.</summary>
    void StartCycle();

    void PauseCycle();
    void ResumeCycle();
    void CancelCycle();

    void RestoreState(WashingMachineModel machine);

    /// <summary>Resumes a previously interrupted cycle in the background. Does NOT block.</summary>
    void ResumeSavedCycle();
}