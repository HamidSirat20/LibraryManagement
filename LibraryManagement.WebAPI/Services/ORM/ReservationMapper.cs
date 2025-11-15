using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.ORM;

public class ReservationMapper : IReservationMapper
{
    public ReservationReadDto ToReservationReadDto(Reservation reservation)
    {
        if (reservation == null) return null;

        return new ReservationReadDto
        {
            Id = reservation.Id,
            BookId = reservation.BookId,
            BookTitle = reservation.Book?.Title ?? "Unknown Book",
            UserId = reservation.UserId,
            ReservedAt = reservation.ReservedAt,
            PickupDeadline = reservation.PickupDeadline,
            ReservationStatus = reservation.ReservationStatus,
            QueuePosition = reservation.QueuePosition

        };
    }

    public ReservationUpdateDto ToReservationUpdateDto(Reservation reservation)
    {
        if (reservation == null) return null;

        return new ReservationUpdateDto
        {
            ReservationStatus = reservation.ReservationStatus
        };
    }

    public Reservation ToReservation(ReservationCreateDto reservationCreateDto)
    {
        if (reservationCreateDto == null) return null;

        return new Reservation
        {
            BookId = reservationCreateDto.BookId,
            ReservedAt = DateTime.UtcNow,
            ReservationStatus = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PickupDeadline = null,
        };
    }

    public Reservation UpdateFromDto(Reservation reservation, ReservationUpdateDto reservationUpdateDto)
    {
        if (reservationUpdateDto == null) return reservation;

        reservation.ReservationStatus = reservationUpdateDto.ReservationStatus;
        reservation.UpdatedAt = DateTime.UtcNow;

        return reservation;
    }
}