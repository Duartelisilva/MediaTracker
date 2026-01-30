using MediaTracker.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;


namespace MediaTracker.ViewModels
{
    public abstract class MediaTabViewModel<T> : TabViewModel, INotifyPropertyChanged where T : Media
    {
        // Fields
        public ObservableCollection<T> MediaCollection { get; } = new();
        public ObservableCollection<Media.SagaGroup<T>> SagaGroups { get; } = new();
        private string? _newTitle;
        private string? _newSaga;
        private string? _newWatchDate;
        private bool _showComments;
        public bool IsDarkMode { get; private set; }
        private Color _newBaseColor = Colors.LightGray;
        public int NewYear { get; set; } = DateTime.Now.Year;

        // Parameters
        public string? NewTitle
        {
            get => _newTitle;
            set { _newTitle = Media.Normalize(value); OnPropertyChanged(); }
        }

        public string? NewSaga
        {
            get => _newSaga;
            set { _newSaga = Media.Normalize(value); OnPropertyChanged(); }
        }

        public string? NewWatchDate
        {
            get => _newWatchDate;
            set { _newWatchDate = value?.Trim(); OnPropertyChanged(); }
        }

        public bool ShowComments
        {
            get => _showComments;
            set { _showComments = value; OnPropertyChanged(); }
        }

        public Color NewBaseColor
        {
            get => _newBaseColor;
            set { _newBaseColor = value; OnPropertyChanged(); }
        }


        // Commands
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand AddWatchDateCommand { get; }
        public ICommand RemoveWatchDateCommand { get; }

        // Constructor
        public MediaTabViewModel()
        {

            ToggleFavoriteCommand = new RelayCommand(obj =>
            {
                if (obj is T media)
                {
                    media.IsFavorite = !media.IsFavorite;
                    OnMediaChanged(media); // call hook for persistence
                }
            });

            AddWatchDateCommand = new RelayCommand(obj =>
            {
                if (obj is T media && !string.IsNullOrWhiteSpace(NewWatchDate))
                {
                    try
                    {
                        AddWatchDate(media, NewWatchDate);
                        NewWatchDate = "";
                        OnPropertyChanged(nameof(NewWatchDate));
                    }
                    catch (InvalidOperationException ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message, "Invalid Input");
                    }
                }
            });

            RemoveWatchDateCommand = new RelayCommand(obj =>
            {
                if (obj is Tuple<T, DateTime> tuple)
                {
                    var media = tuple.Item1;
                    var date = tuple.Item2;
                    var result = System.Windows.MessageBox.Show(
                        $"Are you sure you want to delete the watch date {date:dd/MM/yyyy} for '{media.Title}'?",
                        "Confirm Delete",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Warning);
                    if (result == System.Windows.MessageBoxResult.Yes)
                        media.RemoveWatchDate(date);
                }
            });
        }


        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // Methods
        protected void RefreshSagaGroups()
        {
            SagaGroups.Clear();

            // Group movies by Saga
            var groups = MediaCollection
                .GroupBy(m => string.IsNullOrWhiteSpace(m.Saga) ? "Undefined" : m.Saga)
                .OrderBy(g => g.Key == "Undefined" ? "ZZZ" : g.Key); // Undefined goes last

            foreach (var g in groups)
            {
                var group = new Media.SagaGroup<T> { Name = g.Key };
                foreach (var m in g)
                    group.Items.Add(m);
                SagaGroups.Add(group);
            }
        }

        public void UpdateMoviesDarkMode(bool isDark)
        {
            IsDarkMode = isDark;
            foreach (var saga in SagaGroups)
                foreach (var item in saga.Items)
                    item.SetDarkMode(isDark);
        }

        public void AddWatchDate(T media, string input)
        {
            if (media == null || string.IsNullOrWhiteSpace(input))
                return;

            if (!DateTime.TryParseExact(
                    input.Trim(),
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date))
                throw new InvalidOperationException("Invalid date format");

            if (date.Year < 1900 || date.Year > 2099)
                throw new InvalidOperationException("Year must be between 1900 and 2099");

            if (!media.WatchDates.Contains(date))
                media.WatchDates.Add(date);

            // Sort descending
            var sorted = media.WatchDates.OrderByDescending(d => d).ToList();
            media.WatchDates.Clear();
            foreach (var d in sorted)
                media.WatchDates.Add(d);

            media.OnPropertyChanged(nameof(media.LastWatchedDate));
            media.OnPropertyChanged(nameof(media.Seen));

            // Only if T is Movie, update DisplayMeta
            if (media is Movie movie)
                movie.OnPropertyChanged(nameof(movie.DisplayMeta));

            OnMediaChanged(media);
        }

        public void RemoveWatchDate(T media, DateTime date)
        {
            if (media == null)
                return;

            if (media.WatchDates.Contains(date))
                media.WatchDates.Remove(date);

            media.OnPropertyChanged(nameof(media.LastWatchedDate));
            media.OnPropertyChanged(nameof(media.Seen));

            if (media is Movie movie)
                movie.OnPropertyChanged(nameof(movie.DisplayMeta));
            OnMediaChanged(media);
        }

        protected virtual void OnMediaChanged(T media) { }
    }
}
