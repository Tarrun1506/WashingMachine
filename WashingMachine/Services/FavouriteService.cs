using WashingMachine.Models;
using WashingMachine.Repository;

namespace WashingMachine.Services;

/// <summary>
/// Provides favourite operations.
/// </summary>
public class FavouriteService : IFavouriteService
{
    private readonly IFavouriteRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavouriteService"/> class.
    /// </summary>
    /// <param name="repository">Favourite repository.</param>
    public FavouriteService(IFavouriteRepository repository)
    {
        this._repository = repository;
    }

    /// <inheritdoc/>
    public async Task<List<Favourite>> GetAllAsync()
    {
        return await this._repository.GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(Favourite favourite)
    {
        await this._repository.AddAsync(favourite);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        await this._repository.DeleteAsync(id);
    }

    /// <inheritdoc/>
    public async Task<Favourite?> GetByIdAsync(Guid id)
    {
        return await this._repository.GetByIdAsync(id);
    }
}