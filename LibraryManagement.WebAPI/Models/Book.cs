using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models;

public class Book : BaseEntityWithId, IComparable<Book>, IEquatable<Book>
{
    [Required]
    [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
    public string Title { get; set; }
    [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }
    [Required]
    public string CoverImageUrl { get; set; } = string.Empty;
    public string? CoverImagePublicId { get; set; }
    [Required]
    [DataType(DataType.Date)]
    public DateTime PublishedDate { get; set; }
    public Genre Genre { get; set; }
    public int Pages { get; set; }

    public bool IsAvailable => !Loans.Any(l => l.LoanStatus == LoanStatus.Active || l.LoanStatus == LoanStatus.Overdue)
                           && !Reservations.Any(r => r.ReservationStatus == ReservationStatus.Pending);
    public bool IsAvailableForPickUp => !Loans.Any(l => l.LoanStatus == LoanStatus.Active || l.LoanStatus == LoanStatus.Overdue)
                       && Reservations.Any(r => r.ReservationStatus == ReservationStatus.Notified);

    [Required]
    public IList<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public List<Reservation> Reservations { get; set; } = new();
    public List<Loan> Loans { get; set; } = new List<Loan>();
    public Guid PublisherId { get; set; }
    public Publisher Publisher { get; set; } = default!;

    public Book()
    {

    }
    public Book(string title, string description, DateTime publishedDate, Genre genre, int pages)
    {
        Title = title;
        Description = description;
        PublishedDate = publishedDate;
        Genre = genre;
        Pages = pages;

    }

    public int CompareTo(Book? other) => string.Compare(Title, other?.Title, StringComparison.OrdinalIgnoreCase);


    public bool Equals(Book? other) =>
        other != null &&
        Title == other.Title &&
        Description == other.Description &&
        PublishedDate == other.PublishedDate &&
        Genre == other.Genre &&
        Pages == other.Pages;
}
