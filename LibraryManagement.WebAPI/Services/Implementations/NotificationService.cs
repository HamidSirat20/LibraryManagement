using LibraryManagement.WebAPI.Events;
using LibraryManagement.WebAPI.Services.Interfaces;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class NotificationService : IEventHandler<ReservationReadyEventArgs>,
    IEventHandler<LateReturnFineOrLostEventArgs>,
    IEventHandler<ReservationCreatedEventArgs>
{
    private readonly IEmailService _emailService;
    private readonly IEmailsTemplateService _emailTemplatesService;
    private readonly ILogger<NotificationService> _logger;
    public NotificationService(IEmailService emailService, IEmailsTemplateService emailTemplatesService, ILogger<NotificationService> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _emailTemplatesService = emailTemplatesService ?? throw new ArgumentNullException(nameof(emailTemplatesService));
        _logger = logger;
    }
    public async Task HandleAsync(ReservationReadyEventArgs evnt)
    {
        var emailBody = _emailTemplatesService.GetReservationReadyTemplate(evnt.FirstName, evnt.LastName,
            evnt.BookTitle, evnt.PickUpDeadLine) ?? throw new ArgumentNullException("User's Info is not complete!");

        await _emailService.SendEmailAsync(evnt.UserEmail, "Your reservation is ready for pickup!", emailBody);
    }

    public async Task HandleAsync(LateReturnFineOrLostEventArgs evnt)
    {
        var emailBody = _emailTemplatesService.GetPaymentReminderTemplate(evnt.FirstName, evnt.LastName,
            evnt.BookTitle, evnt.FineType, evnt.FinePrice);

        await _emailService.SendEmailAsync(evnt.UserEmail, "Payment Reminder", emailBody);
    }

    public async Task HandleAsync(ReservationCreatedEventArgs evnt)
    {
        var emailBody = _emailTemplatesService.GetReservationConfirmationTemplate(evnt.FirstName,
            evnt.LastName, evnt.BookTitle, evnt.ReservedAt, evnt.QueuePosition);
        await _emailService.SendEmailAsync(evnt.UserEmail, "Reservation Confirmation", emailBody);
    }
}

