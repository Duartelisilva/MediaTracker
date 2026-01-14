using System.Windows;
using MediaTracker.ViewModels;

namespace MediaTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainViewModel = new MainViewModel();
            mainViewModel.LoadThemePreference(); // load the saved dark/light mode

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
        }
    }
}
