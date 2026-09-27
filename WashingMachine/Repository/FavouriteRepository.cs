using WashingMachine.Constants;
using WashingMachine.Models;
using WashingMachine.Storage;

namespace WashingMachine.Repository;

public class FavouriteRepository : IFavouriteRepository
{
    private readonly IStorage _storage;
    private List<Favourite> _favourites = new();

    public FavouriteRepository(IStorage storage)
    {
        _storage = storage;
    }

    private string FilePath => Configurables.FavouriteFilePath;

    public async Task LoadDataAsync()
    {
        _favourites = await _storage.LoadAsync<Favourite>(FilePath);
    }

    public async Task SaveDataAsync()
    {
        await _storage.SaveAsync(FilePath, _favourites);
    }

    public void SaveData()
    {
        _storage.Save(FilePath, _favourites);
    }

    public List<Favourite> GetAll() => _favourites.ToList();

    public Favourite? GetById(Guid id) => _favourites.FirstOrDefault(f => f.Id == id);

    public void Add(Favourite favorite)
    {
        _favourites.Add(favorite);
    }

    public void Delete(Guid id)
    {
        var item = _favourites.FirstOrDefault(f => f.Id == id);
        if (item != null)
        {
            _favourites.Remove(item);
        }
    }
}
