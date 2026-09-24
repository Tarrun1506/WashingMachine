using System.Text.Json;
using WashingMachine.Models.Entities;

namespace WashingMachine.Storage;

public class JsonFileStorage : IStorage
{
    private readonly string _baseDirectory;

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonFileStorage(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        Directory.CreateDirectory(_baseDirectory);
    }

    private string GetFilePath(string storeName) =>
        Path.Combine(_baseDirectory, $"{storeName}.json");

    public async Task<List<T>> LoadAllAsync<T>(string storeName)
    {
        var path = GetFilePath(storeName);
        if (!File.Exists(path)) return new List<T>();

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
            var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, _options);
            return data ?? new List<T>();
        }
        catch { return new List<T>(); }
    }

    public async Task SaveAllAsync<T>(string storeName, IEnumerable<T> items)
    {
        var path = GetFilePath(storeName);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, items.ToList(), _options);
    }

    public async Task SaveMachineStateAsync(WashingMachineModel machine)
    {
        var path = GetFilePath("machine_state");
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, machine, _options);
    }

    public async Task<WashingMachineModel?> LoadMachineStateAsync()
    {
        var path = GetFilePath("machine_state");
        if (!File.Exists(path)) return null;

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
            var machine = await JsonSerializer.DeserializeAsync<WashingMachineModel>(stream, _options);
            // Restore polymorphic program type
            if (machine?.Settings?.Program != null)
            {
                machine.Settings.Program = Models.Common.ProgramRegistry.GetByNameOrDefault(machine.Settings.Program.Name);
            }
            return machine;
        }
        catch { return null; }
    }
}
