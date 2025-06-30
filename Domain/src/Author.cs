namespace Domain.src;

public class Author()
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    // Navigation properties
    public List<Book> Books { get; set; } = new();
    public override string ToString() => $"{FirstName} {LastName}";
}

