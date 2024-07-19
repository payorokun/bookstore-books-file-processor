using FileProcessor.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.Models;
using Microsoft.Extensions.Logging;

namespace FileProcessor.Handlers;

public class FilterBooksCommandHandler : IRequestHandler<FilterBooksCommand, List<Book>>
{
    private readonly ILogger<FilterBooksCommandHandler> _logger;
    private readonly IMediator _mediator;

    public FilterBooksCommandHandler(ILogger<FilterBooksCommandHandler> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }
    public Task<List<Book>> Handle(FilterBooksCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Filtering books based on business rules");
        var filteredBooks = request.Books.Where(b => b.PublishDate.DayOfWeek!=DayOfWeek.Sunday 
                                                     && !b.Author.Contains("shrub")).ToList();
        var removedCount = request.Books.Count - filteredBooks.Count;
        _logger.LogDebug("Finished filtering books." + (removedCount == 0 ? "No books were removed." : $"Removed {removedCount} books."));
        return Task.FromResult(filteredBooks);
    }
}