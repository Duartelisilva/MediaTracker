using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MediaTracker.Domain
{
    public abstract class Media : INotifyPropertyChanged
    {

        // Fields
        private string _title = string.Empty;
        private int _year;
        private string? _saga;
        private string? _note;
        private bool _datesPanelIsExpanded;
        private bool _isEditing = false;
        private bool _isExpanded = false;
        private bool _isSidePanelOpen;
        private Color _baseColor = Colors.Transparent;
        [JsonIgnore] private Color _baseDarkColor = Colors.Transparent;
        [JsonIgnore] private bool _currentDarkMode;

        // Backup fields
        [JsonIgnore] public string BackupTitle { get; set; } = "";
        [JsonIgnore] public int BackupYear { get; set; }
        [JsonIgnore] public string? BackupSaga { get; set; }
        [JsonIgnore] public string? BackupNote { get; set; }
        [JsonIgnore] public System.Windows.Media.Color BackupBaseColor { get; set; }


        // Constructor
        public Media()
        {
            WatchDates.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(LastWatchedDate));
                OnPropertyChanged(nameof(Seen));
                OnPropertyChanged(nameof(WatchDates));
            };
        }

        // Properties
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

        public string? Saga
        {
            get => _saga;
            set { _saga = value; OnPropertyChanged(); }
        }

        public string? Note
        {
            get => _note;
            set
            {
                _note = Normalize(value); OnPropertyChanged();
            }
        }

        public bool DatesPanelIsExpanded
        {
            get => _datesPanelIsExpanded;
            set
            {
                _datesPanelIsExpanded = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();

                    if (!_isExpanded)
                    {
                        ClearNewWatchDate?.Invoke();
                    }
                }
            }
        }

        public Color BaseColor
        {
            get => _baseColor;
            set
            {
                _baseColor = value;
                _baseDarkColor = DarkenColor(value, 0.4); // darken 40% for dark mode
                OnPropertyChanged();
                OnPropertyChanged(nameof(Brush));
                OnPropertyChanged(nameof(BrushForDarkMode));
            }
        }

        [JsonIgnore]
        public SolidColorBrush BrushForDarkMode =>
            new SolidColorBrush(_currentDarkMode ? _baseDarkColor : _baseColor);

        [JsonIgnore]
        public SolidColorBrush Brush =>
            BaseColor == Colors.Transparent ? Brushes.LightGray : new SolidColorBrush(BaseColor);

        [JsonIgnore] public SolidColorBrush TextBrush
                     {
                         get
                         {
                             // Use LightGray as background fallback
                             Color bg = BaseColor == Colors.Transparent ? Colors.LightGray : BaseColor;

                             // Compute luminance (0=dark, 1=light)
                             double luminance = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255;

                             // Return contrasting color
                             return luminance > 0.5 ? Brushes.Black : Brushes.White;
                         }
                     }
        public bool IsSidePanelOpen
        {
            get => _isSidePanelOpen;
            set { _isSidePanelOpen = value; OnPropertyChanged(); }
        }

        [JsonIgnore] public Action? ClearNewWatchDate;

        public ObservableCollection<DateTime> WatchDates { get; set; } = new();
        public DateTime? LastWatchedDate => WatchDates.Count == 0 ? null : WatchDates.Max();
        public bool Seen => WatchDates.Count > 0;

        // Methods - Watch Dates
        public void AddWatchDate(DateTime date)
        {
            if (!WatchDates.Contains(date))
                WatchDates.Add(date);
        }

        public void RemoveWatchDate(DateTime date)
        {
            WatchDates.Remove(date);
        }

        // Colors
        private static Color DarkenColor(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R * (1 - factor)),
                (byte)(color.G * (1 - factor)),
                (byte)(color.B * (1 - factor))
            );
        }
        public void RefreshBrush() => OnPropertyChanged(nameof(BrushForDarkMode));
        public void SetDarkMode(bool isDark)
        {
            _currentDarkMode = isDark;
            OnPropertyChanged(nameof(BrushForDarkMode));
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Helpers
        public static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return System.Text.RegularExpressions.Regex
                .Replace(text.Trim(), @"\s+", " ");
        }

        public class SagaGroup<T> : INotifyPropertyChanged where T : Media
        {
            public string Name { get; set; } = "Undefined";
            public ObservableCollection<T> Items { get; } = new();

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
}
