namespace LibraryManagement.WebAPI.Services.Interfaces;

public interface IReservationsQueueService
{
    Task ProcessNextReservationAfterReturnAsync(Guid bookId);
    Task<int> GetQueuePositionAsync(Guid bookId, Guid reservationId);
}

