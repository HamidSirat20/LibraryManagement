using LibraryManagement.WebAPI.Models;

namespace LibraryManagement.Test.Test_Data_Builders;
public class AuthorBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string _email = "author@test.com";
    private string _bio = "Default author bio";

    public AuthorBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public AuthorBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public AuthorBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public AuthorBuilder WithBio(string bio)
    {
        _bio = bio;
        return this;
    }

    public Author Build()
    {
        return new Author
        {
            Id = _id,
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email,
            Bio = _bio
        };
    }
}
