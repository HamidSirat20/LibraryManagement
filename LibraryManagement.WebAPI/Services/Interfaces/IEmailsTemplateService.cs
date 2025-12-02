namespace LibraryManagement.WebAPI.Services.Interfaces;

public interface IEmailsTemplateService
{
    // Reservations
    string GetReservationConfirmationTemplate(string firstName, string lastName, string bookTitle, DateTime reservedDate, int queuePosition);
    string GetReservationReadyTemplate(string firstName, string lastName, string bookTitle, DateTime pickupDeadline);

    // Loans & Returns
    string GetReturnReminderTemplate(string firstName, string lastName, string bookTitle, DateTime dueDate, int daysUntilDue);

    // Fines & Payments
    string GetPaymentReminderTemplate(string firstName, string lastName, decimal outstandingBalance);
}