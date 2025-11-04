namespace LibraryManagement.WebAPI.Models.Dtos
{
    public class BookReadDto
    {
        public Guid Id { get; set; }
        public string Title { get; init; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public string ? CoverImagePublicId { get; set; }
        public DateTime PublishedDate { get; set; }
        public Genre Genre { get; set; }
        public int Pages { get; set; }
        public ICollection<Guid> AuthorIds { get; set; }
        public Guid PublisherId { get; set; }


    }
}
