using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models;

public class Loan : BaseEntityWithId
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid BookId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime LoanDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReturnDate { get; set; }

    public LoanStatus LoanStatus { get; set; } = LoanStatus.Active;

    public decimal? LateFee { get; set; }

    // Navigation properties
    public User User { get; set; } = default!;
    public Book Book { get; set; } = default!;
    public List<LateReturnOrLostFee> LateReturnOrLostFees { get; set; } = new();
    public Loan(Guid id, Guid bookId, Guid userId, DateTime loanDate, DateTime dueDate, DateTime? returnDate = null)
    {
        Id = id;
        BookId = bookId;
        UserId = userId;
        LoanDate = loanDate;
        DueDate = dueDate;
        ReturnDate = returnDate;
    }

    public override string ToString() => $"Loan ID: {Id}, Book ID: {BookId}, Member ID: {UserId}, Loan Date: {LoanDate}, Due Date: {DueDate}, Return Date: {ReturnDate?.ToString() ?? "Not Returned"}";
}
