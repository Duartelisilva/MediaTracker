using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaTracker.Domain;
using MediaTracker.ViewModels;

namespace MediaTracker.Views.Helpers;

public static class MediaTabViewHelper<T> where T : Media
{
    public static void ToggleSaga(object sender)
    {
        if (sender is StackPanel sp && sp.DataContext is Media.SagaGroup<T> group)
            group.IsCollapsed = !group.IsCollapsed;
    }

    public static void ToggleSidePanel(FrameworkElement fe, MediaTabViewModel<T> viewModel)
    {
        if (fe.DataContext is T media)
        {
            bool newState = !media.IsSidePanelOpen;
            foreach (var saga in viewModel.SagaGroups)
                foreach (var m in saga.Items)
                    m.IsSidePanelOpen = false;

            media.IsSidePanelOpen = newState;
        }
    }

    public static void PreserveTopVisibleItem(
        ScrollViewer scrollViewer,
        ItemsControl itemsControl,
        Action layoutChangeAction)
    {
        if (scrollViewer == null || itemsControl == null) return;

        FrameworkElement? topElement = null;
        double originalTopOffset = 0;
        double manualOffset = 130;

        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item)
                is FrameworkElement container)
            {
                var transform = container.TransformToAncestor(scrollViewer);
                var position = transform.Transform(new Point(0, 0));
                if (position.Y + container.ActualHeight >= manualOffset)
                {
                    topElement = container;
                    originalTopOffset = position.Y;
                    break;
                }
            }
        }

        layoutChangeAction?.Invoke();
        if (topElement == null) return;

        scrollViewer.Dispatcher.BeginInvoke(new Action(() =>
        {
            var transform = topElement.TransformToAncestor(scrollViewer);
            double delta = transform.Transform(new Point(0, 0)).Y - originalTopOffset;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + delta);
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }
}
