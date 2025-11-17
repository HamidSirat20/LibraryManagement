using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces;
public interface IBooksMapper
{
    BookWithPublisherDto ToBookWithPublisherDto(Book book);
    BookWithoutPublisherDto ToBookWithoutPublisherDto(Book book);
    BookReadDto ToBookReadDto(Book book);
    BookDto ToBookCreateDto(Book book);
    BookUpdateDto ToBookUpdateDto(Book book);
    Book ToBook(BookDto bookCreateDto);
    Book UpdateFromDto(Book book, BookUpdateDto bookUpdateDto);
}
