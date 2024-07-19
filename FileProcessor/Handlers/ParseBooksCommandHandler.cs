using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.Commands;
using FileProcessor.Models;
using FileProcessor.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FileProcessor.Handlers;

public class ParseBooksCommandHandler : IRequestHandler<ParseBooksCommand, List<Book>>
{
    private readonly ILogger<ParseBooksCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly BooksParserService _booksParserService;

    public ParseBooksCommandHandler(ILogger<ParseBooksCommandHandler> logger, IMediator mediator, BooksParserService booksParserService)
    {
        _logger = logger;
        _mediator = mediator;
        _booksParserService = booksParserService;
    }
    public async Task<List<Book>> Handle(ParseBooksCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Parsing Books");
        var books = _booksParserService.ParseBooks(request.JsonData);

        var filteredBooks = await _mediator.Send(new FilterBooksCommand(books), cancellationToken);

        _logger.LogDebug("Finished Parsing Books");
        return filteredBooks;
    }
}


public class BooksParserService
{
    private readonly TypoCorrectionCache _cache;

    public BooksParserService(TypoCorrectionCache cache)
    {
        _cache = cache;
    }
    public List<Book> ParseBooks(string jsonData)
    {
        using var jsonReader = new JsonTextReader(new StringReader(jsonData));
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new BookJsonConverter(_cache));
        return serializer.Deserialize<List<Book>>(jsonReader);
    }
}
