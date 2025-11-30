namespace LibraryManagement.WebAPI.Models;

public class LateReturnOrLostFee : BaseEntityWithId
{
    public decimal Amount { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public FineStatus Status { get; set; } = FineStatus.Pending;
    public FineType FineType { get; set; } = FineType.LateReturn;
    public string? Description { get; set; }

    // Navigation properties
    public Guid UserId { get; set; }
    public Guid LoanId { get; set; }
    public User User { get; set; } = default!;
    public Loan Loan { get; set; } = default!;
}

