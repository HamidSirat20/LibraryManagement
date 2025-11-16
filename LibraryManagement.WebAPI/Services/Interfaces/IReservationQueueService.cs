namespace LibraryManagement.WebAPI.Services.Interfaces;

public interface IReservationQueueService
{
    Task ProcessNextReservationAfterReturnAsync(Guid bookId, string emailSubject, string emailBod);
    Task<int> GetQueuePositionAsync(Guid bookId, Guid reservationId);
}

