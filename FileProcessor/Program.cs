using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using FileProcessor.Handlers;
using Microsoft.Extensions.Azure;
using Microsoft.Azure.Cosmos;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(assembly);
        services.AddSingleton<BooksParserService>();

        services.AddAzureClients(clientBuilder =>
        {
            var blobServiceClientConnectionString = Environment.GetEnvironmentVariable("TriggerFileStorage");
            clientBuilder.AddBlobServiceClient(blobServiceClientConnectionString);
        });

        var cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
        var cosmosClient = new CosmosClient(cosmosConnectionString);
        services.AddSingleton(cosmosClient);
    })
    .Build();
host.Run();
