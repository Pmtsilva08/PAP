using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
            }
        }

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
    }
}