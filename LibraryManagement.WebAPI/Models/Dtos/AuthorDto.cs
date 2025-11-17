using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;

public class AuthorReadDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int BookCount { get; set; }
}
public class AuthorCreateDto
{

    [Required]
    [MaxLength(30, ErrorMessage = "First name cannot be longer than 30 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, ErrorMessage = "Bio cannot be longer than 1000 characters.")]
    public string Bio { get; set; } = string.Empty;

}

public class AuthorUpdateDto
{
    [Required]
    [MaxLength(30, ErrorMessage = "First name cannot be longer than 30 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, ErrorMessage = "Bio cannot be longer than 1000 characters.")]
    public string Bio { get; set; } = string.Empty;

}

public class AuthorWithBooksDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<BookReadDto> Books { get; set; } = new();
}
