using CloudinaryDotNet.Actions;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LibraryManagement.Tests.Services;
public class InMemoryLibraryDbContext : LibraryDbContext
{
    public InMemoryLibraryDbContext(DbContextOptions<LibraryManagement.WebAPI.Data.LibraryDbContext> options)
        : base(options, new ConfigurationBuilder().AddInMemoryCollection().Build()) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // using in-memory database
    }
}

public class BookServiceTests
{
    private readonly Mock<IBooksMapper> _mockMapper;
    private readonly Mock<IImageService> _mockImageService;
    private readonly InMemoryLibraryDbContext _context;
    private readonly BooksService _bookService;

    public BookServiceTests()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new InMemoryLibraryDbContext(options);

        _mockMapper = new Mock<IBooksMapper>();
        _mockImageService = new Mock<IImageService>();

        _bookService = new BooksService(_context, _mockMapper.Object, _mockImageService.Object);
    }

    [Fact]
    public async Task CreateBookAsync_Should_Add_Book_To_Database_And_Upload_Image()
    {
        // Arrange
        var bookCreateDto = new BookDto
        {
            Title = "Divan of Hafez",
            Description = "A collection of poems by the 14th-century Persian poet Hafez.",
            PublishedDate = new DateTime(1368, 1, 1),
            Genre = Genre.Romance,
            Pages = 300,
            AuthorIds = new List<Guid> { Guid.NewGuid() },
            PublisherId = Guid.NewGuid(),
            File = Mock.Of<IFormFile>()
        };

        var mappedBook = new Book
        {
            Id = Guid.NewGuid(),
            Title = bookCreateDto.Title,
            Description = bookCreateDto.Description,
            PublishedDate = bookCreateDto.PublishedDate,
            Genre = bookCreateDto.Genre,
            Pages = bookCreateDto.Pages,
            PublisherId = bookCreateDto.PublisherId
        };

        _mockMapper.Setup(m => m.ToBook(bookCreateDto)).Returns(mappedBook);
        _mockImageService.Setup(i => i.AddImageAsync(bookCreateDto.File))
            .ReturnsAsync(new ImageUploadResult
            {
                SecureUrl = new Uri("https://example.com/image.jpg"),
                PublicId = "public-id"
            });

        // Act
        var result = await _bookService.CreateBookAsync(bookCreateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://example.com/image.jpg", result.CoverImageUrl);
        Assert.Equal("public-id", result.CoverImagePublicId);
        Assert.Single(_context.Books);
        Assert.Equal("Divan of Hafez", _context.Books.First().Title);

        _mockMapper.Verify(m => m.ToBook(It.IsAny<BookDto>()), Times.Once);
        _mockImageService.Verify(i => i.AddImageAsync(It.IsAny<IFormFile>()), Times.Once);
    }


    [Fact]
    public async Task CreateBookAsync_Should_Throw_Exception_When_Image_Upload_Fails()
    {
        // Arrange
        var bookCreateDto = new BookDto
        {
            Title = "Divan of Hafez",
            Description = "A collection of poems by the 14th-century Persian poet Hafez.",
            PublishedDate = new DateTime(1368, 1, 1),
            Genre = Genre.Romance,
            Pages = 300,
            AuthorIds = new List<Guid> { Guid.NewGuid() },
            PublisherId = Guid.NewGuid(),
            File = Mock.Of<IFormFile>()
        };


        var mappedBook = new Book { Id = Guid.NewGuid(), Title = "Book To Fail" };

        _mockMapper.Setup(m => m.ToBook(bookCreateDto)).Returns(mappedBook);
        _mockImageService.Setup(s => s.AddImageAsync(bookCreateDto.File))
            .ReturnsAsync(new ImageUploadResult
            {
                Error = new Error { Message = "Upload failed" }
            });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _bookService.CreateBookAsync(bookCreateDto));
        Assert.Equal("Image upload failed: Upload failed", ex.Message);
    }

    [Fact]
    public async Task DeleteBookByIdAsync_Should_Remove_Book_And_Delete_Image()
    {
        // Arrange
        var publisher = new Publisher
        {
            Id = Guid.NewGuid(),
            Name = "Test Publisher",
            Address = "123 Library St",
            Website = "https://publisher.com"
        };

        
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Book To Delete",
            Description = "A book that will be deleted in this test.",
            CoverImageUrl = "https://cloudinary.com/test.jpg",
            CoverImagePublicId = "public123",
            PublishedDate = DateTime.UtcNow,
            Genre = Genre.Fiction,
            Pages = 200,
            Publisher = publisher,
            PublisherId = publisher.Id
        };

        _context.Publishers.Add(publisher);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        _mockImageService.Setup(s => s.DeleteImageAsync("public123"))
            .ReturnsAsync(new DeletionResult { Result = "ok" });

        // Act
        var result = await _bookService.DeleteBookByIdAsync(book.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(_context.Books); 

        _mockImageService.Verify(s => s.DeleteImageAsync("public123"), Times.Once);
    }

    [Fact]
    public async Task EntityExistAsync_Should_Return_True_If_Book_Exists()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Existing Book",
            Description = "Exists in DB",
            PublishedDate = DateTime.UtcNow,
            Genre = Genre.Fiction,
            Pages = 150,
            CoverImageUrl = "https://example.com/book.jpg",
            PublisherId = Guid.NewGuid()
        };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _bookService.EntityExistAsync(book.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task EntityExistAsync_Should_Return_False_If_Book_Not_Exists()
    {
        // Act
        var exists = await _bookService.EntityExistAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }
    [Fact]
    public async Task GetByIdAsync_Should_Return_Book_When_Found()
    {
        // Arrange
        var publisher = new Publisher { Id = Guid.NewGuid(), Name = "Otava Publishing Company", Address = "Helsinki",Website = "www.helsinki.com" };
        var author = new Author { Id = Guid.NewGuid(), FirstName = "Ali", LastName = "Alawi" ,Email = "ali@mail.com"};
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Fetched Book",
            Description = "Desc",
            PublishedDate = DateTime.UtcNow,
            Genre = Genre.Mystery,
            Pages = 222,
            CoverImageUrl = "https://book.jpg",
            Publisher = publisher,
            PublisherId = publisher.Id,
            BookAuthors = new List<BookAuthor> { new BookAuthor { Author = author, AuthorId = author.Id } }
        };

        _context.Publishers.Add(publisher);
        _context.Authors.Add(author);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        var result = await _bookService.GetByIdAsync(book.Id, includePublisher: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(book.Title, result!.Title);
        Assert.NotNull(result.Publisher);
        Assert.NotEmpty(result.BookAuthors);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        var result = await _bookService.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task ListAllBooksAsync_Should_Return_Paginated_And_Filtered_List()
    {
        // Arrange
        var publisher = new Publisher { Id = Guid.NewGuid(), Name = "Otava Publishing Company", Address = "Helsinki", Website = "www.helsinki.com" };
        var author = new Author { Id = Guid.NewGuid(), FirstName = "Ali", LastName = "Alawi", Email = "ali@mail.com" };
        _context.Books.AddRange(
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Shahnameh",
                Description = "A book written by Ferdosi.",
                PublishedDate = DateTime.UtcNow.AddDays(-10),
                Genre = Genre.Fiction,
                Pages = 100,
                CoverImageUrl = "https://a.jpg",
                PublisherId = publisher.Id,
                Publisher = publisher
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Divan Hafez",
                Description = "A poem book",
                PublishedDate = DateTime.UtcNow,
                Genre = Genre.NonFiction,
                Pages = 200,
                CoverImageUrl = "https://b.jpg",
                PublisherId = publisher.Id,
                Publisher = publisher
            }
        );
        await _context.SaveChangesAsync();

        var options = new QueryOptions
        {
            PageNumber = 1,
            PageSize = 10,
            Genre = Genre.Fiction,
            OrderBy = "title",
            IsDescending = false
        };

        // Act
        var result = await _bookService.ListAllBooksAsync(options);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); 
        Assert.Equal("Shahnameh", result.First().Title);
    }

  
    [Fact]
    public async Task PartiallyUpdateBookAsync_Should_Update_Book_Successfully()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Partial Book",
            Description = "Old Desc",
            CoverImageUrl = "https://book.jpg",
            PublishedDate = DateTime.UtcNow,
            Genre = Genre.Fiction,
            Pages = 100,
            PublisherId = Guid.NewGuid()
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        book.Description = "Updated Desc";
        var updated = await _bookService.PartiallyUpdateBookAsync(book);

        // Assert
        Assert.Equal("Updated Desc", updated.Description);
        var dbBook = await _context.Books.FindAsync(book.Id);
        Assert.Equal("Updated Desc", dbBook!.Description);
    }

    [Fact]
    public async Task PartiallyUpdateBookAsync_Should_Throw_When_Book_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _bookService.PartiallyUpdateBookAsync(null!));
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Return_True_When_Successful()
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Save Book",
            Description = "Desc",
            CoverImageUrl = "https://img.jpg",
            PublishedDate = DateTime.UtcNow,
            Genre = Genre.Fiction,
            Pages = 120,
            PublisherId = Guid.NewGuid()
        };

        _context.Books.Add(book);
        var result = await _bookService.SaveChangesAsync();

        Assert.True(result);
    }
}