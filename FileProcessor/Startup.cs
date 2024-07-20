using System;
using System.Reflection;
using Azure.Storage.Blobs;
using FileProcessor;
using FileProcessor.Handlers;
using FileProcessor.Utils;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;


[assembly: FunctionsStartup(typeof(Startup))]

namespace FileProcessor;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        builder.Services.AddMediatR(assembly);
        builder.Services.AddAutoMapper(assembly);

        //var storageConnectionString = Environment.GetEnvironmentVariable("TypoCorrectionsStorageConnectionString");
        //const string tableName = "TypoCorrections";

        //var cache = new TypoCorrectionCache(storageConnectionString, tableName);
        //builder.Services.AddSingleton(cache);

        //var blobServiceClientConnectionString = Environment.GetEnvironmentVariable("TriggerFileStorage");
        //builder.Services.AddSingleton(_ => new BlobServiceClient(blobServiceClientConnectionString));

        

        builder.Services.AddSingleton<BooksParserService>();
    }
}