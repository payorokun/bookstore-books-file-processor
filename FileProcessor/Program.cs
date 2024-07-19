using System;
using System.Reflection;
using Azure.Storage.Blobs;
using FileProcessor.Handlers;
using FileProcessor.Utils;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(assembly);
        services.AddAutoMapper(assembly);

        var storageConnectionString = Environment.GetEnvironmentVariable("TypoCorrectionsStorageConnectionString");
        const string tableName = "TypoCorrections";

        var blobServiceClientConnectionString = Environment.GetEnvironmentVariable("TriggerFileStorage");
        services.AddSingleton(s => new BlobServiceClient(blobServiceClientConnectionString));

        var cache = new TypoCorrectionCache(storageConnectionString, tableName);
        services.AddSingleton(cache);

        services.AddSingleton<BooksParserService>();
        services.AddSingleton(s =>
        {
            var cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
            var cosmosClient = new CosmosClient(cosmosConnectionString);
            return cosmosClient;
        });
    })
    .Build();

host.Run();