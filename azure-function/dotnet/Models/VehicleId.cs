namespace GetBusData.Models;

public sealed class VehicleId
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("label")]
    public string? Label { get; set; }
}
