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
    public override string Header => "Movies";

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

    private readonly IMediaRepository<Movie> _repository;

    public MoviesTabViewModel()
    {
        NewTitle = "";
        NewFranchise = "";
        NewWatchDate = "";

        _repository = new JsonMediaRepository<Movie>();

        var collectionView = CollectionViewSource.GetDefaultView(MediaCollection);
        collectionView.GroupDescriptions.Clear();
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Saga"));
        MediaCollection.CollectionChanged += (_, __) => RefreshSagaGroups();

        // Load saved movies
        foreach (var movie in _repository.Load())
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
    }

    protected override bool CanAddMedia()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            MessageBox.Show("Title is required.");
            return false;
        }

        if (NewYear < 1900 || NewYear > 2099)
        {
            MessageBox.Show("Invalid year.");
            return false;
        }

        bool hasFranchise = !string.IsNullOrWhiteSpace(NewFranchise);
        bool hasNumber = NewFranchiseNumber.HasValue;

        if (hasFranchise ^ hasNumber)
        {
            MessageBox.Show("Franchise and number must be filled together.");
            return false;
        }

        bool exists = MediaCollection.Any(m =>
            string.Equals(m.Title, NewTitle, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Saga ?? "", NewSaga ?? "", StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            MessageBox.Show("A movie with the same title and Saga already exists.", "Duplicate Movie");
            return false;
        }
        return true;
    }

    protected override Movie CreateMedia()
    {
        var movie = new Movie
        {
            Title = NewTitle!.Trim(),
            Saga = NewSaga,
            Year = NewYear,
            Franchise = string.IsNullOrWhiteSpace(NewFranchise) ? null : NewFranchise,
            FranchiseNumber = NewFranchiseNumber
        };

        var existing = MediaCollection.FirstOrDefault(m =>
            string.Equals(m.Franchise, movie.Franchise, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
            movie.BaseColor = existing.BaseColor;

        movie.SetDarkMode(IsDarkMode);
        return movie;
    }

    protected override void AfterAdd(Movie movie)
    {
        SortMediaCollection();

        _repository.Save(MediaCollection);
        RefreshSagaGroups();

        ResetInputs();
    }

    private void ResetInputs()
    {
        NewTitle = "";
        NewYear = DateTime.Now.Year;
        NewSaga = "";
        NewFranchise = "";
        NewFranchiseNumber = null;
        NewWatchDate = "";
    }

    protected override void AfterRemove(Movie movie)
    {
        base.AfterRemove(movie);
        _repository.Save(MediaCollection); // movie-specific persistence
    }


    private bool ValidateMovie(Movie movie)
    {
        string title = movie.Title?.Trim() ?? "";
        string saga = movie.Saga?.Trim() ?? "";
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
            string.Equals(m.Saga ?? "", saga ?? "", StringComparison.OrdinalIgnoreCase)
        );

        if (exists)
        {
            MessageBox.Show("A movie with the same title and Saga already exists.", "Duplicate Movie");
            return false;
        }

        // If everything passes, update trimmed values
        movie.Title = title;
        movie.Saga = Media.Normalize(saga);
        movie.Franchise = Media.Normalize(franchise);
        movie.Note = movie.Note?.Trim();
        return true;
    }

    protected override bool IsEditing(Movie item) => item.IsEditing;

    protected override void SetEditing(Movie item, bool editing)
    {
        item.IsEditing = editing;
    }
    protected override void BackupItem(Movie movie)
    {
        movie.BackupTitle = movie.Title;
        movie.BackupYear = movie.Year;
        movie.BackupSaga = movie.Saga;
        movie.BackupFranchise = movie.Franchise;
        movie.BackupFranchiseNumber = movie.FranchiseNumber;
        movie.BackupNote = movie.Note;
        movie.BackupBaseColor = movie.BaseColor;
    }

    protected override void UndoEdit(Movie movie)
    {
        movie.Title = movie.BackupTitle;
        movie.Year = movie.BackupYear;
        movie.Saga = movie.BackupSaga;
        movie.Franchise = movie.BackupFranchise;
        movie.FranchiseNumber = movie.BackupFranchiseNumber;
        movie.Note = movie.BackupNote;
        movie.BaseColor = movie.BackupBaseColor;

        movie.IsEditing = false;
    }

    protected override string GetTitle(Movie movie) => movie.Title ?? "Unnamed";

    protected override void OnMediaChanged(Movie media)
    {
        _repository.Save(MediaCollection);
    }

    protected override bool ValidateItem(Movie movie) => ValidateMovie(movie);

    protected override void AfterSave(Movie movie)
    {
        SortMediaCollection();

        if (!string.IsNullOrWhiteSpace(movie.Franchise))
        {
            var sameFranchise = MediaCollection
                .Where(m => string.Equals(m.Franchise, movie.Franchise, StringComparison.OrdinalIgnoreCase));
            foreach (var m in sameFranchise)
                m.BaseColor = movie.BaseColor;
        }

        RefreshSagaGroups();
    }

    private void SortMediaCollection()
    {
        var sorted = MediaCollection
            .OrderBy(m => m.Franchise ?? m.Title)
            .ThenBy(m => m.FranchiseNumber ?? 0)
            .ThenBy(m => m.Year)
            .ToList();

        MediaCollection.Clear();
        foreach (var m in sorted)
            MediaCollection.Add(m);
    }
}