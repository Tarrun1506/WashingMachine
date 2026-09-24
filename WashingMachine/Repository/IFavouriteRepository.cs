using WashingMachine.Models;

namespace WashingMachine.Repository;

/// <summary>
/// Provides favourite repository operations.
/// </summary>
public interface IFavouriteRepository
{
    /// <summary>
    /// Gets all favourites.
    /// </summary>
    /// <returns>Collection of favourites.</returns>
    Task<List<Favourite>> GetAllAsync();

    /// <summary>
    /// Gets a favourite by identifier.
    /// </summary>
    /// <param name="id">Favourite identifier.</param>
    /// <returns>Matching favourite.</returns>
    Task<Favourite?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a favourite.
    /// </summary>
    /// <param name="favorite">Favourite to add.</param>
    Task AddAsync(Favourite favorite);

    /// <summary>
    /// Deletes a favourite.
    /// </summary>
    /// <param name="id">Favourite identifier.</param>
    Task DeleteAsync(Guid id);
}