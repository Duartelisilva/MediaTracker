using MediaTracker.Domain;
using System.Collections.Generic;

namespace MediaTracker.Services
{
    public interface IMediaRepository<T>
    {
        IEnumerable<T> Load();
        void Save(IEnumerable<T> items);
    }
}
