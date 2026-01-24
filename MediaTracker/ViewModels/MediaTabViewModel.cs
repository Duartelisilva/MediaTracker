using MediaTracker.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static MediaTracker.Domain.Movie;

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
            foreach (var saga in SagaGroups)
                foreach (var item in saga.Items)
                    item.SetDarkMode(isDark);
        }
    }
}
