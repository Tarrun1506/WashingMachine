namespace WashingMachine.Constants;

/// <summary>
/// Defines configurable values used throughout the application.
/// </summary>
public static class Configurables
{
    /// <summary>
    /// Path used to store favourite configurations.
    /// </summary>
    public const string FavouriteFilePath = "Data/Favourites.json";

    /// <summary>
    /// Path used to store wash history.
    /// </summary>
    public const string WashHistoryFilePath = "Data/WashHistory.json";

    /// <summary>
    /// Path used to store machine state.
    /// </summary>
    public const string MachineStateFilePath = "Data/MachineState.json";

    /// <summary>
    /// Maximum washing machine capacity.
    /// </summary>
    public const int MaximumCapacity = 10;

    /// <summary>
    /// Refresh interval for progress updates.
    /// </summary>
    public const int ProgressUpdateIntervalInSeconds = 1;

    /// <summary>
    /// Path used to store application logs.
    /// </summary>
    public const string LogFilePath = "Data/app.log";
}