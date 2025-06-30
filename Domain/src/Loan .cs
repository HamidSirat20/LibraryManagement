namespace Domain.src;

public class Loan
{
    // Properties
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = default!;

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = default!;

    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public Loan(Guid id, Guid bookId, Guid memberId, DateTime loanDate, DateTime dueDate, DateTime? returnDate = null)
    {
        Id = id;
        BookId = bookId;
        MemberId = memberId;
        LoanDate = loanDate;
        DueDate = dueDate;
        ReturnDate = returnDate;
    }

    public override string ToString() => $"Loan ID: {Id}, Book ID: {BookId}, Member ID: {MemberId}, Loan Date: {LoanDate}, Due Date: {DueDate}, Return Date: {ReturnDate?.ToString() ?? "Not Returned"}";
}
