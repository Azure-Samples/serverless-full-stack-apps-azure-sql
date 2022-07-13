namespace GetBusData.Models;

public sealed class Header
{
    [JsonProperty("gtfs_realtime_version")]
    public string? Version { get; set; }

    public int Incrementality { get; set; }

    public long Timestamp { get; set; }
}
