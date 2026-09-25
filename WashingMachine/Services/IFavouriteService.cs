using WashingMachine.Models;

namespace WashingMachine.Services;

public interface IFavouriteService
{
    List<Favourite> GetAll();
    Favourite? GetById(Guid id);
    void Add(Favourite favourite);
    void Delete(Guid id);
    void Initialize();
    void SaveChanges();
}
