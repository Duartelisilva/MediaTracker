using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaTracker.Converters
{
    public class NullOrEmptyToSeparatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return string.Empty;
            return parameter?.ToString() ?? " • ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
