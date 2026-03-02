using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaTracker.Domain;
using MediaTracker.ViewModels;
using MediaTracker.Views.Helpers;

namespace MediaTracker.Views;
public partial class BooksTabView
{
    public BooksTabView()
    {
        InitializeComponent();
    }

    private void SagaHeader_Click(object sender, MouseButtonEventArgs e)
    {
        MediaTabViewHelper<Books>.ToggleSaga(sender);
    }

    private void MediaCard_Click(object sender, MouseButtonEventArgs e)
    {
        MediaTabViewHelper<Books>.ToggleSidePanel(
            (FrameworkElement)sender,
            (BooksTabViewModel)DataContext);
    }

    private async void CommentsToggle_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BooksTabViewModel vm)
        {
            MainItemsControl.Visibility = Visibility.Hidden;

            MediaTabViewHelper<Books>.PreserveTopVisibleItem(
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
