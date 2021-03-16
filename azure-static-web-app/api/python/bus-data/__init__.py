import datetime
import logging
import os
import requests
import json
import pyodbc
from datetime import datetime as dt
import azure.functions as func

AZURE_CONN_STRING = str(os.environ["AzureSQLConnectionString"])

def main(req: func.HttpRequest) -> func.HttpResponse:
    result = {}
    
    try:
        rid = int(req.params['rid'])
        gid = int(req.params['gid'])
    except ValueError:
        rid = 0
        gid = 0

    try: 
        conn = pyodbc.connect(AZURE_CONN_STRING)
        
        with conn.cursor() as cursor:
            cursor.execute(f"EXEC [web].[GetMonitoredBusData] ?, ?", rid, gid)

            result = cursor.fetchone()[0]
            
            if result:
                result = json.loads(result)                           
            else:
                result = {}     

            logging.info(result)   
        
    finally:
        cursor.close()

    return func.HttpResponse(json.dumps(result))


