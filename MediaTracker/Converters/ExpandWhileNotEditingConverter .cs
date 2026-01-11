using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaTracker.Converters;

public class ExpandWhileNotEditingConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2)
            return Visibility.Collapsed;

        bool isExpanded = values[0] is bool b1 && b1;
        bool isEditing = values[1] is bool b2 && b2;

        return (isExpanded && !isEditing) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
