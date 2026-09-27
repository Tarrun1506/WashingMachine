using WashingMachine.Models;

namespace WashingMachine.Repository;

public interface IWashHistoryRepository
{
    List<WashHistory> GetAll();
    void Add(WashHistory history);
    Task LoadDataAsync();
    void SaveData();
}
