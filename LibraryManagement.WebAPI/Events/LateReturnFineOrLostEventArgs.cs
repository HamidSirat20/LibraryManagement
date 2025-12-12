
using LibraryManagement.WebAPI.Models;

namespace LibraryManagement.WebAPI.Events;
public class LateReturnFineOrLostEventArgs : IEvent
{
    public DateTime OccurredAt { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public FineType FineType { get; set; } 
    public decimal FinePrice { get; set; } = 0;
    public string UserEmail { get; set; } = string.Empty;

}

