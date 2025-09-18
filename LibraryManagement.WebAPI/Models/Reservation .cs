namespace LibraryManagement.WebAPI.Models;

public class Reservation: BaseEntityWithId
{
    public Guid BookId { get; set; }
    public Book Book { get; set; } = default!;

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public DateTime ReservationDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    public Reservation(Guid id, Guid bookId, Guid userId, DateTime reservationDate, LoanStatus status = LoanStatus.Pending)
    {
        Id = id;
        BookId = bookId;
        UserId = userId;
        ReservationDate = reservationDate;
        Status = status;
    }

    public override string ToString() => $"Reservation ID: {Id}, Book ID: {BookId}, Member ID: {UserId}, Reservation Date: {ReservationDate}, Status: {Status}";
}
