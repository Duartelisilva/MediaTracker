using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MediaTracker.Domain;

public class Movie : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private int _year;
    private string? _franchise;
    private int? _franchiseNumber;
    private Color _franchiseColor = Colors.Transparent;

    private string? _bigFranchise;
    public string? BigFranchise
    {
        get => _bigFranchise;
        set { _bigFranchise = value; OnPropertyChanged(); }
    }

    private string? _note;
    private bool _isEditing = false;

    private bool _isExpanded = false;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    public string Title
    {
        get => _title;
        set { _title = Normalize(value); OnPropertyChanged(); }
    }

    public int Year
    {
        get => _year;
        set { _year = value; OnPropertyChanged(); }
    }

    public string? Franchise
    {
        get => _franchise;
        set
        {
            _franchise = value; // always keep what user types
            OnPropertyChanged();
        }
    }

    public int? FranchiseNumber
    {
        get => _franchiseNumber;
        set { _franchiseNumber = value; OnPropertyChanged(); }
    }

    public Color FranchiseColor
    {
        get => _franchiseColor;
        set
        {
            _franchiseColor = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FranchiseBrush));
        }
    }

    [JsonIgnore] public string BackupTitle { get; set; } = "";
    [JsonIgnore] public int BackupYear { get; set; }
    [JsonIgnore] public string? BackupBigFranchise { get; set; }
    [JsonIgnore] public string? BackupFranchise { get; set; }
    [JsonIgnore] public int? BackupFranchiseNumber { get; set; }
    [JsonIgnore] public string? BackupNote { get; set; }

    [JsonIgnore] public System.Windows.Media.Color BackupFranchiseColor { get; set; }
    [JsonIgnore]
    public SolidColorBrush FranchiseBrush =>
    FranchiseColor == Colors.Transparent
        ? Brushes.LightGray
        : new SolidColorBrush(FranchiseColor);

    [JsonIgnore]
    public SolidColorBrush FranchiseTextBrush
    {
        get
        {
            // Use LightGray as background fallback
            Color bg = FranchiseColor == Colors.Transparent ? Colors.LightGray : FranchiseColor;

            // Compute luminance (0=dark, 1=light)
            double luminance = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255;

            // Return contrasting color
            return luminance > 0.5 ? Brushes.Black : Brushes.White;
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set { _isEditing = value; OnPropertyChanged(); }
    }

    public string? Note
    {
        get => _note;
        set
        {
            _note = Normalize(value); OnPropertyChanged();
        }
    }
    public Movie()
    {
        WatchDates.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(LastWatchedDate));
            OnPropertyChanged(nameof(Seen));
            OnPropertyChanged(nameof(WatchDates));
        };

    }

    public ObservableCollection<DateTime> WatchDates { get; set; } = new();
    public DateTime? LastWatchedDate =>
    WatchDates.Count == 0 ? null : WatchDates.Max();

    public bool Seen => WatchDates.Count > 0;

    public void AddWatchDate(DateTime date)
    {
        if (!WatchDates.Contains(date))
            WatchDates.Add(date);
    }

    public void RemoveWatchDate(DateTime date)
    {
        WatchDates.Remove(date);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return System.Text.RegularExpressions.Regex
            .Replace(text.Trim(), @"\s+", " ");
    }

    public string DisplayMeta
    {
        get
        {
            var parts = new List<string>();

            parts.Add(Year.ToString());

            if (!string.IsNullOrWhiteSpace(Franchise))
            {
                var f = Franchise;
                if (FranchiseNumber.HasValue)
                    f += " " + FranchiseNumber.Value;
                parts.Add(f);
            }

            if (LastWatchedDate.HasValue)
                parts.Add(LastWatchedDate.Value.ToString("dd/MM/yyyy"));

            return string.Join(" • ", parts);
        }
    }
    public class BigFranchiseGroup : INotifyPropertyChanged
    {
        public string Name { get; set; } = "Undefined";
        public ObservableCollection<Movie> Movies { get; } = new();

        private bool _isCollapsed;
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set { _isCollapsed = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}