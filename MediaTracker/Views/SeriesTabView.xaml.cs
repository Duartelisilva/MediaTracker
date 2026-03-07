using MediaTracker.Domain;
using MediaTracker.ViewModels;
using MediaTracker.Views.Helpers;
using System.Windows;
using System.Windows.Input;

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

    private async void CommentsToggle_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SeriesTabViewModel vm)
        {
            MainItemsControl.Visibility = Visibility.Hidden;

            MediaTabViewHelper<Series>.PreserveTopVisibleItem(
            MainScrollViewer,
            MainItemsControl,
            () => { });

            vm.RaiseShowComments();

            await Dispatcher.InvokeAsync(() => { },
                System.Windows.Threading.DispatcherPriority.Render);

            MainItemsControl.Visibility = Visibility.Visible;
        }
    }

    private void Integer_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    private void Integer_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Allow control keys (Backspace, Delete, arrows, Tab)
        if (e.Key == Key.Space)
            e.Handled = true;
    }
}
