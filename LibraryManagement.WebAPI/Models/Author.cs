using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models;

public class Author : BaseEntityWithId, IComparable<Author>, IEquatable<Author>
{
    [Required]
    [MaxLength(30, ErrorMessage = "First name cannot be longer than 100 characters.")]
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [StringLength(1000, ErrorMessage = "Bio cannot be longer than 1000 characters.")]
    public string Bio { get; set; } = string.Empty;

    // Navigation properties
    public List<BookAuthor> BookAuthors { get; set; } = new();

    public int CompareTo(Author? other)
    {
        return string.Compare(LastName, other?.LastName, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(Author? other)
    {
        return other != null && string.Compare(FirstName, other?.FirstName, StringComparison.OrdinalIgnoreCase) == 0 &&
               string.Compare(LastName, other?.LastName, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public override string ToString() => $"{FirstName} {LastName}";
}

