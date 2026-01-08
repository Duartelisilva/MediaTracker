using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaTracker.Domain;

public class Series
{
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<Season> Seasons { get; } = new();
    public List<DateTime> WatchDates { get; } = new(); // whole series

    public bool Seen => WatchDates.Count > 0 || Seasons.Any(s => s.Seen);

    public void AddWatchDate(DateTime date) => WatchDates.Add(date);
    public void RemoveWatchDate(DateTime date) => WatchDates.Remove(date);

    public Season GetOrCreateSeason(int number)
    {
        var season = Seasons.Find(s => s.Number == number);
        if (season == null)
        {
            season = new Season { Number = number };
            Seasons.Add(season);
        }
        return season;
    }
}
