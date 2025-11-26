using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.Test.Test_Data_Builders;

public class AuthorCreateDtoBuilder
{
    private string _firstName = "Default";
    private string _lastName = "Author";
    private string _email = "default.author@example.com";
    private string _bio = "This is a default author bio for testing.";

    public AuthorCreateDtoBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public AuthorCreateDtoBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public AuthorCreateDtoBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public AuthorCreateDtoBuilder WithBio(string bio)
    {
        _bio = bio;
        return this;
    }

    public AuthorCreateDto Build()
    {
        return new AuthorCreateDto
        {
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email,
            Bio = _bio
        };
    }
}
