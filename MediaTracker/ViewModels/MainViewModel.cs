using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MediaTracker.ViewModels;
public sealed class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<TabViewModel> Tabs { get; } = new();

    private bool _isDarkMode;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode == value) return;
            _isDarkMode = value;
            OnPropertyChanged();
            ApplyTheme();
            SaveThemePreference();

            foreach (var moviesTab in Tabs.OfType<MoviesTabViewModel>())
                moviesTab.UpdateMoviesDarkMode(_isDarkMode);
        }
    }

    public ICommand ToggleDarkModeCommand { get; }
    public MainViewModel()
    {
        ToggleDarkModeCommand = new RelayCommand(_ =>
        {
            IsDarkMode = !IsDarkMode;
        });
        // Add the two tabs
        Tabs.Add(new MoviesTabViewModel());
        Tabs.Add(new SeriesTabViewModel());
    }

    public void LoadThemePreference()
    {
        IsDarkMode = Properties.Settings.Default.IsDarkMode;
    }

    public void SaveThemePreference()
    {
        Properties.Settings.Default.IsDarkMode = IsDarkMode;
        Properties.Settings.Default.Save();
    }

    public void ApplyTheme()
    {
        var appResources = Application.Current.Resources.MergedDictionaries;

        var existingTheme = appResources
            .FirstOrDefault(d => d.Source != null &&
                (d.Source.OriginalString.Contains("DarkTheme") ||
                 d.Source.OriginalString.Contains("LightTheme")));

        if (existingTheme != null)
            appResources.Remove(existingTheme);

        appResources.Add(new ResourceDictionary
        {
            Source = new Uri(
                IsDarkMode ? "Themes/Dark.xaml"
                           : "Themes/Light.xaml",
                UriKind.Relative)
        });
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}