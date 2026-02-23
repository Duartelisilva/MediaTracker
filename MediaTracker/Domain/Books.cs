using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MediaTracker.Domain;

public class Books : Media
{

    private int? _bookNumber;
    private string? _author;
    public int? BookNumber
    {
        get => _bookNumber;
        set { _bookNumber = value; OnPropertyChanged(); }
    }

    public string? Author
    {
        get => _author;
        set
        {
            _author = value; // always keep what user types
            OnPropertyChanged();
        }
    }
    [JsonIgnore] public int? BackupBookNumber { get; set; }
    [JsonIgnore] public string? BackupAuthor { get; set; }

    public override string DisplayMeta
    {
        get
        {
            var parts = new List<string>();

            if (Year > 0)
                parts.Add(Year.ToString());

            if (BookNumber.HasValue)
                parts.Add(BookNumber.Value.ToString());

            if (!string.IsNullOrWhiteSpace(Author))
                parts.Add(Author);

            if (LastWatchedDate.HasValue)
                parts.Add(LastWatchedDate.Value.ToString("dd/MM/yyyy"));
            return string.Join(" • ", parts);
        }
    }
}