using WashingMachine.Models;

namespace WashingMachine.Repository;

public interface IFavouriteRepository
{
    List<Favourite> GetAll();
    Favourite? GetById(Guid id);
    void Add(Favourite favorite);
    void Delete(Guid id);
    Task LoadDataAsync();
    Task SaveDataAsync();
    void SaveData();
}
