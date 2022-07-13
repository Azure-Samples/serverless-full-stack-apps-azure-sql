namespace GetBusData.Serialization;

internal class DefaultJsonSerializerSettings
{
    private static readonly Lazy<JsonSerializerSettings> s_lazySettings =
        new(() => new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None
        });

    internal static JsonSerializerSettings Defaults => s_lazySettings.Value;
}
