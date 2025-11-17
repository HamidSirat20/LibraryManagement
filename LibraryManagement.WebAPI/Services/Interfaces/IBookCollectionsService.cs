using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface IBookCollectionsService
{
    Task<IEnumerable<BookReadDto>> CreateBooksAsync(IEnumerable<BookDto> bookCreateDtos);
    Task<IEnumerable<BookReadDto>> GetBookCollectionsAsync(IEnumerable<Guid> bookIds);
}

