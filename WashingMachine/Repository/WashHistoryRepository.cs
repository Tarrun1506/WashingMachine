using WashingMachine.Constants;
using WashingMachine.Models;
using WashingMachine.Storage;

namespace WashingMachine.Repository;

public class WashHistoryRepository : IWashHistoryRepository
{
    private readonly IStorage _storage;
    private List<WashHistory> _history = new();
    private readonly string _filePath = Configurables.WashHistoryFilePath;

    public WashHistoryRepository(IStorage storage)
    {
        _storage = storage;
    }

    public async Task LoadDataAsync()
    {
        _history = await _storage.LoadAsync<WashHistory>(_filePath);
    }

    public async Task SaveDataAsync()
    {
        await _storage.SaveAsync(_filePath, _history);
    }

    public void SaveData()
    {
        _storage.Save(_filePath, _history);
    }

    public List<WashHistory> GetAll() => _history.ToList();

    public void Add(WashHistory history)
    {
        _history.Add(history);
    }
}