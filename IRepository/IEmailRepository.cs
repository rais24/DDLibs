using System.Net.Mail;

namespace Utils.IRepository
{
    public interface IEmailRepository
    {
        bool SendEmail(string toEmail, string fromEmail, string fromName, string subject, string body, Attachment attachment);

        bool EmailOTP(string otp, string userEmail);

        bool EmailForgotPasswordLink(string userEmail, string userName, string token);
    }
}
