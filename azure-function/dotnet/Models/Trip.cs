namespace GetBusData.Models;

public sealed class Trip
{
    [JsonProperty("trip_id")]
    public string? TripId { get; set; }

    [JsonProperty("direction_id")]
    public int DirectionId { get; set; }

    [JsonProperty("route_id")]
    public int RouteId { get; set; }

    [JsonProperty("start_date")]
    public string? StartDate { get; set; }

    [JsonProperty("schedule_relationship")]
    public string? ScheduleType { get; set; }
}
