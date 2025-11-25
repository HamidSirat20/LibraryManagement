using LibraryManagement.Test.Test_Data_Builders;
using LibraryManagement.WebAPI.Models;

namespace LibraryManagement.Test.EntityTests;
public class BookTests
{
    [Theory]
    [InlineData("The Blind Owl", "Surrealist novel by Sadegh Hedayat", 1937, 1, 1, Genre.Horror, 192)]
    [InlineData("Savushun", "Persian novel by Simin Daneshvar", 1969, 1, 1, Genre.Fiction, 347)]
    [InlineData("The Colonel", "Political novel by Mahmoud Dowlatabadi", 2009, 1, 1, Genre.Other, 240)]
    [InlineData("A Time to Love", "Romance novel by Sepideh Jodeyri", 2015, 1, 1, Genre.Romance, 280)]
    public void Build_WithVariousBookData_CreatesBooksWithCorrectProperties(
       string title, string description, int year, int month, int day, Genre genre, int pages)
    {
        // Arrange
        var publishedDate = new DateTime(year, month, day);

        // Act
        var book = new BookBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithPublishedDate(publishedDate)
            .WithGenre(genre)
            .WithPages(pages)
            .Build();

        // Assert
        Assert.Equal(title, book.Title);
        Assert.Equal(description, book.Description);
        Assert.Equal(publishedDate, book.PublishedDate);
        Assert.Equal(genre, book.Genre);
        Assert.Equal(pages, book.Pages);
    }

    [Fact]
    public void Build_WithMediaAndMetadata_CreatesBookWithCorrectMediaData()
    {
        // Arrange
        var expectedCoverImageUrl = "https://example.com/dune-cover.jpg";
        var expectedPublisherId = Guid.NewGuid();

        // Act
        var book = new BookBuilder()
            .WithCoverImage(expectedCoverImageUrl)
            .WithPublisher(expectedPublisherId)
            .Build();

        // Assert
        Assert.Equal(expectedCoverImageUrl, book.CoverImageUrl);
        Assert.Equal(expectedPublisherId, book.PublisherId);
    }

    [Fact]
    public void Build_WithNavigationProperties_CreatesBookWithCorrectRelationships()
    {
        // Arrange
        var expectedAuthorId = Guid.NewGuid();

        // Act
        var book = new BookBuilder()
            .WithAuthor(expectedAuthorId)
            .WithActiveLoan()
            .WithPendingReservation()
            .WithNotifiedReservation()
            .Build();

        // Assert
        var bookAuthor = Assert.Single(book.BookAuthors);
        Assert.Equal(expectedAuthorId, bookAuthor.AuthorId);

        Assert.Single(book.Loans);
        Assert.Equal(LoanStatus.Active, book.Loans.First().LoanStatus);

        Assert.Equal(2, book.Reservations.Count);
        Assert.Contains(book.Reservations, r => r.ReservationStatus == ReservationStatus.Pending);
        Assert.Contains(book.Reservations, r => r.ReservationStatus == ReservationStatus.Notified);
    }
    [Fact]
    public void IsAvailable_NoActiveLoansOrPendingReservations_ReturnsTrue()
    {
        //Arrange
        var bookBuilder = new BookBuilder();
        //Act
        var book = bookBuilder.WithActiveLoan()
                              .Build();
        bool isAvailable = book.IsAvailable;
        //Assert
        Assert.False(isAvailable);
    }

    [Fact]
    public void IsAvailable_WithActiveLoan_ReturnsFalse()
    {
        // Arrange 
        var bookBuilder = new BookBuilder();
        //Act
        var book = bookBuilder.WithActiveLoan()
                              .Build();
        bool isAvailable = book.IsAvailable;

        //Assert
        Assert.False(isAvailable);
    }
    [Fact]
    public void IsAvailable_WithRerturnedLoan_ReturnsFalse()
    {
        //Arrange
        var bookBuilder = new BookBuilder();
        // Act
        var book = bookBuilder.WithActiveLoan()
                              .Build();
        bool isAvailable = book.IsAvailable;

        Assert.False(isAvailable);
    }
    [Fact]
    public void IsAvailableForReservation_WithNotifiedReservationAndReturnLoan_ReturnsTrue()
    {
        var bookBuilder = new BookBuilder();
        //Act
        var book = bookBuilder.WithReturnedLoan()
                              .WithNotifiedReservation()
                              .Build();
        bool isAvailableForReservation = book.IsAvailableForPickUp;
        // Assert
        Assert.True(isAvailableForReservation);
    }
    [Fact]
    public void IsAvailableForReservation_WithActiveLoan_ReturnsFalse()
    {
        var bookBuilder = new BookBuilder();
        //Act
        var book = bookBuilder.WithActiveLoan()
                              .Build();
        bool isAvailableForReservation = book.IsAvailableForPickUp;
        // Assert
        Assert.False(isAvailableForReservation);
    }
    [Fact]
    public void IsAvailableForReservation_WithOverdueLoan_ReturnsFalse()
    {
        var bookBuilder = new BookBuilder();
        //Act
        var book = bookBuilder.WithOverdueLoan()
                              .Build();
        bool isAvailableForReservation = book.IsAvailableForPickUp;
        // Assert
        Assert.False(isAvailableForReservation);
    }
}
