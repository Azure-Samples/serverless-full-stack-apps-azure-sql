import logging
import os
from typing import Any

import azure.functions as func
import pyodbc
import json
import requests

from datetime import datetime
from .bus_data_process import (
    get_bus_data_from_feed,
    get_geo_fences,
    get_monitored_format,
    get_monitored_routes,
    get_route_id,
    trigger_logic_app,
)

AZURE_CONN_STRING: str = os.environ["AzureSQLConnectionString"]
GTFS_REAL_TIME_FEED: str = os.environ["RealTimeFeedUrl"]
LOGIC_APP_URL: str = os.environ.get("LogicAppUrl", "")


def main(GetBusData: func.TimerRequest) -> None:
    """Retrieve the routes we want to monitor from the SQL Database"""
    conn: str = pyodbc.connect(AZURE_CONN_STRING)
    monitored_routes: list[int] = get_monitored_routes(conn)
    logging.info(f"{len(monitored_routes)} routes to check against")
    entities = get_bus_data_from_feed(GTFS_REAL_TIME_FEED)['entity']
    logging.info(monitored_routes)

    # reformat the bus_feed to match the format of the monitored_routes
    monitored_buses = [get_monitored_format(bus['vehicle']) for bus in entities if get_route_id(bus) in monitored_routes]
    logging.info(f"{len(entities)} buses found. {len(monitored_buses)} buses monitored.")
    
    if not monitored_buses:
        logging.info("No Monitored Bus Routes Detected")
        return

    geo_fences = get_geo_fences(conn, monitored_buses) or list()
    logging.info(f"{geo_fences=}")

    ## Send notifications. 
    for fence in geo_fences:
        logging.info(f"Vehicle {fence['VehicleId']}, route {fence['RouteId']}, status: {fence['GeoFenceStatus']} at {fence['TimestampUTC']} UTC")
        trigger_logic_app(fence, LOGIC_APP_URL)