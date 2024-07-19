using System.Collections.Generic;
using FileProcessor.Models;
using MediatR;

namespace FileProcessor.Commands;

public class ParseBooksCommand(string jsonData) : IRequest<List<Book>>
{
    public string JsonData { get; } = jsonData;
}