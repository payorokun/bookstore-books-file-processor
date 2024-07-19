using MediatR;
using System.Collections.Generic;
using FileProcessor.Models;

namespace FileProcessor.Commands;

public class FilterBooksCommand : IRequest<List<Book>>
{
    public FilterBooksCommand(List<Book> books)
    {
        Books = books;
    }

    public List<Book> Books { get; }
}