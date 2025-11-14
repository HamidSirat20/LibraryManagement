using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;
public class LoanCreateDto
{
    [Required]
    public Guid BookId { get; set; }  
}
public class LoanUpdateDto
{
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReturnDate { get; set; }

    public LoanStatus? LoanStatus { get; set; }
}

public class LoanReadDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string? UserName { get; set; }

    public Guid BookId { get; set; }
    public string? BookTitle { get; set; }

    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public LoanStatus LoanStatus { get; set; }

    public decimal? LateFee { get; set; }

    public List<LateReturnOrLostFeeReadDto>? LateReturnOrLostFees { get; set; }
}

