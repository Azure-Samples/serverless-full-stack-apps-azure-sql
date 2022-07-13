namespace GetBusData.Models;

public sealed class Vehicle
{
    public Trip? Trip { get; set; }

    [JsonProperty("vehicle")]
    public VehicleId? VehicleId { get; set; }

    public Position? Position { get; set; }

    public int CurrentStopSequence { get; set; }

    public string? StopId { get; set; }

    public string? CurrentStatus { get; set; }

    public string? BlockId { get; set; }

    public long Timestamp { get; set; }
}
