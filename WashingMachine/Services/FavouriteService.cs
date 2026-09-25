using WashingMachine.Models;
using WashingMachine.Repository;

namespace WashingMachine.Services;

public class FavouriteService : IFavouriteService
{
    private readonly IFavouriteRepository _repository;

    public FavouriteService(IFavouriteRepository repository)
    {
        _repository = repository;
    }

    public List<Favourite> GetAll() => _repository.GetAll();
    public Favourite? GetById(Guid id) => _repository.GetById(id);
    public void Add(Favourite favourite) => _repository.Add(favourite);
    public void Delete(Guid id) => _repository.Delete(id);
    
    public void Initialize() => _repository.LoadDataAsync().GetAwaiter().GetResult();
    public void SaveChanges() => _repository.SaveData();
}
