using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;
    public class AuthorDto
    {
    public Guid Id { get; set; }

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

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int BookCount { get; set; }
}

