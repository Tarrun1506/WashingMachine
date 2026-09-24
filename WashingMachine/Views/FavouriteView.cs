using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides favourite operations.
/// </summary>
public class FavouriteView
{
    public FavouriteMenuOption ShowMenu()
    {
        Console.Clear();

        Console.WriteLine("--- Favourite Menu ---");
        Console.WriteLine("1. Apply Favourite");
        Console.WriteLine("2. Save Current Settings");
        Console.WriteLine("3. Delete Favourite");
        Console.WriteLine("0. Back");

        int.TryParse(Console.ReadLine(), out int option);
        return (FavouriteMenuOption)option;
    }

    public Guid SelectFavourite(List<Favourite> favourites)
    {
        Console.Clear();

        Console.WriteLine("--- Favourites ---");
        Console.WriteLine();
        for (int index = 0; index < favourites.Count; index++)
        {
            Console.WriteLine($"{index + 1}. {favourites[index].Name}");
        }

        Console.WriteLine();
        Console.Write("Select Favourite : ");
        int selectedIndex = int.Parse(Console.ReadLine() ?? "1");
        return favourites[selectedIndex - 1].Id;
    }

    public Favourite CreateFavourite(WashSettings settings)
    {
        Console.Write("Favourite Name : ");
        string name = Console.ReadLine() ?? string.Empty;
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