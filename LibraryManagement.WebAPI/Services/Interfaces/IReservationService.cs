using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
    public interface IReservationService
    {
    Task<PaginatedResponse<ReservationReadDto>> ListAllReservationAsync(QueryOptions queryOptions);
    Task<PaginatedResponse<ReservationReadDto>> ListReservationForAUserAsync();
    Task<ReservationReadDto?> PickReservationByIdAsync(Guid reservationId, Guid currentUserId);
    Task<ReservationReadDto?> CreateReservationAsync(Guid bookId,Guid userId);
    Task<Reservation> DeleteReservationAsync(Guid id);
    }

