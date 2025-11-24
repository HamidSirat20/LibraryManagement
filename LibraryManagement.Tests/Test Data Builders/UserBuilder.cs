using LibraryManagement.WebAPI.Models;

namespace LibraryManagement.Test.Test_Data_Builders;
public class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _firstName = "Hamid";
    private string _lastName = "Sirat";
    private string _fullName => $"{_firstName} {_lastName}";
    private string _email = "example@mail.com";
    private string _password = "defaultPassword123";
    private string _phone = "+1234567890";
    private string _address = "123 Main Street, City, Country";
    private byte[] _salt = new byte[16]; // Default empty salt
    private string _avatarUrl = "https://example.com/default-avatar.jpg";
    private string? _publiceId = null;
    private UserRole _role = UserRole.User;
    private DateTime _membershipStartDate = DateTime.UtcNow;
    private DateTime _membershipEndDate = DateTime.UtcNow.AddYears(1);

    // Navigation properties
    private List<Loan> _loans = new();
    private List<Reservation> _reservations = new();
    private List<LateReturnOrLostFee> _lateReturnOrLostFees = new();

    public UserBuilder WithId(Guid id) { _id = id; return this; }
    public UserBuilder WithPublicId(string publicId) { _publiceId = publicId; return this; }
    public UserBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }
    public UserBuilder WithFirstNameAndLastName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }
    public UserBuilder WithFullName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    public UserBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }
    public UserBuilder WithPhone(string phone) { _phone = phone; return this; }
    public UserBuilder WithPhoneNumber(string phone) { _phone = phone; return this; }
    public UserBuilder WithAvatarUrl(string avatarUrl) { _avatarUrl = avatarUrl; return this; }

    public UserBuilder WithRole(UserRole role) { _role = role; return this; }
    public UserBuilder WithMemberShipStartAndEndDate(DateTime membershipStart, DateTime membershipEnd)
    {
        _membershipStartDate = membershipStart;
        _membershipEndDate = membershipEnd;
        return this;
    }

    public User Build()
    {
        return new User(
            _firstName,
            _lastName,
            _email,
            _phone,
            _address,
            _membershipStartDate,
            _membershipEndDate
        )
        {
            Id = Guid.NewGuid(),
            Password = _password,
            Salt = _salt,
            AvatarUrl = _avatarUrl,
            PublicId = _publiceId,
            Role = _role,
            Loans = _loans,
            Reservations = _reservations,
            LateReturnOrLostFees = _lateReturnOrLostFees
        };
    }

}
