using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Security.Cryptography;

namespace Projeto_PAP
{
    public sealed partial class QuickUnlockPage : Page
    {
        private string _userEmail;
        private string _totpSecret;

        public QuickUnlockPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Carregar dados da sessão
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            _userEmail = localSettings.Values["SessionEmail"]?.ToString() ?? "";
            _totpSecret = localSettings.Values["SessionTotp"]?.ToString() ?? "";

            EmailText.Text = _userEmail;

            // Limpar flag de mudança de idioma
            localSettings.Values["PendingLanguageChange"] = false;
        }

        private async void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(password))
            {
                ShowError(resourceLoader.GetString("TOTP_PasswordRequired"));
                return;
            }

            // Verificar password
            var loginResult = await LoginQuery.LoginVerification(_userEmail, password);

            if (!loginResult.Success)
            {
                ShowError(resourceLoader.GetString("TOTP_InvalidPassword"));
                return;
            }

            // Criar encryptor com a password e o salt
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                loginResult.Salt,
                100000,
                HashAlgorithmName.SHA256,
                32
            );

            var encryptor = new PasswordEncrypt(key);

            // Limpar dados de sessão
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove("SessionEmail");
            localSettings.Values.Remove("SessionTotp");

            // Navegar para MainPage
            var parameters = new NavigationsParameters
            {
                Encryptor = encryptor,
                UserEmail = _userEmail,
                TotpSecret = _totpSecret
            };

            Frame.Navigate(typeof(MainPage), parameters);
        }

        private void DifferentAccount_Click(object sender, RoutedEventArgs e)
        {
            // Limpar sessão e ir para login
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove("SessionEmail");
            localSettings.Values.Remove("SessionTotp");
            localSettings.Values.Remove("PendingLanguageChange");

            Frame.Navigate(typeof(LoginPage));
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorPanel.Visibility = Visibility.Visible;
        }
    }
}