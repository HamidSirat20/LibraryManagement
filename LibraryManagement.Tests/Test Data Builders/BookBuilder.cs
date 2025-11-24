using LibraryManagement.WebAPI.Models;

namespace LibraryManagement.Test.Test_Data_Builders;
public class BookBuilder
{
    private string _title = "Default Book Title";
    private string _description = "Default description";
    private DateTime _publishedDate = new DateTime(2000, 1, 1);
    private Genre _genre = Genre.Fiction;
    private int _pages = 300;
    private string _coverImageUrl = "https://example.com/default-cover.jpg";
    private Guid _publisherId = Guid.NewGuid();
    private List<Loan> _loans = new();
    private List<Reservation> _reservations = new();
    private List<BookAuthor> _bookAuthors = new();
    public BookBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public BookBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public BookBuilder WithPublishedDate(DateTime date)
    {
        _publishedDate = date;
        return this;
    }

    public BookBuilder WithGenre(Genre genre)
    {
        _genre = genre;
        return this;
    }

    public BookBuilder WithPages(int pages)
    {
        _pages = pages;
        return this;
    }

    public BookBuilder WithCoverImage(string url)
    {
        _coverImageUrl = url;
        return this;
    }

    public BookBuilder WithPublisher(Guid publisherId)
    {
        _publisherId = publisherId;
        return this;
    }

    public BookBuilder WithActiveLoan()
    {
        _loans.Add(new Loan { LoanStatus = LoanStatus.Active });
        return this;
    }

    public BookBuilder WithPendingLoan()
    {
        _loans.Add(new Loan { LoanStatus = LoanStatus.Pending });
        return this;
    }

    public BookBuilder WithReturnedLoan()
    {
        _loans.Add(new Loan { LoanStatus = LoanStatus.Returned });
        return this;
    }

    public BookBuilder WithOverdueLoan()
    {
        _loans.Add(new Loan { LoanStatus = LoanStatus.Overdue });
        return this;
    }

    public BookBuilder WithPendingReservation()
    {
        _reservations.Add(new Reservation { ReservationStatus = ReservationStatus.Pending });
        return this;
    }

    public BookBuilder WithNotifiedReservation()
    {
        _reservations.Add(new Reservation { ReservationStatus = ReservationStatus.Notified });
        return this;
    }

    public BookBuilder WithAuthor(Guid authorId)
    {
        _bookAuthors.Add(new BookAuthor { AuthorId = authorId });
        return this;
    }

    public Book Build()
    {
        var book = new Book(_title, _description, _publishedDate, _genre, _pages)
        {
            CoverImageUrl = _coverImageUrl,
            PublisherId = _publisherId,
            Loans = _loans,
            Reservations = _reservations,
            BookAuthors = _bookAuthors
        };
        return book;
    }
}