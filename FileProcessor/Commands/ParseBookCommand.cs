using System.Collections.Generic;
using FileProcessor.Models;
using MediatR;

namespace FileProcessor.Commands;

public class ParseBooksCommand : IRequest<List<Book>>
{
    public ParseBooksCommand(string jsonData)
    {
        JsonData = jsonData;
    }

    public string JsonData { get; }
}