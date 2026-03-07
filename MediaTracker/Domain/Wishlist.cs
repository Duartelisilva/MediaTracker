using System.Text.Json.Serialization;

namespace MediaTracker.Domain;

public enum WishlistType
{
    Movie,
    Series,
    Books
}

public class Wishlist : Media
{

    private WishlistType _type;
    private string? _franchise;
    private int? _seasonNumber;
    private string? _author;

    public WishlistType Type
    {
        get => _type;
        set
        {
            _type = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(DisplayMeta));
        }
    }


    // Movie
    public string? Franchise
    {
        get => _franchise;
        set { _franchise = value; OnPropertyChanged(); }
    }

    // Series
    public int? SeasonNumber
    {
        get => _seasonNumber;
        set { _seasonNumber = value; OnPropertyChanged(); }
    }

    // Book
    public string? Author
    {
        get => _author;
        set { _author = value; OnPropertyChanged(); }
    }


    [JsonIgnore]
    public string Category => Type.ToString();

    [JsonIgnore]
    public string? BackupFranchise { get; set; }

    [JsonIgnore]
    public int? BackupSeasonNumber { get; set; }

    [JsonIgnore]
    public string? BackupAuthor { get; set; }

    public override string DisplayMeta
    {
        get
        {
            var parts = new List<string>();

            if (Year > 0)
                parts.Add(Year.ToString());

            switch (Type)
            {
                case WishlistType.Movie:
                    if (!string.IsNullOrWhiteSpace(Franchise))
                        parts.Add(Franchise);
                    break;

                case WishlistType.Series:
                    if (SeasonNumber.HasValue)
                        parts.Add($"Season {SeasonNumber}");
                    break;

                case WishlistType.Books:
                    if (!string.IsNullOrWhiteSpace(Author))
                        parts.Add(Author);
                    break;
            }

            return string.Join(" • ", parts);
        }
    }
}