using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.WebAPI.Models.Dtos;

public class ReservationReadDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid BookId { get; set; }
    [Required]
    public string BookTitle { get; set; }
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime ReservedAt { get; set; }
    public DateTime? PickupDeadline { get; set; }

    [Required]
    public ReservationStatus ReservationStatus { get; set; }
    [Required]
    public int QueuePosition { get; set; }
}

public class ReservationCreateDto
{
    [Required(ErrorMessage = "BookId is required.")]
    public Guid BookId { get; set; }
}

public class ReservationUpdateDto
{
    [Required(ErrorMessage = "ReservationStatus is required.")]
    [EnumDataType(typeof(ReservationStatus), ErrorMessage = "Invalid reservation status.")]
    public ReservationStatus ReservationStatus { get; set; }
}
