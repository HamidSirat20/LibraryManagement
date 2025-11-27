using LibraryManagement.Test.Fixtures;
using LibraryManagement.Test.Test_Data_Builders;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Implementations;
using Moq;

namespace LibraryManagement.Test.Services;
public class BookCollectionsServiceTests : IClassFixture<BookCollectionsServiceFixture>
{
    private readonly BookCollectionsServiceFixture _fixture;
    private readonly LibraryDbContext _dbContext;
    private readonly BookCollectionsService _bookCollectionsService;
    public BookCollectionsServiceTests(BookCollectionsServiceFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
        _bookCollectionsService = fixture.BookCollectionsService;
    }

    [Fact]
    public async Task GetBookCollectionsAsync_WithValidIds_ReturnsBooks()
    {
        // Arrange
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

        var bookBuilder = new BookBuilder();

        var book1 = bookBuilder
            .WithId(Guid.NewGuid())
            .WithPublisher(publisher.Id)
            .WithAuthor(author.Id)
            .Build();
        var book2 = bookBuilder
            .WithId(Guid.NewGuid())
            .WithPublisher(publisher.Id)
            .WithAuthor(author.Id)
            .Build();
        var book3 = bookBuilder
            .WithId(Guid.NewGuid())
            .WithPublisher(publisher.Id)
            .WithAuthor(author.Id)
            .Build();

        _dbContext.Books.AddRange(book1, book2, book3);
        await _dbContext.SaveChangesAsync();
        // 
        _fixture.BookMapperMock.Setup(m => m.ToBookReadDto(It.IsAny<Book>()))
            .Returns<Book>(b => new BookReadDto
            {
                Id = b.Id,
                Title = b.Title,
                PublishedDate = b.PublishedDate,
                Description = b.Description,
                Genre = b.Genre,
                Pages = b.Pages,
                AuthorIds = b.BookAuthors.Select(ba => ba.AuthorId).ToList(),
                PublisherId = b.PublisherId,

            });
        var listOfBookId = new List<Guid>() { book1.Id, book2.Id, book3.Id };
        // Act
        var result = await _bookCollectionsService.GetBookCollectionsAsync(listOfBookId);
        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        // Verify that the mapper was called for each book
        _fixture.BookMapperMock.Verify(m => m.ToBookReadDto(It.IsAny<Book>()), Times.Exactly(3));
        _fixture.ResetMocks();
    }
    [Fact]
    public async Task GetBookCollectionsAsync_WithNullIds_ThrowsException()
    {
        // Arrange

        var listOfBookId = new List<Guid>() { Guid.Empty, Guid.Empty, Guid.Empty };
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            var result = await _bookCollectionsService.GetBookCollectionsAsync(listOfBookId);
        });
        _fixture.ResetMocks();

    }
    [Fact]
    public async Task AddBookCollectionsAsync_WithValidBookDtos_CreateBooks()
    {
        // Arrange
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

        var bookCreateDto1 = new BookCreateDto
        {
            Title = "Clean Code",
            Description = "A handbook of agile software craftsmanship.",
            PublishedDate = new DateTime(2008, 8, 1),
            Genre = Genre.Other,
            Pages = 464,
            AuthorIds = new List<Guid> { author.Id },
            PublisherId = publisher.Id
        };

        var bookCreateDto2 = new BookCreateDto
        {
            Title = "The Pragmatic Programmer",
            Description = "Journey to mastery in modern software development.",
            PublishedDate = new DateTime(1999, 10, 20),
            Genre = Genre.Other,
            Pages = 352,
            AuthorIds = new List<Guid> { author.Id },
            PublisherId = publisher.Id
        };

        var bookCreateDto3 = new BookCreateDto
        {
            Title = "Design Patterns",
            Description = "The classic guide to reusable object-oriented software.",
            PublishedDate = new DateTime(1994, 10, 31),
            Genre = Genre.Other,
            Pages = 395,
            AuthorIds = new List<Guid> { author.Id },
            PublisherId = publisher.Id
        };

        // 
        _fixture.BookMapperMock.Setup(m => m.ToBookReadDto(It.IsAny<Book>()))
            .Returns<Book>(b => new BookReadDto
            {
                Id = b.Id,
                Title = b.Title,
                PublishedDate = b.PublishedDate,
                Description = b.Description,
                Genre = b.Genre,
                Pages = b.Pages,
                AuthorIds = b.BookAuthors.Select(ba => ba.AuthorId).ToList(),
                PublisherId = b.PublisherId,

            });

        var listOfBookCreateDtos = new List<BookCreateDto>() { bookCreateDto1, bookCreateDto2, bookCreateDto3 };

        // Act
        var result = await _bookCollectionsService.CreateBooksAsync(listOfBookCreateDtos);
        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.Equal(bookCreateDto2.Title, resultList[1].Title);

        // Verify that the mapper was called for each book
        _fixture.BookMapperMock.Verify(m => m.ToBookReadDto(It.IsAny<Book>()), Times.Exactly(3));
        _fixture.ResetMocks();
    }
    [Fact]
    public async Task AddBookCollectionsAsync_WithNullBookDtos_ThrowsException()
    {
        // Arrange

        var bookCreateDto1 = (BookCreateDto)null;

        var bookCreateDto2 = (BookCreateDto)null;

        var bookCreateDto3 = (BookCreateDto)null;


        var listOfBookCreateDtos = new List<BookCreateDto>() { bookCreateDto1, bookCreateDto2, bookCreateDto3 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var result = await _bookCollectionsService.CreateBooksAsync(listOfBookCreateDtos);
        });
        _fixture.ResetMocks();

    }
}
