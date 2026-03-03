using MediaTracker.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;


namespace MediaTracker.ViewModels
{
    public abstract class MediaTabViewModel<T> : TabViewModel, INotifyPropertyChanged where T : Media
    {
        // Fields
        public ObservableCollection<T> MediaCollection { get; } = [];
        public ObservableCollection<Media.SagaGroup<T>> SagaGroups { get; } = [];
        private string? _newTitle;
        private string? _newSaga;
        private string? _newWatchDate;
        private bool _showComments;
        public bool IsDarkMode { get; private set; }
        private Color _newBaseColor = Colors.LightGray;
        public int NewYear { get; set; } = DateTime.Now.Year;
        private string? _searchText;

        protected SearchOption CurrentSearchOption { get; private set; } = SearchOption.Title;

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
            set { _showComments = value; }
        }

        public Color NewBaseColor
        {
            get => _newBaseColor;
            set { _newBaseColor = value; OnPropertyChanged(); }
        }

        public override void SetSearch(string? text)
        {
            _searchText = text;
            RefreshSagaGroups();
        }

        public override void SetSearchOption(SearchOption option)
        {
            CurrentSearchOption = option;
            RefreshSagaGroups();
        }

        // Commands
        public ICommand AddMediaCommand { get; }
        public ICommand RemoveMediaCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand UndoItemCommand { get; }
        public ICommand ToggleExpandCommand { get; }
        public ICommand ToggleSidePanelCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand AddWatchDateCommand { get; }
        public ICommand RemoveWatchDateCommand { get; }

        // Overridable hooks for commands
        protected virtual bool CanAddMedia() => true;
        protected abstract T CreateMedia();
        protected virtual void BeforeAdd(T media) { }
        protected virtual void AfterAdd(T media)
        {
            RefreshSagaGroups();
            OnMediaChanged(media);
        }


        // Constructor
        public MediaTabViewModel()
        {
            AddMediaCommand = new RelayCommand(_ => AddMedia());

            ToggleFavoriteCommand = new RelayCommand(obj =>
            {
                if (obj is T media)
                {
                    media.IsFavorite = !media.IsFavorite;
                    OnMediaChanged(media); // call hook for persistence
                }
            });

            RemoveMediaCommand = new RelayCommand(obj =>
            {
                if (obj is T item)
                    RemoveItem(item);
            });

            EditItemCommand = new RelayCommand(obj =>
            {
                if (obj is T item)
                    BeginEdit(item);
            });

            SaveItemCommand = new RelayCommand(obj =>
            {
                if (obj is T item)
                {
                    if (!ValidateItem(item))
                        return;

                    SetEditing(item, false);

                    AfterSave(item);

                    OnMediaChanged(item);
                }
            });

            UndoItemCommand = new RelayCommand(obj =>
            {
                if (obj is T item)
                {
                    UndoEdit(item);
                    SetEditing(item, false);
                }
            });

            ToggleExpandCommand = new RelayCommand(obj =>
            {
                if (obj is T clickedItem)
                {
                    // Collapse all others
                    foreach (var item in MediaCollection)
                        if (!ReferenceEquals(item, clickedItem))
                            item.IsExpanded = false;

                    clickedItem.IsExpanded = !clickedItem.IsExpanded;
                }
            });

            ToggleSidePanelCommand = new RelayCommand(obj =>
            {
                if (obj is T item)
                {
                    // Close other panels
                    foreach (var other in MediaCollection)
                        if (!ReferenceEquals(other, item))
                            other.IsSidePanelOpen = false;

                    item.IsSidePanelOpen = !item.IsSidePanelOpen;
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
                        RemoveWatchDate(media, date);
                }
            });
        }


        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaiseShowComments()
        {
            OnPropertyChanged(nameof(ShowComments));
        }


        // Methods
        protected void RefreshSagaGroups()
        {
            SagaGroups.Clear();

            // Search 
            IEnumerable<T> source = MediaCollection;
            if (CurrentSearchOption == SearchOption.Favorite)
            {
                source = source.Where(m => m.IsFavorite);
            }
            else if (!string.IsNullOrWhiteSpace(_searchText))
            {
                string search = Media.Normalize(_searchText.ToLower());

                source = CurrentSearchOption switch
                {
                    SearchOption.Title => source.Where(m => (m.Title ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)),
                    SearchOption.Year => source.Where(m => m.Year.ToString().Contains(search)),
                    SearchOption.Saga => source.Where(m => (m.Saga ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)),
                    SearchOption.Franchise => source.Where(m => (m is Movie movie && (movie.Franchise ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)) || false),
                    SearchOption.Author => source.Where(m => (m is Books book && (book.Author ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)) || false),
                    _ => source
                };
            }

            // Group movies by Saga
            var groups = source
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

        public override void UpdateMediaDarkMode(bool isDark)
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

            var formats = new[]
            {
                "d/M/yyyy",
                "dd/M/yyyy",
                "d/MM/yyyy",
                "dd/MM/yyyy",
                "M/yyyy",
                "MM/yyyy"
               };

            var trimmed = input.Trim();

            if (!DateTime.TryParseExact(
                    input.Trim(),
                    formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date))
                throw new InvalidOperationException("Invalid date format");

            if (trimmed.Count(c => c == '/') == 1)
                date = new DateTime(date.Year, date.Month, 1);

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

            media.OnPropertyChanged(nameof(media.DisplayMeta));

            OnMediaChanged(media);
        }

        public void RemoveWatchDate(T media, DateTime date)
        {
            if (media == null)
                return;

            media.WatchDates.Remove(date);

            media.OnPropertyChanged(nameof(media.LastWatchedDate));
            media.OnPropertyChanged(nameof(media.Seen));

           // if (media is Movie || media is Series || media is Books)
                media.OnPropertyChanged(nameof(media.DisplayMeta));
            OnMediaChanged(media);
        }

        protected virtual void OnMediaChanged(T media) { }

        private void AddMedia()
        {
            if (!CanAddMedia())
                return;

            var media = CreateMedia();
            if (media == null)
                return;

            BeforeAdd(media);

            MediaCollection.Add(media);

            AfterAdd(media);
        }

        protected virtual void RemoveItem(T item)
        {
            if (item == null) return;

            var itemType = typeof(T).Name; // e.g., "Movie"
            var title = item.Title ?? "Unnamed";

            var result = MessageBox.Show(
                $"Are you sure you want to delete the {itemType} '{title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            MediaCollection.Remove(item);
            AfterRemove(item);
        }

        protected virtual void AfterRemove(T item)
        {
            RefreshSagaGroups();
            OnMediaChanged(item);
        }

        protected virtual void BeginEdit(T item)
        {
            if (item == null) return;

            // Undo other edits
            var otherEditing = MediaCollection.FirstOrDefault(m => m != item && IsEditing(m));
            if (otherEditing != null)
            {
                var result = MessageBox.Show(
                    $"{typeof(T).Name} '{GetTitle(otherEditing)}' is already being edited.\n\n" +
                    "Click Yes to discard changes and edit this item, or No to cancel.",
                    "Editing in progress",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;

                UndoEdit(otherEditing);
            }

            BackupItem(item);
            SetEditing(item, true);
        }

        // Hooks to override in derived classes
        protected virtual bool IsEditing(T item) => false;
        protected virtual void SetEditing(T item, bool editing) { }
        protected virtual void BackupItem(T item) { }
        protected virtual void UndoEdit(T item) { }
        protected virtual string GetTitle(T item) => "";

        // Hooks to override Save
        protected virtual bool ValidateItem(T item) => true;
        protected virtual void AfterSave(T item) { }
    }
}
