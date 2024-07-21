using AutoMapper;
using FileProcessor.Commands;
using FileProcessor.Models.Database;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using CosmosContainer = Microsoft.Azure.Cosmos.Container;

namespace FileProcessor.Handlers;

public class SaveBooksCommandHandler(ILogger<SaveBooksCommandHandler> logger, IMapper mapper, CosmosClient cosmosClient)
    : IRequestHandler<SaveBooksCommand>
{
    public class DatabaseUpdateException(Exception wrappedException)
        : Exception(wrappedException.Message, wrappedException);

    private readonly CosmosContainer _container = cosmosClient.GetContainer("BookstoreDatabase", "BooksContainer");

    public async Task Handle(SaveBooksCommand request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Storing Books in database");
        var updatedItemsCount = 0;
        try
        {
            foreach (var book in request.Books)
            {
                var bookEntity = mapper.Map<BookEntity>(book);
                await _container.UpsertItemAsync(bookEntity, new PartitionKey(bookEntity.PartitionKey), null, cancellationToken);
                updatedItemsCount++;
            }
        }
        catch (Exception e)
        {
            throw new DatabaseUpdateException(e);
        }
        logger.LogDebug($"Updated {updatedItemsCount} items");
    }
}