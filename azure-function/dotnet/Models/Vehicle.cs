namespace GetBusData.Models;

public sealed class Vehicle
{
    [JsonProperty("trip")]
    public Trip? Trip { get; set; }

    [JsonProperty("vehicle")]
    public VehicleId? VehicleId { get; set; }

    [JsonProperty("position")]
    public Position? Position { get; set; }

    [JsonProperty("current_stop_sequence")]
    public int CurrentStopSequence { get; set; }

    [JsonProperty("stop_id")]
    public string? StopId { get; set; }

    [JsonProperty("current_status")]
    public string? CurrentStatus { get; set; }

    [JsonProperty("block_id")]
    public string? BlockId { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
}
