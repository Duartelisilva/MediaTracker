using System.Windows.Controls;
using static MediaTracker.Domain.Movie;
using System.Windows.Input;
using MediaTracker.Domain;

namespace MediaTracker.Views;

public partial class MoviesTabView : UserControl
{
    public MoviesTabView()
    {
        InitializeComponent();
    }
    private void BigFranchiseHeader_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is StackPanel sp && sp.DataContext is Movie.BigFranchiseGroup group)
        {
            group.IsCollapsed = !group.IsCollapsed;
        }
    }


}
