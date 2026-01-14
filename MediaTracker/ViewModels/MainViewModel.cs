using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MediaTracker.ViewModels; 
public sealed class MainViewModel 
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
        }
    }

    public MainViewModel()
    { 
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
        var dict = new ResourceDictionary
        {
            Source = new Uri(
                IsDarkMode ? "Themes/DarkTheme.xaml"
                           : "Themes/LightTheme.xaml",
                UriKind.Relative)
        };

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(dict);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}