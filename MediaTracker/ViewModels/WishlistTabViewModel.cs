using MediaTracker.Domain;
using MediaTracker.Services;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MediaTracker.ViewModels;

public sealed class WishlistTabViewModel : MediaTabViewModel<Wishlist>
{
    public override string Header => "Wishlist";

    public IEnumerable<WishlistType> WishlistTypes { get; } =
    Enum.GetValues(typeof(WishlistType)).Cast<WishlistType>();

    private WishlistType _selectedType;
    public WishlistType SelectedType
    {
        get => _selectedType;
        set { _selectedType = value; OnPropertyChanged(); }
    }

    private string? _newFranchise;
    public string? NewFranchise
    {
        get => _newFranchise;
        set { _newFranchise = Media.Normalize(value); OnPropertyChanged(); }
    }

    private int? _newSeasonNumber;
    public int? NewSeasonNumber
    {
        get => _newSeasonNumber;
        set { _newSeasonNumber = value; OnPropertyChanged(); }
    }

    private string? _newAuthor;
    public string? NewAuthor
    {
        get => _newAuthor;
        set { _newAuthor = Media.Normalize(value); OnPropertyChanged(); }
    }


    private readonly IMediaRepository<Wishlist> _repository;

    public WishlistTabViewModel()
    {
        _selectedType = WishlistType.Movie;
        _repository = new JsonMediaRepository<Wishlist>();
        var collectionView = CollectionViewSource.GetDefaultView(MediaCollection);
        collectionView.GroupDescriptions.Clear();
        collectionView.GroupDescriptions.Add(
            new PropertyGroupDescription(nameof(Wishlist.Category)));
        MediaCollection.CollectionChanged += (_, __) => RefreshSagaGroups();

        // Load saved Wishlist items
        foreach (var item in _repository.Load())
        {
            item.IsExpanded = false;
            item.IsSidePanelOpen = false;
            MediaCollection.Add(item);
        }
        RefreshSagaGroups();
    }

    protected override bool CanAddMedia()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            MessageBox.Show("Title is required.");
            return false;
        }

        if (NewYear < 1900 || NewYear > 2099)
        {
            MessageBox.Show("Invalid year.");
            return false;
        }


        bool exists = MediaCollection.Any(m =>
            m.Type == SelectedType &&
            string.Equals(m.Title, NewTitle, StringComparison.OrdinalIgnoreCase)
        );

        if (exists)
        {
            MessageBox.Show("Item already exists in wishlist.", "Duplicate Wishlist item");
            return false;
        }
        return true;
    }

    protected override Wishlist CreateMedia()
    {
        var wishlist = new Wishlist
        {
            Title = NewTitle!.Trim(),
            Year = NewYear,
            Type = SelectedType,
            Franchise = SelectedType == WishlistType.Movie ? NewFranchise : null,
            SeasonNumber = SelectedType == WishlistType.Series ? NewSeasonNumber : null,
            Author = SelectedType == WishlistType.Books ? NewAuthor : null,
            Saga = SelectedType.ToString()
        };

        wishlist.BaseColor = Colors.Transparent;
        wishlist.SetDarkMode(IsDarkMode);
        return wishlist;
    }

    protected override void AfterAdd(Wishlist wishlist)
    {
        SortMediaCollection();

        _repository.Save(MediaCollection);
        RefreshSagaGroups();

        ResetInputs();
    }

    private void ResetInputs()
    {
        NewTitle = "";
        NewYear = DateTime.Now.Year;
        NewFranchise = "";
        NewSeasonNumber = null;
        NewAuthor = "";
    }

    protected override void AfterRemove(Wishlist wishlist)
    {
        base.AfterRemove(wishlist);
        _repository.Save(MediaCollection); // book-specific persistence
    }


    private bool ValidateWishlist(Wishlist wishlist)
    {
        // force the irrelevant variables to null
        switch (wishlist.Type)
        {
            case WishlistType.Movie:
                wishlist.Author = null;
                wishlist.SeasonNumber = null;
                break;

            case WishlistType.Books:
                wishlist.Franchise = null;
                wishlist.SeasonNumber = null;
                break;

            case WishlistType.Series:
                wishlist.Author = null;
                wishlist.Franchise = null;
                break;
        }

        string title = wishlist.Title?.Trim() ?? "";
        string? franchise = wishlist.Franchise?.Trim();
        string? author = wishlist.Author?.Trim();
        int year = wishlist.Year;

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Title is required.", "Invalid input");
            return false;
        }

        if (year < 1900 || year > 2099)
        {
            MessageBox.Show("Year must be between 1900 and 2099.", "Invalid input");
            return false;
        }

        // Check for duplicates excluding itself
        bool exists = MediaCollection.Any(m =>
            m != wishlist && m.Type == wishlist.Type &&
            string.Equals(m.Title, title, StringComparison.OrdinalIgnoreCase)
        );

        if (exists)
        {
            MessageBox.Show("A book with the same title and Saga already exists.", "Duplicate Book");
            return false;
        }

        // If everything passes, update trimmed values
        wishlist.Title = title;
        wishlist.Franchise = Media.Normalize(franchise);
        wishlist.Author = Media.Normalize(author);
        wishlist.Note = wishlist.Note?.Trim();
        return true;
    }

    protected override bool IsEditing(Wishlist item) => item.IsEditing;

    protected override void SetEditing(Wishlist item, bool editing)
    {
        item.IsEditing = editing;
    }
    protected override void BackupItem(Wishlist wishlist)
    {
        wishlist.BackupTitle = wishlist.Title;
        wishlist.BackupYear = wishlist.Year;
        wishlist.BackupFranchise = wishlist.Franchise;
        wishlist.BackupSeasonNumber = wishlist.SeasonNumber;
        wishlist.BackupAuthor = wishlist.Author;
        wishlist.BackupNote = wishlist.Note;
        wishlist.BackupBaseColor = wishlist.BaseColor;
    }

    protected override void UndoEdit(Wishlist wishlist)
    {
        wishlist.Title = wishlist.BackupTitle;
        wishlist.Year = wishlist.BackupYear;
        wishlist.Franchise = wishlist.BackupFranchise;
        wishlist.SeasonNumber = wishlist.BackupSeasonNumber;
        wishlist.Author = wishlist.BackupAuthor;
        wishlist.Note = wishlist.BackupNote;
        wishlist.BaseColor = wishlist.BackupBaseColor;

        wishlist.IsEditing = false;
    }

    protected override string GetTitle(Wishlist wishlist) => wishlist.Title ?? "Unnamed";

    protected override void OnMediaChanged(Wishlist wishlist)
    {
        _repository.Save(MediaCollection);
    }

    protected override bool ValidateItem(Wishlist wishlist) => ValidateWishlist(wishlist);

    protected override void AfterSave(Wishlist wishlist)
    {
        wishlist.Saga = wishlist.Type.ToString();
        SortMediaCollection();
        RefreshSagaGroups();
    }

    private void SortMediaCollection()
    {
        var sorted = MediaCollection
            .OrderBy(m => m.Type)
            .ThenBy(m => GetGroupingKey(m))
            .ThenBy(m => m.Year)
            .ThenBy(m => m.Title)
            .ToList();

        MediaCollection.Clear();
        foreach (var m in sorted)
            MediaCollection.Add(m);
    }

    private string GetGroupingKey(Wishlist m)
    {
        return m.Type switch
        {
            WishlistType.Movie => m.Franchise ?? "",
            WishlistType.Books => m.Author ?? "",
            WishlistType.Series => m.Title ?? "",
            _ => ""
        };
    }
}