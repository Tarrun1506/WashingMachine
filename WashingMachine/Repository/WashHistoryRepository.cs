using WashingMachine.Constants;
using WashingMachine.Models;
using WashingMachine.Storage;

namespace WashingMachine.Repository;

public class WashHistoryRepository : IWashHistoryRepository
{
    private readonly IStorage _storage;
    private List<WashHistory> _history = new();

    public WashHistoryRepository(IStorage storage)
    {
        _storage = storage;
    }

    private string FilePath => Configurables.WashHistoryFilePath;

    public async Task LoadDataAsync()
    {
        _history = await _storage.LoadAsync<WashHistory>(FilePath);
    }

    public async Task SaveDataAsync()
    {
        await _storage.SaveAsync(FilePath, _history);
    }

    public void SaveData()
    {
        _storage.Save(FilePath, _history);
    }

    public List<WashHistory> GetAll() => _history.ToList();

    public void Add(WashHistory history)
    {
        _history.Add(history);
    }
}