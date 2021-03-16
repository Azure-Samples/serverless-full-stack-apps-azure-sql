using System;
using System.Collections.Generic;
using System.Text;

namespace GetBusData
{
    public static class Utils
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromPosixTime(double value)
        {
            return UnixEpoch.AddSeconds(value);
        }
    }
}
