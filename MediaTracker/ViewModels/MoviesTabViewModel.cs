using MediaTracker.Domain;
using MediaTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using static MediaTracker.Domain.Movie;

namespace MediaTracker.ViewModels;

public sealed class MoviesTabViewModel : TabViewModel, INotifyPropertyChanged
{
    public override string Header => "Movies";

    public ObservableCollection<Movie> MoviesCollection { get; } = new();

    // Properties bound to input fields
    private string? _newTitle;
    public string? NewTitle
    {
        get => _newTitle;
        set { _newTitle = Normalize(value); OnPropertyChanged(); }
    }

    private string? _newFranchise;
    public string? NewFranchise
    {
        get => _newFranchise;
        set { _newFranchise = Normalize(value); OnPropertyChanged(); }
    }

    private string? _newBigFranchise;
    public string? NewBigFranchise
    {
        get => _newBigFranchise;
        set { _newBigFranchise = Normalize(value); OnPropertyChanged(); }
    }
    public int NewYear { get; set; } = DateTime.Now.Year;

    private int? _newFranchiseNumber;
    public int? NewFranchiseNumber
    {
        get => _newFranchiseNumber;
        set { _newFranchiseNumber = value; OnPropertyChanged(); }
    }

    // New Watch Date
    private string? _newWatchDate;
    public string? NewWatchDate
    {
        get => _newWatchDate;
        set { _newWatchDate = value?.Trim(); OnPropertyChanged(); }
    }
    public ObservableCollection<BigFranchiseGroup> BigFranchiseGroups { get; } = new();

    // Commands
    public ICommand AddMovieCommand { get; }
    public ICommand AddWatchDateCommand { get; }
    public ICommand RemoveWatchDateCommand { get; }
    public ICommand RemoveMovieCommand { get; }
    public ICommand EditMovieCommand { get; }
    public ICommand SaveMovieCommand { get; }

    public ICommand ToggleExpandCommand { get; }

    private readonly IMediaRepository _repository;

    public MoviesTabViewModel()
    {
        NewTitle = "";
        NewFranchise = "";
        NewWatchDate = "";

        _repository = new JsonMediaRepository();

        var collectionView = CollectionViewSource.GetDefaultView(MoviesCollection);
        collectionView.GroupDescriptions.Clear();
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription("BigFranchise"));
        MoviesCollection.CollectionChanged += (_, __) => RefreshBigFranchiseGroups();

        ToggleExpandCommand = new RelayCommand(obj =>
        {
            if (obj is Movie movie)
            {
                // Collapse all other movies first
                foreach (var m in MoviesCollection)
                    if (m != movie) m.IsExpanded = false;

                // Toggle the clicked movie
                movie.IsExpanded = !movie.IsExpanded;
            }
        });
        // Load saved movies
        foreach (var movie in _repository.LoadMovies())
        {
            movie.IsExpanded = false;
            MoviesCollection.Add(movie);
        }
        RefreshBigFranchiseGroups();

        AddMovieCommand = new RelayCommand(_ => AddMovie());
        AddWatchDateCommand = new RelayCommand(obj => AddWatchDate((Movie)obj!));
        RemoveWatchDateCommand = new RelayCommand(obj =>
        {
            var tuple = (Tuple<Movie, DateTime>)obj!;
            RemoveWatchDate(tuple);
        });

        RemoveMovieCommand = new RelayCommand(obj =>
        {
            if (obj is Movie movie)
                RemoveMovie(movie);
        });
        EditMovieCommand = new RelayCommand(obj =>
        {
            if (obj is Movie movie)
                movie.IsEditing = true;
        });
        SaveMovieCommand = new RelayCommand(obj =>
        {
            if (obj is Movie movie)
            {
                if (!ValidateMovie(movie))
                    return; // don't save if validation fails

                movie.IsEditing = false;

                // Re-sort movies after editing
                var sorted = MoviesCollection
                    .OrderBy(m => m.Franchise ?? m.Title)
                    .ThenBy(m => m.FranchiseNumber ?? 0)
                    .ThenBy(m => m.Year)
                    .ToList();

                MoviesCollection.Clear();
                foreach (var m in sorted)
                    MoviesCollection.Add(m);

                SaveMovies();
                RefreshBigFranchiseGroups();
            }
        });
    }


    private void AddMovie()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            MessageBox.Show("Title is required.", "Invalid input");
            return;
        }

        if (NewYear < 1900 || NewYear > 2099)
        {
            MessageBox.Show("Year must be between 1900 and 2099.", "Invalid input");
            return;
        }

        // Franchise/franchise number rules
        bool hasFranchise = !string.IsNullOrWhiteSpace(NewFranchise);
        bool hasFranchiseNumber = NewFranchiseNumber.HasValue;

        if ((hasFranchise && !hasFranchiseNumber) || (!hasFranchise && hasFranchiseNumber)) // only one filled
        {
            MessageBox.Show("Both Franchise and Franchise Number must be filled together.", "Invalid input");
            return;
        }


        string trimmedTitle = NewTitle?.Trim() ?? "";
        string? trimmedFranchise = NewFranchise?.Trim();

        // Duplicate check: same title and same franchise
        bool exists = MoviesCollection.Any(m =>
            string.Equals(m.Title, trimmedTitle, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Franchise ?? "", trimmedFranchise ?? "", StringComparison.OrdinalIgnoreCase)
        );

        if (exists)
        {
            MessageBox.Show("A movie with the same title and franchise already exists.", "Duplicate Movie");
            return;
        }

        // Create movie
        var movie = new Movie
        {
            Title = trimmedTitle,
            Franchise = hasFranchise ? trimmedFranchise : null,
            FranchiseNumber = hasFranchiseNumber ? NewFranchiseNumber : null,
            BigFranchise = NewBigFranchise,
            Year = NewYear
        };

        MoviesCollection.Add(movie);

        // Sort movies: group by franchise, then by franchise number
        var sorted = MoviesCollection
            .OrderBy(m => m.Franchise ?? m.Title)
            .ThenBy(m => m.FranchiseNumber ?? 0)
            .ThenBy(m => m.Year)
            .ToList();

        MoviesCollection.Clear();
        foreach (var m in sorted)
            MoviesCollection.Add(m);

        RefreshGroups();
        SaveMovies();
        RefreshBigFranchiseGroups();

        NewTitle = "";
        NewYear = DateTime.Now.Year;
        NewBigFranchise = "";
        NewFranchise = "";
        NewFranchiseNumber = null;
        NewWatchDate = "";
    }

    private void AddWatchDate(Movie movie)
    {
        if (movie == null)
            return;

        string input = NewWatchDate?.Trim() ?? "";

        if (!DateTime.TryParseExact(
                input,
                "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var date))
        {
            MessageBox.Show(
                "Invalid date format.\nUse: dd/MM/yyyy (example: 21/08/2024)",
                "Invalid date");
            return;
        }

        movie.WatchDates.Add(date);
        NewWatchDate = "";
        OnPropertyChanged(nameof(NewWatchDate));
        SaveMovies();
    }

    private void RemoveWatchDate(Tuple<Movie, DateTime> param)
    {
        var movie = param.Item1;
        var date = param.Item2;

        movie?.WatchDates.Remove(date);

        SaveMovies();
    }
    private void SaveMovies()
    {
        _repository.SaveMovies(MoviesCollection);
    }

    private void RemoveMovie(Movie movie)
    {
        if (movie == null) return;

        MoviesCollection.Remove(movie); // UI updates automatically because it's ObservableCollection
        SaveMovies();                  // persist changes to JSON
        RefreshBigFranchiseGroups();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool ValidateMovie(Movie movie)
    {
        string title = movie.Title?.Trim() ?? "";
        string? franchise = movie.Franchise?.Trim();
        int year = movie.Year;

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Title is required.", "Invalid input");
            return false;
        }

        if (year < 1900 || year > 2099)
        {
            MessageBox.Show("Year must be between 1900 and 2099.", "Invalid input");
            return false;
        }

        bool hasFranchise = !string.IsNullOrWhiteSpace(franchise);
        bool hasFranchiseNumber = movie.FranchiseNumber.HasValue;

        // Only invalid if exactly one is filled
        if ((hasFranchise && !hasFranchiseNumber) || (!hasFranchise && hasFranchiseNumber))
        {
            MessageBox.Show("Both Franchise and Franchise Number must be filled together.", "Invalid input");
            return false;
        }

        // If both are empty, that’s OK — we’ll just save as null
        if (!hasFranchise && !hasFranchiseNumber)
        {
            movie.Franchise = null;
            movie.FranchiseNumber = null;
        }


        // Check for duplicates excluding itself
        bool exists = MoviesCollection.Any(m =>
            m != movie &&
            string.Equals(m.Title, title, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Franchise ?? "", franchise ?? "", StringComparison.OrdinalIgnoreCase)
        );

        if (exists)
        {
            MessageBox.Show("A movie with the same title and franchise already exists.", "Duplicate Movie");
            return false;
        }

        // If everything passes, update trimmed values
        movie.Title = title;
        movie.Franchise = franchise;
        movie.Note = movie.Note?.Trim();
        return true;
    }
    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return System.Text.RegularExpressions.Regex
            .Replace(text.Trim(), @"\s+", " ");
    }

    private void RefreshGroups()
    {
        var groups = MoviesCollection
            .GroupBy(m => string.IsNullOrWhiteSpace(m.BigFranchise) ? "Undefined" : m.BigFranchise)
            .OrderBy(g => g.Key == "Undefined" ? "ZZZ" : g.Key) // Undefined at bottom
            .Select(g =>
            {
                var group = new Movie.BigFranchiseGroup { Name = g.Key };
                foreach (var movie in g.OrderBy(m => m.Franchise ?? m.Title)
                                       .ThenBy(m => m.FranchiseNumber ?? 0))
                {
                    group.Movies.Add(movie);
                }
                return group;
            });

        BigFranchiseGroups.Clear();
        foreach (var group in groups)
            BigFranchiseGroups.Add(group);
    }

    private void RefreshBigFranchiseGroups()
    {
        BigFranchiseGroups.Clear();

        // Group movies by BigFranchise
        var groups = MoviesCollection
            .GroupBy(m => string.IsNullOrWhiteSpace(m.BigFranchise) ? "Undefined" : m.BigFranchise)
            .OrderBy(g => g.Key == "Undefined" ? "ZZZ" : g.Key); // Undefined goes last

        foreach (var g in groups)
        {
            var group = new Movie.BigFranchiseGroup { Name = g.Key };
            foreach (var m in g)
                group.Movies.Add(m);
            BigFranchiseGroups.Add(group);
        }
    }

}
