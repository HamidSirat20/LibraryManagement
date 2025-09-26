using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;
    public class UserReadDto
    {
    public Guid Id { get; set; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = "https://www.gravatar.com/avatar/";
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime MembershipStartDate { get; set; }
    public DateTime MembershipEndDate { get; set; }
}
    public class UserUpdateDto
    {
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = "https://www.gravatar.com/avatar/";
}
    public class UserCreateDto
    {
    [Required]
    public string FirstName { get; init; } = string.Empty;
    [Required]
    public string LastName { get; init; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required, Phone]
    public string Phone { get; init; } = string.Empty;
    [Required]
    public string Address { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = "https://www.gravatar.com/avatar/";
}
