using WashingMachine.Controllers;
using WashingMachine.Exceptions;
using WashingMachine.Helpers;
using WashingMachine.Repository;
using WashingMachine.Services;
using WashingMachine.Storage;

namespace WashingMachine;

/// <summary>
/// Entry point of the application.
/// </summary>
internal class Program
{
    /// <summary>
    /// Starts the application.
    /// </summary>
    public static async Task Main(string[] args)
    {
        Console.Title = "WashMate - Smart Washing Machine";
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Logger logger = new();
        logger.Log("App", "======= Application Started =======");

        try
        {
            IStorage storage = new JsonStorage();

            IFavouriteRepository    favouriteRepository    = new FavouriteRepository(storage);
            IWashHistoryRepository  historyRepository      = new WashHistoryRepository(storage);
            IMachineStateRepository machineStateRepository = new MachineStateRepository(storage);

            IWashHistoryService    historyService        = new WashHistoryService(historyRepository);
            IFavouriteService      favouriteService      = new FavouriteService(favouriteRepository);
            IWashingMachineService washingMachineService = new WashingMachineService(historyService, logger);

            IWashingMachineController controller = new WashingMachineController(
                washingMachineService,
                favouriteService,
                historyService,
                machineStateRepository,
                logger);

            await controller.StartAsync();
        }
        catch (StorageException ex)
        {
            logger.LogError("Storage", ex.Message);
            Console.WriteLine($"Storage Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError("Fatal", ex.Message);
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
        finally
        {
            logger.Log("App", "======= Application Stopped =======");
        }
    }
}