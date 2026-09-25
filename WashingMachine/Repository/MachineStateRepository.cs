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
    private readonly string _filePath = Configurables.MachineStateFilePath;

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public MachineStateRepository(IStorage storage)
    {
        _storage = storage;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(WashingMachineModel machine)
    {
        await _storage.SaveSingleAsync(_filePath, machine);
    }

    /// <summary>
    /// Saves machine state synchronously — safe to call during process shutdown.
    /// </summary>
    public void Save(WashingMachineModel machine)
    {
        _storage.SaveSingle(_filePath, machine);
    }

    /// <inheritdoc/>
    public async Task<WashingMachineModel?> LoadAsync()
    {
        return await _storage.LoadSingleAsync<WashingMachineModel>(_filePath);
    }

    /// <summary>
    /// Loads machine state synchronously — used at startup.
    /// </summary>
    public WashingMachineModel? Load()
    {
        if (!File.Exists(_filePath)) return null;
        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<WashingMachineModel>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }
}