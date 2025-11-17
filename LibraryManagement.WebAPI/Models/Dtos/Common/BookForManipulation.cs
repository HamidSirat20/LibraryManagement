using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos.Common;
public abstract class BookForManipulation
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Published date is required.")]
    [DataType(DataType.Date)]
    [Range(typeof(DateTime), "1/1/1800", "1/1/2100", ErrorMessage = "Published date must be between 1800 and 2100.")]
    public DateTime PublishedDate { get; set; }

    [Required(ErrorMessage = "Genre is required.")]
    [EnumDataType(typeof(Genre), ErrorMessage = "Please select a valid genre.")]
    public Genre Genre { get; set; }

    [Required(ErrorMessage = "Page count is required.")]
    [Range(1, 2000, ErrorMessage = "Page count must be between 1 and 2000.")]
    public int Pages { get; set; }
}
