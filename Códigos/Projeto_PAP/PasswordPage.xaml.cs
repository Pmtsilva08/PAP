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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Projeto_PAP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PasswordsPage : Page
    {

        public ObservableCollection<PasswordItem> Passwords { get; set; }
        private PasswordEncrypt _encryptor;
        private string _userEmail;
        private PasswordQuery _passwordQuery;

        public PasswordsPage()
        {
            InitializeComponent();
            Passwords = new ObservableCollection<PasswordItem>();
            _passwordQuery = new PasswordQuery();
            // Eventos de hover para o botão
            AddButton.PointerEntered += AddButton_PointerEntered;
            AddButton.PointerExited += AddButton_PointerExited;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NavigationsParameters parameters)
            {
                _encryptor = parameters.Encryptor;
                _userEmail = parameters.UserEmail;
            }

            await LoadPasswordsFromDatabase();
        }

        private async Task LoadPasswordsFromDatabase()
        {
            Passwords.Clear();

            var passwordsFromDb = await _passwordQuery.GetPasswordsAsync(_userEmail);

            foreach (var password in passwordsFromDb)
            {
                Passwords.Add(password);
            }

            if (Passwords.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                PasswordsList.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                PasswordsList.Visibility = Visibility.Visible;
            }
        }

        private void AddButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var storyboard = (Storyboard)AddButton.Resources["HoverIn"];
            storyboard.Begin();
        }

        private void AddButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var storyboard = (Storyboard)AddButton.Resources["HoverOut"];
            storyboard.Begin();
        }


        private void LoadPasswords()
        {
            if (Passwords.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                PasswordsList.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                PasswordsList.Visibility = Visibility.Visible;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialogContent = new AddPasswordDialog();
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

            ContentDialog dialog = new ContentDialog()
            {
                Title = resourceLoader.GetString("AddDialogTitle"),
                Content = dialogContent,
                PrimaryButtonText = resourceLoader.GetString("SaveButton"),
                CloseButtonText = resourceLoader.GetString("CancelButton"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            dialog.PrimaryButtonClick += (s, args) =>
            {
                if (!dialogContent.IsValid())
                {
                    args.Cancel = true;
                    dialogContent.ShowError();
                }
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // ENCRIPTA a password!
                string encryptedPassword = _encryptor.Encrypt(dialogContent.Password);

                var newPassword = new PasswordItem
                {
                    SiteName = dialogContent.SiteName,
                    Url = dialogContent.Url,
                    Email = dialogContent.Email,
                    EncryptedPassword = encryptedPassword,  // Password encriptada!
                    Category = dialogContent.Category,
                    Notes = dialogContent.Notes
                };

                // Guarda no MongoDB
                bool success = await _passwordQuery.AddPasswordAsync(_userEmail, newPassword);

                if (success)
                {
                    Passwords.Add(newPassword);
                    await LoadPasswordsFromDatabase();
                }
            }
        }

        private string GetLocalizedString(string key)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            return resourceLoader.GetString(key);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ViewPassword_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void EditPassword_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeletePassword_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
