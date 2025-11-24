using LibraryManagement.Test.Test_Data_Builders;

namespace LibraryManagement.Test.Utilities;
public class UserTests
{
    [Theory]
    [InlineData("Matti", "Virtanen", "matti.virtanen@example.com", "+358401234567", "Mannerheimintie 15, 00100 Helsinki", "2024-01-01", "2025-12-31")]
    [InlineData("Liisa", "Korhonen", "liisa.korhonen@example.com", "+358501234567", "Aleksanterinkatu 52, 00100 Helsinki", "2024-01-15", "2025-06-30")]
    [InlineData("Jukka", "Mäkinen", "jukka.makinen@example.com", "+358451234567", "Hämeenkatu 12, 33100 Tampere", "2024-02-01", "2025-09-15")]
    [InlineData("Anna", "Nieminen", "anna.nieminen@example.com", "+358441234567", "Kauppatori 4, 20100 Turku", "2024-01-20", "2025-03-20")]
    public void Build_WithUsers_CreatesUsersWithCorrectProperties (string firstName, string lastName,string email, string phone,string address,
        string membershipStart,string membershipEnd)
    {
        //Arrange
        var fullName = $"{firstName} {lastName}";
        var expectedMembershipStart = DateTime.Parse(membershipStart);
        var expectedMembershipEnd = DateTime.Parse(membershipEnd);
        //Act
        var user = new UserBuilder()
                    .WithFirstNameAndLastName(firstName,lastName)
                    .WithEmail(email)
                    .WithPhoneNumber(phone)
                    .WithAddress(address)
                    .WithMemberShipStartAndEndDate(expectedMembershipStart, expectedMembershipEnd)
                    .Build();

        //Assert
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(firstName,user.FirstName);
        Assert.Equal(lastName,user.LastName);
        Assert.Equal(email, user.Email);
        Assert.Equal(phone, user.Phone);
        Assert.Equal(address, user.Address);
        Assert.Equal(expectedMembershipStart, user.MembershipStartDate);
        Assert.Equal(expectedMembershipEnd, user.MembershipEndDate);
    }    
}
