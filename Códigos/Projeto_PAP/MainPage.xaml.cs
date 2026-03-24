using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;

namespace Projeto_PAP
{
    public sealed partial class MainPage : Page
    {
        private PasswordEncrypt _encryptor;
        private string _userEmail;
        private string _totpSecret;
        private bool _requireVerification;

        public MainPage()
        {
            InitializeComponent();
            TextHome.Visibility = Visibility.Collapsed;
            TextPasswords.Visibility = Visibility.Collapsed;
            TextSettings.Visibility = Visibility.Collapsed;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NavigationsParameters parameters)
            {
                _encryptor = parameters.Encryptor;
                _userEmail = parameters.UserEmail;
                _totpSecret = parameters.TotpSecret;
            }

            // Carregar preferência de verificação da base de dados
            _requireVerification = await LoginQuery.GetRequireVerificationSetting(_userEmail);

            // Navegar para HomePage por defeito
            ContentFrame.Navigate(typeof(HomePage), new NavigationsParameters
            {
                Encryptor = _encryptor,
                UserEmail = _userEmail,
                TotpSecret = _totpSecret,
                RequireVerificationForActions = _requireVerification
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // MENU TOGGLE
        // ═══════════════════════════════════════════════════════════════
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void SplitView_PaneOpening(SplitView sender, object args)
        {
            TextHome.Visibility = Visibility.Visible;
            TextPasswords.Visibility = Visibility.Visible;
            TextSettings.Visibility = Visibility.Visible;
        }

        private void SplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            TextHome.Visibility = Visibility.Collapsed;
            TextPasswords.Visibility = Visibility.Collapsed;
            TextSettings.Visibility = Visibility.Collapsed;
        }

        // ═══════════════════════════════════════════════════════════════
        // NAVEGAÇÃO
        // ═══════════════════════════════════════════════════════════════
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new NavigationsParameters
            {
                Encryptor = _encryptor,
                UserEmail = _userEmail,
                TotpSecret = _totpSecret,
                RequireVerificationForActions = _requireVerification  // ← ADICIONA
            };
            ContentFrame.Navigate(typeof(HomePage), parameters);
        }

        private void PasswordsButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new NavigationsParameters
            {
                Encryptor = _encryptor,
                UserEmail = _userEmail,
                TotpSecret = _totpSecret,
                RequireVerificationForActions = _requireVerification  // ← ADICIONA
            };
            ContentFrame.Navigate(typeof(PasswordsPage), parameters);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new NavigationsParameters
            {
                Encryptor = _encryptor,
                UserEmail = _userEmail,
                TotpSecret = _totpSecret,
                RequireVerificationForActions = _requireVerification  // ← ADICIONA
            };
            ContentFrame.Navigate(typeof(SettingsPage), parameters);
        }
    }
}