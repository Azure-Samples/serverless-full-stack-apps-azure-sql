namespace GetBusData.Models;

public sealed class Entity
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("vehicle")]
    public Vehicle? Vehicle { get; set; }
}
