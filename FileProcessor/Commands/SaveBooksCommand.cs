using System.Collections.Generic;
using FileProcessor.Models;
using MediatR;

namespace FileProcessor.Commands;

public class SaveBooksCommand(List<Book> books) : IRequest
{
    public List<Book> Books { get; } = books;
}