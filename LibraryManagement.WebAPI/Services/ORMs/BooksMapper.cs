using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.ORM;
public class BooksMapper : IBooksMapper
{
    public BookWithPublisherDto ToBookWithPublisherDto(Book book)
    {
        if (book == null) return null;

        return new BookWithPublisherDto
        {
            Id = book.Id,
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            CoverImageUrl = book.CoverImageUrl,
            PublishedDate = book.PublishedDate,
            Pages = book.Pages,
            BookAuthors = book.BookAuthors
                .Select(ba => new AuthorReadDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName,
                    Email = ba.Author.Email,
                    Bio = ba.Author.Bio,
                    BookCount = ba.Author.BookAuthors.Count
                }).ToList(),
            Publisher = new PublisherReadDto
            {
                Id = book.Publisher.Id,
                Name = book.Publisher.Name,
                Address = book.Publisher.Address,
                Email = book.Publisher.Email,
                Website = book.Publisher.Website,
                BookCount = book.Publisher.Books.Count
            }
        };
    }

    public BookWithoutPublisherDto ToBookWithoutPublisherDto(Book book)
    {
        if (book == null) return null;

        return new BookWithoutPublisherDto
        {
            Id = book.Id,
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            CoverImageUrl = book.CoverImageUrl,
            PublishedDate = book.PublishedDate,
            Pages = book.Pages,
            BookAuthors = book.BookAuthors
                .Select(ba => new AuthorReadDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName,
                    Email = ba.Author.Email,
                    Bio = ba.Author.Bio
                }).ToList()
        };
    }

    public BookReadDto ToBookReadDto(Book book)
    {
        if (book == null) return null;

        return new BookReadDto
        {
            Id = book.Id,
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            CoverImageUrl = book.CoverImageUrl,
            PublishedDate = book.PublishedDate,
            Pages = book.Pages,
            PublisherId = book.PublisherId,
            CoverImagePublicId = book.CoverImagePublicId,
            AuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToList()
        };
    }

    public BookCreateDto ToBookCreateDto(Book book)
    {
        if (book == null) return null;

        return new BookCreateDto
        {
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            PublishedDate = book.PublishedDate,
            Pages = book.Pages,
            PublisherId = book.PublisherId,
            AuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToList()
        };
    }

    public BookUpdateDto ToBookUpdateDto(Book book)
    {
        if (book == null) return null;

        return new BookUpdateDto
        {
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            PublishedDate = book.PublishedDate,
            Pages = book.Pages
        };
    }

    public Book ToBook(BookCreateDto bookCreateDto)
    {
        if (bookCreateDto == null) return null;

        return new Book(
            bookCreateDto.Title,
            bookCreateDto.Description,
            bookCreateDto.PublishedDate,
            bookCreateDto.Genre,
            bookCreateDto.Pages
        )
        {
            BookAuthors = bookCreateDto.AuthorIds
                .Select(authorId => new BookAuthor { AuthorId = authorId })
                .ToList(),
            PublisherId = bookCreateDto.PublisherId
        };
    }

    public Book UpdateFromDto(Book book, BookUpdateDto bookUpdateDto)
    {
        if (bookUpdateDto == null) return book;

        if (bookUpdateDto.Title != null)
            book.Title = bookUpdateDto.Title;

        if (bookUpdateDto.Description != null)
            book.Description = bookUpdateDto.Description;

        if (bookUpdateDto.PublishedDate != default)
            book.PublishedDate = bookUpdateDto.PublishedDate;

        if (bookUpdateDto.Genre != default)
            book.Genre = bookUpdateDto.Genre;

        if (bookUpdateDto.Pages > 0)
            book.Pages = bookUpdateDto.Pages;

        book.UpdatedAt = DateTime.UtcNow;
        return book;
    }
}