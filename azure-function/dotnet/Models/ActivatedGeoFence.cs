namespace GetBusData.Models;

public sealed class ActivatedGeoFence
{
    public int BusDataId { get; set; }
    public int VehicleId { get; set; }
    public int DirectionId { get; set; }
    public int RouteId { get; set; }
    public string? RouteName { get; set; }
    public int GeoFenceId { get; set; }
    public string? GeoFenceName { get; set; }
    public string? GeoFenceStatus { get; set; }
    public DateTime TimestampUTC { get; set; }
}
