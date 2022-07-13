namespace GetBusData.Services;

public sealed class BusDataManagerService : IBusDataManagerService
{
    private readonly ILogger<BusDataManagerService> _log;
    private readonly HttpClient _client;
    private readonly BusDataOptions _options;

    public BusDataManagerService(
        ILogger<BusDataManagerService> log,
        HttpClient client,
        IOptions<BusDataOptions> options) =>
        (_log, _client, _options) =
            (log, client, options.Value);

    public async Task ProcessBusDataAsync()
    {
        // Get the real-time bus location feed
        var feed = await GetRealTimeFeedAsync();

        // Get the routes we want to monitor
        var monitoredRoutes = await GetMonitoredRoutesAsync();

        // Filter only the routes we want to monitor
        var buses = feed?.Entities?.FindAll(
            e => monitoredRoutes.Contains(e.Vehicle?.Trip?.RouteId ?? -1));

        _log.LogInformation(
            "Received {FeedEntityCount} buses positions, found {BusesCount} buses in monitored routes",
            feed?.Entities?.Count,
            buses?.Count);

        // Push data to Azure SQL and get the activated geofences
        var activatedGeofences = await ProcessGeoFencesAsync(buses);

        // Send notifications
        foreach (var geoFence in activatedGeofences)
        {
            _log.LogInformation(
                "Vehicle {VehicleId}, route {RouteName }, {GeoFenceStatus} GeoFence {GeoFenceName} at {TimestampUTC} UTC",
                geoFence.VehicleId,
                geoFence.RouteName,
                geoFence.GeoFenceStatus,
                geoFence.GeoFenceName,
                geoFence.TimestampUTC);

            await TriggerLogicAppAsync(geoFence);
        }
    }

    private async Task<Feed> GetRealTimeFeedAsync()
    {
        var response = await _client.GetAsync(_options.RealTimeFeedUrl);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        var feed = JsonConvert.DeserializeObject<Feed>(
            responseJson, DefaultJsonSerializerSettings.Defaults);

        return feed;
    }

    private async Task<List<int>> GetMonitoredRoutesAsync()
    {
        using var conn = new SqlConnection(_options.AzureSQLConnectionString);
        var queryResult = await conn.QuerySingleOrDefaultAsync<string>(
            "web.GetMonitoredRoutes", commandType: CommandType.StoredProcedure);
        var result = JArray.Parse(queryResult);

        return result.Select(e => (int)e["RouteId"]).ToList();
    }

    private async Task<List<ActivatedGeoFence>> ProcessGeoFencesAsync(List<Entity>? buses)
    {
        // Build payload
        var busData = new JArray();
        buses?.ForEach(b =>
        {
            var d = new JObject
            {
                ["DirectionId"] = b.Vehicle?.Trip?.DirectionId,
                ["RouteId"] = b.Vehicle?.Trip?.RouteId,
                ["VehicleId"] = b.Vehicle?.VehicleId?.Id,
                ["Position"] = new JObject
                {
                    ["Latitude"] = b.Vehicle?.Position?.Latitude,
                    ["Longitude"] = b.Vehicle?.Position?.Longitude
                },
                ["TimestampUTC"] = b.Vehicle?.Timestamp.FromPosixTime()
            };

            busData.Add(d);
        });

        if (buses is null or { Count: 0 }) return new List<ActivatedGeoFence>();

        using var conn = new SqlConnection(_options.AzureSQLConnectionString);

        var queryResult = await conn.QuerySingleOrDefaultAsync<string>(
            "web.AddBusData", new { payload = busData.ToString() }, commandType: CommandType.StoredProcedure);
        var result = JsonConvert.DeserializeObject<List<ActivatedGeoFence>>(
            queryResult ?? "[]" /* Use the default JSON serializer settings from Newtonsoft.Json */);
        _log.LogInformation("Found {Count} buses activating a geofence", result.Count);

        return result;
    }

    private async Task TriggerLogicAppAsync(ActivatedGeoFence geoFence)
    {
        var content = JObject.Parse(
            $"{{'value1':'{geoFence.VehicleId}', 'value2': '{geoFence.GeoFenceStatus}'}}");

        _log.LogInformation("Calling Logic App webhook for {VehicleId}", geoFence.VehicleId);

        using var stringContent = new StringContent(
            JsonConvert.SerializeObject(
                content /* Use the default JSON serializer settings from Newtonsoft.Json */),
            Encoding.UTF8,
            "application/json");

        var logicAppResult = await _client.PostAsync(_options.LogicAppUrl, stringContent);

        logicAppResult.EnsureSuccessStatusCode();

        _log.LogInformation(
            "[{VehicleId}/{DirectionId}/{GeoFenceId}] WebHook called successfully",
            geoFence.VehicleId,
            geoFence.DirectionId,
            geoFence.GeoFenceId);
    }
}
