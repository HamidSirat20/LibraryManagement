using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos
{
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
        public IList<AuthorDto> BookAuthors { get; set; } = new List<AuthorDto>();
    }
}
