namespace MediaTracker.ViewModels;

public abstract class TabViewModel
{
    public abstract string Header { get; }
    public virtual void SetSearch(string? text) { }

}
