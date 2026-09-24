namespace WashingMachine.Controllers;

/// <summary>
/// Defines washing machine controller operations.
/// </summary>
public interface IWashingMachineController
{
    /// <summary>
    /// Starts the application.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    Task StartAsync();
}