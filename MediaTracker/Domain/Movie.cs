using System.Text.Json.Serialization;

namespace MediaTracker.Domain;

public class Movie : Media
{

    private string? _franchise;
    private int? _franchiseNumber;

    public string? Franchise
    {
        get => _franchise;
        set
        {
            _franchise = value;
            OnPropertyChanged();
        }
    }

    public int? FranchiseNumber
    {
        get => _franchiseNumber;
        set { _franchiseNumber = value; OnPropertyChanged(); }
    }

    [JsonIgnore] 
    public string? BackupFranchise { get; set; }

    [JsonIgnore] 
    public int? BackupFranchiseNumber { get; set; }

    public override string DisplayMeta
    {
        get
        {
            var parts = new List<string>();

            if (Year > 0)
                parts.Add(Year.ToString());

            if (!string.IsNullOrWhiteSpace(Franchise))
                parts.Add(Franchise);

            if (FranchiseNumber.HasValue)
                parts.Add(FranchiseNumber.Value.ToString());


            if (LastWatchedDate.HasValue)
                parts.Add(LastWatchedDate.Value.ToString("dd/MM/yyyy"));
            return string.Join(" • ", parts);
        }
    }
}