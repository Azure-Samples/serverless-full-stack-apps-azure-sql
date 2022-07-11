namespace GetBusData.Models;

public sealed class Header
{
    [JsonProperty("gtfs_realtime_version")]
    public string? Version { get; set; }

    [JsonProperty("incrementality")]
    public int Incrementality { get; set; }

    [JsonProperty("timestamp")]
    public int Timestamp { get; set; }
}
