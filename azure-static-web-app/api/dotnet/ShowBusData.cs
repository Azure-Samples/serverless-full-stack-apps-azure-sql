using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ShowBusData
{    
    public static class ShowBusDataMain
    {
        private static HttpClient httpClient = new HttpClient();
        private static readonly string AZURE_CONN_STRING = Environment.GetEnvironmentVariable("AzureSQLConnectionString");

        [FunctionName("ShowBusData")]
        public static async Task<IActionResult> ShowBusData([HttpTrigger("get", Route = "bus-data")] HttpRequest req, ILogger log)
        {                              
            int rid = 0, gid = 0;

            Int32.TryParse(req.Query["rid"], out rid);
            Int32.TryParse(req.Query["gid"], out gid);
            
            using(var conn = new SqlConnection(AZURE_CONN_STRING))
            {
                var result = await conn.QuerySingleOrDefaultAsync<string>(
                    "web.GetMonitoredBusData", 
                    new {
                        @RouteId = rid,
                        @GeofenceId = gid
                    }, commandType: CommandType.StoredProcedure);                
                
                return new OkObjectResult(JObject.Parse(result));
            }            
        }
    }
}
