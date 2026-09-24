using WashingMachine.Models.Common;
using WashingMachine.Models.Entities;

namespace WashingMachine.Services;

/// <summary>Service for managing favourite wash configurations.</summary>
public interface IFavouriteService
{
    Task<List<Favourite>> GetAllAsync();
    Task<ServiceResult> AddAsync(string name, WashSettings settings);
    Task<ServiceResult> DeleteAsync(Guid id);
    Task<Favourite?> GetByIdAsync(Guid id);
}
