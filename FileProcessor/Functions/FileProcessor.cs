using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FileProcessor.Commands;
using FileProcessor.Handlers;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileProcessor.Functions;

public class FileProcessor(IMediator mediator, ILogger<FileProcessor> logger, BlobServiceClient blobServiceClient)
{
    [Function("FileProcessor")]
    public async Task Run([BlobTrigger("publisher-files/{name}", Connection = "TriggerFileStorage")] Stream myBlob, string name)
    {
        logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

        var containerClient = blobServiceClient.GetBlobContainerClient("publisher-files");
        var blobClient = containerClient.GetBlobClient(name);

        try
        {
            // Check if the blob has already been processed
            var properties = await blobClient.GetPropertiesAsync();
            if (properties.Value.Metadata.ContainsKey("Processed") && properties.Value.Metadata["Processed"] == "true")
            {
                logger.LogInformation($"Blob {name} has already been processed.");
                return;
            }

            using var reader = new StreamReader(myBlob);
            var jsonData = await reader.ReadToEndAsync();
        
            // Forward the list of books to the ParseBookCommand
            var parsedData = await mediator.Send(new ParseBooksCommand(jsonData));

            try
            {
                // Forward the list of books to the SaveBooksCommand
                await mediator.Send(new SaveBooksCommand(parsedData));
            }
            catch (SaveBooksCommandHandler.DatabaseUpdateException e)
            {
                //if a database update exception occurs, move the file to the dead-letter queue
                throw;
            }
            catch(Exception e)
            {
                //otherwise, log the error and continue because we know that the data was stored
                logger.LogError($"A non-critical error occurred during file processing: {e}");
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
            logger.LogError($"Error processing blob {name}: {ex.Message}");

            // Move blob to dead-letter queue
            var deadLetterContainerClient = blobServiceClient.GetBlobContainerClient("dead-letter-files");
            await deadLetterContainerClient.CreateIfNotExistsAsync();
            var deadLetterBlobClient = deadLetterContainerClient.GetBlobClient(name);

            await deadLetterBlobClient.StartCopyFromUriAsync(blobClient.Uri);
            await blobClient.DeleteIfExistsAsync();

            logger.LogInformation($"Blob {name} moved to dead-letter queue.");

            throw;
        }
    }
}