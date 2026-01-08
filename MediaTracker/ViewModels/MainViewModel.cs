using System.Collections.ObjectModel;
namespace MediaTracker.ViewModels; 
public sealed class MainViewModel 
{ 
    public ObservableCollection<TabViewModel> 
        Tabs { get; } = new(); 
    public MainViewModel()
    { 
        // Add the two tabs
        Tabs.Add(new MoviesTabViewModel());
        Tabs.Add(new SeriesTabViewModel()); 
    } 
}