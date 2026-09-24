using WashingMachine.Models.Entities;

namespace WashingMachine.Repository.Abstractions;

/// <summary>
/// CRUD repository for favourite wash configurations.
/// Depends only on abstractions — concrete storage is injected.
/// </summary>
public interface IFavouriteRepository
{
    Task<List<Favourite>> GetAllAsync();
    Task<Favourite?> GetByIdAsync(Guid id);
    Task<Favourite?> GetByNameAsync(string name);
    Task AddAsync(Favourite favourite);
    Task UpdateAsync(Favourite favourite);
    Task DeleteAsync(Guid id);
}
