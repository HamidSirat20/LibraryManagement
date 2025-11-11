namespace LibraryManagement.WebAPI.Models;

public class Reservation: BaseEntityWithId
{
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public ReservationStatus ReservationStatus { get; set; } = ReservationStatus.Pending;

    public Book Book { get; set; } = default!;
    public User User { get; set; } = default!;
    public Reservation()
    {
        
    }
    public Reservation(Guid id, Guid bookId, Guid userId, DateTime reservedAt, ReservationStatus status = ReservationStatus.Pending)
    {
        Id = id;
        BookId = bookId;
        UserId = userId;
        ReservedAt = reservedAt;
        ReservationStatus = status;
    }

    public override string ToString() => $"Reservation ID: {Id}, Book ID: {BookId}, Member ID: {UserId},  Status: {ReservationStatus}";
}
