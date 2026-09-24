using WashingMachine.Models;

namespace WashingMachine.Repository;

/// <summary>
/// Provides machine state repository operations.
/// </summary>
public interface IMachineStateRepository
{
    /// <summary>
    /// Saves machine state.
    /// </summary>
    /// <param name="machine">Machine state.</param>
    /// <returns>A task representing the operation.</returns>
    Task SaveAsync(WashingMachineModel machine);

    /// <summary>
    /// Loads machine state.
    /// </summary>
    /// <returns>Saved machine state.</returns>
    Task<WashingMachineModel?> LoadAsync();
}