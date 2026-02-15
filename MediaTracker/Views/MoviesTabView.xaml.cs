using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaTracker.Domain;
using MediaTracker.ViewModels;
using MediaTracker.Views.Helpers;

namespace MediaTracker.Views;
public partial class MoviesTabView
{
    public MoviesTabView()
    {
        InitializeComponent();
    }

    private void SagaHeader_Click(object sender, MouseButtonEventArgs e)
    {
        MediaTabViewHelper<Movie>.ToggleSaga(sender);
    }

    private void MediaCard_Click(object sender, MouseButtonEventArgs e)
    {
        MediaTabViewHelper<Movie>.ToggleSidePanel(
            (FrameworkElement)sender,
            (MoviesTabViewModel)DataContext);
    }

    private void CommentsToggle_Click(object sender, RoutedEventArgs e)
    {
        MediaTabViewHelper<Movie>.PreserveTopVisibleItem(
            MainScrollViewer,
            MainItemsControl,
            () => { });
    }
}
