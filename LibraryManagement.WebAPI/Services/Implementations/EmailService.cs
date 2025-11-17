using FluentEmail.Core;
using LibraryManagement.WebAPI.Services.Interfaces;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class EmailService : IEmailService
{
    private readonly IFluentEmail _email;

    public EmailService(IFluentEmail email)
    {
        _email = email;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var response = await _email
            .To(toEmail)
            .Subject(subject)
            .Body(body, isHtml: false)
            .SendAsync();

        if (!response.Successful)
            throw new Exception(string.Join("; ", response.ErrorMessages));
    }
}

