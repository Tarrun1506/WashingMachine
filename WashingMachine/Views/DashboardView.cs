using WashingMachine.Constants;
using WashingMachine.Enums;
using WashingMachine.Models;

namespace WashingMachine.Views;

/// <summary>
/// Provides dashboard operations with real-time progress display.
/// </summary>
public class DashboardView
{
    private DashboardRenderer? _renderer;

    public MenuOption Show(WashingMachineModel machine)
    {
        if (_renderer == null)
        {
            _renderer = new DashboardRenderer(machine);
            Console.Clear();
            _renderer.RenderDashboard();
        }
        else
        {
            _renderer.UpdateMachine(machine);
            _renderer.RenderDashboard();
        }

        // Move to content area and show menu
        _renderer.MoveToContentArea();
        DisplayMenu();
        int choice = ConsoleInput.ReadIntInRange("Enter Choice : ", 0, 7, ErrorMessages.InvalidMenuChoice);
        return (MenuOption)choice;
    }

    public void DisplayOnly(WashingMachineModel machine)
    {
        if (_renderer == null)
        {
            _renderer = new DashboardRenderer(machine);
            Console.Clear();
            _renderer.RenderDashboard();
        }
        else
        {
            _renderer.UpdateMachine(machine);
            _renderer.RenderDashboard();
        }

        // Move to content area and show menu
        _renderer.MoveToContentArea();
        DisplayMenu();
        Console.WriteLine("Press any key to access menu...");
    }

    /// <summary>
    /// Updates only the dashboard section without clearing the content area.
    /// </summary>
    public void UpdateDashboard(WashingMachineModel machine)
    {
        if (_renderer == null)
        {
            _renderer = new DashboardRenderer(machine);
            Console.Clear();
            _renderer.RenderDashboard();
        }
        else
        {
            _renderer.UpdateMachine(machine);
            _renderer.RenderDashboard();
        }
    }

    /// <summary>
    /// Ensures the dashboard renderer is initialized.
    /// </summary>
    public void EnsureRendererInitialized(WashingMachineModel machine)
    {
        if (_renderer == null)
        {
            _renderer = new DashboardRenderer(machine);
            Console.Clear();
            _renderer.RenderDashboard();
        }
    }

    /// <summary>
    /// Gets the dashboard renderer for use by other views.
    /// </summary>
    public DashboardRenderer? GetRenderer()
    {
        return _renderer;
    }

    /// <summary>
    /// Clears the content area (below the dashboard).
    /// </summary>
    public void ClearContentArea()
    {
        _renderer?.ClearContentArea();
    }

    /// <summary>
    /// Moves cursor to the start of the content area.
    /// </summary>
    public void MoveToContentArea()
    {
        _renderer?.MoveToContentArea();
    }

    private void DisplayMenu()
    {
        Console.WriteLine("+----------------------------------------------------------+");
        Console.WriteLine("| 1. Start Washing                                         |");
        Console.WriteLine("| 2. Configure Settings                                    |");
        Console.WriteLine("| 3. Add Clothes                                           |");
        Console.WriteLine("| 4. Remove Clothes                                        |");
        Console.WriteLine("| 5. Favourites                                            |");
        Console.WriteLine("| 6. View History                                          |");
        Console.WriteLine("| 7. Pause Cycle                                           |");
        Console.WriteLine("| 0. Exit                                                  |");
        Console.WriteLine("+----------------------------------------------------------+");
    }

    public int ReadClothesCount(int min, int max)
    {
        return ConsoleInput.ReadIntInRange(
            "Enter Clothes Count : ",
            min,
            max,
            $"Please enter a number between {min} and {max}.");
    }

    public void DisplayMessage(string message)
    {
        ClearContentArea();
        MoveToContentArea();
        Console.WriteLine(message);
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }

    public void DisplayError(string message)
    {
        ClearContentArea();
        MoveToContentArea();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }

    public void DisplaySuccess(string message)
    {
        ClearContentArea();
        MoveToContentArea();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
    }

    public void DisplayMessageWithRedraw(WashingMachineModel machine, string message)
    {
        ClearContentArea();
        MoveToContentArea();
        Console.WriteLine(message);
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
        ForceFullRedraw(machine);
    }

    public void DisplayErrorWithRedraw(WashingMachineModel machine, string message)
    {
        ClearContentArea();
        MoveToContentArea();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
        ForceFullRedraw(machine);
    }

    public void DisplaySuccessWithRedraw(WashingMachineModel machine, string message)
    {
        ClearContentArea();
        MoveToContentArea();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("\nPress Enter To Continue...");
        Console.ReadLine();
        ForceFullRedraw(machine);
    }

    /// <summary>
    /// Shows the main menu with dashboard at top.
    /// </summary>
    public void ShowMenuWithDashboard(WashingMachineModel machine)
    {
        UpdateDashboard(machine);
        MoveToContentArea();
        DisplayMenu();
    }

    /// <summary>
    /// Forces a full screen redraw (clears everything and redraws dashboard).
    /// </summary>
    public void ForceFullRedraw(WashingMachineModel machine)
    {
        if (_renderer == null)
        {
            _renderer = new DashboardRenderer(machine);
        }
        else
        {
            _renderer.UpdateMachine(machine);
        }

        _renderer.ForceFullRedraw();
    }

    private string CenterText(string text, int width)
    {
        if (text.Length >= width) return text.Substring(0, width);
        int padding = (width - text.Length) / 2;
        return new string(' ', padding) + text + new string(' ', width - text.Length - padding);
    }

    private string PadRight(string text, int width)
    {
        return text.Length >= width ? text.Substring(0, width) : text + new string(' ', width - text.Length);
    }
}
