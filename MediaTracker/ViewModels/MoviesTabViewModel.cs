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
using System.Windows.Media;
using System.Text.Json.Serialization;

namespace MediaTracker.ViewModels;

public sealed class MoviesTabViewModel : MediaTabViewModel<Movie>
{
    public override string Header => "Items";

    private string? _newFranchise;
    public string? NewFranchise
    {
        get => _newFranchise;
        set { _newFranchise = Media.Normalize(value); OnPropertyChanged(); }
    }

    private int? _newFranchiseNumber;
    public int? NewFranchiseNumber
    {
        get => _newFranchiseNumber;
        set { _newFranchiseNumber = value; OnPropertyChanged(); }
    }



    // Commands
    public ICommand AddMovieCommand { get; }
    public ICommand AddWatchDateCommand { get; }
    public ICommand RemoveWatchDateCommand { get; }
    public ICommand RemoveMovieCommand { get; }
    public ICommand EditMovieCommand { get; }
    public ICommand SaveMovieCommand { get; }
    public ICommand ToggleExpandCommand { get; }
    public ICommand ToggleSidePanelCommand { get; }
    public ICommand UndoMovieCommand { get; }

    private readonly IMediaRepository _repository;


    public MoviesTabViewModel()
    {
        NewTitle = "";
        NewFranchise = "";
        NewWatchDate = "";

        _repository = new JsonMediaRepository();

        var collectionView = CollectionViewSource.GetDefaultView(MediaCollection);
        collectionView.GroupDescriptions.Clear();
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Saga"));
        MediaCollection.CollectionChanged += (_, __) => RefreshSagaGroups();

        // Load saved movies
        foreach (var movie in _repository.LoadMovies())
        {
            movie.IsExpanded = false;
            movie.IsSidePanelOpen = false;
            MediaCollection.Add(movie);

        }
        RefreshSagaGroups();

        // Attach collapse callback
        foreach (var movie in MediaCollection)
        {
            movie.ClearNewWatchDate = () => NewWatchDate = "";
        }

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
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{movie.Title}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                RemoveMovie(movie);
            }

        });

        EditMovieCommand = new RelayCommand(obj =>
        {
            if (obj is Movie movie)
            {
                // Check if another movie is already being edited
                var otherEditing = MediaCollection.FirstOrDefault(m => m != movie && m.IsEditing);
                if (otherEditing != null)
                {
                    var result = MessageBox.Show(
                        $"Movie '{otherEditing.Title}' is already being edited.\n\n" +
                        "Click Yes to discard changes and edit this movie, or No to cancel.",
                        "Editing in progress",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        // User chooses to cancel — do nothing
                        return;
                    }

                    // User chooses Yes — undo previous edits
                    otherEditing.Title = otherEditing.BackupTitle;
                    otherEditing.Year = otherEditing.BackupYear;
                    otherEditing.Saga = otherEditing.BackupSaga;
                    otherEditing.Franchise = otherEditing.BackupFranchise;
                    otherEditing.FranchiseNumber = otherEditing.BackupFranchiseNumber;
                    otherEditing.Note = otherEditing.BackupNote;
                    otherEditing.BaseColor = otherEditing.BackupBaseColor;

                    otherEditing.IsEditing = false;
                }

                // Backup current values for the new movie
                movie.BackupTitle = movie.Title;
                movie.BackupYear = movie.Year;
                movie.BackupSaga = movie.Saga;
                movie.BackupFranchise = movie.Franchise;
                movie.BackupFranchiseNumber = movie.FranchiseNumber;
                movie.BackupNote = movie.Note;
                movie.BackupBaseColor = movie.BaseColor;

                // Enable edit mode for this movie
                movie.IsEditing = true;
            }
        });

        SaveMovieCommand = new RelayCommand(obj =>
        {
            if (obj is Movie movie)
            {
                if (!ValidateMovie(movie))
                    return; // don't save if validation fails

                movie.IsEditing = false;

                // Re-sort movies after editing
                var sorted = MediaCollection
                    .OrderBy(m => m.Franchise ?? m.Title)
                    .ThenBy(m => m.FranchiseNumber ?? 0)
                    .ThenBy(m => m.Year)
                    .ToList();

                MediaCollection.Clear();
                foreach (var m in sorted)
                    MediaCollection.Add(m);

                if (!string.IsNullOrWhiteSpace(movie.Franchise))
                {
                    var sameFranchise = MediaCollection
                        .Where(m => string.Equals(m.Franchise, movie.Franchise, StringComparison.OrdinalIgnoreCase));

                    foreach (var m in sameFranchise)
                        m.BaseColor = movie.BaseColor;
                }

                SaveMovies();
                RefreshSagaGroups();
                movie.IsExpanded = false;
            }
        });

        UndoMovieCommand = new RelayCommand(obj =>
        {
            if (obj is Movie movie)
            {
                // Restore backup values
                movie.Title = movie.BackupTitle;
                movie.Year = movie.BackupYear;
                movie.Saga = movie.BackupSaga;
                movie.Franchise = movie.BackupFranchise;
                movie.FranchiseNumber = movie.BackupFranchiseNumber;
                movie.Note = movie.BackupNote;
                movie.BaseColor = movie.BackupBaseColor;

                movie.IsEditing = false;
            }
        });

        ToggleExpandCommand = new RelayCommand(obj =>
        {
            if (obj is Movie clickedMovie)
            {
                // Close all other movies
                foreach (var movie in MediaCollection)
                {
                    if (movie != clickedMovie)
                        movie.IsExpanded = false;
                }

                // Toggle the clicked movie
                clickedMovie.IsExpanded = !clickedMovie.IsExpanded;
            }
        });

        ToggleSidePanelCommand = new RelayCommand(obj =>
        {
            if (obj is Movie movie)
            {
                // Close other panels
                foreach (var m in MediaCollection)
                    if (m != movie) m.IsSidePanelOpen = false;

                // Toggle clicked panel
                movie.IsSidePanelOpen = !movie.IsSidePanelOpen;
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
        bool exists = MediaCollection.Any(m =>
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
            Saga = NewSaga,
            Year = NewYear,
        };

        if (!string.IsNullOrWhiteSpace(trimmedFranchise))
        {
            var existing = MediaCollection.FirstOrDefault(m =>
                string.Equals(m.Franchise, trimmedFranchise, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                movie.BaseColor = existing.BaseColor; // inherit color from existing franchise
        }
        movie.SetDarkMode(IsDarkMode);
        MediaCollection.Add(movie);

        // Sort movies: group by franchise, then by franchise number
        var sorted = MediaCollection
            .OrderBy(m => m.Franchise ?? m.Title)
            .ThenBy(m => m.FranchiseNumber ?? 0)
            .ThenBy(m => m.Year)
            .ToList();

        MediaCollection.Clear();
        foreach (var m in sorted)
            MediaCollection.Add(m);

        RefreshGroups();
        SaveMovies();
        RefreshSagaGroups();

        NewTitle = "";
        NewYear = DateTime.Now.Year;
        NewSaga = "";
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

        if (!movie.WatchDates.Contains(date))
            movie.WatchDates.Add(date);

        // Sort dates descending
        var sorted = movie.WatchDates.OrderByDescending(d => d).ToList();
        movie.WatchDates.Clear();
        foreach (var d in sorted)
            movie.WatchDates.Add(d);

        // Refresh computed properties

        movie.OnPropertyChanged(nameof(movie.LastWatchedDate));
        movie.OnPropertyChanged(nameof(movie.Seen));
        movie.OnPropertyChanged(nameof(movie.DisplayMeta));

        NewWatchDate = "";
        OnPropertyChanged(nameof(NewWatchDate));
        SaveMovies();
    }

    private void RemoveWatchDate(Tuple<Movie, DateTime> param)
    {
        var movie = param.Item1;
        var date = param.Item2;
        var result = MessageBox.Show(
            $"Are you sure you want to delete the watch date {date:dd/MM/yyyy} for '{movie.Title}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
            return; // user canceled

        movie?.WatchDates.Remove(date);

        movie?.OnPropertyChanged(nameof(movie.LastWatchedDate));
        movie?.OnPropertyChanged(nameof(movie.Seen));
        movie?.OnPropertyChanged(nameof(movie.DisplayMeta));
        SaveMovies();
    }
    private void SaveMovies()
    {
        _repository.SaveMovies(MediaCollection);
    }

    private void RemoveMovie(Movie movie)
    {
        if (movie == null) return;

        MediaCollection.Remove(movie); // UI updates automatically because it's ObservableCollection
        SaveMovies();                  // persist changes to JSON
        RefreshSagaGroups();
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
        bool exists = MediaCollection.Any(m =>
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
        movie.Franchise = Media.Normalize(franchise);
        movie.Note = movie.Note?.Trim();
        return true;
    }
    private void RefreshGroups()
    {
        var groups = MediaCollection
            .GroupBy(m => string.IsNullOrWhiteSpace(m.Saga) ? "Undefined" : m.Saga)
            .OrderBy(g => g.Key == "Undefined" ? "ZZZ" : g.Key) // Undefined at bottom
            .Select(g =>
            {
                var group = new Media.SagaGroup<Movie> { Name = g.Key };
                foreach (var item in g.OrderBy(m => m.Franchise ?? m.Title)
                                       .ThenBy(m => m.FranchiseNumber ?? 0))
                {
                    group.Items.Add(item);
                }
                return group;
            });

        SagaGroups.Clear();
        foreach (var group in groups)
            SagaGroups.Add(group);
    }
}