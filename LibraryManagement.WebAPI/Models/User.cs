using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.WebAPI.Models;

public class User : BaseEntityWithId
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public byte[] Salt { get; set; }
    public string AvatarUrl { get; set; }
    public string? PublicId { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime MembershipStartDate { get; set; }
    public DateTime MembershipEndDate { get; set; }
    [NotMapped]
    public bool IsActive => MembershipEndDate > DateTime.UtcNow;

    // Navigation properties
    public List<Loan> Loans { get; set; } = new();
    public List<Reservation> Reservations { get; set; } = new();
    public List<LateReturnOrLostFee> LateReturnOrLostFees { get; set; } = new();
    public User() { }

    public User(string fName, string lName, string email, string phone, string address, DateTime membershipStartDate, DateTime membershipEndDate)
    {
        FirstName = fName;
        LastName = lName;
        Email = email;
        Phone = phone;
        Address = address;
        MembershipStartDate = membershipStartDate;
        MembershipEndDate = membershipEndDate;
    }
    public override string ToString() => $"{FirstName} {LastName} ({Email}) - {Phone}";

}
