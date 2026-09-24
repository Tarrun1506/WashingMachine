using WashingMachine.Models;

namespace WashingMachine.Services;

/// <summary>
/// Provides favourite operations.
/// </summary>
public interface IFavouriteService
{
    Task<List<Favourite>> GetAllAsync();

    Task AddAsync(Favourite favourite);

    Task DeleteAsync(Guid id);

    Task<Favourite?> GetByIdAsync(Guid id);
}