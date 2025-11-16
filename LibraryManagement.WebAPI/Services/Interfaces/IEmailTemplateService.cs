namespace LibraryManagement.WebAPI.Services.Interfaces;

public interface IEmailTemplateService
{
    // User Account & Authentication
    string GetWelcomeEmailTemplate(string firstName, string lastName);
    string GetPasswordResetEmailTemplate(string firstName, string lastName, string resetLink);

    // Reservations
    string GetReservationConfirmationTemplate(string firstName, string lastName, string bookTitle, DateTime reservedDate, int queuePosition);
    string GetReservationReadyTemplate(string firstName, string lastName, string bookTitle, DateTime pickupDeadline);
    string GetReservationCancelledTemplate(string firstName, string lastName, string bookTitle, string cancellationReason);
    string GetReservationExpiredTemplate(string firstName, string lastName, string bookTitle);

    // Loans & Returns
    string GetLoanConfirmationTemplate(string firstName, string lastName, string bookTitle, DateTime dueDate);
    string GetLoanOverdueTemplate(string firstName, string lastName, string bookTitle, int daysOverdue, decimal fineAmount);
    string GetReturnReminderTemplate(string firstName, string lastName, string bookTitle, DateTime dueDate, int daysUntilDue);
    string GetReturnConfirmationTemplate(string firstName, string lastName, string bookTitle, DateTime returnDate);

    // Fines & Payments
    string GetFineIssuedTemplate(string firstName, string lastName, string bookTitle, decimal fineAmount, string reason);
    string GetPaymentConfirmationTemplate(string firstName, string lastName, decimal amountPaid, decimal remainingBalance);
    string GetPaymentReminderTemplate(string firstName, string lastName, decimal outstandingBalance);

    // Membership
    string GetMembershipRenewalEmailTemplate(string firstName, string lastName, DateTime renewalDate);
    string GetMembershipExpiryReminderTemplate(string firstName, string lastName, DateTime expiryDate, int daysUntilExpiry);
    string GetMembershipRenewedTemplate(string firstName, string lastName, DateTime newExpiryDate);
    string GetMembershipExpiredTemplate(string firstName, string lastName);

    // Notifications & System
    string GetBookAvailableNotificationTemplate(string firstName, string lastName, string bookTitle);
    string GetSystemMaintenanceTemplate(string firstName, string lastName, DateTime maintenanceStart, DateTime maintenanceEnd);
}