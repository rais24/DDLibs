using Utils.IRepository;
using System.Net.Mail;
using System.Text;

namespace Utils.Repository
{
    /// <inheritdoc/>
    public class EmailRepository : IEmailRepository
    {
        private readonly IConfiguration _config;

        /// <inheritdoc/>
        public EmailRepository(IConfiguration configuration)
        {
            _config = configuration;
        }

        private string EmailSignature()
        {
            StringBuilder signature = new();
            signature.Append("Regards,<br/><br/><b>Dignitas Digital</b><br/>");

            return signature.ToString();
        }
/// <inheritdoc/>

        public bool EmailOTP(string otp, string userEmail)
        {
            StringBuilder emailBody = new();
            emailBody.Append("<body>To whom it may concern, <br/><br/>");
            emailBody.Append($"Your OTP to login is {otp}. <br/>");
            emailBody.Append("Your OTP will expire in 10 mins.<br/><br/>");
            emailBody.Append(EmailSignature());

            return SendEmail(userEmail, "otp@test.com", "Test", "Test OTP", emailBody.ToString(), null);
        }
/// <inheritdoc/>

        public bool EmailForgotPasswordLink(string userEmail,string userName, string token)
        {
            string webUrl = _config["WebUrl"];
            webUrl += "resetpassword?token="+token;

            StringBuilder emailBody = new();
            emailBody.Append($"<body>Hi {userName}, <br/><br/>");
            emailBody.Append($"Click on this <a href='{webUrl}' target='_blank'>link</a> to reset your password. This link will expire in 24 hrs. <br/><br/>");
            emailBody.Append(EmailSignature());

            return SendEmail(userEmail, "otp@test.com", "Test", "Test Reset Password", emailBody.ToString(), null);

        }
        /// <inheritdoc/>
        public bool SendEmail(string toEmail, string fromEmail, string fromName, string subject, string body, Attachment? attachment)
        {
            bool emailSent = true;
            string testingMode = _config["TestingMode"];
            string testingEmails = _config["TestingEmails"];
            string[] toEmails = testingEmails.Split(',');
            using (SmtpClient client = new())
            {
                try
                {
                    client.Host = "smtp.office365.com";
                    System.Net.NetworkCredential basicauthenticationinfo = new("test@test.com", "****");
                    client.Port = 587;
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = basicauthenticationinfo;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    MailMessage mailMessage = new()
                    {
                        From = new MailAddress("test@test.com", fromName)
                    };
                    if (testingMode.ToLower().Equals("on"))
                    {
                        foreach(string email in toEmails)
                            mailMessage.To.Add(email);
                    }
                    else
                        mailMessage.To.Add(toEmail);
                    mailMessage.Subject = subject;
                    mailMessage.IsBodyHtml = true;
                    if(attachment != null)
                        mailMessage.Attachments.Add(attachment);
                    mailMessage.Body = body;

                    client.Send(mailMessage);
                    mailMessage.Dispose();
                }
                catch 
                {
                    emailSent = false;
                }
            }

            return emailSent;
        }
    }
}
