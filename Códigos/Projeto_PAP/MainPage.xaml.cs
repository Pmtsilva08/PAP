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

        public MainPage()
        {
            InitializeComponent();
            TextHome.Visibility = Visibility.Collapsed;
            TextPasswords.Visibility = Visibility.Collapsed;
            TextSettings.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NavigationsParameters parameters)
            {
                _encryptor = parameters.Encryptor;
                _userEmail = parameters.UserEmail;
                _totpSecret = parameters.TotpSecret;

                // Navegar automaticamente para HomePage
                var homeParameters = new NavigationsParameters
                {
                    Encryptor = _encryptor,
                    UserEmail = _userEmail,
                    TotpSecret = _totpSecret
                };
                ContentFrame.Navigate(typeof(HomePage), homeParameters);
            }
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
                TotpSecret = _totpSecret
            };
            ContentFrame.Navigate(typeof(HomePage), parameters);
        }

        private void PasswordsButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new NavigationsParameters
            {
                Encryptor = _encryptor,
                UserEmail = _userEmail,
                TotpSecret = _totpSecret
            };
            ContentFrame.Navigate(typeof(PasswordsPage), parameters);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new NavigationsParameters
            {
                Encryptor = _encryptor,
                UserEmail = _userEmail,
                TotpSecret = _totpSecret
            };
            ContentFrame.Navigate(typeof(SettingsPage), parameters);
        }
    }
}