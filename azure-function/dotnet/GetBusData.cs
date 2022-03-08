using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace GetBusData
{    
    public static class GetBusData
    {
        [FunctionName("GetBusData")]
        public async static Task Run([TimerTrigger("*/15 * * * * *")]TimerInfo myTimer, ILogger log)
        {            
            var m = new BusDataManager(log);
            await m.ProcessBusData();
        }

    }
}
