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
                           Bio = ba.Author.Bio
                       }).ToList(),
                Publisher = new PublisherDto
                {
                    Id = book.Publisher.Id,
                    Name = book.Publisher.Name,
                    Address = book.Publisher.Address,
                    Website = book.Publisher.Website
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


    }



