using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Projeto_PAP
{
    internal static class EmailService
    {
        private static readonly string[] Scopes = { GmailService.Scope.GmailSend };
        private static readonly string ApplicationName = "PasswordSecurityApp";
        private static readonly string SenderEmail = "manager.pass.secure.app@gmail.com";
        private static readonly string SenderName = "Password Security";

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO AUXILIAR - Reutiliza o token guardado
        // ═══════════════════════════════════════════════════════════════
        private static async Task<GmailService> GetGmailServiceAsync()
        {
            string credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
            string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token");

            UserCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true)
                );
            }

            return new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO AUXILIAR - Envia o email
        // ═══════════════════════════════════════════════════════════════
        private static async Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlBody)
        {
            try
            {
                var service = await GetGmailServiceAsync();

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(SenderName, SenderEmail));
                mimeMessage.To.Add(new MailboxAddress("", recipientEmail));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart("html") { Text = htmlBody };

                using (var memoryStream = new MemoryStream())
                {
                    await mimeMessage.WriteToAsync(memoryStream);
                    var rawMessage = Convert.ToBase64String(memoryStream.ToArray())
                        .Replace('+', '-')
                        .Replace('/', '_')
                        .Replace("=", "");

                    var gmailMessage = new Message { Raw = rawMessage };
                    await service.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao enviar email: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // CÓDIGO DE VERIFICAÇÃO (Login / CreateAccount)
        // ═══════════════════════════════════════════════════════════════
        internal static async Task<bool> SendVerificationCodeAsync(string recipientEmail, string code)
        {
            string subject = "Password Security - Verification Code";
            string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #333;'>Email Verification</h2>
                    <p>Your verification code is:</p>
                    <h1 style='color: #007bff; font-size: 32px; letter-spacing: 5px;'>{code}</h1>
                    <p>This code expires in <strong>5 minutes</strong>.</p>
                    <p style='color: #666; font-size: 12px;'>
                        If you didn't request this code, please ignore this email.
                    </p>
                </body>
                </html>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }

        // ═══════════════════════════════════════════════════════════════
        // CÓDIGO DE MUDANÇA DE PASSWORD
        // ═══════════════════════════════════════════════════════════════
        internal static async Task<bool> SendPasswordChangeCodeAsync(string recipientEmail, string code)
        {
            string subject = "Password Security - Password Change Request";
            string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #667eea;'>Password Change Request</h2>
                    <p>You have requested to change your master password.</p>
                    <p>Your verification code is:</p>
                    <h1 style='color: #e67e22; font-size: 32px; letter-spacing: 5px;'>{code}</h1>
                    <p>This code expires in <strong>5 minutes</strong>.</p>
                    <p style='color: #888; font-size: 12px;'>
                        If you did not request this change, please ignore this email.
                    </p>
                </body>
                </html>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }
    }
}