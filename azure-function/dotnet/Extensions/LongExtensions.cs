namespace GetBusData.Extensions;

internal static class LongExtensions
{
    internal static DateTime FromPosixTime(this long timestamp) =>
        DateTime.UnixEpoch.AddSeconds(timestamp);
}
