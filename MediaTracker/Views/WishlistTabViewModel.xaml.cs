using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaTracker.Domain;
using MediaTracker.ViewModels;
using MediaTracker.Views.Helpers;

namespace MediaTracker.Views;
public partial class WishlistTabView
{
    public WishlistTabView()
    {
        InitializeComponent();
    }

    private void SagaHeader_Click(object sender, MouseButtonEventArgs e)
    {
        MediaTabViewHelper<Wishlist>.ToggleSaga(sender);
    }

    private void MediaCard_Click(object sender, MouseButtonEventArgs e)
    {
        MediaTabViewHelper<Wishlist>.ToggleSidePanel(
            (FrameworkElement)sender,
            (WishlistTabViewModel)DataContext);
    }

    private async void CommentsToggle_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is WishlistTabViewModel vm)
        {
            MainItemsControl.Visibility = Visibility.Hidden;

            MediaTabViewHelper<Wishlist>.PreserveTopVisibleItem(
            MainScrollViewer,
            MainItemsControl,
            () => { });

            vm.RaiseShowComments();

            await Dispatcher.InvokeAsync(() => { },
                System.Windows.Threading.DispatcherPriority.Render);

            MainItemsControl.Visibility = Visibility.Visible;
        }
    }
}
