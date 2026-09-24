using WashingMachine.Models.Common;
using WashingMachine.Models.Entities;
using WashingMachine.Repository.Abstractions;
using WashingMachine.Services;

namespace WashingMachine.Services;

/// <summary>
/// Manages favourite configurations.
/// Uses LINQ in the repository layer and enforces name-uniqueness here.
/// </summary>
public sealed class FavouriteService : IFavouriteService
{
    private readonly IFavouriteRepository _repository;

    public FavouriteService(IFavouriteRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Favourite>> GetAllAsync() =>
        _repository.GetAllAsync();

    public async Task<ServiceResult> AddAsync(string name, WashSettings settings)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Fail("Favourite name cannot be empty.");

        // LINQ: check duplicate name
        var existing = await _repository.GetByNameAsync(name);
        if (existing is not null)
            return ServiceResult.Fail($"A favourite named '{name}' already exists.");

        var favourite = new Favourite
        {
            Name        = name.Trim(),
            ProgramName = settings.Program.Name,
            Temperature = settings.Temperature,
            SpinSpeed   = settings.SpinSpeed,
            WaterLevel  = settings.WaterLevel,
            PreWash     = settings.PreWash,
            ExtraRinse  = settings.ExtraRinse,
            QuickMode   = settings.QuickMode
        };

        await _repository.AddAsync(favourite);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return ServiceResult.Fail("Favourite not found.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok();
    }

    public Task<Favourite?> GetByIdAsync(Guid id) =>
        _repository.GetByIdAsync(id);
}
