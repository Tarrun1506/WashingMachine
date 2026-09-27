using WashingMachine.Models;

namespace WashingMachine.Repository;

/// <summary>
/// Provides machine state repository operations.
/// </summary>
public interface IMachineStateRepository
{
    /// <summary>
    /// Saves machine state synchronously — safe to call during process shutdown.
    /// </summary>
    void Save(WashingMachineModel machine);

    /// <summary>
    /// Loads machine state synchronously — used at startup.
    /// </summary>
    WashingMachineModel? Load();
}
