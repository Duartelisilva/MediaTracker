using System.Windows.Controls;
using static MediaTracker.Domain.Movie;
using System.Windows.Input;
using MediaTracker.Domain;
using MediaTracker.ViewModels;

namespace MediaTracker.Views;

public partial class MoviesTabView : UserControl
{
    public MoviesTabView()
    {
        InitializeComponent();
    }
    private void SagaHeader_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is StackPanel sp && sp.DataContext is Movie.SagaGroup group)
        {
            group.IsCollapsed = !group.IsCollapsed;
        }
    }

    private void CardBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Prevent toggle if the click was on a button
        if (e.OriginalSource is Button)
            return;

        if (sender is Border border && border.DataContext is Movie movie)
        {
            // Collapse other movies
            foreach (var m in ((MoviesTabViewModel)DataContext).MoviesCollection)
                if (m != movie) m.IsExpanded = false;

            // Toggle clicked movie
            movie.IsExpanded = !movie.IsExpanded;
        }
    }
}
