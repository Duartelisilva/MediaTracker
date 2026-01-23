using MediaTracker.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace MediaTracker.Services
{
    public class JsonMediaRepository : IMediaRepository
    {
        private readonly string _filePath = "movies.json";

        public IEnumerable<Movie> LoadMovies()
        {
            if (!File.Exists(_filePath))
                return new List<Movie>();

            string json = File.ReadAllText(_filePath); // <--- define json here
            var movies = JsonSerializer.Deserialize<List<Movie>>(json) ?? new List<Movie>();

            // Convert WatchDates to ObservableCollection
            foreach (var movie in movies)
            {
                movie.WatchDates = movie.WatchDates != null
                    ? new ObservableCollection<DateTime>(movie.WatchDates)
                    : new ObservableCollection<DateTime>();
            }

            return movies;
        }

        public void SaveMovies(IEnumerable<Movie> movies)
        {
            var json = JsonSerializer.Serialize(movies, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_filePath, json);
        }
    }
}