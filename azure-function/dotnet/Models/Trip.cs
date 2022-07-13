namespace GetBusData.Models;

public sealed class Trip
{
    public string? TripId { get; set; }

    public int DirectionId { get; set; }

    public int RouteId { get; set; }

    public string? StartDate { get; set; }

    [JsonProperty("schedule_relationship")]
    public string? ScheduleType { get; set; }
}
