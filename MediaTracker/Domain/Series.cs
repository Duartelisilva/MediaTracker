using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MediaTracker.Domain;

public class Series : Media
{
    private int? _yearEnd;
    private int? _numberOfSeasons;

    public int? YearEnd
    {
        get => _yearEnd;
        set { _yearEnd = value; OnPropertyChanged(); }
    }

    public int? NumberOfSeasons
    {
        get => _numberOfSeasons;
        set { _numberOfSeasons = value; OnPropertyChanged(); }
    }

    [JsonIgnore] public int? BackupYearEnd { get; set; }
    [JsonIgnore] public int? BackupNumberOfSeasons { get; set; }

    public override string DisplayMeta
    {
        get
        {
            var parts = new List<string>();

            if (Year > 0)
            {
                string yearRange = YearEnd.HasValue ? $"{Year} – {YearEnd}" : $"{Year} – Ongoing";
                parts.Add(yearRange);
            }

            if (NumberOfSeasons.HasValue)
                parts.Add(NumberOfSeasons.Value.ToString());

            if (LastWatchedDate.HasValue)
                parts.Add(LastWatchedDate.Value.ToString("dd/MM/yyyy"));
            return string.Join(" • ", parts);
        }
    }
}