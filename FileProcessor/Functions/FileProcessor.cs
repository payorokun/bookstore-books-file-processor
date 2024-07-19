using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FileProcessor.Commands;
using FileProcessor.Handlers;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FileProcessor.Functions;

public class FileProcessor
{
    private readonly IMediator _mediator;
    private readonly BlobServiceClient _blobServiceClient;

    public FileProcessor(IMediator mediator, BlobServiceClient blobServiceClient)
    {
        _mediator = mediator;
        _blobServiceClient = blobServiceClient;
    }

    [FunctionName("FileProcessor")]
    public async Task Run([BlobTrigger("publisher-files/{name}", Connection = "TriggerFileStorage")] Stream myBlob, string name, ILogger log)
    {
        log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

        var containerClient = _blobServiceClient.GetBlobContainerClient("publisher-files");
        var blobClient = containerClient.GetBlobClient(name);

        try
        {
            // Check if the blob has already been processed
            var properties = await blobClient.GetPropertiesAsync();
            if (properties.Value.Metadata.ContainsKey("Processed") && properties.Value.Metadata["Processed"] == "true")
            {
                log.LogInformation($"Blob {name} has already been processed.");
                return;
            }

            using var reader = new StreamReader(myBlob);
            var jsonData = await reader.ReadToEndAsync();
        
            // Forward the list of books to the ParseBookCommand
            var parsedData = await _mediator.Send(new ParseBooksCommand(jsonData));

            try
            {
                // Forward the list of books to the SaveBooksCommand
                await _mediator.Send(new SaveBooksCommand(parsedData));
            }
            catch (SaveBooksCommandHandler.DatabaseUpdateException e)
            {
                //if a database update exception occurs, move the file to the dead-letter queue
                throw;
            }
            catch(Exception e)
            {
                //otherwise, log the error and continue because we know that the data was stored
                log.LogError($"A non-critical error occurred during file processing: {e}");
            }

            // Mark the blob as processed
            var metadata = new Dictionary<string, string>
            {
                { "Processed", "true" }
            };
            await blobClient.SetMetadataAsync(metadata);
        }
        catch (Exception ex)
        {
            log.LogError($"Error processing blob {name}: {ex.Message}");

            // Move blob to dead-letter queue
            var deadLetterContainerClient = _blobServiceClient.GetBlobContainerClient("dead-letter-files");
            await deadLetterContainerClient.CreateIfNotExistsAsync();
            var deadLetterBlobClient = deadLetterContainerClient.GetBlobClient(name);

            await deadLetterBlobClient.StartCopyFromUriAsync(blobClient.Uri);
            await blobClient.DeleteIfExistsAsync();

            log.LogInformation($"Blob {name} moved to dead-letter queue.");

            throw;
        }
    }
}