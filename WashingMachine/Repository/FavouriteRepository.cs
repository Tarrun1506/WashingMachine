using WashingMachine.Constants;
using WashingMachine.Models;
using WashingMachine.Storage;

namespace WashingMachine.Repository;

/// <summary>
/// Provides favourite repository operations.
/// </summary>
public class FavouriteRepository : IFavouriteRepository
{
    private readonly IStorage _storage;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavouriteRepository"/> class.
    /// </summary>
    /// <param name="storage">Storage implementation.</param>
    public FavouriteRepository(IStorage storage)
    {
        this._storage = storage;
    }

    /// <inheritdoc/>
    public async Task<List<Favourite>> GetAllAsync()
    {
        return await this._storage.LoadAsync<Favourite>(Configurables.FavouriteFilePath);
    }

    /// <inheritdoc/>
    public async Task<Favourite?> GetByIdAsync(Guid id)
    {
        List<Favourite> favourites = await this.GetAllAsync();
        return favourites.FirstOrDefault(favourite => favourite.Id == id);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Favourite favourite)
    {
        List<Favourite> favourites = await this.GetAllAsync();
        favourites.Add(favourite);
        await this._storage.SaveAsync(Configurables.FavouriteFilePath, favourites);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        List<Favourite> favourites = await this.GetAllAsync();
        Favourite? favourite = favourites.FirstOrDefault(item => item.Id == id);
        if (favourite != null)
        {
            favourites.Remove(favourite);
        }

        await this._storage.SaveAsync(Configurables.FavouriteFilePath, favourites);
    }
}
