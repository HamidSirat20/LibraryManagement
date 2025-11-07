using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.ORM;

public class AuthorMapper : IAuthorMapper
{
    public AuthorReadDto ToAuthorReadDto(Author author)
    {
        if (author == null) return null;

        return new AuthorReadDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Email = author.Email,
            Bio = author.Bio,
            CreatedAt = author.CreatedAt,
            UpdatedAt = author.UpdatedAt,
            BookCount = author.BookAuthors?.Count ?? 0
        };
    }

    public AuthorWithBooksDto ToAuthorWithBooksDto(Author author)
    {
        if (author == null) return null;

        return new AuthorWithBooksDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Email = author.Email,
            Bio = author.Bio,
            CreatedAt = (DateTime)author.CreatedAt,
            UpdatedAt = (DateTime)author.UpdatedAt,
            Books = author.BookAuthors?
                .Select(ba => new BookReadDto
                {
                    Id = ba.Book.Id,
                    Title = ba.Book.Title,
                    Genre = ba.Book.Genre,
                    Description = ba.Book.Description,
                    CoverImageUrl = ba.Book.CoverImageUrl,
                    PublishedDate = ba.Book.PublishedDate,
                    Pages = ba.Book.Pages
                }).ToList() ?? new List<BookReadDto>()
        };
    }

    public AuthorCreateDto ToAuthorCreateDto(Author author)
    {
        if (author == null) return null;

        return new AuthorCreateDto
        {
            FirstName = author.FirstName,
            LastName = author.LastName,
            Email = author.Email,
            Bio = author.Bio
        };
    }

    public AuthorUpdateDto ToAuthorUpdateDto(Author author)
    {
        if (author == null) return null;

        return new AuthorUpdateDto
        {
            FirstName = author.FirstName,
            LastName = author.LastName,
            Email = author.Email,
            Bio = author.Bio
        };
    }

    public Author ToAuthor(AuthorCreateDto authorCreateDto)
    {
        if (authorCreateDto == null) return null;

        return new Author
        {
            FirstName = authorCreateDto.FirstName,
            LastName = authorCreateDto.LastName,
            Email = authorCreateDto.Email,
            Bio = authorCreateDto.Bio,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Author UpdateFromDto(Author author, AuthorUpdateDto authorUpdateDto)
    {
        if (authorUpdateDto == null) return author;

        if (!string.IsNullOrWhiteSpace(authorUpdateDto.FirstName))
            author.FirstName = authorUpdateDto.FirstName;

        if (!string.IsNullOrWhiteSpace(authorUpdateDto.LastName))
            author.LastName = authorUpdateDto.LastName;

        if (!string.IsNullOrWhiteSpace(authorUpdateDto.Email))
            author.Email = authorUpdateDto.Email;

        if (!string.IsNullOrWhiteSpace(authorUpdateDto.Bio))
            author.Bio = authorUpdateDto.Bio;

        author.UpdatedAt = DateTime.UtcNow;

        return author;
    }
}