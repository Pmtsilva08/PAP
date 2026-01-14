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
                // BaseClientService.Initializer é o "formulário de configuração" usado por todos os serviços Google.
                // HttpClientInitializer recebe o token OAuth2 (prova de autorização).
                // ApplicationName identifica a app nos logs da Google.
                // GmailService é o serviço específico para trabalhar com emails, baseado no modelo genérico BaseClientService.


            }
        }
    }
}
