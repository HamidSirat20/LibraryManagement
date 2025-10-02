using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM;

    public static  class BookToDtoMapper
    {
        public static BookWithPublisherDto MapBookToBookWithPublisherDto(this Book book)
        {
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
                .Select(ba => new AuthorDto
                       {
                           Id = ba.Author.Id,
                           FirstName = ba.Author.FirstName,
                           LastName = ba.Author.LastName,
                           Email = ba.Author.Email,
                           Bio = ba.Author.Bio,
                           BookCount = ba.Author.BookAuthors.Count
                }).ToList(),
                Publisher = new PublisherDto
                {
                    Id = book.Publisher.Id,
                    Name = book.Publisher.Name,
                    Address = book.Publisher.Address,
                    Website = book.Publisher.Website,
                    BookCount = book.Publisher.Books.Count
                }
            };
        }

    public static BookWithoutPublisherDto MapBookToBookWithoutPublisherDto(this Book book)
    {
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
            .Select(ba => new AuthorDto
            {
                Id = ba.Author.Id,
                FirstName = ba.Author.FirstName,
                LastName = ba.Author.LastName,
                Email = ba.Author.Email,
                Bio = ba.Author.Bio
            }).ToList()
        };
    }

    public static BookCreateDto MapBookToBookCreateDto(this Book book)
    {
        return new BookCreateDto
        {
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            CoverImageUrl = book.CoverImageUrl,
            PublishedDate = book.PublishedDate,
            Pages = book.Pages,
            PublisherId = book.PublisherId,
            AuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToList()
        };
    }

    public static Book MapBookCreateDtoToBook(this BookCreateDto bookCreateDto)
    {
        return new Book(
            bookCreateDto.Title,
            bookCreateDto.Description,
            bookCreateDto.CoverImageUrl,
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

    public static BookReadDto MapBookToBookReadDto(this Book book)
    {
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
            AuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToList()
        };
    }
    public static BookUpdateDto MapBookToBookUpdateDto(this Book book)
    {
        return new BookUpdateDto
        {
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            CoverImageUrl = book.CoverImageUrl,
            PublishedDate = book.PublishedDate,
            Pages = book.Pages
        };
    }

    public static Book MapBookUpdateDtoToBook(this BookUpdateDto bookUpdateDto, Book existingBook)
    {
        if (bookUpdateDto.Title != null)
            existingBook.Title = bookUpdateDto.Title;

        if (bookUpdateDto.Description != null)
            existingBook.Description = bookUpdateDto.Description;

        if (bookUpdateDto.CoverImageUrl != null)
            existingBook.CoverImageUrl = bookUpdateDto.CoverImageUrl;

        if (bookUpdateDto.PublishedDate != default)
            existingBook.PublishedDate = bookUpdateDto.PublishedDate;

        if (bookUpdateDto.Genre != default)
            existingBook.Genre = bookUpdateDto.Genre;

        if (bookUpdateDto.Pages > 0)
            existingBook.Pages = bookUpdateDto.Pages;

        existingBook.UpdatedAt = DateTime.UtcNow;
        return existingBook;
    }
}



