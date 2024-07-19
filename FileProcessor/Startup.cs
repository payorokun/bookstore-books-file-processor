﻿using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FileProcessor;
using FileProcessor.Handlers;
using FileProcessor.Utils;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


[assembly: FunctionsStartup(typeof(Startup))]

namespace FileProcessor;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        builder.Services.AddMediatR(assembly);
        builder.Services.AddAutoMapper(assembly);

        var storageConnectionString = Environment.GetEnvironmentVariable("TypoCorrectionsStorageConnectionString");
        const string tableName = "TypoCorrections";

        var blobServiceClientConnectionString = Environment.GetEnvironmentVariable("TriggerFileStorage");
        builder.Services.AddSingleton(s => new BlobServiceClient(blobServiceClientConnectionString));

        var cache = new TypoCorrectionCache(storageConnectionString, tableName);
        builder.Services.AddSingleton(cache);

        builder.Services.AddSingleton<BooksParserService>();
        builder.Services.AddSingleton(s =>
        {
            var cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
            var cosmosClient = new CosmosClient(cosmosConnectionString);
            return cosmosClient;
        });
    }
}