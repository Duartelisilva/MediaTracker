using System.Windows.Controls;
using static MediaTracker.Domain.Series;
using System.Windows.Input;
using MediaTracker.Domain;
using MediaTracker.ViewModels;
using System.Windows;

namespace MediaTracker.Views;

public partial class SeriesTabView : UserControl
{
    public SeriesTabView()
    {
        InitializeComponent();
    }
    private void SagaHeader_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is StackPanel sp && sp.DataContext is Media.SagaGroup<Series> group)
        {
            group.IsCollapsed = !group.IsCollapsed;
        }
    }

    private void CardBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Prevent toggle if the click was on a button
        if (e.OriginalSource is Button)
            return;

        if (sender is Border border && border.DataContext is Series series)
        {
            // Collapse other series
            foreach (var m in ((SeriesTabViewModel)DataContext).MediaCollection)
                if (m != series) m.IsExpanded = false;

            // Toggle clicked series
            series.IsExpanded = !series.IsExpanded;
        }
    }

    private void SeriesCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SeriesTabViewModel viewModel)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Series series)
            {
                bool newState = !series.IsSidePanelOpen; // toggle
                foreach (var saga in viewModel.SagaGroups)
                {
                    foreach (var m in saga.Items)
                    {
                        m.IsSidePanelOpen = false; // close all
                    }
                }
                series.IsSidePanelOpen = newState; // open only clicked if toggled on
            }
        }
    }
}
