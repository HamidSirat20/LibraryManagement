namespace Domain.src;

public class Librarian
{
    // Properties
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string PasswordHash { get; init; }
    public string FullName { get; init; }
    public string Role { get; set; }

    public Librarian(Guid id, string username, string passwordHash, string fullName, string role)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
    }

    public override string ToString() => $"{FullName} ({Username}) - Role: {Role}";
}
