using WashingMachine.Constants;
using WashingMachine.Models;
using WashingMachine.Storage;

namespace WashingMachine.Repository;

public class FavouriteRepository : IFavouriteRepository
{
    private readonly IStorage _storage;
    private List<Favourite> _favourites = new();
    private readonly string _filePath = Configurables.FavouriteFilePath;

    public FavouriteRepository(IStorage storage)
    {
        _storage = storage;
    }

    public async Task LoadDataAsync()
    {
        _favourites = await _storage.LoadAsync<Favourite>(_filePath);
    }

    public async Task SaveDataAsync()
    {
        await _storage.SaveAsync(_filePath, _favourites);
    }

    public void SaveData()
    {
        _storage.Save(_filePath, _favourites);
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
