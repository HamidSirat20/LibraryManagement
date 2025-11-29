using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;
public class LateReturnOrLostFeeCreateDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid LoanId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
    public DateTime IssuedDate { get; set; }

    public FineStatus Status { get; set; }
    public FineType FineType { get; set; }

}

public class LateReturnOrLostFeeUpdateDto
{
    [Range(0.0, double.MaxValue, ErrorMessage = "Amount cannot be negative.")]
    public decimal? Amount { get; set; }

    public DateTime? PaidDate { get; set; }

    public FineStatus? Status { get; set; }
    public FineType? FineType { get; set; }
}
public class LateReturnOrLostFeeReadDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string? UserName { get; set; }

    public Guid LoanId { get; set; }
    public string? BookTitle { get; set; }
    public FineType FineType { get; set; }

    public decimal Amount { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime? PaidDate { get; set; }

    public FineStatus Status { get; set; }
}