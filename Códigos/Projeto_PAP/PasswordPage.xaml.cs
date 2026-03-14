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

        public PasswordsPage()
        {
            InitializeComponent();
            Passwords = new ObservableCollection<PasswordItem>();
            // Eventos de hover para o botão
            AddButton.PointerEntered += AddButton_PointerEntered;
            AddButton.PointerExited += AddButton_PointerExited;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadPasswords();
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            
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
