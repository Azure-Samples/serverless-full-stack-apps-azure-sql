import datetime
import logging
import os
import requests
import json
import pyodbc
from datetime import datetime as dt
import azure.functions as func

AZURE_CONN_STRING = str(os.environ["AzureSQLConnectionString"])
GTFS_RT_FEED = str(os.environ["RealTimeFeedUrl"])
LOGIC_APP_URL = str(os.environ["LogicAppUrl"])

def main(GetBusData: func.TimerRequest) -> func.HttpResponse:
    ## Get the routes we want to monitor
    routes = GetMonitoredRoutes()
    
    ## Get the real-time bus location feed
    feed = GetRealTimeFeed()
    
    ## Filter only the routes we want to monitor
    buses = [f for f in feed if int(f["RouteId"]) in routes]

    logging.info('Received {0} buses positions, found {1} buses in monitored routes'.format(len(feed), len(buses)))

    ## Push data to Azure SQL and get the activated geofences
    activatedGeofences = ProcessGeoFences(buses)

    ## Send notifications 
    for gf in activatedGeofences:
        logging.info('Vehicle %i, route %s, %sing GeoFence %s at %s UTC', gf["VehicleId"], gf["RouteId"], gf["GeoFenceStatus"], gf["GeoFence"], gf["TimestampUTC"])
        TriggerLogicApp(gf)

def GetRealTimeFeed():
    response = requests.get(GTFS_RT_FEED)
    entities = json.loads(response.text)['entity']
    busData = []
    for entity in entities:
        v = entity['vehicle']
        busDetails = {
            "DirectionId": v['trip']['direction_id'],
            "RouteId": v['trip']['route_id'],
            "VehicleId": v['vehicle']['id'],
            "Position": {
                "Latitude": v['position']['latitude'],
                "Longitude": v['position']['longitude']
            },
            "TimestampUTC": dt.utcfromtimestamp(v['timestamp']).isoformat(sep=' ')
        }
        busData.append(busDetails)    
    return busData

def GetMonitoredRoutes():
    result = executeQueryJSON('web.GetMonitoredRoutes')    
    return [r["RouteId"] for r in result]

def ProcessGeoFences(payload):    
    result = {}
    if payload:
        result = executeQueryJSON('web.AddBusData', payload)
        logging.info('Found %i buses activating a geofence',len(result))
    return result

def TriggerLogicApp(geoFence):
    content = {
        "value1": str(geoFence["VehicleId"]), 
        "value2": str(geoFence["GeoFenceStatus"])
    }

    logging.info("Calling Logic App webhook for {0}".format(geoFence["VehicleId"]))

    params = { 
        "Content-type": "application/json" 
    }

    response = requests.post(LOGIC_APP_URL, json=content, headers=params)
    if response.status_code != 202:
        logging.info('Error calling Logic App: {0}'.format(response.status_code))        
    else:
        logging.info("[%i/%i/%i] WebHook called successfully", geoFence["VehicleId"], geoFence["DirectionId"], geoFence["GeoFenceId"])

def executeQueryJSON(procedure, payload=None):
    result = {}
    try: 
        conn = pyodbc.connect(AZURE_CONN_STRING)
        
        with conn.cursor() as cursor:
            if payload:            
                cursor.execute(f"EXEC {procedure} ?", json.dumps(payload))
            else:
                cursor.execute(f"EXEC {procedure}")

            result = cursor.fetchone()[0]
            
            if result:
                result = json.loads(result)                           
            else:
                result = {}        
        
    finally:
        pass

    return result
