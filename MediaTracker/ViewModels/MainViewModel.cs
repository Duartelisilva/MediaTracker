using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MediaTracker.ViewModels;

public enum SearchOption
{
    Title,
    Year,
    Saga,
    Favorite,
    Franchise, // Movies only
    Author     // Books only
}

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

            foreach (var tab in Tabs)
                tab.UpdateMediaDarkMode(_isDarkMode);
        }
    }

    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            OnPropertyChanged();

            foreach (var tab in Tabs)
                tab.SetSearch(_searchText);
        }
    }

    private SearchOption _selectedSearchOption = SearchOption.Title;
    public SearchOption SelectedSearchOption
    {
        get => _selectedSearchOption;
        set
        {
            if (_selectedSearchOption == value) return;
            _selectedSearchOption = value;
            OnPropertyChanged();

            foreach (var tab in Tabs)
                tab.SetSearchOption(_selectedSearchOption);
        }
    }

    private object? _selectedTab;
    public object? SelectedTab
    {
        get => _selectedTab;
        set
        {
            _selectedTab = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsMoviesTab));
            OnPropertyChanged(nameof(IsBooksTab));

            // Reset invalid search options
            if (!IsMoviesTab && SelectedSearchOption == SearchOption.Franchise)
                SelectedSearchOption = SearchOption.Title;

            if (!IsBooksTab && SelectedSearchOption == SearchOption.Author)
                SelectedSearchOption = SearchOption.Title;
        }
    }


    public bool IsMoviesTab => SelectedTab is MoviesTabViewModel;
    public bool IsBooksTab => SelectedTab is BooksTabViewModel;


    public ICommand ToggleDarkModeCommand { get; }
    public MainViewModel()
    {
        ToggleDarkModeCommand = new RelayCommand(_ =>
        {
            IsDarkMode = !IsDarkMode;
        });
        // Add the tabs
        Tabs.Add(new MoviesTabViewModel());
        Tabs.Add(new SeriesTabViewModel());
        Tabs.Add(new BooksTabViewModel());
        Tabs.Add(new WishlistTabViewModel());
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