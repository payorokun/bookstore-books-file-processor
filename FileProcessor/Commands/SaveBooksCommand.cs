using System.Collections.Generic;
using FileProcessor.Models;
using MediatR;

namespace FileProcessor.Commands;

public class SaveBooksCommand : IRequest
{
    public SaveBooksCommand(List<Book> books)
    {
        Books = books;
    }

    public List<Book> Books { get; }
}