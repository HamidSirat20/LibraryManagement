using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces;

public interface IAuthorsMapper
{
    AuthorReadDto ToAuthorReadDto(Author author);
    AuthorWithBooksDto ToAuthorWithBooksDto(Author author);
    AuthorCreateDto ToAuthorCreateDto(Author author);
    AuthorUpdateDto ToAuthorUpdateDto(Author author);
    Author ToAuthor(AuthorCreateDto authorCreateDto);
    Author UpdateFromDto(Author author, AuthorUpdateDto authorUpdateDto);
}