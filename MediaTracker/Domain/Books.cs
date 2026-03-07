using System.Text.Json.Serialization;

namespace MediaTracker.Domain;

public class Books : Media
{

    private int? _bookNumber;
    private string? _author;
    public int? BookNumber
    {
        get => _bookNumber;
        set { _bookNumber = value; OnPropertyChanged(); }
    }

    public string? Author
    {
        get => _author;
        set
        {
            _author = value;
            OnPropertyChanged();
        }
    }
    [JsonIgnore] 
    public int? BackupBookNumber { get; set; }

    [JsonIgnore] 
    public string? BackupAuthor { get; set; }

    public override string DisplayMeta
    {
        get
        {
            var parts = new List<string>();

            if (Year > 0)
                parts.Add(Year.ToString());

            if (BookNumber.HasValue)
                parts.Add(BookNumber.Value.ToString());

            if (!string.IsNullOrWhiteSpace(Author))
                parts.Add(Author);

            if (LastWatchedDate.HasValue)
                parts.Add(LastWatchedDate.Value.ToString("dd/MM/yyyy"));
            return string.Join(" • ", parts);
        }
    }
}