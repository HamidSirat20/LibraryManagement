using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces;
public interface IBookMapper
{
    BookWithPublisherDto ToBookWithPublisherDto(Book book);
    BookWithoutPublisherDto ToBookWithoutPublisherDto(Book book);
    BookReadDto ToBookReadDto(Book book);
    BookCreateDto ToBookCreateDto(Book book);
    BookUpdateDto ToBookUpdateDto(Book book);
    Book ToBook(BookCreateDto bookCreateDto);
    Book UpdateFromDto(Book book, BookUpdateDto bookUpdateDto);
}
