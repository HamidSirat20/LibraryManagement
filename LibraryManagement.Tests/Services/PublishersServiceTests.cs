using LibraryManagement.Test.Fixtures;
using LibraryManagement.Test.Test_Data_Builders;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Implementations;
using Moq;

namespace LibraryManagement.Test.Services;
public class PublishersServiceTests : IClassFixture<PublishersServiceFixture>
{
    private readonly PublishersServiceFixture _fixture;
    private readonly PublishersService _publishersService;
    private readonly LibraryDbContext _dbContext;
    public PublishersServiceTests(PublishersServiceFixture fixture)
    {
        _fixture = fixture;
        _publishersService = fixture.PublishersService;
        _dbContext = fixture.DbContext;
    }
    [Fact]
    public async Task CreatePublisherAsync_WhenValidDtoProvided_CreatesPublisher()
    {
        // Arrange
        var publisherBuilder = new PublisherBuilder();
        var publisher = publisherBuilder
            .WithId(Guid.NewGuid())
            .WithName("Penguin Random House")
            .WithAddress("1745 Broadway, New York, NY")
            .WithEmail("contact@penguinrandomhouse.com")
            .WithWebsite("https://penguinrandomhouse.com")
            .Build();
        var publisherCreateDto = new PublisherCreateDto
        {
            Name = "Penguin Random House",
            Address = "1745 Broadway, New York, NY",
            Email = "contact@penguinrandomhouse.com",
            Website = "https://penguinrandomhouse.com"
        };

        // Setup mapper mock
        _fixture.PublishersMapperMock.Setup(m => m.ToPublisher(publisherCreateDto))
            .Returns(publisher);

        // Act
        var result = await _publishersService.CreatePublisherAsync(publisherCreateDto);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(publisherCreateDto.Name, result!.Name);
        Assert.Equal(publisherCreateDto.Address, result.Address);
        Assert.Equal(publisherCreateDto.Email, result.Email);
        _fixture.Reset();
    }
    [Fact]
    public async Task CreatePublisherAsync_WhenNullDtoProvided_ThrowsException()
    {
        // Arrange

        var publisherCreateDto = (PublisherCreateDto)null;
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _publishersService.CreatePublisherAsync(publisherCreateDto);
        });
        _fixture.Reset();
    }
    [Fact]
    public async Task DeletePublisherAsync_WhenValidPublisherIdProvided_DeletePublisher()
    {
        // Arrange
        var publisherBuilder = new PublisherBuilder();
        var publisher = publisherBuilder
            .WithId(Guid.NewGuid())
            .WithName("Penguin Random House")
            .WithAddress("1745 Broadway, New York, NY")
            .WithEmail("contact@penguinrandomhouse.com")
            .WithWebsite("https://penguinrandomhouse.com")
            .Build();

        _dbContext.Publishers.Add(publisher);
        await _dbContext.SaveChangesAsync();
        // Act
        var result = await _publishersService.DeletePublisherAsync(publisher.Id);
        // Assert
        Assert.NotNull(result);
        Assert.Null(_dbContext.Publishers.Find(publisher.Id));
        Assert.True(_dbContext.Publishers.Count(e => e.Id == publisher.Id) == 0);
        _fixture.Reset();
    }
    [Fact]
    public async Task DeletePublisherAsync_WhenPublisherHasBooks_ThrowsException()
    {
        // Arrange
        var publisherBuilder = new PublisherBuilder();
        var publisher = publisherBuilder
            .WithId(Guid.NewGuid())
            .WithName("Penguin Random House")
            .WithAddress("1745 Broadway, New York, NY")
            .WithEmail("contact@penguinrandomhouse.com")
            .WithWebsite("https://penguinrandomhouse.com")
            .Build();
        _dbContext.Publishers.Add(publisher);
        await _dbContext.SaveChangesAsync();

        var author = new AuthorBuilder()
         .WithId(Guid.NewGuid())
         .WithName("F. Scott", "Fitzgerald")
         .WithEmail("author@gmail.com")
         .Build();

        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

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

        _dbContext.Books.Add(newBook);
        await _dbContext.SaveChangesAsync();


        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _publishersService.DeletePublisherAsync(publisher.Id);
        });

    }
    [Fact]
    public async Task GetPublisherByEmailAsync_WhenValidPublisherEmailProvided_ReturnPublisher()
    {
        // Arrange
        var publisherBuilder = new PublisherBuilder();
        var publisher = publisherBuilder
            .WithId(Guid.NewGuid())
            .WithName("Penguin Random House")
            .WithAddress("1745 Broadway, New York, NY")
            .WithEmail("contact@penguinrandomhouse.com")
            .WithWebsite("https://penguinrandomhouse.com")
            .Build();
        _dbContext.Publishers.Add(publisher);
        await _dbContext.SaveChangesAsync();

        // Act 
        var foundPublisher = _publishersService.GetPublisherByEmailAsync("contact@penguinrandomhouse.com");

        // Assert
        Assert.NotNull(foundPublisher);
        Assert.Equal(publisher.Email, foundPublisher.Result!.Email);

    }
    [Fact]
    public async Task GetPublisherByEmailAsync_WhenPublisherNotFound_ThrowsException()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _publishersService.GetPublisherByEmailAsync("publisher@email.com");
        });
    }
    [Fact]
    public async Task GetPublisherByIdAsync_WhenValidPublisherIdProvided_ReturnPublisher()
    {
        // Arrange
        var publisherBuilder = new PublisherBuilder();
        var publisher = publisherBuilder
            .WithId(Guid.NewGuid())
            .WithName("Penguin Random House")
            .WithAddress("1745 Broadway, New York, NY")
            .WithEmail("contact@penguinrandomhouse.com")
            .WithWebsite("https://penguinrandomhouse.com")
            .Build();
        _dbContext.Publishers.Add(publisher);
        await _dbContext.SaveChangesAsync();

        // Act 
        var foundPublisher = _publishersService.GetPublisherByIdAsync(publisher.Id);

        // Assert
        Assert.NotNull(foundPublisher);
        Assert.Equal(publisher.Email, foundPublisher.Result!.Email);

    }
    [Fact]
    public async Task GetPublisherByIdAsync_WhenPublisherNotFound_ThrowsException()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _publishersService.GetPublisherByIdAsync(Guid.NewGuid());
        });
    }

    [Fact]
    public async Task GetAllPublishersAsync_WhenValidPaginationAndSearchTermProvided_ReturnPublishers()
    {
        // Arrange
        var publisherBuilder = new PublisherBuilder();
        var publisher1 = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Penguin Random House")
            .WithAddress("1745 Broadway, New York, NY")
            .WithEmail("contact@penguinrandomhouse.com")
            .WithWebsite("https://penguinrandomhouse.com")
            .Build();

        var publisher2 = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .WithName("HarperCollins")
            .WithAddress("195 Broadway, New York, NY")
            .WithEmail("info@harpercollins.com")
            .WithWebsite("https://harpercollins.com")
            .Build();

        var publisher3 = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Scribner")
            .WithAddress("123 Publisher St, New York, NY")
            .WithEmail("publisher@scribner.com")
            .WithWebsite("https://scribner.com")
            .Build();

        _dbContext.Publishers.AddRange(publisher1, publisher2, publisher3);
        await _dbContext.SaveChangesAsync();

        var queryOptions = new QueryOptions()
        {
            PageNumber = 1,
            PageSize = 2,
            SearchTerm = "New York"
        };

        // Act
        var result = await _publishersService.ListAllPublisherAsync(queryOptions);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        _fixture.Reset();
    }

    [Fact]
    public async Task UpdatePublisherAsync_WhenValidDtoProvided_UpdatePublisher()
    {
        // Arrange
        var publisherBuilder = new PublisherBuilder();
        var publisher = new PublisherBuilder()
                     .WithId(Guid.NewGuid())
                     .WithName("HarperCollins")
                     .WithAddress("195 Broadway, New York, NY")
                     .WithEmail("info@harpercollins.com")
                     .WithWebsite("https://harpercollins.com")
                     .Build();

        _dbContext.Publishers.Add(publisher);
        await _dbContext.SaveChangesAsync();

        var publisherUpdateDto = new PublisherUpdateDto
        {
            Name = "Penguin Random House",
            Address = "1745 Broadway, New York, NY",
            Email = "contact@penguinrandomhouse.com",
            Website = "https://penguinrandomhouse.com"
        };

        // Setup mapper mock
        _fixture.PublishersMapperMock
             .Setup(m => m.UpdateFromDto(It.IsAny<Publisher>(), It.IsAny<PublisherUpdateDto>()))
             .Callback<Publisher, PublisherUpdateDto>((entity, dto) =>
             {
                 // Simulate the actual mapping behavior
                 entity.Name = dto.Name;
                 entity.Address = dto.Address;
                 entity.Email = dto.Email;
                 entity.Website = dto.Website;
             });

        // Act
        var result = await _publishersService.UpdatePublisherAsync(publisher.Id, publisherUpdateDto);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(publisherUpdateDto.Name, result!.Name);
        Assert.Equal(publisherUpdateDto.Address, result.Address);
        Assert.Equal(publisherUpdateDto.Email, result.Email);
        _fixture.Reset();
    }
}
