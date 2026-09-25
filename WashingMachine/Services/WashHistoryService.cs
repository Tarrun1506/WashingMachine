using WashingMachine.Models;
using WashingMachine.Repository;

namespace WashingMachine.Services;

public class WashHistoryService : IWashHistoryService
{
    private readonly IWashHistoryRepository _repository;

    public WashHistoryService(IWashHistoryRepository repository)
    {
        _repository = repository;
    }

    public List<WashHistory> GetAll() => _repository.GetAll();
    public void Add(WashHistory history) => _repository.Add(history);
    
    public void Initialize() => _repository.LoadDataAsync().GetAwaiter().GetResult();
    public void SaveChanges() => _repository.SaveData();
}
