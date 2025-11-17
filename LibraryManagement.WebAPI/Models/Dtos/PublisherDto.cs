using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;
public class PublisherReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public int? BookCount { get; set; }
}

public class PublisherCreateDto
{
    [Required(ErrorMessage = "Publisher name is a required field.")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Publisher address is a required field.")]
    public string Address { get; set; } = string.Empty;
    [Required(ErrorMessage = "Publisher email address is a required field.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Publisher website is a required field.")]
    [Url(ErrorMessage = "Invalid website URL format.")]
    public string Website { get; set; } = string.Empty;
}
public class PublisherUpdateDto
{
    [Required(ErrorMessage = "Publisher name is a required field.")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Publisher address is a required field.")]
    public string Address { get; set; } = string.Empty;
    [Required(ErrorMessage = "Publisher email address is a required field.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Publisher website is a required field.")]
    [Url(ErrorMessage = "Invalid website URL format.")]
    public string Website { get; set; } = string.Empty;
}

