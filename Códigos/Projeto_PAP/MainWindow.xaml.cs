using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Projeto_PAP
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Personalização da barra de título
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Guardar referência ao Frame
            App.MainFrame = MainFrame;

            // Verificar sessão pendente
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            bool pendingLanguageChange = (bool?)localSettings.Values["PendingLanguageChange"] ?? false;
            string sessionEmail = localSettings.Values["SessionEmail"]?.ToString();
            string sessionTotp = localSettings.Values["SessionTotp"]?.ToString();
            string sessionKey = localSettings.Values["SessionKey"]?.ToString();

            if (pendingLanguageChange &&
                !string.IsNullOrEmpty(sessionEmail) &&
                !string.IsNullOrEmpty(sessionKey))
            {
                byte[] key = Convert.FromBase64String(sessionKey);
                var encryptor = new PasswordEncrypt(key);

                var parameters = new NavigationsParameters
                {
                    Encryptor = encryptor,
                    UserEmail = sessionEmail,
                    TotpSecret = sessionTotp
                };

                localSettings.Values.Remove("PendingLanguageChange");
                localSettings.Values.Remove("SessionEmail");
                localSettings.Values.Remove("SessionTotp");
                localSettings.Values.Remove("SessionKey");

                MainFrame.Navigate(typeof(MainPage), parameters);
            }
            else
            {
                MainFrame.Navigate(typeof(LoginPage));
            }
        }
    }
}