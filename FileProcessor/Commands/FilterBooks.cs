using MediatR;
using System.Collections.Generic;
using FileProcessor.Models;

namespace FileProcessor.Commands;

public class FilterBooksCommand(List<Book> books) : IRequest<List<Book>>
{
    public List<Book> Books { get; } = books;
}