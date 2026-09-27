using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides pause screen operations.
/// </summary>
public class PauseView
{
    private readonly DashboardRenderer? _renderer;

    public PauseView(DashboardRenderer? renderer = null)
    {
        _renderer = renderer;
    }

    public PauseMenuOption Show(WashingMachineModel? machine = null)
    {
        _renderer?.ClearContentArea();
        _renderer?.MoveToContentArea();

        Console.WriteLine();
        Console.WriteLine("--- Cycle Paused ---");
        if (machine != null)
        {
            Console.WriteLine($"Current Clothes: {machine.ClothesCount}/{WashingMachineModel.MaximumCapacity}");
        }
        Console.WriteLine("1. Resume");
        Console.WriteLine("2. Cancel");
        Console.WriteLine("3. Add Clothes");
        Console.WriteLine("4. Remove Clothes");

        int option = ConsoleInput.ReadIntInRange("\nChoice : ", 1, 4, ErrorMessages.InvalidMenuChoice);
        return (PauseMenuOption)option;
    }
}
