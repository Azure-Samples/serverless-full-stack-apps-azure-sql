namespace GetBusData.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddBusDataManagerServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging()
            .AddOptions()
            .Configure<BusDataOptions>(options =>
            {
                options.AzureSQLConnectionString = configuration[nameof(options.AzureSQLConnectionString)];
                options.RealTimeFeedUrl = configuration[nameof(options.RealTimeFeedUrl)];
                options.LogicAppUrl = configuration[nameof(options.LogicAppUrl)];
            })
            .AddSingleton<IBusDataManagerService, BusDataManagerService>();

        services.AddHttpClient<BusDataManagerService>();

        return services;
    }
}
