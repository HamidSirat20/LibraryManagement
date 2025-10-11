using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces
{
    public interface IBookCollectionService
    {
        Task<IEnumerable<BookReadDto>> CreateBooksAsync(IEnumerable<BookCreateDto> bookCreateDtos);

    }
}
