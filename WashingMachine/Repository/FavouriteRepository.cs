using WashingMachine.Models.Entities;
using WashingMachine.Repository.Abstractions;
using WashingMachine.Storage;

namespace WashingMachine.Repository.Implementations;

/// <summary>
/// Favourite repository implementation.
/// Delegates all file I/O to IStorage — demonstrates Dependency Inversion and
/// the Repository Pattern: callers work against an interface, not concrete storage.
/// LINQ is used for in-memory filtering and lookup.
/// </summary>
public sealed class FavouriteRepository : IFavouriteRepository
{
    private const string StoreName = "favourites";
    private readonly IStorage _storage;

    public FavouriteRepository(IStorage storage)
    {
        _storage = storage;
    }

    public Task<List<Favourite>> GetAllAsync() =>
        _storage.LoadAllAsync<Favourite>(StoreName);

    public async Task<Favourite?> GetByIdAsync(Guid id)
    {
        var all = await GetAllAsync();
        // LINQ: FirstOrDefault to find by id
        return all.FirstOrDefault(f => f.Id == id);
    }

    public async Task<Favourite?> GetByNameAsync(string name)
    {
        var all = await GetAllAsync();
        // LINQ: case-insensitive name search
        return all.FirstOrDefault(f =>
            f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(Favourite favourite)
    {
        var all = await GetAllAsync();
        all.Add(favourite);
        await _storage.SaveAllAsync(StoreName, all);
    }

    public async Task UpdateAsync(Favourite favourite)
    {
        var all = await GetAllAsync();
        var index = all.FindIndex(f => f.Id == favourite.Id);
        if (index >= 0) all[index] = favourite;
        await _storage.SaveAllAsync(StoreName, all);
    }

    public async Task DeleteAsync(Guid id)
    {
        var all = await GetAllAsync();
        // LINQ: RemoveAll to delete by predicate
        all.RemoveAll(f => f.Id == id);
        await _storage.SaveAllAsync(StoreName, all);
    }
}
