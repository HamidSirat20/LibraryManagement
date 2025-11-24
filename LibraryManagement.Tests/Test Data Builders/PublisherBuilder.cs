using LibraryManagement.WebAPI.Models;

namespace LibraryManagement.Test.Test_Data_Builders;
public class PublisherBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Default Publisher";
    private string _address = "123 Main Street";
    private string _website = "https://example.com";
    private string _email = "publisher@example.com";

    public PublisherBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublisherBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PublisherBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    public PublisherBuilder WithWebsite(string website)
    {
        _website = website;
        return this;
    }

    public PublisherBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public Publisher Build()
    {
        return new Publisher
        {
            Id = _id,
            Name = _name,
            Address = _address,
            Website = _website,
            Email = _email
        };
    }
}

