using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.WebAPI.Models;

public class Publisher : BaseEntityWithId
{
    public string Name { get; init; }
    public string Address { get; set; }
    public string Website { get; set; }
    // Navigation properties
    public List<Book> Books { get; set; } = new();

    public Publisher() { }

    public Publisher(Guid id, string name, string address, string website)
    {
        Id = id;
        Name = name;
        Address = address;
        Website = website;
    }

    public override string ToString() => $"{Name} ({Website}) - {Address}";

}
