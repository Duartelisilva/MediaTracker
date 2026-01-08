using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MediaTracker.Domain;

namespace MediaTracker.ViewModels;

public sealed class SeriesTabViewModel : TabViewModel
{
    public override string Header => "Series";

    public ObservableCollection<Series> SeriesCollection { get; } = new();

    // Input fields
    public string NewTitle { get; set; } = string.Empty;
    public int NewYear { get; set; } = DateTime.Now.Year;
    public int NewSeasonNumber { get; set; } = 1;
    public string NewWatchDate { get; set; } = string.Empty;

    public ICommand AddSeriesCommand { get; }
    public ICommand AddSeasonCommand { get; }
    public ICommand AddWatchDateCommand { get; }
    public ICommand RemoveWatchDateCommand { get; }

    public SeriesTabViewModel()
    {
        AddSeriesCommand = new RelayCommand(_ =>
        {
            if (string.IsNullOrWhiteSpace(NewTitle) || NewYear < 1900 || NewYear > 2099)
                return;

            SeriesCollection.Add(new Series { Title = NewTitle, Year = NewYear });
            NewTitle = string.Empty;
            NewYear = DateTime.Now.Year;
        });

        AddSeasonCommand = new RelayCommand(param =>
        {
            if (param is Series series && NewSeasonNumber > 0)
            {
                series.Seasons.Add(new Season { Number = NewSeasonNumber });
            }
        });

        AddWatchDateCommand = new RelayCommand(param =>
        {
            if (param is Tuple<Series, DateTime> seriesTuple)
                seriesTuple.Item1.AddWatchDate(seriesTuple.Item2);
        });

        RemoveWatchDateCommand = new RelayCommand(param =>
        {
            if (param is Tuple<Series, DateTime> seriesTuple)
                seriesTuple.Item1.RemoveWatchDate(seriesTuple.Item2);
        });
    }
}
