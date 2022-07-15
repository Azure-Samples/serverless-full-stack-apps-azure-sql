const fetch = require('node-fetch');
const sql = require('mssql');

const AZURE_CONN_STRING = process.env['AzureSQLConnectionString'];
const GTFS_RT_FEED = process.env['RealTimeFeedUrl'];
const LOGIC_APP_URL = process.env['LogicAppUrl'];

module.exports = async function (context, myTimer) {
  // Get the routes we want to monitor
  const routes = await GetMonitoredRoutes();

  // Get the real-time bus location feed
  const feed = await GetRealTimeFeed();

  // Filter only the routes we want to monitor
  var buses = feed.entity.filter((e) =>
    routes.includes(parseInt(e.vehicle.trip.route_id))
  );

  context.log(
    `Received ${feed.entity.length} buses positions, found ${buses.length} buses in monitored routes`
  );

  // Push data to Azure SQL and get the activated geofences
  const activatedGeofences = await ProcessGeoFences(context, buses);

  // Send notifications
  // (using 'map' instead of 'forEach' to make sure all calls are awaited: https://advancedweb.hu/how-to-use-async-functions-with-array-foreach-in-javascript/)
  await Promise.all(
    activatedGeofences.map(async (gf) => {
      context.log(
        `Vehicle ${gf.VehicleId}, route ${gf.RouteId}, ${gf.GeoFenceStatus} Geofence ${gf.GeoFence} at ${gf.TimestampUTC} UTC.`
      );
      await TriggerLogicApp(context, gf);
    })
  );
};

async function GetMonitoredRoutes() {
  const pool = await sql.connect(AZURE_CONN_STRING);
  const queryResult = await pool.request().execute('web.GetMonitoredRoutes');
  var monitoredRutes = JSON.parse(queryResult.recordset[0]['MonitoredRoutes']);
  if (monitoredRutes == null) return [];
  return monitoredRutes.map((i) => i.RouteId);
}

async function GetRealTimeFeed() {
  const response = await fetch(GTFS_RT_FEED);
  const feed = await response.json();

  return feed;
}

async function ProcessGeoFences(context, buses) {
  const busData = buses.map((e) => {
    return {
      DirectionId: e.vehicle.trip.direction_id,
      RouteId: e.vehicle.trip.route_id,
      VehicleId: e.vehicle.vehicle.id,
      Position: {
        Latitude: e.vehicle.position.latitude,
        Longitude: e.vehicle.position.longitude,
      },
      TimestampUTC: new Date(e.vehicle.timestamp * 1000),
    };
  });

  const pool = await sql.connect(AZURE_CONN_STRING);
  const queryResult = await pool
    .request()
    .input('payload', sql.NVarChar, JSON.stringify(busData))
    .execute('web.AddBusData');

  var geoFences = JSON.parse(queryResult.recordset[0]['ActivatedGeoFences']);
  if (geoFences == null) return [];

  context.log(`Found ${geoFences.length} buses activating a geofence`);

  return geoFences;
}

async function TriggerLogicApp(context, geofence) {
  const content = {
    value1: geofence.VehicleId,
    value2: geofence.GeoFenceStatus,
  };

  context.log(`Calling Logic App webhook for ${geofence.VehicleId}`);

  try {
    const response = await fetch(LOGIC_APP_URL, {
      method: 'post',
      body: JSON.stringify(content),
      headers: { 'Content-Type': 'application/json' },
    });
    if (response.ok)
      context.log(
        `[${geofence.VehicleId}/${geofence.DirectionId}/${geofence.GeoFenceId}] WebHook called successfully`
      );
    else
      context.log(
        `Error calling Logic App. HTTP Response is: ${response.status}`
      );
  } catch (err) {
    context.log(`Error calling Logic App. Error is: ${err}`);
  }
}
