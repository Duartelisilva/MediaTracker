namespace MediaTracker.ViewModels;

public abstract class TabViewModel
{
    public abstract string Header { get; }
    public virtual void UpdateMediaDarkMode(bool isDark) { }
    public virtual void SetSearch(string? text) { }
}
