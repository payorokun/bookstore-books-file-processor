using AutoMapper;
using FileProcessor.Models.Database;
using FileProcessor.Models;

namespace FileProcessor.Profiles;
public class BookEntityMappingProfile : Profile
{
    public BookEntityMappingProfile()
    {
        CreateMap<Book, BookEntity>();
    }
}
