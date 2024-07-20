using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FileProcessor.Commands;
using FileProcessor.Models.Database;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Container = Microsoft.Azure.Cosmos.Container;

namespace FileProcessor.Handlers;

public class SaveBooksCommandHandler : IRequestHandler<SaveBooksCommand>
{
    private readonly ILogger<SaveBooksCommandHandler> _logger;
    private readonly IMapper _mapper;

    public SaveBooksCommandHandler(ILogger<SaveBooksCommandHandler> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
        var cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
        var cosmosClient = new CosmosClient(cosmosConnectionString);
        _container = cosmosClient.GetContainer("BookstoreDatabase", "BooksContainer");
    }
    public class DatabaseUpdateException : Exception{
        public DatabaseUpdateException(Exception wrappedException) : base(wrappedException.Message, wrappedException) { }
    }

    private readonly Container _container;

    public async Task<Unit> Handle(SaveBooksCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Storing Books in database");
        var updatedItemsCount = 0;
        try
        {
            foreach (var book in request.Books)
            {
                var bookEntity = _mapper.Map<BookEntity>(book);
                await _container.UpsertItemAsync(bookEntity, new PartitionKey(bookEntity.PartitionKey), null, cancellationToken);
                updatedItemsCount++;
            }
        }
        catch (Exception e)
        {
            throw new DatabaseUpdateException(e);
        }
        _logger.LogDebug($"Updated {updatedItemsCount} items");
        return Unit.Value;
    }
}