using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;

namespace GetBusData.GTFS.RealTime
{
    public class Feed
    {
        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("entity")]
        public List<Entity> Entities { get; set; }
    }

    public class Header
    {
        [JsonProperty("gtfs_realtime_version")]
        public string Version { get; set; }

        [JsonProperty("incrementality")]
        public int Incrementality { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }
    }

    public class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("vehicle")]
        public Vehicle Vehicle { get; set; }
    }

    public class Vehicle
    {
        [JsonProperty("trip")]
        public Trip Trip { get; set; }

        [JsonProperty("vehicle")]
        public VehicleId VehicleId { get; set; }

        [JsonProperty("position")]
        public Position Position { get; set; }

        [JsonProperty("current_stop_sequence")]
        public int CurrentStopSequence { get; set; }

        [JsonProperty("stop_id")]
        public string StopId { get; set; }

        [JsonProperty("current_status")]
        public string CurrentStatus { get; set; }

        [JsonProperty("block_id")]
        public string BlockId { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }

    public class Trip
    {
        [JsonProperty("trip_id")]
        public string TripId { get; set; }

        [JsonProperty("direction_id")]
        public int DirectionId { get; set; }

        [JsonProperty("route_id")]
        public int RouteId { get; set; }

        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        [JsonProperty("schedule_relationship")]
        public string ScheduleType { get; set; }
    }

    public class VehicleId
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("label")]
        public string Label{ get; set; }
    }

    public class Position
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}
