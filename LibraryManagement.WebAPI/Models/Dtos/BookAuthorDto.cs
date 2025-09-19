namespace LibraryManagement.WebAPI.Models.Dtos
{
    public class BookAuthorDto
    {
        public Guid AuthorId { get; set; }
        public Guid BookId { get; set; }
        public int? Count { get; set; }
    }
}
