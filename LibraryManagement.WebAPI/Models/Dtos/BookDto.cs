using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;

public class BookReadDto
{
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
    public IList<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public Guid PublisherId { get; set; }
    public Publisher Publisher { get; set; } = default!;
}

public class BookCreateDto
{
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
}

public class BookUpdateDto
{
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
   
}
