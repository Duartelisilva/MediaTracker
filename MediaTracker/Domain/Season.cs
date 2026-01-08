using System;
using System.Collections.Generic;

namespace MediaTracker.Domain;

public class Season
{
    public int Number { get; set; }
    public List<DateTime> WatchDates { get; } = new();
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
}
