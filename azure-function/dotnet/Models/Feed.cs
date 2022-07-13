namespace GetBusData.Models;

public sealed class Feed
{
    public Header? Header { get; set; }

    [JsonProperty("entity")]
    public List<Entity>? Entities { get; set; }
}
