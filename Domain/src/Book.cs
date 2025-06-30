using System.ComponentModel.DataAnnotations;

namespace Domain.src;

public class Book : IComparable<Book>, IEquatable<Book>
{
    [Required]
    public string ISBN { get; init; }
    [Required]
    [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
    public string Title { get; init; }
    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }

    // Relationships

    [Required]
    public IList<Author> Authors { get; set; } = default!;
    public Guid PublisherId { get; set; }
    public Publisher Publisher { get; set; } = default!;
    public DateTime PublishedDate { get; set; }
    public Genre Genre { get; set; }
    public int Pages { get; set; }

    public List<Loan> Loans { get; set; } = new();
    public List<Reservation> Reservations { get; set; } = new();

    public Book() { }
    public Book(string isbn, string title, string description, IList<Author> authors, DateTime publishedDate, Genre genre, int pages)
    {
        Title = title;
        ISBN = isbn;
        Description = description;
        Authors = authors;
        PublishedDate = publishedDate;
        Genre = genre;
        Pages = pages;
    }

    public int CompareTo(Book? other)=>string.Compare(Title, other?.Title, StringComparison.OrdinalIgnoreCase);


    public bool Equals(Book? other)=>
        other != null &&
        ISBN == other.ISBN &&
        Title == other.Title &&
        Description == other.Description &&
        PublishedDate == other.PublishedDate &&
        Genre == other.Genre &&
        Pages == other.Pages;
}
