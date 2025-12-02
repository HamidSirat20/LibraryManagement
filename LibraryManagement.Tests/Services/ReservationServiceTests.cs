using LibraryManagement.Test.Fixtures;
using LibraryManagement.Test.Test_Data_Builders;
using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using Moq;

namespace LibraryManagement.Test.Services;
public class ReservationServiceTests : IClassFixture<ReservationsServiceFixture>
{
    private readonly ReservationsServiceFixture _fixture;
    private readonly LibraryDbContext _dbContext;
    public ReservationServiceTests(ReservationsServiceFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
    }
    [Fact]
    public async Task CreateReservationAsync_WhenValidUserIdAndBookIdProvided_ReserveBook()
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
        await _dbContext.Books.AddAsync(newBook);
        await _dbContext.SaveChangesAsync();

        var userAlreadyLoanedBook = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("Ali", "Jawadi")
            .WithEmail("ali@gmail.com")
            .Build();
        _dbContext.Users.Add(userAlreadyLoanedBook);
        await _dbContext.SaveChangesAsync();

        // create a loan from the book so loaned book can be reserved
        var loan = new Loan
         (
             id: Guid.NewGuid(),
             bookId: newBook.Id,
             userId: userAlreadyLoanedBook.Id,
             loanDate: DateTime.UtcNow,
             dueDate: DateTime.UtcNow.AddDays(14)
         );

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();

        var userToLoanBook = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        await _dbContext.Users.AddAsync(userToLoanBook);
        await _dbContext.SaveChangesAsync();

        _fixture.ReservationsMapperMock
                .Setup(m => m.ToReservationReadDto(It.IsAny<Reservation>()))
                .Returns((Reservation r) => new ReservationReadDto
                {
                    BookId = r.BookId,
                    BookTitle = newBook.Title,
                    UserId = r.UserId,
                    ReservedAt = r.ReservedAt,
                    ReservationStatus = r.ReservationStatus,
                    QueuePosition = r.QueuePosition
                });

        var emailContent = _fixture.EmailsTemplateServiceMock.Setup(e => e.GetReservationConfirmationTemplate(userToLoanBook.FirstName, userToLoanBook.LastName
            , newBook.Title, new DateTime(2025, 11, 12), 1)).Returns("Book reserved");
        _fixture.EmailServiceMock.Setup(e => e.SendEmailAsync(userToLoanBook.Email, "Book Reservation Confirmation",
            It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        var result = await _fixture.ReservationsService.CreateReservationAsync(newBook.Id, userToLoanBook.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newBook.Id, result.BookId);
        Assert.Equal(ReservationStatus.Pending, result.ReservationStatus);
        Assert.Equal(1, result.QueuePosition);

        _fixture.Reset();
    }
    [Fact]
    public async Task CreateReservationAsync_WhenBookIsAvailableForLoan_ThrowsException()
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
        await _dbContext.Books.AddAsync(newBook);
        await _dbContext.SaveChangesAsync();

        var userToLoanBook = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        await _dbContext.Users.AddAsync(userToLoanBook);
        await _dbContext.SaveChangesAsync();

        _fixture.ReservationsMapperMock.Setup(m => m.ToReservationReadDto(It.IsAny<Reservation>()))
            .Returns(new ReservationReadDto
            {
                BookId = newBook.Id,
                BookTitle = newBook.Title,
                UserId = userToLoanBook.Id,
                ReservedAt = DateTime.UtcNow,
                ReservationStatus = ReservationStatus.Pending,
            });

        var emailContent = _fixture.EmailsTemplateServiceMock.Setup(e => e.GetReservationConfirmationTemplate(userToLoanBook.FirstName, userToLoanBook.LastName
            , newBook.Title, new DateTime(2025, 11, 12), 1)).Returns("Book reserved");
        _fixture.EmailServiceMock.Setup(e => e.SendEmailAsync(userToLoanBook.Email, "Book Reservation Confirmation",
            It.IsAny<string>())).Returns(Task.CompletedTask);
        // Act & Assert

        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _fixture.ReservationsService.CreateReservationAsync(newBook.Id, userToLoanBook.Id);
        });
        _fixture.Reset();
    }
    [Fact]
    public async Task CreateReservationAsync_WhenAlreadyHaveReservation_ThrowsExceptions()
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
        await _dbContext.Books.AddAsync(newBook);
        await _dbContext.SaveChangesAsync();

        //create user
        var userToLoanBook = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        _dbContext.Users.Add(userToLoanBook);
        await _dbContext.SaveChangesAsync();

        // create a loan from the book so loaned book can be reserved
        var loan = new Loan
         (
             id: Guid.NewGuid(),
             bookId: newBook.Id,
             userId: userToLoanBook.Id,
             loanDate: DateTime.UtcNow,
             dueDate: DateTime.UtcNow.AddDays(14)
         );

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();

        //create already existing reservation for the same user
        var existingReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = newBook.Id,
            UserId = userToLoanBook.Id,
            Book = newBook,
            User = userToLoanBook,
            ReservedAt = DateTime.UtcNow
        };
        _dbContext.Reservations.Add(existingReservation);
        await _dbContext.SaveChangesAsync();

        // setup mocks
        _fixture.ReservationsMapperMock
                  .Setup(m => m.ToReservationReadDto(It.IsAny<Reservation>()))
                  .Returns((Reservation r) => new ReservationReadDto
                  {
                      BookId = r.BookId,
                      BookTitle = newBook.Title,
                      UserId = r.UserId,
                      ReservedAt = r.ReservedAt,
                      ReservationStatus = r.ReservationStatus,
                      QueuePosition = r.QueuePosition
                  });

        var emailContent = _fixture.EmailsTemplateServiceMock.Setup(e => e.GetReservationConfirmationTemplate(userToLoanBook.FirstName, userToLoanBook.LastName
            , newBook.Title, new DateTime(2025, 11, 12), 1)).Returns("Book reserved");
        _fixture.EmailServiceMock.Setup(e => e.SendEmailAsync(userToLoanBook.Email, "Book Reservation Confirmation",
            It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _fixture.ReservationsService.CreateReservationAsync(newBook.Id, userToLoanBook.Id);
        });
        _fixture.Reset();
    }
    [Fact]
    public async Task CancelReservationAsync_WhenValidUserIdAndBookIdProvided_CancelReservation()
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
        await _dbContext.Books.AddAsync(newBook);
        await _dbContext.SaveChangesAsync();

        //create user
        var userToLoanedBook = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        _dbContext.Users.Add(userToLoanedBook);
        await _dbContext.SaveChangesAsync();

        // create a loan from the book so loaned book can be reserved
        var loan = new Loan
         (
             id: Guid.NewGuid(),
             bookId: newBook.Id,
             userId: userToLoanedBook.Id,
             loanDate: DateTime.UtcNow,
             dueDate: DateTime.UtcNow.AddDays(14)
         );

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();
        //create user
        var userReservedBook = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("Hanna", "Danish")
            .WithEmail("reserved@gmail.com")
            .Build();
        _dbContext.Users.Add(userReservedBook);
        await _dbContext.SaveChangesAsync();

        //create already existing reservation for the same user
        var existingReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = newBook.Id,
            UserId = userReservedBook.Id,
            Book = newBook,
            User = userReservedBook,
            ReservedAt = DateTime.UtcNow
        };
        _dbContext.Reservations.Add(existingReservation);
        await _dbContext.SaveChangesAsync();

        // Act
        var reservation = await _fixture.ReservationsService.CancelReservationAsync(existingReservation.Id, userReservedBook.Id);
        // Assert
        Assert.NotNull(reservation);
        Assert.Equal(ReservationStatus.Cancelled, reservation.ReservationStatus);
        _fixture.Reset();
    }
    [Fact]
    public async Task CancelReservationAsync_WhenDifferentUserTryToCancelReservation_ThrowsException()
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
        await _dbContext.Books.AddAsync(newBook);
        await _dbContext.SaveChangesAsync();

        //create user
        var userLoanedBook = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        _dbContext.Users.Add(userLoanedBook);
        await _dbContext.SaveChangesAsync();

        // create a loan from the book so loaned book can be reserved
        var loan = new Loan
         (
             id: Guid.NewGuid(),
             bookId: newBook.Id,
             userId: userLoanedBook.Id,
             loanDate: DateTime.UtcNow,
             dueDate: DateTime.UtcNow.AddDays(14)
         );

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();
        //create user
        var userReservedBook = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("Hanna", "Danish")
            .WithEmail("reserved@gmail.com")
            .Build();
        _dbContext.Users.Add(userReservedBook);
        await _dbContext.SaveChangesAsync();

        //create already existing reservation for the same user
        var existingReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = newBook.Id,
            UserId = userReservedBook.Id,
            Book = newBook,
            User = userReservedBook,
            ReservedAt = DateTime.UtcNow
        };
        _dbContext.Reservations.Add(existingReservation);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _fixture.ReservationsService.CancelReservationAsync(existingReservation.Id, userLoanedBook.Id);
        });
        _fixture.Reset();
    }
    [Fact]
    public async Task CancelReservationAsync_WhenReservationDoesNotExist_ThrowsException()
    {
        //Arrange
        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _fixture.ReservationsService.CancelReservationAsync(Guid.NewGuid(), Guid.NewGuid());
        });
        _fixture.Reset();
    }

    [Fact]
    public async Task PickReservationByIdAsync_WhenValidReservationIdAndUserIdProvided_ReturnsReservation()
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
            .WithAddress("123 Publisher St")
            .WithWebsite("https://scribner.com")
            .WithEmail("publisher@gmail.com")
            .Build();
        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var book = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("The Great Gatsby")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/gatsby.jpg")
            .WithDescription("A novel set in the Jazz Age")
            .WithPublishedDate(DateTime.Now)
            .WithGenre(Genre.Fiction)
            .WithPages(180)
            .WithPublisher(publisher.Id)
            .Build();

        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        // Create user
        var user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        //  Create Active loan (someone else is borrowing the book)
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            LoanStatus = LoanStatus.Active
        };

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();

        // Create reservation for this user
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            ReservedAt = DateTime.UtcNow,
            ReservationStatus = ReservationStatus.Pending
        };

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        // Return the book so it becomes pickable
        loan.LoanStatus = LoanStatus.Returned;
        _dbContext.Loans.Update(loan);

        // Mark reservation as NOTIFIED (required for pickup)
        reservation.ReservationStatus = ReservationStatus.Notified;
        _dbContext.Reservations.Update(reservation);

        await _dbContext.SaveChangesAsync();

        // Setup mapper mock
        _fixture.ReservationsMapperMock
            .Setup(m => m.ToReservationReadDto(It.IsAny<Reservation>()))
            .Returns((Reservation r) => new ReservationReadDto
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = book.Title,
                UserId = r.UserId,
                ReservedAt = r.ReservedAt,
                ReservationStatus = r.ReservationStatus
            });

        // Act 
        var result = await _fixture.ReservationsService
            .PickReservationByIdAsync(reservation.Id, user.Id);

        // Assert 
        Assert.NotNull(result);
        Assert.Equal(reservation.Id, result.Id);
        Assert.Equal(ReservationStatus.Fulfilled, result.ReservationStatus);

        _fixture.Reset();
    }
    [Fact]
    public async Task PickReservationByIdAsync_WhenReservationIsNull_ThrowsException()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _fixture.ReservationsService
                .PickReservationByIdAsync(Guid.NewGuid(), Guid.NewGuid());
        });

        _fixture.Reset();
    }
    [Fact]
    public async Task PickReservationByIdAsync_WhenSomeoneElseTriesToPickup_ThrowsException()
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
            .WithAddress("123 Publisher St")
            .WithWebsite("https://scribner.com")
            .WithEmail("publisher@gmail.com")
            .Build();
        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var book = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("The Great Gatsby")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/gatsby.jpg")
            .WithDescription("A novel set in the Jazz Age")
            .WithPublishedDate(DateTime.Now)
            .WithGenre(Genre.Fiction)
            .WithPages(180)
            .WithPublisher(publisher.Id)
            .Build();

        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        // Create user
        var user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        //  Create Active loan (someone else is borrowing the book)
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            LoanStatus = LoanStatus.Active
        };

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();

        // Create reservation for this user
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            ReservedAt = DateTime.UtcNow,
            ReservationStatus = ReservationStatus.Pending
        };

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        // Return the book so it becomes pickable
        loan.LoanStatus = LoanStatus.Returned;
        _dbContext.Loans.Update(loan);

        // Mark reservation as NOTIFIED (required for pickup)
        reservation.ReservationStatus = ReservationStatus.Notified;
        _dbContext.Reservations.Update(reservation);

        await _dbContext.SaveChangesAsync();

        // Setup mapper mock
        _fixture.ReservationsMapperMock
            .Setup(m => m.ToReservationReadDto(It.IsAny<Reservation>()))
            .Returns((Reservation r) => new ReservationReadDto
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = book.Title,
                UserId = r.UserId,
                ReservedAt = r.ReservedAt,
                ReservationStatus = r.ReservationStatus
            });

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _fixture.ReservationsService
                .PickReservationByIdAsync(reservation.Id, Guid.NewGuid());
        });


        _fixture.Reset();
    }

    [Fact]
    public async Task PickReservationByIdAsync_WhenBookIsNotAvailableForPickup_ThrowsException()
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
            .WithAddress("123 Publisher St")
            .WithWebsite("https://scribner.com")
            .WithEmail("publisher@gmail.com")
            .Build();
        _dbContext.Publishers.Add(publisher);
        _dbContext.SaveChanges();

        var book = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("The Great Gatsby")
            .WithAuthor(author.Id)
            .WithCoverImage("https://example.com/gatsby.jpg")
            .WithDescription("A novel set in the Jazz Age")
            .WithPublishedDate(DateTime.Now)
            .WithGenre(Genre.Fiction)
            .WithPages(180)
            .WithPublisher(publisher.Id)
            .Build();

        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        // Create user
        var user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        //  Create Active loan (someone else is borrowing the book)
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            LoanStatus = LoanStatus.Active
        };

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();

        // Create reservation for this user
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            ReservedAt = DateTime.UtcNow,
            ReservationStatus = ReservationStatus.Pending
        };

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        // Setup mapper mock
        _fixture.ReservationsMapperMock
            .Setup(m => m.ToReservationReadDto(It.IsAny<Reservation>()))
            .Returns((Reservation r) => new ReservationReadDto
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = book.Title,
                UserId = r.UserId,
                ReservedAt = r.ReservedAt,
                ReservationStatus = r.ReservationStatus
            });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _fixture.ReservationsService
                .PickReservationByIdAsync(reservation.Id, user.Id);
        });

        _fixture.Reset();
    }

    [Fact]
    public async Task ListAllReservationAsync_WhenThreePendingReservationsExist_ReturnsAll()
    {
        // Arrange
        var author = new AuthorBuilder()
            .WithId(Guid.NewGuid())
            .WithName("F. Scott", "Fitzgerald")
            .WithEmail("author@gmail.com")
            .Build();

        _dbContext.Authors.Add(author);

        var publisher = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Scribner")
            .WithAddress("123 Publisher St")
            .WithWebsite("https://scribner.com")
            .WithEmail("publisher@gmail.com")
            .Build();

        _dbContext.Publishers.Add(publisher);

        await _dbContext.SaveChangesAsync();

        var book = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("The Great Gatsby")
            .WithAuthor(author.Id)
            .WithGenre(Genre.Fiction)
            .WithPages(180)
            .WithPublisher(publisher.Id)
            .Build();

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        // Create three users
        var user1 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        var user2 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("Anna", "Smith")
            .WithEmail("anna@gmail.com")
            .Build();

        var user3 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("Mike", "Kidman")
            .WithEmail("mike@gmail.com")
            .Build();

        _dbContext.Users.AddRange(user1, user2, user3);
        await _dbContext.SaveChangesAsync();

        // Create 3 reservations
        var r1 = new Reservation { Id = Guid.NewGuid(), BookId =
            book.Id, UserId = user1.Id, Book = book, User = user1 };
        var r2 = new Reservation { Id = Guid.NewGuid(), BookId =
            book.Id, UserId = user2.Id, Book = book, User = user2 };
        var r3 = new Reservation { Id = Guid.NewGuid(), BookId = 
            book.Id, UserId = user3.Id, Book = book, User = user3 };

        _dbContext.Reservations.AddRange(r1, r2, r3);
        await _dbContext.SaveChangesAsync();

        // Query Options
        var queryOptions = new QueryOptions
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "ReservedAt",
            IsDescending = false,
            SearchTerm = null
        };

        // Act
        var result = await _fixture.ReservationsService.ListAllReservationAsync(queryOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.True(result.All(r => r.ReservationStatus == ReservationStatus.Pending));

        _fixture.Reset();
    }
    [Fact]
    public async Task ListReservationForAUserAsync_WhenUserHasThreeReservations_ReturnsAll()
    {
        // Arrange
        var author = new AuthorBuilder()
            .WithId(Guid.NewGuid())
            .WithName("F. Scott", "Fitzgerald")
            .WithEmail("author@gmail.com")
            .Build();

        _dbContext.Authors.Add(author);

        var publisher = new PublisherBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Scribner")
            .WithAddress("123 Publisher St")
            .WithWebsite("https://scribner.com")
            .WithEmail("publisher@gmail.com")
            .Build();

        _dbContext.Publishers.Add(publisher);
        await _dbContext.SaveChangesAsync();

        var book = new BookBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("The Great Gatsby")
            .WithAuthor(author.Id)
            .WithGenre(Genre.Fiction)
            .WithPages(180)
            .WithPublisher(publisher.Id)
            .Build();

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        // Create one user
        var user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("John", "Doe")
            .WithEmail("john@gmail.com")
            .Build();

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create 3 reservations for the same user
        var r1 = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            Book = book,
            User = user,
            ReservedAt = DateTime.UtcNow
        };

        var r2 = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            Book = book,
            User = user,
            ReservedAt = DateTime.UtcNow.AddMinutes(1)
        };

        var r3 = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = book.Id,
            UserId = user.Id,
            Book = book,
            User = user,
            ReservedAt = DateTime.UtcNow.AddMinutes(2)
        };

        _dbContext.Reservations.AddRange(r1, r2, r3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _fixture.ReservationsService.ListReservationForAUserAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.All(result, r => Assert.Equal(user.Id, r.UserId));

        _fixture.Reset();
    }

}
