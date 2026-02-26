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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Projeto_PAP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            // Esconde os textos inicialmente
            TextHome.Visibility = Visibility.Collapsed;
            TextPasswords.Visibility = Visibility.Collapsed;
            TextSettings.Visibility = Visibility.Collapsed;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void SplitView_PaneOpening(SplitView sender, object args)
        {
            // Mostra os textos quando abre
            TextHome.Visibility = Visibility.Visible;
            TextPasswords.Visibility = Visibility.Visible;
            TextSettings.Visibility = Visibility.Visible;
        }

        private void SplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            // Esconde os textos quando fecha
            TextHome.Visibility = Visibility.Collapsed;
            TextPasswords.Visibility = Visibility.Collapsed;
            TextSettings.Visibility = Visibility.Collapsed;
        }

    }
}
