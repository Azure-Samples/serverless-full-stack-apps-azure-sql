const sql = require('mssql')

const AZURE_CONN_STRING = process.env["AzureSQLConnectionString"];

module.exports = async function (context, req) {    
    const pool = await sql.connect(AZURE_CONN_STRING);    

    const busData = await pool.request()
        .input("routeId", sql.Int, parseInt(req.query.rid))
        .input("geofenceId", sql.Int, parseInt(req.query.gid))
        .execute("web.GetMonitoredBusData");        

    context.res = {        
        body: JSON.parse(busData.recordset[0]["locationData"])
    };
}

