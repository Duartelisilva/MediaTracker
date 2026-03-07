using MediaTracker.Domain;
using MediaTracker.Services;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MediaTracker.ViewModels;

public sealed class BooksTabViewModel : MediaTabViewModel<Books>
{
    public override string Header => "Books";


    private int? _newBookNumber;
    public int? NewBookNumber
    {
        get => _newBookNumber;
        set { _newBookNumber = value; OnPropertyChanged(); }
    }

    private string? _newAuthor;
    public string? NewAuthor
    {
        get => _newAuthor;
        set { _newAuthor = Media.Normalize(value); OnPropertyChanged(); }
    }


    private readonly IMediaRepository<Books> _repository;

    public BooksTabViewModel()
    {
        NewTitle = "";
        NewWatchDate = "";

        _repository = new JsonMediaRepository<Books>();

        var collectionView = CollectionViewSource.GetDefaultView(MediaCollection);
        collectionView.GroupDescriptions.Clear();
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Saga"));
        MediaCollection.CollectionChanged += (_, __) => RefreshSagaGroups();

        // Load saved Books
        foreach (var book in _repository.Load())
        {
            book.IsExpanded = false;
            book.IsSidePanelOpen = false;
            MediaCollection.Add(book);
        }
        RefreshSagaGroups();

        // Attach collapse callback
        foreach (var book in MediaCollection)
        {
            book.ClearNewWatchDate = () => NewWatchDate = "";
        }
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
            string.Equals(m.Title, NewTitle, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Saga ?? "", NewSaga ?? "", StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            MessageBox.Show("A book with the same title and Saga already exists.", "Duplicate Book");
            return false;
        }
        return true;
    }

    protected override Books CreateMedia()
    {
        var book = new Books
        {
            Title = NewTitle!.Trim(),
            Saga = NewSaga,
            Year = NewYear,
            Author = string.IsNullOrWhiteSpace(NewAuthor) ? null : NewAuthor,
            BookNumber = NewBookNumber
        };

        book.BaseColor = Colors.Transparent;
        book.SetDarkMode(IsDarkMode);
        return book;
    }

    protected override void AfterAdd(Books book)
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
        NewSaga = "";
        NewAuthor = "";
        NewBookNumber = null;
        NewWatchDate = "";
    }

    protected override void AfterRemove(Books book)
    {
        base.AfterRemove(book);
        _repository.Save(MediaCollection); // book-specific persistence
    }

    private bool ValidateBook(Books book)
    {
        string title = book.Title?.Trim() ?? "";
        string saga = book.Saga?.Trim() ?? "";
        string? author = book.Author?.Trim();
        int year = book.Year;

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
            m != book &&
            string.Equals(m.Title, title, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Saga ?? "", saga ?? "", StringComparison.OrdinalIgnoreCase)
        );

        if (exists)
        {
            MessageBox.Show("A book with the same title and Saga already exists.", "Duplicate Book");
            return false;
        }

        // If everything passes, update trimmed values
        book.Title = title;
        book.Saga = Media.Normalize(saga);
        book.Author = Media.Normalize(author);
        book.Note = book.Note?.Trim();
        return true;
    }

    protected override bool IsEditing(Books item) => item.IsEditing;

    protected override void SetEditing(Books item, bool editing)
    {
        item.IsEditing = editing;
    }

    protected override void BackupItem(Books book)
    {
        book.BackupTitle = book.Title;
        book.BackupYear = book.Year;
        book.BackupSaga = book.Saga;
        book.BackupBookNumber = book.BookNumber;
        book.BackupAuthor = book.Author;
        book.BackupNote = book.Note;
        book.BackupBaseColor = book.BaseColor;
    }

    protected override void UndoEdit(Books book)
    {
        book.Title = book.BackupTitle;
        book.Year = book.BackupYear;
        book.Saga = book.BackupSaga;
        book.BookNumber = book.BackupBookNumber;
        book.Author = book.BackupAuthor;
        book.Note = book.BackupNote;
        book.BaseColor = book.BackupBaseColor;

        book.IsEditing = false;
    }

    protected override string GetTitle(Books book) => book.Title ?? "Unnamed";

    protected override void OnMediaChanged(Books book)
    {
        _repository.Save(MediaCollection);
    }

    protected override bool ValidateItem(Books book) => ValidateBook(book);

    protected override void AfterSave(Books book)
    {
        SortMediaCollection();
        RefreshSagaGroups();
    }

    private void SortMediaCollection()
    {
        var sorted = MediaCollection
            .OrderBy(m => m.Year)
            .ThenBy(m => m.BookNumber)
            .ThenBy(m => m.Title)
            .ToList();

        MediaCollection.Clear();
        foreach (var m in sorted)
            MediaCollection.Add(m);
    }
}