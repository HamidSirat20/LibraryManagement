namespace LibraryManagement.WebAPI.Models;

public class BookAuthor : BaseEntityWithId
    {

        public Guid AuthorId { get; set; }
        public Guid BookId { get; set; }

        // Navigation Properties
        public Author Author { get; set; } = default!;
        public Book Book { get; set; } = default!;
    }

