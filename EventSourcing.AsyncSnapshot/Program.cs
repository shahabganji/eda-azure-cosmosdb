using EventSourcingCosmosDB.Serializers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton(sp =>
        {
            var cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("DatabaseConnection"),
                new CosmosClientOptions
                {
                    ApplicationName = "EventSourcingCosmosDB - Demo",
                    EnableContentResponseOnWrite = false,

                    Serializer = new CosmosSystemTextJsonSerializer(),

                    ApplicationPreferredRegions = ["West Europe"],
                });
            var database = cosmosClient.GetDatabase("Demo");
            var container = database.GetContainer("Snapshots");

            return container;
        });
    })
    .UseSerilog()
    .Build();

host.Run();
