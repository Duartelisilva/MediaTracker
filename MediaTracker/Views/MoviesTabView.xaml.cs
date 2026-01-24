using System.Windows.Controls;
using static MediaTracker.Domain.Movie;
using System.Windows.Input;
using MediaTracker.Domain;
using MediaTracker.ViewModels;
using System.Windows;

namespace MediaTracker.Views;

public partial class MoviesTabView : UserControl
{
    public MoviesTabView()
    {
        InitializeComponent();
    }
    private void SagaHeader_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is StackPanel sp && sp.DataContext is Media.SagaGroup<Movie> group)
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
            foreach (var m in ((MoviesTabViewModel)DataContext).MediaCollection)
                if (m != movie) m.IsExpanded = false;

            // Toggle clicked movie
            movie.IsExpanded = !movie.IsExpanded;
        }
    }

    private void MovieCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MoviesTabViewModel viewModel)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Movie movie)
            {
                bool newState = !movie.IsSidePanelOpen; // toggle
                foreach (var saga in viewModel.SagaGroups)
                {
                    foreach (var m in saga.Items)
                    {
                        m.IsSidePanelOpen = false; // close all
                    }
                }
                movie.IsSidePanelOpen = newState; // open only clicked if toggled on
            }
        }
    }
}
