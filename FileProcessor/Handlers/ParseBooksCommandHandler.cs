using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.Commands;
using FileProcessor.Models;
using FileProcessor.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FileProcessor.Handlers;

public class ParseBooksCommandHandler(ILogger<ParseBooksCommandHandler> logger, IMediator mediator, BooksParserService booksParserService) : IRequestHandler<ParseBooksCommand, List<Book>>
{
    public async Task<List<Book>> Handle(ParseBooksCommand request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Parsing Books");
        var books = await booksParserService.ParseBooksAsync(request.JsonData);

        var filteredBooks = await mediator.Send(new FilterBooksCommand(books), cancellationToken);

        logger.LogDebug("Finished Parsing Books");
        return filteredBooks;
    }
}


public class BooksParserService(TypoCorrectionCache cache)
{
    public async Task<List<Book>> ParseBooksAsync(string jsonData)
    {
        await using var jsonReader = new JsonTextReader(new StringReader(jsonData));
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new BookJsonConverter(cache));
        return serializer.Deserialize<List<Book>>(jsonReader);
    }
}
