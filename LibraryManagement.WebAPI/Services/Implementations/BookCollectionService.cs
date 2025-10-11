using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM;

namespace LibraryManagement.WebAPI.Services.Implementations
{
    public class BookCollectionService : IBookCollectionService
    {
        private readonly LibraryDbContext _dbContext;

        public BookCollectionService(LibraryDbContext libraryDb)
        {
            _dbContext = libraryDb;
        }
        public async Task<IEnumerable<BookReadDto>> CreateBooksAsync(IEnumerable<BookCreateDto> bookCreateDtos)
        {
            if (bookCreateDtos == null || !bookCreateDtos.Any())
            {
                throw new ArgumentNullException(nameof(bookCreateDtos));
            }

            var books = new List<Book>();

            foreach (var bookCreateDto in bookCreateDtos)
            {
                var book = new Book()
                {
                    Title = bookCreateDto.Title,
                    Description = bookCreateDto.Description,
                    CoverImageUrl = bookCreateDto.CoverImageUrl,
                    Genre = bookCreateDto.Genre,
                    PublishedDate = bookCreateDto.PublishedDate,
                    Pages = bookCreateDto.Pages,
                    PublisherId = bookCreateDto.PublisherId
                };

                foreach (var authorId in bookCreateDto.AuthorIds)
                {
                    book.BookAuthors.Add(new BookAuthor()
                    {
                        AuthorId = authorId,
                        Book = book 
                    });
                }

                books.Add(book);
                _dbContext.Books.Add(book);
            }

            await _dbContext.SaveChangesAsync();
            return books.Select(book=>book.MapBookToBookReadDto());
        }
    }
}
