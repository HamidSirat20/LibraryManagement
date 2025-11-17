using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface IReservationService
{
    Task<PaginatedResponse<Reservation?>> ListAllReservationAsync(QueryOptions queryOptions);
    Task<IEnumerable<Reservation?>> ListReservationForAUserAsync(Guid userId);
    Task<ReservationReadDto?> PickReservationByIdAsync(Guid reservationId, Guid currentUserId);
    Task<ReservationReadDto?> CreateReservationAsync(Guid bookId, Guid userId);
    Task<Reservation> CancelReservationAsync(Guid reservationId, Guid userId);
}

