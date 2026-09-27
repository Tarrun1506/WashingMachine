namespace WashingMachine.Constants;

/// <summary>
/// Defines configurable values used throughout the application.
/// </summary>
public static class Configurables
{
    /// <summary>
    /// Gets the base directory for data storage.
    /// </summary>
    public static string BaseDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

    /// <summary>
    /// Path used to store favourite configurations.
    /// </summary>
    public static string FavouriteFilePath => Path.Combine(BaseDirectory, "Favourites.json");

    /// <summary>
    /// Path used to store wash history.
    /// </summary>
    public static string WashHistoryFilePath => Path.Combine(BaseDirectory, "WashHistory.json");

    /// <summary>
    /// Path used to store machine state.
    /// </summary>
    public static string MachineStateFilePath => Path.Combine(BaseDirectory, "MachineState.json");

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
    public static string LogFilePath => Path.Combine(BaseDirectory, "app.log");
}