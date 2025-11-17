namespace LibraryManagement.WebAPI.Services.Interfaces;

public interface IReservationsQueueService
{
    Task ProcessNextReservationAfterReturnAsync(Guid bookId, string emailSubject, string emailBod);
    Task<int> GetQueuePositionAsync(Guid bookId, Guid reservationId);
}

