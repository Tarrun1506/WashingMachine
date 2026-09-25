using WashingMachine.Models;

namespace WashingMachine.Services;

public interface IWashHistoryService
{
    List<WashHistory> GetAll();
    void Add(WashHistory history);
    void Initialize();
    void SaveChanges();
}
