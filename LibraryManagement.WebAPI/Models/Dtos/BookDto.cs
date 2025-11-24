using LibraryManagement.WebAPI.Models.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;

public class BookCreateDto : BookForManipulation
{
    [Required(ErrorMessage = "At least one author is required.")]
    [MinLength(1, ErrorMessage = "At least one author is required.")]
    public ICollection<Guid> AuthorIds { get; set; } = new List<Guid>();
    [Required(ErrorMessage = "Cover image file is required.")]
    public IFormFile File { get; set; } = null!;
    public Guid PublisherId { get; set; }
}

public class BookReadDto
{
    public Guid Id { get; set; }
    public string Title { get; init; }
    public string Description { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string? CoverImagePublicId { get; set; }
    public DateTime PublishedDate { get; set; }
    public Genre Genre { get; set; }
    public int Pages { get; set; }
    public ICollection<Guid> AuthorIds { get; set; }
    public Guid PublisherId { get; set; }


}

public class BookUpdateDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    public IFormFile? File { get; set; }

    [Required(ErrorMessage = "Published date is required.")]
    [DataType(DataType.Date)]
    [Range(typeof(DateTime), "1/1/1000", "1/1/2100", ErrorMessage = "Published date must be between 1800 and 2100.")]
    public DateTime PublishedDate { get; set; }

    [Required(ErrorMessage = "Genre is required.")]
    [EnumDataType(typeof(Genre), ErrorMessage = "Please select a valid genre.")]
    public Genre Genre { get; set; }

    [Required(ErrorMessage = "Page count is required.")]
    [Range(1, 2000, ErrorMessage = "Page count must be between 1 and 2000.")]
    public int Pages { get; set; }
}

public class BookWithoutPublisherDto
{
    public Guid Id { get; set; }
    public string Title { get; init; }
    [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }
    [Required]
    public string CoverImageUrl { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Date)]
    public DateTime PublishedDate { get; set; }
    public Genre Genre { get; set; }
    public int Pages { get; set; }
    public IList<AuthorReadDto> BookAuthors { get; set; } = new List<AuthorReadDto>();
}
public class BookWithPublisherDto
{
    public Guid Id { get; set; }
    public string Title { get; init; }
    [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }
    [Required]
    public string CoverImageUrl { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Date)]
    public DateTime PublishedDate { get; set; }
    public Genre Genre { get; set; }
    public int Pages { get; set; }
    public IList<AuthorReadDto> BookAuthors { get; set; } = new List<AuthorReadDto>();
    public Guid PublisherId { get; set; }
    public PublisherReadDto Publisher { get; set; } = default!;
}
