using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces;

public interface IReservationMapper
{
    ReservationReadDto ToReservationReadDto(Reservation reservation);
    ReservationUpdateDto ToReservationUpdateDto(Reservation reservation);
    Reservation ToReservation(ReservationCreateDto reservationCreateDto);
    Reservation UpdateFromDto(Reservation reservation, ReservationUpdateDto reservationUpdateDto);
}