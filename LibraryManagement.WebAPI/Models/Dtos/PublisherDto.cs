namespace LibraryManagement.WebAPI.Models.Dtos
{
    public class PublisherDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public int? BookCount { get; set; }
    }
}
