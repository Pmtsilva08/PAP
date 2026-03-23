using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Projeto_PAP
{
    public partial class App : Application
    {
        private Window? _window;

        public static Window AppMainWindow { get; private set; }
        public static Frame MainFrame { get; set; }  

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            AppMainWindow = _window;
            _window.Activate();
        }
    }
}