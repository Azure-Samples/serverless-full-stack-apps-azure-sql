namespace GetBusData.Options;

public sealed class BusDataOptions
{
    /// <summary>
    /// Bound to an environment variable named "AzureSQLConnectionString".
    /// </summary>
    public string AzureSQLConnectionString { get; set; } = null!;

    /// <summary>
    /// Bound to an environment variable named "RealTimeFeedUrl".
    /// </summary>
    public string RealTimeFeedUrl { get; set; } = null!;

    /// <summary>
    /// Bound to an environment variable named "LogicAppUrl".
    /// </summary>
    public string LogicAppUrl { get; set; } = null!;
}
