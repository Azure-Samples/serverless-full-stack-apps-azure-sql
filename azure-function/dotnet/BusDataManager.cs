using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GetBusData
{
    public class BusDataManager
    {
        public class ActivatedGeoFence
        {
            public int BusDataId { get; set; }
            public int VehicleId { get; set; }
            public int DirectionId { get; set; }
            public int RouteId { get; set; }
            public string RouteName { get; set; }
            public int GeoFenceId { get; set; }
    		public string GeoFenceName { get; set; }          
		    public string GeoFenceStatus { get; set; }
            public DateTime TimestampUTC { get; set; }
        }

        private readonly string AZURE_CONN_STRING = Environment.GetEnvironmentVariable("AzureSQLConnectionString");
        private readonly string GTFS_RT_FEED = Environment.GetEnvironmentVariable("RealTimeFeedUrl");
        private readonly string LOGIC_APP_URL = Environment.GetEnvironmentVariable("LogicAppUrl");

        private readonly ILogger _log;
        private readonly HttpClient _client = new HttpClient();

        public BusDataManager(ILogger log)
        {
            _log = log;
        }

        public async Task ProcessBusData()
        {
            // Get the real-time bus location feed
            var feed = await GetRealTimeFeed();
            
            // Get the routes we want to monitor
            var monitoredRoutes = await GetMonitoredRoutes();
            
            // Filter only the routes we want to monitor
            var buses = feed.Entities.FindAll(e => monitoredRoutes.Contains(e.Vehicle.Trip.RouteId));

            _log.LogInformation($"Received {feed.Entities.Count()} buses positions, found {buses.Count()} buses in monitored routes");

            // Push data to Azure SQL and get the activated geofences
            var activatedGeofences = await ProcessGeoFences(buses);

            // Send notifications
            foreach(var gf in activatedGeofences)
            {
                _log.LogInformation($"Vehicle {gf.VehicleId}, route {gf.RouteName}, {gf.GeoFenceStatus} GeoFence {gf.GeoFenceName} at {gf.TimestampUTC} UTC");                    
                await TriggerLogicApp(gf);
            }

        }

        private async Task<GTFS.RealTime.Feed> GetRealTimeFeed()
        {
            var response = await _client.GetAsync(GTFS_RT_FEED);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var feed = JsonConvert.DeserializeObject<GTFS.RealTime.Feed>(responseString);

            return feed;
        }

        private async Task<List<int>> GetMonitoredRoutes()
        {
            using var conn = new SqlConnection(AZURE_CONN_STRING);
            var queryResult = await conn.QuerySingleOrDefaultAsync<string>("web.GetMonitoredRoutes", commandType: CommandType.StoredProcedure);
            var result = JArray.Parse(queryResult);
            return  result.Select(e => (int)(e["RouteId"])).ToList();            
        }

        private async Task<List<ActivatedGeoFence>> ProcessGeoFences(List<GTFS.RealTime.Entity> buses)
        {
            // Build payload
            var busData = new JArray();
            buses.ForEach(b =>
            {
                //_log.LogInformation($"{b.Vehicle.VehicleId.Id}: {b.Vehicle.Position.Latitude}, {b.Vehicle.Position.Longitude}");
                var d = new JObject
                {
                    ["DirectionId"] = b.Vehicle.Trip.DirectionId,
                    ["RouteId"] = b.Vehicle.Trip.RouteId,
                    ["VehicleId"] = b.Vehicle.VehicleId.Id,
                    ["Position"] = new JObject
                    {
                        ["Latitude"] = b.Vehicle.Position.Latitude,
                        ["Longitude"] = b.Vehicle.Position.Longitude
                    },
                    ["TimestampUTC"] = Utils.FromPosixTime(b.Vehicle.Timestamp)
                };

                busData.Add(d);
            });
            
            if (buses.Count() == 0) return new List<ActivatedGeoFence>();

            using var conn = new SqlConnection(AZURE_CONN_STRING);
            {
                var queryResult = await conn.QuerySingleOrDefaultAsync<string>("web.AddBusData", new { payload = busData.ToString() },  commandType: CommandType.StoredProcedure);
                var result = JsonConvert.DeserializeObject<List<ActivatedGeoFence>>(queryResult ?? "[]");
                _log.LogInformation($"Found {result.Count()} buses activating a geofence");
                return result;
            }            
        }

        public async Task TriggerLogicApp(ActivatedGeoFence geoFence)
        {
            var content = JObject.Parse("{" + $"'value1':'{geoFence.VehicleId}', 'value2': '{geoFence.GeoFenceStatus}'" + "}");

            _log.LogInformation($"Calling Logic App webhook for {geoFence.VehicleId}");

            var stringContent = new StringContent(JsonConvert.SerializeObject(content, Formatting.None), Encoding.UTF8, "application/json");
            var logicAppResult = await _client.PostAsync(LOGIC_APP_URL, stringContent);

            logicAppResult.EnsureSuccessStatusCode();

            _log.LogInformation($"[{geoFence.VehicleId}/{geoFence.DirectionId}/{geoFence.GeoFenceId}] WebHook called successfully");
        }    

    }
}
