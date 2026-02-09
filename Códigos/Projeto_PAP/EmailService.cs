using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // para trabalhar com ficheiros
using System.Threading;
using Google.Apis.Auth.OAuth2; // biblioteca da google para autenticação OAuth2 (pedir permissão)
using Google.Apis.Services; // Classe base para criar serviços (como o GmailService)
using Google.Apis.Gmail.v1; // biblioteca da google para ausar a Gmail API (enviar emails)
using Google.Apis.Gmail.v1.Data; // Classes de dados da Gmail API (como Message).
using Google.Apis.Util.Store; // para guardar o token localmente
using MimeKit; // Biblioteca para criar emails formatados

namespace Projeto_PAP
{
    internal static class EmailService
    {
        private static readonly string[] Scopes = { GmailService.Scope.GmailSend }; // readonly - (não pode ser alterado depois de definido); GmailService.Scope.GmailSend - (Permissão para enviar emails, mas mesmo só para enviar).  O que é Scope? São as permissões que a app pede ao dono do email que envia emails. Aqui só pedimos para enviar emails, não para ler, apagar, etc.
        private static readonly string ApplicationName = "PasswordSecurityApp"; // Nome da aplicação. Aparece nos logs da Google e ajuda a identificar a app.

        // Logs são registos/histórico de tudo o que acontece num sistema. A Google guarda logs de tudo Para que servem os logs:
        // - Algo correu mal (Ver o que aconteceu e quando)
        // - Alguém fez abuso (Identificar quem foi)
        // - Debugging (Perceber onde está o erro)
        // - Estatísticas (Quantos emails enviaste)


        private static readonly string SenderEmail = "manager.pass.secure.app@gmail.com"; // O email que vai enviar os emails
        private static readonly string SenderName = "Password Security"; // define o nome que vai aparecer junto ao email

        internal static async Task<bool> SendVerificationCodeAsync(string recipientEmail, string code) // Este é o método principal que envia o email. Recebe para quem enviar e que código incluir
        {
            try
            {
                UserCredential credential; // guarda o token de acesso dado pela Google após autorização para enviar emails (diferente do credentials.json que é a identidade da app)
                string credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json"); // caminho para o ficheiro de credenciais (credentials.json) que contém as informações necessárias para autenticar a app junto da Google (ID da app, segredo, etc.)
                // AppDomain.CurrentDomain.BaseDirectory é a pasta onde a app está a correr; Path.Combine junta os caminhos de forma correta. 

                // Path.Combine é melhor do que concatenar strings com + porque trata automaticamente dos separadores de pastas (/, \) dependendo do sistema operativo.
                // Path.Combine adiciona a \ automaticamente se precisar!

                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read)) // vai abrir o ficheiro credentials.json apenas no modo de leitura. o using garante que o ficheiro é fechado quando não se precisar mais dele
                {
                    string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token"); // caminho para a pasta onde o token de acesso vai ser guardado
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets, // Converte o credentials.json (JSON) para objeto C# e obtém as credenciais (ClientId + ClientSecret) que identificam a app perante a Google
                        Scopes, // Permissões que a app está a pedir (neste caso, só enviar emails)
                        "user", //
                        CancellationToken.None, // Não permito cancelamento da autorização
                        new FileDataStore(tokenPath, true) // Guarda o token de acesso na pasta "token" para não ser preciso pedir autorização outra vez. É uma classe que guarda e lê dados em ficheiros. Funciona como uma "base de dados" simples em ficheiros.
                    );
                }

                var service = new GmailService(new BaseClientService.Initializer() //GmailService é a classe principal para interagir com a Gmail API
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Cria o serviço Gmail configurado e pronto a enviar emails.
                // BaseClientService.Initializer é o "formulário de configuração" usado por todos os serviços Google. Eu só uso o BaseClientService para "encontrar" a classe Initializer.
                // HttpClientInitializer recebe o token OAuth2 (prova de autorização).
                // ApplicationName identifica a app nos logs da Google.
                // GmailService é o serviço específico para trabalhar com emails, baseado no modelo genérico BaseClientService.

                var mimeMessage = new MimeMessage(); // Cria um novo email formatado(com a estrutura de um email) usando a biblioteca MimeKit(serve principalmente para emails)
                mimeMessage.From.Add(new MailboxAddress(SenderName, SenderEmail)); // Define o remetente do email (quem envia)
                mimeMessage.To.Add(new MailboxAddress("", recipientEmail));
                mimeMessage.Subject = "Password Security - Verification Code";
                mimeMessage.Body = new TextPart("html")
                {
                    Text = $@"
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
                        </html>"
                };
                using (var memoryStream = new MemoryStream()) // Cria um ficheiro temporario na RAM. Mas porque? Porque a Gmail API precisa de uma string de texto base64.
                                                              // Mas o mimeMessage não consegue converter diretamente o objeto para string. Por isso é que faço um passo intermédio que é armazenar o objeto num ficheiro temporário na RAM e então depois converter para texto.
                {
                    await mimeMessage.WriteToAsync(memoryStream); // Escreve o conteúdo do email formatado para o ficheiro temporário na RAM e bytes.
                    var rawMessage = Convert.ToBase64String(memoryStream.ToArray()) // Converte o conteúdo do ficheiro temporário (bytes) para uma string em base64.o memoryStream já está em bytes mas para converter para string é preciso usar o ToArray() para obter o array de bytes, é preciso mesmo um array de bytes.

                        // A Gmail API exige que a mensagem seja codificada em base64url, que é uma variação da base64 padrão, é uma regra deles.
                        //Existem certos caracteres que na Base64 normal não são seguros para URLs como '+' e '/'.
                        .Replace('+', '-') //+ passa para -
                        .Replace('/', '_') // / passa para _
                        .Replace("=", ""); //o igual desaparece

                    var gmailMessage = new Message // Crio o objeto Message da Gmail API que representa o email a ser enviado
                    {
                        Raw = rawMessage // Define o conteúdo do email codificado em base64url
                    };

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
    }
}
