using MediaTracker.Domain;
using System.Collections.Generic;

namespace MediaTracker.Services
{
    public interface IMediaRepository
    {
        IEnumerable<Movie> LoadMovies();
        void SaveMovies(IEnumerable<Movie> movies);
    }
}
