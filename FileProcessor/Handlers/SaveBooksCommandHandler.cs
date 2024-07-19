using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FileProcessor.Commands;
using FileProcessor.Models;
using FileProcessor.Models.Database;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace FileProcessor.Handlers;

public class SaveBooksCommandHandler(CosmosClient cosmosClient, ILogger<SaveBooksCommandHandler> logger, IMapper mapper) : IRequestHandler<SaveBooksCommand>
{
    public class DatabaseUpdateException(Exception innerException) : Exception{}
    private readonly Container _container = cosmosClient.GetContainer("BookstoreDatabase", "BooksContainer");

    public async Task<Unit> Handle(SaveBooksCommand request, CancellationToken cancellationToken)
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
        return Unit.Value;
    }
}