using LibraryManagement.Test.Fixtures;
using LibraryManagement.Test.Test_Data_Builders;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Implementations;
using Moq;

namespace LibraryManagement.Test.Services;
public class AuthorsServiceTests : IClassFixture<AuthorsServiceFixture>
{
    private readonly AuthorsServiceFixture _fixture;
    private readonly LibraryDbContext _dbContext;
    private readonly AuthorsService _authorsService;
    public AuthorsServiceTests(AuthorsServiceFixture authorsServiceFixture)
    {
        _fixture = authorsServiceFixture;
        _dbContext = authorsServiceFixture.DbContext;
        _authorsService = authorsServiceFixture.AuthorsService;
    }
    [Fact]
    public async Task CreateAuthorAsync_WhenValidAuthorProvided_ReturnsCreatedAuthor()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                        .Build();
        var authorCreateDtoBuilder = new AuthorCreateDtoBuilder();
        var authorCreateDto = authorCreateDtoBuilder
                         .Build();
        // setup mapper mock
        _fixture.AuthorMapperMock.Setup(m => m.ToAuthor(It.IsAny<AuthorCreateDto>())).Returns(author);
        //Act
        var createdAuthor = await _authorsService.CreateAuthorAsync(authorCreateDto);
        //Assert
        Assert.NotNull(createdAuthor);
        Assert.Equal(author.FirstName, createdAuthor.FirstName);
        Assert.Equal(author.LastName, createdAuthor.LastName);
    }
    [Fact]
    public async Task CreateAuthorAsync_WhenNullAuthorCreateDtoProvided_ThrowsException()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                    .WithEmail("author5gmail.com")
                    .Build();
        var authorCreateDtoBuilder = new AuthorCreateDtoBuilder();
        var authorCreateDto = (AuthorCreateDto)null;
        // setup mapper mock
        _fixture.AuthorMapperMock.Setup(m => m.ToAuthor(It.IsAny<AuthorCreateDto>())).Returns(author);
        //Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _authorsService.CreateAuthorAsync(authorCreateDto);
        });
    }
    [Fact]
    public async Task DeleteAuthorAsync_WhenAuthorIdIsProvided_DeleteAuthor()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                    .WithEmail("author5gmail.com")
                    .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        //Act
        var result = await _authorsService.DeleteAuthorAsync(author.Id);
        //Assert
        Assert.NotNull(result);
        Assert.Equal(author.Id, result.Id);
    }
    [Fact]
    public async Task DeleteAuthorAsync_WhenEmptyIdIsProvided_ThrowsException()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
            .WithEmail("author6gmail.com")
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _authorsService.DeleteAuthorAsync(Guid.Empty);
        });
    }
    [Fact]
    public async Task DeleteAuthorAsync_WhenAuthorNotFound_ThrowsException()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
            .WithEmail("author7gmail.com")
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        //Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _authorsService.DeleteAuthorAsync(Guid.NewGuid());
        });
    }

    [Fact]
    public async Task GetAuthorByEmailAsync_WhenValidEmailProvided_ReturnAuthor()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
            .WithEmail("author8gmail.com")
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();
        //Act
        var result = await _authorsService.GetAuthorByEmailAsync(author.Email);
        //Assert
        Assert.NotNull(result);
        Assert.Equal(author.Id, result.Id);
        Assert.Equal(author.Email, result.Email);
        Assert.Equal(author.FirstName, result.FirstName);

    }
    [Fact]
    public async Task GetAuthorByEmailAsync_WhenNullEmailProvided_ThrowsException()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                         .WithEmail("authoremail@gmail.com")
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();
        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _authorsService.GetAuthorByEmailAsync(null);
        });
    }

    [Fact]
    public async Task GetAuthorByEmailAsync_WhenEmailNotFound_ThrowsException()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        //Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _authorsService.GetAuthorByEmailAsync("test@email.com");
        });
    }

    [Fact]
    public async Task GetAuthorByIdAsync_WhenValidIdIsProvided_ReturnAuthor()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();
        //Act
        var result = await _authorsService.GetAuthorByIdAsync(author.Id);
        //Assert
        Assert.NotNull(result);
        Assert.Equal(author.Id, result.Id);
        Assert.Equal(author.Email, result.Email);
        Assert.Equal(author.FirstName, result.FirstName);

    }
    [Fact]
    public async Task GetAuthorByIdAsync_WhenEmptyIdProvided_ThrowsException()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();
        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _authorsService.GetAuthorByIdAsync(Guid.Empty);
        });
    }

    [Fact]
    public async Task GetAuthorByIdAsync_WhenIdNotFound_ThrowsException()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        //Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _authorsService.GetAuthorByIdAsync(Guid.NewGuid());
        });
    }

    [Fact]
    public async Task ListAllAuthors_WithPaginationAndSearch_ReturnAllAuthors()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author1 = authorBuilder
            .WithEmail("author1@mail.com")
            .WithId(Guid.NewGuid())
           .Build();
        var author2 = authorBuilder
            .WithEmail("author2@mail.com")
            .WithId(Guid.NewGuid())
           .Build();
        var author3 = authorBuilder
            .WithEmail("author3@mail.com")
            .WithId(Guid.NewGuid())
           .Build();

        _dbContext.Authors.AddRange(author1, author2, author3);
        _dbContext.SaveChanges();

        QueryOptions queryOptions = new()
        {
            PageNumber = 1,
            PageSize = 2,
            SearchTerm = "author"
        };

        //Act
        var result = await _authorsService.ListAllAuthorsAsync(queryOptions);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
    [Fact]
    public async Task UpdateAuthorAsync_WhenValidAuthorDtoProvided_ReturnCreatedAuthor()
    {
        //Arrange
        var authorBuilder = new AuthorBuilder();
        var author = authorBuilder
                        .WithEmail("author4gmail.com")
                        .Build();
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        var authorUpdateDto = new AuthorUpdateDto
        {
            FirstName = author.FirstName,
            LastName = author.LastName,
            Email = author.Email,
            Bio = author.Bio
        };

        // setup mapper mock
        _fixture.AuthorMapperMock.Setup(m => m.UpdateFromDto(It.IsAny<Author>(), It.IsAny<AuthorUpdateDto>()))
            .Callback<Author, AuthorUpdateDto>((author, dto) =>
            {
                author.FirstName = dto.FirstName;
                author.LastName = dto.LastName;
                author.Email = dto.Email;
                author.Bio = dto.Bio;
            }).Returns(author);
        //Act
        var updatedAuthor = await _authorsService.UpdateAuthorAsync(author.Id, authorUpdateDto);
        //Assert
        Assert.NotNull(updatedAuthor);
        Assert.Equal(author.FirstName, updatedAuthor.FirstName);
        Assert.Equal(author.LastName, updatedAuthor.LastName);
    }
}

