using LibraryManagement.WebAPI.Models.Dtos.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos
{
    public class UserCreateDto : UserForManipulationDto
    {
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Upload avatar image for profile.")]
        public IFormFile File { get; set; } = null!;
    }

    public class UserUpdateDto : UserForManipulationDto
    {
        public IFormFile? File { get; set; } 
    }

    public class UserReadDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } 
        public string? PublicId { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime MembershipStartDate { get; set; }
        public DateTime MembershipEndDate { get; set; }
    }
}
