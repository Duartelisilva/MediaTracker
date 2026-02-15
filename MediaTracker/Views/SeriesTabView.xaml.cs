using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaTracker.Domain;
using MediaTracker.ViewModels;
using MediaTracker.Views.Helpers;

namespace MediaTracker.Views;
public partial class SeriesTabView
{
    public SeriesTabView()
    {
        InitializeComponent();
    }

    private void SagaHeader_Click(object sender, MouseButtonEventArgs e)
    {
        MediaTabViewHelper<Series>.ToggleSaga(sender);
    }

    private void MediaCard_Click(object sender, MouseButtonEventArgs e)
    {
        MediaTabViewHelper<Series>.ToggleSidePanel(
            (FrameworkElement)sender,
            (SeriesTabViewModel)DataContext);
    }

    private void CommentsToggle_Click(object sender, RoutedEventArgs e)
    {
        MediaTabViewHelper<Series>.PreserveTopVisibleItem(
            MainScrollViewer,
            MainItemsControl,
            () => { });
    }
}
