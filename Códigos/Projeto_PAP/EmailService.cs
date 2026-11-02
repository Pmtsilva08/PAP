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
        // Conta Gmail que envia os emails (tem de ser a mesma que autoriza o OAuth no credentials.json)
        private static readonly string SenderEmail = Environment.GetEnvironmentVariable("PAP_SENDER_EMAIL") ?? string.Empty;
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

        internal static async Task<bool> SendDeleteAccountCodeAsync(string recipientEmail, string code)
        {
            string subject = "⚠️ Código para Apagar Conta - Password Security";
            string body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                <h1 style='color: white; margin: 0;'>⚠️ Apagar Conta</h1>
            </div>
            <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px;'>
                <p style='color: #333; font-size: 16px;'>Recebemos um pedido para <strong>apagar permanentemente</strong> a tua conta do Password Security.</p>
                
                <p style='color: #e74c3c; font-size: 14px; font-weight: bold;'>⚠️ ATENÇÃO: Esta ação é irreversível! Todas as tuas palavras-passe guardadas serão perdidas.</p>
                
                <div style='background: #f8f9fa; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0;'>
                    <p style='color: #666; margin: 0 0 10px 0; font-size: 14px;'>O teu código de verificação é:</p>
                    <h2 style='color: #e74c3c; font-size: 32px; letter-spacing: 5px; margin: 0;'>{code}</h2>
                </div>
                
                <p style='color: #666; font-size: 14px;'>Este código expira em <strong>5 minutos</strong>.</p>
                <p style='color: #666; font-size: 14px;'>Se não foste tu a fazer este pedido, ignora este email e a tua conta permanecerá segura.</p>
                
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px; text-align: center;'>Password Security - Gestor de Palavras-passe</p>
            </div>
        </div>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }

        internal static async Task<bool> SendPasswordRecoveryCodeAsync(string recipientEmail, string code)
        {
            string subject = "🔑 Recuperação de Palavra-passe - Password Security";
            string body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                <h1 style='color: white; margin: 0;'>🔑 Recuperar Palavra-passe</h1>
            </div>
            <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px;'>
                <p style='color: #333; font-size: 16px;'>Recebemos um pedido para recuperar a tua palavra-passe do Password Security.</p>
                
                <div style='background: #f8f9fa; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0;'>
                    <p style='color: #666; margin: 0 0 10px 0; font-size: 14px;'>O teu código de verificação é:</p>
                    <h2 style='color: #667eea; font-size: 32px; letter-spacing: 5px; margin: 0;'>{code}</h2>
                </div>
                
                <p style='color: #666; font-size: 14px;'>Este código expira em <strong>5 minutos</strong>.</p>
                <p style='color: #666; font-size: 14px;'>Se não foste tu a fazer este pedido, ignora este email.</p>
                
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px; text-align: center;'>Password Security - Gestor de Palavras-passe</p>
            </div>
        </div>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }

        // Email de alerta - tentativa falhada de recuperação
        internal static async Task<bool> SendSecurityAlertFailedRecoveryAsync(string recipientEmail)
        {
            string subject = "🚨 Alerta de Segurança - Password Security";
            string body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                <h1 style='color: white; margin: 0;'>🚨 Alerta de Segurança</h1>
            </div>
            <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px;'>
                <p style='color: #333; font-size: 16px;'>Detetámos uma <strong>tentativa falhada de recuperação de palavra-passe</strong> na tua conta do Password Security.</p>
                
                <div style='background: #fef3f2; border-left: 4px solid #e74c3c; padding: 15px; margin: 20px 0;'>
                    <p style='color: #c0392b; margin: 0; font-weight: bold;'>⚠️ Alguém tentou responder às tuas perguntas de segurança incorretamente.</p>
                </div>
                
                <p style='color: #666; font-size: 14px;'>Se foste tu, não te preocupes. Podes tentar novamente em 2 horas.</p>
                <p style='color: #666; font-size: 14px;'>Se <strong>não foste tu</strong>, recomendamos que:</p>
                <ul style='color: #666; font-size: 14px;'>
                    <li>Verifiques a segurança da tua conta de email</li>
                    <li>Consideres alterar as tuas perguntas de segurança</li>
                </ul>
                
                <p style='color: #999; font-size: 12px;'>Data e hora: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px; text-align: center;'>Password Security - Gestor de Palavras-passe</p>
            </div>
        </div>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }

        // Email de alerta - código de verificação falhado
        internal static async Task<bool> SendSecurityAlertFailedCodeAsync(string recipientEmail)
        {
            string subject = "🚨 Alerta de Segurança - Código Incorreto - Password Security";
            string body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                <h1 style='color: white; margin: 0;'>🚨 Alerta de Segurança</h1>
            </div>
            <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px;'>
                <p style='color: #333; font-size: 16px;'>Detetámos uma <strong>tentativa falhada de introdução do código de verificação</strong> durante a recuperação da tua conta.</p>
                
                <div style='background: #fef3f2; border-left: 4px solid #e74c3c; padding: 15px; margin: 20px 0;'>
                    <p style='color: #c0392b; margin: 0; font-weight: bold;'>⚠️ Alguém pode ter tentado aceder à tua conta!</p>
                </div>
                
                <p style='color: #666; font-size: 14px;'>Esta pessoa conseguiu responder às tuas perguntas de segurança, mas falhou o código de verificação.</p>
                <p style='color: #666; font-size: 14px;'>Se <strong>não foste tu</strong>, recomendamos que:</p>
                <ul style='color: #666; font-size: 14px;'>
                    <li>Alteres imediatamente a tua palavra-passe</li>
                    <li>Alteres as tuas perguntas de segurança</li>
                    <li>Verifiques a segurança da tua conta de email</li>
                </ul>
                
                <p style='color: #999; font-size: 12px;'>Data e hora: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px; text-align: center;'>Password Security - Gestor de Palavras-passe</p>
            </div>
        </div>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }

        // Email informativo - password alterada nas definições
        internal static async Task<bool> SendPasswordChangedNotificationAsync(string recipientEmail)
        {
            string subject = "✅ Palavra-passe Alterada - Password Security";
            string body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #27ae60 0%, #2ecc71 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                <h1 style='color: white; margin: 0;'>✅ Palavra-passe Alterada</h1>
            </div>
            <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px;'>
                <p style='color: #333; font-size: 16px;'>A tua <strong>palavra-passe mestra</strong> foi alterada com sucesso no Password Security.</p>
                
                <div style='background: #f0fdf4; border-left: 4px solid #27ae60; padding: 15px; margin: 20px 0;'>
                    <p style='color: #166534; margin: 0;'>✓ A alteração foi realizada através das definições da aplicação.</p>
                </div>
                
                <p style='color: #666; font-size: 14px;'>Se <strong>não foste tu</strong> a fazer esta alteração, contacta-nos imediatamente e tenta recuperar a tua conta.</p>
                
                <p style='color: #999; font-size: 12px;'>Data e hora: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px; text-align: center;'>Password Security - Gestor de Palavras-passe</p>
            </div>
        </div>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }

        // Email informativo - perguntas de segurança alteradas
        internal static async Task<bool> SendSecurityQuestionsChangedAsync(string recipientEmail)
        {
            string subject = "✅ Perguntas de Segurança Alteradas - Password Security";
            string body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #27ae60 0%, #2ecc71 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                <h1 style='color: white; margin: 0;'>✅ Perguntas de Segurança Alteradas</h1>
            </div>
            <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px;'>
                <p style='color: #333; font-size: 16px;'>As tuas <strong>perguntas de segurança</strong> foram alteradas com sucesso no Password Security.</p>
                
                <div style='background: #f0fdf4; border-left: 4px solid #27ae60; padding: 15px; margin: 20px 0;'>
                    <p style='color: #166534; margin: 0;'>✓ A alteração foi realizada através das definições da aplicação.</p>
                </div>
                
                <p style='color: #666; font-size: 14px;'>Se <strong>não foste tu</strong> a fazer esta alteração, a tua conta pode estar comprometida. Altera imediatamente a tua palavra-passe.</p>
                
                <p style='color: #999; font-size: 12px;'>Data e hora: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px; text-align: center;'>Password Security - Gestor de Palavras-passe</p>
            </div>
        </div>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }

        // Código para alterar perguntas de segurança
        internal static async Task<bool> SendSecurityQuestionsChangeCodeAsync(string recipientEmail, string code)
        {
            string subject = "🔐 Código para Alterar Perguntas de Segurança - Password Security";
            string body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                <h1 style='color: white; margin: 0;'>🔐 Alterar Perguntas de Segurança</h1>
            </div>
            <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 10px 10px;'>
                <p style='color: #333; font-size: 16px;'>Recebemos um pedido para alterar as tuas perguntas de segurança no Password Security.</p>
                
                <div style='background: #f8f9fa; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0;'>
                    <p style='color: #666; margin: 0 0 10px 0; font-size: 14px;'>O teu código de verificação é:</p>
                    <h2 style='color: #667eea; font-size: 32px; letter-spacing: 5px; margin: 0;'>{code}</h2>
                </div>
                
                <p style='color: #666; font-size: 14px;'>Este código expira em <strong>5 minutos</strong>.</p>
                <p style='color: #666; font-size: 14px;'>Se não foste tu a fazer este pedido, ignora este email.</p>
                
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px; text-align: center;'>Password Security - Gestor de Palavras-passe</p>
            </div>
        </div>";

            return await SendEmailAsync(recipientEmail, subject, body);
        }
    }
}