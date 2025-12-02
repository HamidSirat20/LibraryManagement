using AutoFixture;
using LibraryManagement.Test.Fixtures;
using LibraryManagement.Test.Test_Data_Builders;
using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Implementations;
using Moq;
using System.Runtime.Intrinsics.X86;
using static LibraryManagement.WebAPI.Helpers.ApiRoutes;

namespace LibraryManagement.Test.Services;
public class LoanServiceTests : IClassFixture<LoansServiceFixture>
{
    private readonly LoansServiceFixture _fixture;
    private readonly LibraryDbContext _dbContext;
    private readonly LoansService _loanService;
    public LoanServiceTests(LoansServiceFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
        _loanService = fixture.LoansService;
    }

    [Fact]
    public async Task MakeLoanAsync_WhenValidLoanDtoAndUserIdProvided_ShouldLoanBook()
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

        var userBuilder = new UserBuilder();
        var user = userBuilder
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Users.Add(user);
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

        var loanCreateDto = new LoanCreateDto
        {
            BookId = newBook.Id
        };

        // setup mapper
        _fixture.LoansMapperMock.Setup(m => m.ToLoanReadDto(It.IsAny<Loan>()))
            .Returns<Loan>(b => new LoanReadDto
            {
                Id = b.Id,
                UserId = b.UserId,
                UserName = b.User.FullName,
                BookId = b.BookId,
                BookTitle = b.Book.Title,
                LoanDate = b.LoanDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                LoanStatus = b.LoanStatus,
                LateFee = b.LateFee,
                LateReturnOrLostFees = b.LateReturnOrLostFees?.Select(f => new LateReturnOrLostFeeReadDto()
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    UserName = f.User.FullName,
                    LoanId = f.LoanId,
                    BookTitle = f.Loan.Book.Title,
                    Amount = f.Amount,
                    IssuedDate = f.IssuedDate,
                    PaidDate = f.PaidDate,
                    Status = f.Status
                }).ToList()

            });

        // Act
        var loanDto = await _loanService.MakeLoanAsync(loanCreateDto, user.Id);

        // Assert

        Assert.NotNull(loanDto);
        Assert.Equal(user.Id, loanDto.UserId);
        Assert.Equal(newBook.Id, loanDto.BookId);
        Assert.Equal(LoanStatus.Active, loanDto.LoanStatus);
        _fixture.Reset();
    }
    [Fact]
    public async Task MakeLoanAsync_WhenBookIsNotAvailable_ThrowsException()
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

        var userBuilder = new UserBuilder();
        var userA = userBuilder
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Users.Add(userA);
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
        // setup mapper
        _fixture.LoansMapperMock.Setup(m => m.ToLoanReadDto(It.IsAny<Loan>()))
            .Returns<Loan>(b => new LoanReadDto
            {
                Id = b.Id,
                UserId = b.UserId,
                UserName = b.User.FullName,
                BookId = b.BookId,
                BookTitle = b.Book.Title,
                LoanDate = b.LoanDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                LoanStatus = b.LoanStatus,
                LateFee = b.LateFee,
                LateReturnOrLostFees = b.LateReturnOrLostFees?.Select(f => new LateReturnOrLostFeeReadDto()
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    UserName = f.User.FullName,
                    LoanId = f.LoanId,
                    BookTitle = f.Loan.Book.Title,
                    Amount = f.Amount,
                    IssuedDate = f.IssuedDate,
                    PaidDate = f.PaidDate,
                    Status = f.Status
                }).ToList()

            });


        var loanCreateDto = new LoanCreateDto
        {
            BookId = newBook.Id
        };

        //reserve the book first so loan satatus becomes not available
        await _loanService.MakeLoanAsync(loanCreateDto, userA.Id);


        var userB = userBuilder
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Users.Add(userB);
        await _dbContext.SaveChangesAsync();


        // Act & Assert

        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _loanService.MakeLoanAsync(loanCreateDto, userB.Id);
        });
        _fixture.Reset();

    }

    [Fact]
    public async Task GetYourOwnLoansAsync_WhenValidUserIdProvided_ShouldReturnLoans()
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

        _dbContext.Books.Add(newBook);
        await _dbContext.SaveChangesAsync();

        var userBuilder = new UserBuilder();
        var user = userBuilder
            .WithId(Guid.NewGuid())
            .Build();


        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var loan = new Loan
              (
                id: Guid.NewGuid(),
                bookId: newBook.Id,
                userId: user.Id,
                loanDate: DateTime.UtcNow,
                dueDate: DateTime.UtcNow.AddDays(14)
              );

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();
        // 
        _fixture.LoansMapperMock.Setup(m => m.ToLoanReadDto(It.IsAny<Loan>()))
            .Returns<Loan>(b => new LoanReadDto
            {
                Id = b.Id,
                UserId = b.UserId,
                UserName = b.User.FullName,
                BookId = b.BookId,
                BookTitle = b.Book.Title,
                LoanDate = b.LoanDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                LoanStatus = b.LoanStatus,
                LateFee = b.LateFee,
                LateReturnOrLostFees = b.LateReturnOrLostFees?.Select(f => new LateReturnOrLostFeeReadDto()
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    UserName = f.User.FullName,
                    LoanId = f.LoanId,
                    BookTitle = f.Loan.Book.Title,
                    Amount = f.Amount,
                    IssuedDate = f.IssuedDate,
                    PaidDate = f.PaidDate,
                    Status = f.Status
                }).ToList()

            });

        // Act
        var loans = await _loanService.GetYourOwnLoansAsync(user.Id);
        // Assert

        Assert.NotNull(loans);
        Assert.Single(loans);
        // Assert.Equal(user.Id, loans.First().UserId);
        Assert.Equal(newBook.Id, loans.First().BookId);
        _fixture.Reset();
    }

    [Fact]
    public async Task GetLoansByUserIdAsync_WhenValidUserIdProvided_ShouldReturnLoans()
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

        _dbContext.Books.Add(newBook);
        await _dbContext.SaveChangesAsync();
        // 
        _fixture.LoansMapperMock.Setup(m => m.ToLoanReadDto(It.IsAny<Loan>()))
            .Returns<Loan>(b => new LoanReadDto
            {
                Id = b.Id,
                UserId = b.UserId,
                UserName = b.User.FullName,
                BookId = b.BookId,
                BookTitle = b.Book.Title,
                LoanDate = b.LoanDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                LoanStatus = b.LoanStatus,
                LateFee = b.LateFee,
                LateReturnOrLostFees = b.LateReturnOrLostFees?.Select(f => new LateReturnOrLostFeeReadDto()
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    UserName = f.User.FullName,
                    LoanId = f.LoanId,
                    BookTitle = f.Loan.Book.Title,
                    Amount = f.Amount,
                    IssuedDate = f.IssuedDate,
                    PaidDate = f.PaidDate,
                    Status = f.Status
                }).ToList()

            });

        var userBuilder = new UserBuilder();
        var user = userBuilder
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var loanCreateDto = new LoanCreateDto
        {
            BookId = newBook.Id
        };
        var loanDto = await _loanService.MakeLoanAsync(loanCreateDto, user.Id);

        // Act
        var loans = await _loanService.GetYourOwnLoansAsync(user.Id);
        // Assert

        Assert.NotNull(loans);
        Assert.Single(loans);
        Assert.Equal(user.Id, loans.First().UserId);
        Assert.Equal(newBook.Id, loans.First().BookId);
        _fixture.Reset();
    }
    [Fact]
    public async Task GetAllLoansAsync_WhenQueryOptionsAreValid_ShouldReturnPaginatedLoans()
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

        var book1 = new BookBuilder()
             .WithId(Guid.NewGuid())
             .WithTitle("The Great Gatsby")
             .WithAuthor(author.Id)
             .WithCoverImage("https://example.com/gatsby.jpg")
             .WithDescription("A classic novel about wealth and tragedy in the Jazz Age.")
             .WithPublishedDate(new DateTime(1925, 4, 10))
             .WithGenre(Genre.Fiction)
             .WithPages(180)
             .WithPublisher(publisher.Id)
             .Build();

        var book2 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("To Kill a Mockingbird")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/mockingbird.jpg")
            .WithDescription("A powerful story about justice and innocence in the American South.")
            .WithPublishedDate(new DateTime(1960, 7, 11))
            .WithGenre(Genre.Romance)
            .WithPages(281)
            .WithPublisher(publisher.Id)
            .Build();

        var book3 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("1984")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/1984.jpg")
            .WithDescription("A dystopian novel exploring surveillance and totalitarianism.")
            .WithPublishedDate(new DateTime(1949, 6, 8))
            .WithGenre(Genre.ScienceFiction)
            .WithPages(328)
            .WithPublisher(publisher.Id)
            .Build();


        _dbContext.Books.AddRange(book1, book2, book3);
        await _dbContext.SaveChangesAsync();

        var userBuilder = new UserBuilder();
        var user1 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();

        var user2 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();
        var user3 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();


        _dbContext.Users.AddRange(user1, user2, user3);
        await _dbContext.SaveChangesAsync();

        var loanCreateDto1 = new LoanCreateDto
        {
            BookId = book1.Id
        };
        var loanCreateDto2 = new LoanCreateDto
        {
            BookId = book2.Id
        };
        var loanCreateDto3 = new LoanCreateDto
        {
            BookId = book3.Id
        };
        var loanDto1 = await _loanService.MakeLoanAsync(loanCreateDto1, user1.Id);
        var loanDto2 = await _loanService.MakeLoanAsync(loanCreateDto2, user2.Id);
        var loanDto3 = await _loanService.MakeLoanAsync(loanCreateDto3, user3.Id);

        var queryOptions = new QueryOptions()
        {
            PageNumber = 1,
            PageSize = 2,
            SearchTerm = null,
        };

        // Act
        var loans = await _loanService.GetAllLoansAsync(queryOptions);
        // Assert

        Assert.Equal(2, loans.Count());
        Assert.NotNull(loans);
        _fixture.Reset();
    }
    [Fact]
    public async Task GetAllLoansAsync_WhenNoSearchTermProvided_ShouldReturnZeroLoans()
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

        var book1 = new BookBuilder()
             .WithId(Guid.NewGuid())
             .WithTitle("The Great Gatsby")
             .WithAuthor(author.Id)
             .WithCoverImage("https://example.com/gatsby.jpg")
             .WithDescription("A classic novel about wealth and tragedy in the Jazz Age.")
             .WithPublishedDate(new DateTime(1925, 4, 10))
             .WithGenre(Genre.Fiction)
             .WithPages(180)
             .WithPublisher(publisher.Id)
             .Build();

        var book2 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("To Kill a Mockingbird")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/mockingbird.jpg")
            .WithDescription("A powerful story about justice and innocence in the American South.")
            .WithPublishedDate(new DateTime(1960, 7, 11))
            .WithGenre(Genre.Romance)
            .WithPages(281)
            .WithPublisher(publisher.Id)
            .Build();

        var book3 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("1984")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/1984.jpg")
            .WithDescription("A dystopian novel exploring surveillance and totalitarianism.")
            .WithPublishedDate(new DateTime(1949, 6, 8))
            .WithGenre(Genre.ScienceFiction)
            .WithPages(328)
            .WithPublisher(publisher.Id)
            .Build();


        _dbContext.Books.AddRange(book1, book2, book3);
        await _dbContext.SaveChangesAsync();

        var userBuilder = new UserBuilder();
        var user1 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();
        var user2 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();
        var user3 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Users.AddRange(user1, user2, user3);
        await _dbContext.SaveChangesAsync();

        var loanCreateDto1 = new LoanCreateDto
        {
            BookId = book1.Id
        };
        var loanCreateDto2 = new LoanCreateDto
        {
            BookId = book2.Id
        };
        var loanCreateDto3 = new LoanCreateDto
        {
            BookId = book3.Id
        };
        var loanDto1 = await _loanService.MakeLoanAsync(loanCreateDto1, user1.Id);
        var loanDto2 = await _loanService.MakeLoanAsync(loanCreateDto2, user2.Id);
        var loanDto3 = await _loanService.MakeLoanAsync(loanCreateDto3, user3.Id);

        var queryOptions = new QueryOptions()
        {
            PageNumber = 1,
            PageSize = 2,
            SearchTerm = "Pro C# 10 with .NET 6: Foundational Principles and Practices in Programming"
        };

        // Act
        var loans = await _loanService.GetAllLoansAsync(queryOptions);
        // Assert

        Assert.Equal(0, loans.Count());
        Assert.Empty(loans);
        _fixture.Reset();

    }
    [Fact]
    public async Task GetAllLoansAsync_WhenPaginationExceedsExistingLoans_ShouldReturnLoans()
    {
        _fixture.Reset();
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

        var book1 = new BookBuilder()
             .WithId(Guid.NewGuid())
             .WithTitle("The Great Gatsby")
             .WithAuthor(author.Id)
             .WithCoverImage("https://example.com/gatsby.jpg")
             .WithDescription("A classic novel about wealth and tragedy in the Jazz Age.")
             .WithPublishedDate(new DateTime(1925, 4, 10))
             .WithGenre(Genre.Fiction)
             .WithPages(180)
             .WithPublisher(publisher.Id)
             .Build();

        var book2 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("To Kill a Mockingbird")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/mockingbird.jpg")
            .WithDescription("A powerful story about justice and innocence in the American South.")
            .WithPublishedDate(new DateTime(1960, 7, 11))
            .WithGenre(Genre.Romance)
            .WithPages(281)
            .WithPublisher(publisher.Id)
            .Build();

        var book3 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("1984")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/1984.jpg")
            .WithDescription("A dystopian novel exploring surveillance and totalitarianism.")
            .WithPublishedDate(new DateTime(1949, 6, 8))
            .WithGenre(Genre.ScienceFiction)
            .WithPages(328)
            .WithPublisher(publisher.Id)
            .Build();


        _dbContext.Books.AddRange(book1, book2, book3);
        await _dbContext.SaveChangesAsync();

        var userBuilder = new UserBuilder();
        var user1 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();
        var user2 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();
        var user3 = userBuilder
            .WithId(Guid.NewGuid())
            .Build();

        _dbContext.Users.AddRange(user1, user2, user3);
        await _dbContext.SaveChangesAsync();

        var loan1 = new Loan
                    (
                      id: Guid.NewGuid(),
                      bookId: book1.Id,
                      userId: user1.Id,
                      loanDate: DateTime.UtcNow,
                      dueDate: DateTime.UtcNow.AddDays(14)
                    );
        var loan2 = new Loan
                    (
                      id: Guid.NewGuid(),
                      bookId: book2.Id,
                      userId: user2.Id,
                      loanDate: DateTime.UtcNow,
                      dueDate: DateTime.UtcNow.AddDays(14)
                    );
        var loan3 = new Loan
                    (
                      id: Guid.NewGuid(),
                      bookId: book3.Id,
                      userId: user3.Id,
                      loanDate: DateTime.UtcNow,
                      dueDate: DateTime.UtcNow.AddDays(14)
                    );

        _dbContext.Loans.AddRange(loan1, loan2, loan3);
        await _dbContext.SaveChangesAsync();

        var queryOptions = new QueryOptions()
        {
            PageNumber = 1,
            PageSize = 7,
            SearchTerm = null,
        };

        // Act
        var loans = await _loanService.GetAllLoansAsync(queryOptions);
        // Assert

        Assert.Equal(3, loans.Count());
        Assert.NotNull(loans);
        _fixture.Reset();
    }
    [Fact]
    public async Task GetLoanByIdAsync_WhenValidLoanIdProvided_ShouldReturnLoan()
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

        var book1 = new BookBuilder()
             .WithId(Guid.NewGuid())
             .WithTitle("The Great Gatsby")
             .WithAuthor(author.Id)
             .WithCoverImage("https://example.com/gatsby.jpg")
             .WithDescription("A classic novel about wealth and tragedy in the Jazz Age.")
             .WithPublishedDate(new DateTime(1925, 4, 10))
             .WithGenre(Genre.Fiction)
             .WithPages(180)
             .WithPublisher(publisher.Id)
             .Build();

        _dbContext.Books.Add(book1);
        await _dbContext.SaveChangesAsync();

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Johnson",
            Phone = "0456789123",
            Email = "alice.johnson@example.com",
            Password = "hashed_password_1",
            Address = "123 Maple Street, Denver, CO",
            AvatarUrl = "https://postimg.cc/AliceAvatar",
            Salt = new byte[] { 1, 2, 3 },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        await _dbContext.SaveChangesAsync();

        var loan = new Loan
     (
         id: Guid.NewGuid(),
         bookId: book1.Id,
         userId: user1.Id,
         loanDate: DateTime.UtcNow,
         dueDate: DateTime.UtcNow.AddDays(14)
     );

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();
        // Act
        var loanFound = await _loanService.GetLoanByIdAsync(loan.Id);
        // Assert

        Assert.Equal(loan.Id, loanFound.Id);
        Assert.NotNull(loanFound);
        _fixture.Reset();

    }

    [Fact]
    public async Task GetLoanByIdAsync_WhenLoanIsNull_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _loanService.GetLoanByIdAsync(Guid.NewGuid());
        });
    }

    [Fact]
    public async Task ReturnBookAsync_WhenValidLoanIdProvided_ShouldMakeLoanStatusReturned()
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

        var book1 = new BookBuilder()
             .WithId(Guid.NewGuid())
             .WithTitle("The Great Gatsby")
             .WithAuthor(author.Id)
             .WithCoverImage("https://example.com/gatsby.jpg")
             .WithDescription("A classic novel about wealth and tragedy in the Jazz Age.")
             .WithPublishedDate(new DateTime(1925, 4, 10))
             .WithGenre(Genre.Fiction)
             .WithPages(180)
             .WithPublisher(publisher.Id)
             .Build();

        _dbContext.Books.Add(book1);
        await _dbContext.SaveChangesAsync();

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Johnson",
            Phone = "0456789123",
            Email = "alice.johnson@example.com",
            Password = "hashed_password_1",
            Address = "123 Maple Street, Denver, CO",
            AvatarUrl = "https://postimg.cc/AliceAvatar",
            Salt = new byte[] { 1, 2, 3 },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user1);
        await _dbContext.SaveChangesAsync();

        var loan = new Loan
     (
         id: Guid.NewGuid(),
         bookId: book1.Id,
         userId: user1.Id,
         loanDate: DateTime.UtcNow,
         dueDate: DateTime.UtcNow.AddDays(14)
     );

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();

        _fixture.EmailsTemplateServiceMock
      .Setup(b => b.GetReservationReadyTemplate(
          user1.FirstName,
          user1.LastName,
          book1.Title,
          It.IsAny<DateTime>()))
      .Returns("EMAIL_TEMPLATE_BODY");
        _fixture.ReservationQueueServiceMock
            .Setup(b => b.ProcessNextReservationAfterReturnAsync(
                book1.Id,
                "Book is ready for pickup",
                "EMAIL_TEMPLATE_BODY"))
            .Returns(Task.CompletedTask);
        _dbContext.ChangeTracker.Clear();
        // Act
        var loanFound = await _loanService.ReturnBookAsync(loan.Id);
        // Assert

        Assert.Equal(LoanStatus.Returned, loanFound.LoanStatus);
        Assert.NotNull(loanFound);
        _fixture.Reset();

    }
    [Fact]
    public async Task ReturnBookAsync_WhenLoanNotFound_ShouldThrowException()
    {
        //Arrange
        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _loanService.ReturnBookAsync(Guid.NewGuid());
        });
        _fixture.Reset();
    }

    [Fact]
    public async Task UpdateLoanAsync_WhenValidLoanIdProvided_ShouldMakeLoanStatusReturned()
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

        var book1 = new BookBuilder()
             .WithId(Guid.NewGuid())
             .WithTitle("The Great Gatsby")
             .WithAuthor(author.Id)
             .WithCoverImage("https://example.com/gatsby.jpg")
             .WithDescription("A classic novel about wealth and tragedy in the Jazz Age.")
             .WithPublishedDate(new DateTime(1925, 4, 10))
             .WithGenre(Genre.Fiction)
             .WithPages(180)
             .WithPublisher(publisher.Id)
             .Build();

        _dbContext.Books.Add(book1);
        await _dbContext.SaveChangesAsync();

        var userBuilder = new UserBuilder();
        var user = userBuilder
            .WithId(Guid.NewGuid())
            .Build();



        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var loan = new Loan
     (
         id: Guid.NewGuid(),
         bookId: book1.Id,
         userId: user.Id,
         loanDate: DateTime.UtcNow,
         dueDate: new DateTime(2025, 11, 1)
     );

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();
        // Act
        var loanFound = await _loanService.UpdateLoanAsync(loan.Id);

        var expectedDueDate = new DateTime(2025, 11, 1).AddDays(30);
        // Assert

        Assert.Equal(LoanStatus.Active, loanFound.LoanStatus);
        Assert.Equal(expectedDueDate.Date, loanFound.DueDate.Date);
        Assert.NotNull(loanFound);
        _fixture.Reset();

    }

    [Fact]
    public async Task UpdateLoanAsync_WhenBookHasActiveReservation_ShouldThrowInvalidOperationException()
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

        var book1 = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("The Great Gatsby")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/gatsby.jpg")
            .WithDescription("A classic novel about wealth and tragedy in the Jazz Age.")
            .WithPublishedDate(new DateTime(1925, 4, 10))
            .WithGenre(Genre.Fiction)
            .WithPages(180)
            .WithPublisher(publisher.Id)
            .Build();
        _dbContext.Books.Add(book1);
        await _dbContext.SaveChangesAsync();

        var loanUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Johnson",
            Phone = "0456789123",
            Email = "alice.johnson@example.com",
            Password = "pw",
            Address = "123 Maple Street",
            Salt = new byte[] { 1, 2, 3 },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            AvatarUrl = "https://example.com/avatar-bob.png"
        };
        _dbContext.Users.Add(loanUser);
        await _dbContext.SaveChangesAsync();

        var reservingUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Smith",
            Phone = "0245689312",
            Email = "bob.smith@example.com",
            Password = "pw2",
            Address = "555 Oak Street",
            Salt = new byte[] { 7, 8, 9 },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            AvatarUrl = "https://example.com/avatar-bob.png"
        };
        _dbContext.Users.Add(reservingUser);
        await _dbContext.SaveChangesAsync();

        // Create loan
        var loan = new Loan(
            id: Guid.NewGuid(),
            bookId: book1.Id,
            userId: loanUser.Id,
            loanDate: DateTime.UtcNow,
            dueDate: new DateTime(2025, 11, 1)
        );
        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();

        // Create active reservation for SAME BOOK by ANOTHER USER
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = book1.Id,
            UserId = reservingUser.Id,
            ReservationStatus = ReservationStatus.Pending,
            ReservedAt = DateTime.UtcNow
        };
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _loanService.UpdateLoanAsync(loan.Id)
        );
        _fixture.Reset();
    }

}
