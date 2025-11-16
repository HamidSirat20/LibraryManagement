using LibraryManagement.WebAPI.Services.Interfaces;
using System.Text;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class EmailTemplateService : IEmailTemplateService
{
    public string GetBookAvailableNotificationTemplate(string firstName, string lastName, string bookTitle)
    {
        throw new NotImplementedException();
    }

    public string GetFineIssuedTemplate(string firstName, string lastName, string bookTitle, decimal fineAmount, string reason)
    {
        throw new NotImplementedException();
    }

    public string GetLoanConfirmationTemplate(string firstName, string lastName, string bookTitle, DateTime dueDate)
    {
        throw new NotImplementedException();
    }

    public string GetLoanOverdueTemplate(string firstName, string lastName, string bookTitle, int daysOverdue, decimal fineAmount)
    {
        throw new NotImplementedException();
    }

    public string GetMembershipExpiredTemplate(string firstName, string lastName)
    {
        throw new NotImplementedException();
    }

    public string GetMembershipExpiryReminderTemplate(string firstName, string lastName, DateTime expiryDate, int daysUntilExpiry)
    {
        throw new NotImplementedException();
    }

    public string GetMembershipRenewalEmailTemplate(string firstName, string lastName, DateTime renewalDate)
    {
        throw new NotImplementedException();
    }

    public string GetMembershipRenewedTemplate(string firstName, string lastName, DateTime newExpiryDate)
    {
        throw new NotImplementedException();
    }

    public string GetPasswordResetEmailTemplate(string firstName, string lastName, string resetLink)
    {
        throw new NotImplementedException();
    }

    public string GetPaymentConfirmationTemplate(string firstName, string lastName, decimal amountPaid, decimal remainingBalance)
    {
        throw new NotImplementedException();
    }

    public string GetPaymentReminderTemplate(string firstName, string lastName, decimal outstandingBalance)
    {
        throw new NotImplementedException();
    }

    public string GetReservationCancelledTemplate(string firstName, string lastName, string bookTitle, string cancellationReason)
    {
        throw new NotImplementedException();
    }

    public string GetReservationConfirmationTemplate(string firstName, string lastName, string bookTitle, DateTime reservedDate, int queuePosition)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"Dear {firstName} {lastName},");
        sb.AppendLine();

        // Main message
        sb.AppendLine("Thank you for your reservation! We've successfully added you to the waitlist.");
        sb.AppendLine();

        // Reservation details
        sb.AppendLine("*** RESERVATION DETAILS ***");
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Reservation Date: {reservedDate:MMMM dd, yyyy 'at' h:mm tt}");
        sb.AppendLine($"Your Position in Queue: #{queuePosition}");

        sb.AppendLine();

        // Queue information
        sb.AppendLine("*** QUEUE INFORMATION ***");
        if (queuePosition == 1)
        {
            sb.AppendLine("🌟 You are FIRST in line! The book is currently being processed.");
            sb.AppendLine("You should receive a pickup notification within a few business days.");
        }
        else if (queuePosition <= 5)
        {
            sb.AppendLine($"You are #{queuePosition} in line.");
            sb.AppendLine("Based on current turnover, you should receive the book soon.");
        }
        else
        {
            sb.AppendLine($"You are #{queuePosition} in line with {queuePosition - 1} readers ahead of you.");
            sb.AppendLine("Popular books may take longer - we appreciate your patience!");
        }
        sb.AppendLine();

        // Next steps
        sb.AppendLine("*** WHAT HAPPENS NEXT ***");
        sb.AppendLine("1. We'll notify you by email when the book is ready");
        sb.AppendLine("2. You'll have 3 business days to pick it up");
        sb.AppendLine("3. Bring your library card for verification");
        sb.AppendLine();

        // Footer
        sb.AppendLine("Need to cancel or modify your reservation?");
        sb.AppendLine("Visit your account dashboard on our library website.");
        sb.AppendLine();
        sb.AppendLine("Happy reading!");
        sb.AppendLine();
        sb.AppendLine("Best regards,");
        sb.AppendLine("The Library Team");

        return sb.ToString();
    }

    public string GetReservationExpiredTemplate(string firstName, string lastName, string bookTitle)
    {
        throw new NotImplementedException();
    }

    public string GetReservationReadyTemplate(string firstName, string lastName, string bookTitle, DateTime pickupDeadline)
    {

        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"Dear {firstName} {lastName},");
        sb.AppendLine();

        // Updated for "ready" notification
        sb.AppendLine("🎉 Great news! The book you reserved is now available for pickup!");
        sb.AppendLine();

        // Reservation details 
        sb.AppendLine("*** BOOK READY FOR PICKUP ***");
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Pickup Deadline: {pickupDeadline:MMMM dd, yyyy}");
        sb.AppendLine();

        //Updated for ready notification
        sb.AppendLine("*** IMPORTANT INFORMATION ***");
        sb.AppendLine("• You have 3 business days to pick up the book");
        sb.AppendLine("• Bring your library card for verification");
        sb.AppendLine("• Unclaimed books will be passed to the next person in line and subject to a fine.");
        sb.AppendLine();

        // Pickup instructions
        sb.AppendLine("*** WHERE TO PICK UP ***");
        sb.AppendLine("📍 Main Library - Front Desk");
        sb.AppendLine("🕒 During library hours:");
        sb.AppendLine("   Monday-Friday: 9 AM - 6 PM");
        sb.AppendLine("   Saturday: 10 AM - 4 PM");
        sb.AppendLine("   Sunday: Closed");
        sb.AppendLine();

        // Footer
        sb.AppendLine("Best regards,");
        sb.AppendLine("The Library Team");

        return sb.ToString();
    }

    public string GetReturnConfirmationTemplate(string firstName, string lastName, string bookTitle, DateTime returnDate)
    {
        throw new NotImplementedException();
    }

    public string GetReturnReminderTemplate(string firstName, string lastName, string bookTitle, DateTime dueDate, int daysUntilDue)
    {
        throw new NotImplementedException();
    }

    public string GetSystemMaintenanceTemplate(string firstName, string lastName, DateTime maintenanceStart, DateTime maintenanceEnd)
    {
        throw new NotImplementedException();
    }

    public string GetWelcomeEmailTemplate(string firstName, string lastName)
    {
        throw new NotImplementedException();
    }
}
