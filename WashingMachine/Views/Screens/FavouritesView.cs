using System;
using System.Collections.Generic;
using WashingMachine.Models.Entities;

namespace WashingMachine.Views.Screens;

public static class FavouritesView
{
    public enum FavouriteAction { None, UseFavourite, AddFavourite, DeleteFavourite, Back }

    public static (FavouriteAction action, Guid selectedId) Show(List<Favourite> favourites)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("               FAVOURITES               ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();

        if (favourites.Count == 0)
        {
            Console.WriteLine("No favourites saved yet.");
        }
        else
        {
            for (int i = 0; i < favourites.Count; i++)
            {
                var f = favourites[i];
                Console.WriteLine($"{i + 1}. {f.Name} ({f.ProgramName}, {f.Temperature}, {(int)f.SpinSpeed} RPM)");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("1. Use a Favourite");
        Console.WriteLine("2. Save Current as Favourite");
        Console.WriteLine("3. Delete a Favourite");
        Console.WriteLine("0. Back");
        Console.WriteLine();
        Console.Write("Choice: ");

        var input = Console.ReadLine()?.Trim();
        switch (input)
        {
            case "1": return PickFavourite(favourites, FavouriteAction.UseFavourite);
            case "2": return (FavouriteAction.AddFavourite, Guid.Empty);
            case "3": return PickFavourite(favourites, FavouriteAction.DeleteFavourite);
            default:  return (FavouriteAction.Back, Guid.Empty);
        }
    }

    private static (FavouriteAction, Guid) PickFavourite(List<Favourite> favourites, FavouriteAction action)
    {
        if (favourites.Count == 0) return (FavouriteAction.None, Guid.Empty);
        
        Console.Write("\nEnter the number of the favourite: ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= favourites.Count)
        {
            return (action, favourites[idx - 1].Id);
        }
        return (FavouriteAction.None, Guid.Empty);
    }

    public static string? PromptFavouriteName()
    {
        Console.Write("\nEnter a name for this favourite: ");
        return Console.ReadLine()?.Trim();
    }
}
