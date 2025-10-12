using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos.Common
{
    public abstract class UserForManipulationDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please provide a valid phone number.")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = string.Empty;

        [Url(ErrorMessage = "Avatar URL must be a valid URL.")]
        [MaxLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters.")]
        public string AvatarUrl { get; set; } = "https://www.gravatar.com/avatar/";
    }
}
