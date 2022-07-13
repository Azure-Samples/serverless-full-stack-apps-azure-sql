[assembly: FunctionsStartup(typeof(Startup))]
namespace GetBusData;

internal class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;
        builder.Services
            .AddBusDataManagerServices(configuration)
            .BuildServiceProvider(true);
    }
}
