using LibraryManagement.WebAPI.Services.Interfaces;
using System.Text;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class EmailsTemplateService : IEmailsTemplateService
{
    public string GetPaymentReminderTemplate(string firstName, string lastName, decimal outstandingBalance)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"Dear {firstName} {lastName},");
        sb.AppendLine();

        // Main message
        sb.AppendLine("This is a friendly reminder that your library account has an outstanding balance.");
        sb.AppendLine();

        // Payment details
        sb.AppendLine(" PAYMENT DETAILS ");
        sb.AppendLine($"Outstanding Balance: ${outstandingBalance:F2}");
        sb.AppendLine();

        // Guidance based on balance
        if (outstandingBalance == 0)
        {
            sb.AppendLine("Great news! Your account has no pending fees.");
        }
        else if (outstandingBalance <= 10)
        {
            sb.AppendLine("Your balance is relatively small. Please make a payment at your earliest convenience.");
        }
        else
        {
            sb.AppendLine("Your outstanding balance is significant. Prompt payment is recommended to avoid interruptions in borrowing privileges.");
        }
        sb.AppendLine();

        // Next steps
        sb.AppendLine(" NEXT STEPS ");
        sb.AppendLine("1. Visit our library website or your account dashboard to make a payment");
        sb.AppendLine("2. Payments can be made via card, PayPal, or in person at the library");
        sb.AppendLine("3. Contact our support if you believe this balance is incorrect");
        sb.AppendLine();

        // Footer
        sb.AppendLine("Thank you for your prompt attention to this matter.");
        sb.AppendLine();
        sb.AppendLine("Best regards,");
        sb.AppendLine("The Library Team");

        return sb.ToString();
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
        sb.AppendLine(" RESERVATION DETAILS ");
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Reservation Date: {reservedDate:MMMM dd, yyyy 'at' h:mm tt}");
        sb.AppendLine($"Your Position in Queue: #{queuePosition}");

        sb.AppendLine();

        // Queue information
        sb.AppendLine(" QUEUE INFORMATION ");
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
        sb.AppendLine(" WHAT HAPPENS NEXT ");
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
        sb.AppendLine(" BOOK READY FOR PICKUP ");
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Pickup Deadline: {pickupDeadline:MMMM dd, yyyy}");
        sb.AppendLine();

        //Updated for ready notification
        sb.AppendLine(" IMPORTANT INFORMATION ");
        sb.AppendLine("• You have 3 business days to pick up the book");
        sb.AppendLine("• Bring your library card for verification");
        sb.AppendLine("• Unclaimed books will be passed to the next person in line and subject to a fine.");
        sb.AppendLine();

        // Pickup instructions
        sb.AppendLine(" WHERE TO PICK UP ");
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

    public string GetReturnReminderTemplate(
     string firstName,
     string lastName,
     string bookTitle,
     DateTime dueDate,
     int daysUntilDue)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"Dear {firstName} {lastName},");
        sb.AppendLine();

        // Main message
        sb.AppendLine("⏰ This is a friendly reminder about your borrowed book.");
        sb.AppendLine();
        sb.AppendLine($"The following book is due in **{daysUntilDue} day(s)**:");
        sb.AppendLine();

        // Book details
        sb.AppendLine(" BOOK DUE SOON ");
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Due Date: {dueDate:MMMM dd, yyyy}");
        sb.AppendLine();

        // Important notes
        sb.AppendLine(" IMPORTANT INFORMATION ");
        sb.AppendLine("• Late returns will result in daily late fees");
        sb.AppendLine("• If the due date has passed, please return the book as soon as possible");
        sb.AppendLine("• You may renew this loan online if no one else has reserved the book");
        sb.AppendLine();

        // Renewal instructions
        sb.AppendLine(" RENEW OR MANAGE YOUR LOAN ");
        sb.AppendLine("You can renew or check your loan status at:");
        sb.AppendLine("📌 https://yourlibrarywebsite.com/account/loans");  
        sb.AppendLine();

        // Footer
        sb.AppendLine("Thank you for using our library!");
        sb.AppendLine("Best regards,");
        sb.AppendLine("The Library Team");

        return sb.ToString();
    }
}
