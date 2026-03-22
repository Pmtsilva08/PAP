using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Projeto_PAP
{
    public sealed partial class ChangePasswordPage : Page
    {
        private PasswordEncrypt _encryptor;
        private string _userEmail;
        private string _totpSecret;

        public ChangePasswordPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NavigationsParameters parameters)
            {
                _encryptor = parameters.Encryptor;
                _userEmail = parameters.UserEmail;
                _totpSecret = parameters.TotpSecret;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Validações
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowError(resourceLoader.GetString("SettingsPage_FillAllFields"));
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowError(resourceLoader.GetString("SettingsPage_PasswordsNotMatch"));
                return;
            }

            if (newPassword.Length < 8)
            {
                ShowError(resourceLoader.GetString("SettingsPage_PasswordTooShort"));
                return;
            }

            // Atualizar password
            bool updated = await LoginQuery.UpdatePassword(_userEmail, newPassword);

            if (!updated)
            {
                ShowError(resourceLoader.GetString("SettingsPage_UpdateFailed"));
                return;
            }

            // Sucesso
            ContentDialog successDialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("SettingsPage_PasswordChanged"),
                Content = resourceLoader.GetString("SettingsPage_PasswordChangedMessage"),
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await successDialog.ShowAsync();

            // Logout
            _encryptor?.Clear();
            Frame.Navigate(typeof(LoginPage));
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorPanel.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}