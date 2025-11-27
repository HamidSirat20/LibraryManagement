using CloudinaryDotNet.Actions;
using LibraryManagement.Test.Fixtures;
using LibraryManagement.Test.Test_Data_Builders;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace LibraryManagement.Test.Services;
public class BooksServiceTests : IClassFixture<BooksServiceFixture>
{
    private readonly BooksServiceFixture _fixture;
    private readonly LibraryDbContext _dbContext;
    private readonly BooksService _booksService;
    public BooksServiceTests()
    {
        _fixture = new BooksServiceFixture();
        _dbContext = _fixture.DbContext;
        _booksService = _fixture.BooksService;
    }

    [Fact]
    public async Task CreateBook_WhenValidDataProvided_CreateBook()
    {
        //Arrange
        var author = new AuthorBuilder()
            .WithId(Guid.NewGuid())
            .WithName("F. Scott", "Fitzgerald")
            .WithEmail("author@gmail.com")
            .Build();

        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var publisher = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Scribner")
            .WithAddress("123 Publisher St, New York, NY")
            .WithWebsite("https://scribner.com")
            .WithEmail("publisher@gmail.com")
            .Build();

        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var newBook = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("The Great Gatsby")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/gatsby.jpg")
            .WithDescription("A novel set in the Jazz Age...")
            .WithPublishedDate(DateTime.Now)
            .WithGenre(Genre.Fiction)
            .WithPages(180)
            .WithPublisher(publisher.Id)
            .Build();

        _fixture.BookMapperMock
                    .Setup(m => m.ToBook(It.IsAny<BookCreateDto>()))
                    .Returns(new Book
                    {
                        Id = newBook.Id,
                        Title = newBook.Title,
                        Description = newBook.Description,
                        Genre = newBook.Genre,
                        Pages = newBook.Pages,
                        PublisherId = newBook.PublisherId,
                        CoverImageUrl = newBook.CoverImageUrl,
                        BookAuthors = new List<BookAuthor>()
                    });

        var fakeFileContent = Encoding.UTF8.GetBytes("fake image content");
        var fakeStream = new MemoryStream(fakeFileContent);
        IFormFile fakeFile = new FormFile(fakeStream, 0, fakeFileContent.Length, "File", "cover.jpg");

        var bookCreateDto = new BookCreateDto
        {
            Title = "The Great Gatsby",
            Description = "A novel set in the Jazz Age...",
            PublishedDate = DateTime.Now,
            Genre = Genre.Fiction,
            Pages = 180,
            File = fakeFile,
            PublisherId = publisher.Id,
            AuthorIds = new List<Guid> { author.Id }
        };

        _fixture.ImageServiceMock
            .Setup(s => s.AddImageAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(new ImageUploadResult
            {
                SecureUrl = new Uri("https://example.com/gatsby.jpg"),
                PublicId = "sample_public_id"
            });

        //Act
        var createdBook = await _booksService.CreateBookAsync(bookCreateDto);

        //Assert
        Assert.NotNull(createdBook);
        Assert.Equal("The Great Gatsby", createdBook.Title);
        Assert.Equal("A novel set in the Jazz Age...", createdBook.Description);
        Assert.Equal(Genre.Fiction, createdBook.Genre);
        Assert.Equal(180, createdBook.Pages);
        Assert.Equal(publisher.Id, createdBook.PublisherId);
        Assert.Single(createdBook.BookAuthors);
        Assert.Equal(author.Id, createdBook.BookAuthors.First().AuthorId);
        _fixture.Reset();
    }
    [Fact]
    public async Task CreateBook_WhenNullBookCreateDtoProvided_ThrowException()
    {
        //Arrange
        var bookCreateDto = (BookCreateDto)null;

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _booksService.CreateBookAsync(bookCreateDto);
        });
        _fixture.Reset();
    }
    [Fact]
    public async Task CreateBook_WhenNullImageProvided_CreateBook()
    {
        //Arrange
        var author = new AuthorBuilder()
            .WithId(Guid.NewGuid())
            .WithName("F. Scott", "Fitzgerald")
            .WithEmail("author@gmail.com")
            .Build();

        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var publisher = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Scribner")
            .WithAddress("123 Publisher St, New York, NY")
            .WithWebsite("https://scribner.com")
            .WithEmail("publisher@gmail.com")
            .Build();

        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var newBook = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("The Great Gatsby")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/gatsby.jpg")
            .WithDescription("A novel set in the Jazz Age...")
            .WithPublishedDate(DateTime.Now)
            .WithGenre(Genre.Fiction)
            .WithPages(180)
            .WithPublisher(publisher.Id)
            .Build();

        _fixture.BookMapperMock
                    .Setup(m => m.ToBook(It.IsAny<BookCreateDto>()))
                    .Returns(new Book
                    {
                        Id = newBook.Id,
                        Title = newBook.Title,
                        Description = newBook.Description,
                        Genre = newBook.Genre,
                        Pages = newBook.Pages,
                        PublisherId = newBook.PublisherId,
                        CoverImageUrl = newBook.CoverImageUrl,
                        BookAuthors = new List<BookAuthor>()
                    });

        IFormFile fakeFile = null;
        var bookCreateDto = new BookCreateDto
        {
            Title = "The Great Gatsby",
            Description = "A novel set in the Jazz Age...",
            PublishedDate = DateTime.Now,
            Genre = Genre.Fiction,
            Pages = 180,
            File = fakeFile,
            PublisherId = publisher.Id,
            AuthorIds = new List<Guid> { author.Id }
        };

        //Act
        var createdBook = await _booksService.CreateBookAsync(bookCreateDto);

        //Assert
        Assert.NotNull(createdBook);
        Assert.Equal("The Great Gatsby", createdBook.Title);
        Assert.Equal("A novel set in the Jazz Age...", createdBook.Description);
        Assert.Equal(Genre.Fiction, createdBook.Genre);
        Assert.Equal(180, createdBook.Pages);
        Assert.Equal(publisher.Id, createdBook.PublisherId);
        Assert.Single(createdBook.BookAuthors);
        Assert.Equal(author.Id, createdBook.BookAuthors.First().AuthorId);
        _fixture.Reset();
    }

    [Fact]
    public async Task DeleteBook_WhenBookIdProvided_DeletesBook()
    {
        // Arrange
        var author = new AuthorBuilder()
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var publisher = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var book = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithAuthor(author.Id)
            .WithPublisher(publisher.Id)
            .Build();

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        // Act
        var deletedBook = await _booksService.DeleteBookByIdAsync(book.Id);

        // Assert
        var storedBook = await _dbContext.Books.FindAsync(book.Id);
        Assert.Null(storedBook);
        _fixture.Reset();
    }
    [Fact]
    public async Task DeleteBook_WhenWrongIdProvided_ThrowsExceptions()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _booksService.DeleteBookByIdAsync(Guid.NewGuid());
        });
        _fixture.Reset();
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExists_ReturnsBook()
    {
        //Arrange
        var author = new AuthorBuilder()
           .WithId(Guid.NewGuid())
           .Build();

        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var publisher = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var book = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithAuthor(author.Id)
            .WithPublisher(publisher.Id)
            .Build();

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        //Act
        var result = await _booksService.GetByIdAsync(book.Id);
        //Assert
        Assert.NotNull(result);
        Assert.Equal(book.Id, result.Id);
        Assert.Equal("Default Book Title", result.Title);
        Assert.Equal(publisher.Id, result.PublisherId);
        Assert.Single(result.BookAuthors);
        Assert.Equal(author.Id, result.BookAuthors.First().AuthorId);
        _fixture.Reset();
    }

    [Fact]
    public async Task ListAllBooks_WithPaginationAndSearch_ReturnAllBooks()
    {
        //Arrange
        var author = new AuthorBuilder()
           .WithId(Guid.NewGuid())
           .Build();

        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var publisher = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var book1 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithAuthor(author.Id)
            .WithPublisher(publisher.Id)
            .Build();
        var book2 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithAuthor(author.Id)
            .WithPublisher(publisher.Id)
            .Build();
        var book3 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithAuthor(author.Id)
            .WithPublisher(publisher.Id)
            .Build();

        _dbContext.Books.AddRange(book1, book2, book3);
        await _dbContext.SaveChangesAsync();

        QueryOptions queryOptions = new()
        {
            PageNumber = 1,
            PageSize = 2,
            SearchTerm = "Book"
        };

        //Act
        var result = await _booksService.ListAllBooksAsync(queryOptions);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _fixture.Reset();
    }

    [Fact]
    public async Task UpdateBookAsync_WhenNewDataProvided_UpdateBookWithNewProperties()
    {
        //Arrange
        var author = new AuthorBuilder()
            .WithId(Guid.NewGuid())
            .WithName("F. Scott", "Fitzgerald")
            .WithEmail("author@gmail.com")
            .Build();

        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var publisher = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Scribner")
            .WithAddress("123 Publisher St, New York, NY")
            .WithWebsite("https://scribner.com")
            .WithEmail("publisher@gmail.com")
            .Build();

        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var newBook = new BookBuilder()
           .WithId(Guid.NewGuid())
           .WithAuthor(author.Id)
           .WithPublisher(publisher.Id)
           .Build();

        _dbContext.Books.Add(newBook);
        await _dbContext.SaveChangesAsync();



        _fixture.BookMapperMock
                .Setup(m => m.UpdateFromDto(It.IsAny<Book>(), It.IsAny<BookUpdateDto>()))
                .Callback<Book, BookUpdateDto>((existing, dto) =>
                {
                    existing.Title = dto.Title;
                    existing.Description = dto.Description;
                    existing.Genre = dto.Genre;
                    existing.Pages = dto.Pages;
                })
                .Returns((Book existing, BookUpdateDto dto) => existing);

        IFormFile fakeFile = null;
        var bookUpdateDto = new BookUpdateDto
        {
            Title = "The Great Gatsby",
            Description = "A novel set in the Jazz Age...",
            PublishedDate = DateTime.Now,
            Genre = Genre.Fiction,
            Pages = 180,
            File = fakeFile
        };

        //Act
        var createdBook = await _booksService.UpdateBookAsync(newBook.Id, bookUpdateDto);

        //Assert
        Assert.NotNull(createdBook);
        Assert.Equal("The Great Gatsby", createdBook.Title);
        Assert.Equal("A novel set in the Jazz Age...", createdBook.Description);
        Assert.Equal(Genre.Fiction, createdBook.Genre);
        Assert.Equal(180, createdBook.Pages);
        Assert.Equal(publisher.Id, createdBook.PublisherId);
        Assert.Single(createdBook.BookAuthors);
        Assert.Equal(author.Id, createdBook.BookAuthors.First().AuthorId);
        _fixture.Reset();
    }

}
