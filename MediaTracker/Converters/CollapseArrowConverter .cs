using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaTracker.Converters
{
    public class CollapseArrowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool collapsed = (bool)value;
            return collapsed ? "▶" : "▼"; // right arrow when collapsed, down arrow when expanded
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}


