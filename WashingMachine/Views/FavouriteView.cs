using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Helpers;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides favourite operations.
/// </summary>
public class FavouriteView
{
    public FavouriteMenuOption ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("--- Favourite Menu ---");
        Console.WriteLine("1. Apply Favourite");
        Console.WriteLine("2. Save Current Settings");
        Console.WriteLine("3. Delete Favourite");
        Console.WriteLine("0. Back");
        Console.WriteLine();

        int option = ConsoleInput.ReadIntInRange("Choice : ", 0, 3, ErrorMessages.InvalidMenuChoice);
        return (FavouriteMenuOption)option;
    }

    public Guid SelectFavourite(List<Favourite> favourites)
    {
        Console.WriteLine();
        Console.WriteLine("--- Favourites ---");
        Console.WriteLine();
        for (int index = 0; index < favourites.Count; index++)
        {
            Console.WriteLine($"{index + 1}. {favourites[index].Name}");
        }

        Console.WriteLine();
        int selectedIndex = ConsoleInput.ReadIntInRange(
            "Select Favourite : ",
            1,
            favourites.Count,
            ErrorMessages.InvalidFavouriteSelection);

        return favourites[selectedIndex - 1].Id;
    }

    public Favourite CreateFavourite(WashSettings settings)
    {
        Console.WriteLine();
        string name = ConsoleInput.ReadRequiredText(
            "Favourite Name : ",
            input =>
            {
                if (!SettingsValidator.TryValidateFavouriteName(input, out _, out string error))
                    return error;
                return null;
            });

        return new Favourite
        {
            Name = name,
            ProgramName = settings.ProgramName,
            Temperature = settings.Temperature,
            SpinSpeed = settings.SpinSpeed,
            WaterLevel = settings.WaterLevel,
            IsPreWashEnabled = settings.IsPreWashEnabled,
            IsExtraRinseEnabled = settings.IsExtraRinseEnabled,
            IsQuickWashEnabled = settings.IsQuickWashEnabled,
        };
    }
}
