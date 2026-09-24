using WashingMachine.Constants;
using WashingMachine.Models;
using WashingMachine.Storage;

namespace WashingMachine.Repository;

/// <summary>
/// Provides machine state repository operations.
/// </summary>
public class MachineStateRepository : IMachineStateRepository
{
    private readonly IStorage _storage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineStateRepository"/> class.
    /// </summary>
    /// <param name="storage">Storage implementation.</param>
    public MachineStateRepository(IStorage storage)
    {
        this._storage = storage;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(WashingMachineModel machine)
    {
        await this._storage.SaveSingleAsync(Configurables.MachineStateFilePath, machine);
    }

    /// <inheritdoc/>
    public async Task<WashingMachineModel?> LoadAsync()
    {
        return await this._storage.LoadSingleAsync<WashingMachineModel>(Configurables.MachineStateFilePath);
    }
}