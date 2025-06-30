namespace Domain.src;

public class Member
{

    // Properties
    public Guid Id { get; init; }
    public string FullName { get; init; }
    public string Email { get; init; }
    public string Phone { get; init; }
    public string Address { get; set; }
    public DateTime MembershipStartDate { get; set; }
    public DateTime MembershipEndDate { get; set; }

    // Navigation properties
    public List<Loan> Loans { get; set; } = new();
    public List<Reservation> Reservations { get; set; } = new();
    public Member() { }

    public Member(Guid id, string fullName, string email, string phone, string address, DateTime membershipStartDate, DateTime membershipEndDate)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        Phone = phone;
        Address = address;
        MembershipStartDate = membershipStartDate;
        MembershipEndDate = membershipEndDate;
    }

    public override string ToString() => $"{FullName} ({Email}) - {Phone}";

}
