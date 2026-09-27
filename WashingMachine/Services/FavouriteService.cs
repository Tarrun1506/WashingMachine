using WashingMachine.Constants;
using WashingMachine.Exceptions;
using WashingMachine.Helpers;
using WashingMachine.Models;
using WashingMachine.Repository;

namespace WashingMachine.Services;

public class FavouriteService : IFavouriteService
{
    private readonly IFavouriteRepository _repository;

    public FavouriteService(IFavouriteRepository repository)
    {
        _repository = repository;
    }

    public List<Favourite> GetAll() => _repository.GetAll();
    public Favourite? GetById(Guid id) => _repository.GetById(id);

    public void Add(Favourite favourite)
    {
        if (favourite == null)
            throw new MachineOperationException(ErrorMessages.FavouriteNotFound);

        if (!SettingsValidator.TryValidateFavouriteName(favourite.Name, out string name, out string nameError))
            throw new MachineOperationException(nameError);

        if (_repository.GetAll().Any(existing =>
                string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new MachineOperationException(ErrorMessages.DuplicateFavouriteName);
        }

        WashSettings settings = new()
        {
            ProgramName = favourite.ProgramName,
            Temperature = favourite.Temperature,
            SpinSpeed = favourite.SpinSpeed,
            WaterLevel = favourite.WaterLevel,
            IsPreWashEnabled = favourite.IsPreWashEnabled,
            IsExtraRinseEnabled = favourite.IsExtraRinseEnabled,
            IsQuickWashEnabled = favourite.IsQuickWashEnabled
        };

        if (!SettingsValidator.TryValidateSettings(settings, out string settingsError))
            throw new MachineOperationException(settingsError);

        favourite.Name = name;
        favourite.ProgramName = settings.ProgramName;
        _repository.Add(favourite);
    }

    public void Delete(Guid id)
    {
        if (_repository.GetById(id) == null)
            throw new MachineOperationException(ErrorMessages.FavouriteNotFound);

        _repository.Delete(id);
    }

    public void Initialize() => _repository.LoadDataAsync().GetAwaiter().GetResult();
    public void SaveChanges() => _repository.SaveData();
}
