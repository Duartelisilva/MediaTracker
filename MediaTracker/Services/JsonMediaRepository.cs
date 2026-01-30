using MediaTracker.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace MediaTracker.Services
{
    public class JsonMediaRepository<T> : IMediaRepository<T>
    {
        private readonly string _filePath = typeof(T).Name.ToLower() + "s.json";

        public IEnumerable<T> Load()
        {
            if (!File.Exists(_filePath)) return new List<T>();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        public void Save(IEnumerable<T> items)
        {
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}