using System.Text.Json;
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
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public MachineStateRepository(IStorage storage)
    {
        _storage = storage;
    }

    private string FilePath => Configurables.MachineStateFilePath;

    /// <inheritdoc/>
    public async Task SaveAsync(WashingMachineModel machine)
    {
        await _storage.SaveSingleAsync(FilePath, machine);
    }

    /// <summary>
    /// Saves machine state synchronously — safe to call during process shutdown.
    /// </summary>
    public void Save(WashingMachineModel machine)
    {
        _storage.SaveSingle(FilePath, machine);
    }

    /// <inheritdoc/>
    public async Task<WashingMachineModel?> LoadAsync()
    {
        return await _storage.LoadSingleAsync<WashingMachineModel>(FilePath);
    }
    /// <summary>
    /// Loads machine state synchronously — used at startup.
    /// </summary>
    public WashingMachineModel? Load()
    {
        string filePath = FilePath;
        if (!File.Exists(filePath)) return null;
        try
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<WashingMachineModel>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
