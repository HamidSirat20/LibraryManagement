namespace Domain.src;

public class Reservation
{
    // Properties
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = default!;

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = default!;

    public DateTime ReservationDate { get; set; }
    public Status Status { get; set; } = Status.Pending;

    public Reservation(Guid id, Guid bookId, Guid memberId, DateTime reservationDate, Status status = Status.Pending)
    {
        Id = id;
        BookId = bookId;
        MemberId = memberId;
        ReservationDate = reservationDate;
        Status = status;
    }

    public override string ToString() => $"Reservation ID: {Id}, Book ID: {BookId}, Member ID: {MemberId}, Reservation Date: {ReservationDate}, Status: {Status}";
}
