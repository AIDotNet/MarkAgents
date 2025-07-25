namespace MarkAgent.Application.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default);
    Task SendEmailVerificationAsync(string toEmail, string verificationToken, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string toEmail, string userKey, CancellationToken cancellationToken = default);
}