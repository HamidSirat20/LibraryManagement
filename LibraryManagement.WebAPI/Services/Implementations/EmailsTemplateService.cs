using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Services.Interfaces;
using System.Text;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class EmailsTemplateService : IEmailsTemplateService
{
    public string GetPaymentReminderTemplate(string firstName, string lastName, string bookTitle, FineType fineReason, decimal outstandingBalance)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Dear {firstName} {lastName},");
        sb.AppendLine();

        sb.AppendLine("This is a reminder that your library account has an outstanding balance.");
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Reason: {fineReason}");
        sb.AppendLine($"Outstanding Balance: ${outstandingBalance:F2}");
        sb.AppendLine();

        if (outstandingBalance == 0)
        {
            sb.AppendLine("Good news—your account has no pending fees.");
        }
        else if (outstandingBalance <= 10)
        {
            sb.AppendLine("Please clear this small balance at your earliest convenience.");
        }
        else
        {
            sb.AppendLine("Please make a payment soon to avoid interruptions to your borrowing privileges.");
        }
        sb.AppendLine();

        sb.AppendLine("You may pay online or in person. Contact us if you believe this is an error.");
        sb.AppendLine();

        sb.AppendLine("Thank you,");
        sb.AppendLine("The Library Team");

        return sb.ToString();
    }
    public string GetReservationConfirmationTemplate(string firstName, string lastName, string bookTitle, DateTime reservedDate, int queuePosition)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Dear {firstName} {lastName},");
        sb.AppendLine();
        sb.AppendLine("Your reservation has been added to the waitlist.");
        sb.AppendLine();
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Reserved On: {reservedDate:MMMM dd, yyyy 'at' h:mm tt}");
        sb.AppendLine($"Queue Position: #{queuePosition}");
        sb.AppendLine();

        if (queuePosition == 1)
            sb.AppendLine("You are first in line. Expect a pickup notice soon.");
        else
            sb.AppendLine($"You are #{queuePosition} in line. We'll notify you when it's your turn.");
        sb.AppendLine();

        sb.AppendLine("You’ll receive an email when the book is ready. You’ll have 3 business days to pick it up.");
        sb.AppendLine();
        sb.AppendLine("Thank you,");
        sb.AppendLine("The Library Team");

        return sb.ToString();
    }
    public string GetReservationReadyTemplate(string firstName, string lastName, string bookTitle, DateTime pickupDeadline)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Dear {firstName} {lastName},");
        sb.AppendLine();
        sb.AppendLine("Good news! Your reserved book is ready for pickup.");
        sb.AppendLine();
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Pickup Deadline: {pickupDeadline:MMMM dd, yyyy}");
        sb.AppendLine();
        sb.AppendLine("Please bring your library card. Unclaimed books go to the next person in line.");
        sb.AppendLine();
        sb.AppendLine("Pickup Location: Main Library – Front Desk");
        sb.AppendLine("Hours: Mon–Fri 9–6, Sat 10–4, Sun Closed");
        sb.AppendLine();
        sb.AppendLine("Thank you,");
        sb.AppendLine("The Library Team");

        return sb.ToString();
    }
    public string GetReturnReminderTemplate(
        string firstName,
        string lastName,
        string bookTitle,
        DateTime dueDate,
        int daysUntilDue)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Dear {firstName} {lastName},");
        sb.AppendLine();
        sb.AppendLine("This is a reminder about your borrowed book.");
        sb.AppendLine();
        sb.AppendLine($"Book: {bookTitle}");
        sb.AppendLine($"Due In: {daysUntilDue} day(s)");
        sb.AppendLine($"Due Date: {dueDate:MMMM dd, yyyy}");
        sb.AppendLine();
        sb.AppendLine("Late returns incur fees. You may renew online if the book has no holds.");
        sb.AppendLine();
        sb.AppendLine("Renew here:");
        sb.AppendLine("https://yourlibrarywebsite.com/account/loans");
        sb.AppendLine();
        sb.AppendLine("Thank you,");
        sb.AppendLine("The Library Team");

        return sb.ToString();
    }

}
