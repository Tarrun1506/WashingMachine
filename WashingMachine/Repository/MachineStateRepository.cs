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

    public MachineStateRepository(IStorage storage)
    {
        _storage = storage;
    }

    private string FilePath => Configurables.MachineStateFilePath;

    /// <inheritdoc/>
    public void Save(WashingMachineModel machine)
    {
        _storage.SaveSingle(FilePath, machine);
    }

    /// <inheritdoc/>
    public WashingMachineModel? Load()
    {
        return _storage.LoadSingle<WashingMachineModel>(FilePath);
    }
}
