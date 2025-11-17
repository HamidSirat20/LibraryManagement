namespace LibraryManagement.WebAPI.Models;

public class Publisher : BaseEntityWithId
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Website { get; set; }
    public string Email { get; set; }
    // Navigation properties
    public List<Book> Books { get; set; } = new();

    public Publisher() { }

    public Publisher(string name, string address, string website, string email)
    {
        Name = name;
        Address = address;
        Website = website;
        Email = email;
    }

    public override string ToString() => $"{Name} ({Website}) - {Address} - {Email}";

}
