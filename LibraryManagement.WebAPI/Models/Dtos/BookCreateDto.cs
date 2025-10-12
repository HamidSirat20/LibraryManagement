using LibraryManagement.WebAPI.Models.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;

public class BookCreateDto : BookForManipulation
{
    [Required(ErrorMessage = "At least one author is required.")]
    [MinLength(1, ErrorMessage = "At least one author is required.")]
    public ICollection<Guid> AuthorIds { get; set; } = new List<Guid>();
    public Guid PublisherId { get; set; }
}
  