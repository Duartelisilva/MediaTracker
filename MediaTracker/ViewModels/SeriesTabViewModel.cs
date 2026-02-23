using MediaTracker.Domain;
using MediaTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using static MediaTracker.Domain.Series;
using System.Windows.Media;
using System.Text.Json.Serialization;

namespace MediaTracker.ViewModels;

public sealed class SeriesTabViewModel : MediaTabViewModel<Series>
{
    public override string Header => "Series";

    private int? _newYearEnd;
    public int? NewYearEnd
    {
        get => _newYearEnd;
        set { _newYearEnd = value; OnPropertyChanged(); }
    }

    private int? _newNumberOfSeasons;
    public int? NewNumberOfSeasons
    {
        get => _newNumberOfSeasons;
        set { _newNumberOfSeasons = value; OnPropertyChanged(); }
    }

    private readonly IMediaRepository<Series> _repository;

    public SeriesTabViewModel()
    {
        NewTitle = "";
        NewWatchDate = "";

        _repository = new JsonMediaRepository<Series>();

        var collectionView = CollectionViewSource.GetDefaultView(MediaCollection);
        collectionView.GroupDescriptions.Clear();
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Saga"));
        MediaCollection.CollectionChanged += (_, __) => RefreshSagaGroups();

        // Load saved series
        foreach (var series in _repository.Load())
        {
            series.IsExpanded = false;
            series.IsSidePanelOpen = false;
            MediaCollection.Add(series);
        }
        RefreshSagaGroups();

        // Attach collapse callback
        foreach (var series in MediaCollection)
        {
            series.ClearNewWatchDate = () => NewWatchDate = "";
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
            MessageBox.Show("Invalid start year.");
            return false;
        }

        if (NewYearEnd < 1900 || NewYearEnd > 2099 || NewYearEnd < NewYear)
        {
            MessageBox.Show("Invalid end year.");
            return false;
        }

        bool hasEndYear = NewYearEnd.HasValue;
        bool hasNumber = NewNumberOfSeasons.HasValue;

        bool exists = MediaCollection.Any(m =>
            string.Equals(m.Title, NewTitle, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Saga ?? "", NewSaga ?? "", StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            MessageBox.Show("A series with the same title and Saga already exists.", "Duplicate Series");
            return false;
        }
        return true;
    }

    protected override Series CreateMedia()
    {
        var series = new Series
        {
            Title = NewTitle!.Trim(),
            Saga = NewSaga,
            Year = NewYear,
            YearEnd = NewYearEnd,
            NumberOfSeasons = NewNumberOfSeasons
        };

        series.SetDarkMode(IsDarkMode);
        return series;
    }

    protected override void AfterAdd(Series series)
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
        NewYearEnd = null;
        NewNumberOfSeasons = null;
        NewWatchDate = "";
    }

    protected override void AfterRemove(Series series)
    {
        base.AfterRemove(series);
        _repository.Save(MediaCollection);
    }

    private bool ValidateSeries(Series series)
    {
        string title = series.Title?.Trim() ?? "";
        string saga = series.Saga?.Trim() ?? "";
        int? yearEnd = series.YearEnd;
        int year = series.Year;

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Title is required.", "Invalid input");
            return false;
        }

        if (year < 1900 || year > 2099)
        {
            MessageBox.Show("Start year must be between 1900 and 2099.", "Invalid input");
            return false;
        }

        if (yearEnd.HasValue)
        {
            if (yearEnd < 1900 || yearEnd > 2099)
            {
                MessageBox.Show("End year must be between 1900 and 2099.", "Invalid input");
                return false;
            }
            else if (yearEnd < year)
            {
                MessageBox.Show("End year can't be lower than the start year", "Invalid input");
                return false;
            }
        }

        // Check for duplicates excluding itself
        bool exists = MediaCollection.Any(m =>
            m != series &&
            string.Equals(m.Title, title, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Saga ?? "", saga ?? "", StringComparison.OrdinalIgnoreCase)
        );

        if (exists)
        {
            MessageBox.Show("A series with the same title and Saga already exists.", "Duplicate Series");
            return false;
        }

        // If everything passes, update trimmed values
        series.Title = title;
        series.Saga = Media.Normalize(saga);
        series.Note = series.Note?.Trim();
        return true;
    }

    protected override bool IsEditing(Series item) => item.IsEditing;

    protected override void SetEditing(Series item, bool editing)
    {
        item.IsEditing = editing;
    }
    protected override void BackupItem(Series series)
    {
        series.BackupTitle = series.Title;
        series.BackupYear = series.Year;
        series.BackupSaga = series.Saga;
        series.BackupYearEnd = series.YearEnd;
        series.BackupNumberOfSeasons = series.NumberOfSeasons;
        series.BackupNote = series.Note;
        series.BackupBaseColor = series.BaseColor;
    }

    protected override void UndoEdit(Series series)
    {
        series.Title = series.BackupTitle;
        series.Year = series.BackupYear;
        series.Saga = series.BackupSaga;
        series.YearEnd = series.BackupYearEnd;
        series.NumberOfSeasons = series.BackupNumberOfSeasons;
        series.Note = series.BackupNote;
        series.BaseColor = series.BackupBaseColor;

        series.IsEditing = false;
    }

    protected override string GetTitle(Series series) => series.Title ?? "Unnamed";

    protected override void OnMediaChanged(Series series)
    {
        _repository.Save(MediaCollection);
    }

    protected override bool ValidateItem(Series series) => ValidateSeries(series);

    protected override void AfterSave(Series series)
    {
        SortMediaCollection();
        RefreshSagaGroups();
    }

    private void SortMediaCollection()
    {
        var sorted = MediaCollection
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Title)
            .ToList();

        MediaCollection.Clear();
        foreach (var m in sorted)
            MediaCollection.Add(m);
    }
}