using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaTracker.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        // ConverterParameter should be "Show Text|Hide Text"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string str && str.Contains("|"))
            {
                var parts = str.Split('|');
                bool b = value is bool val && val;
                return b ? parts[1] : parts[0];
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string str && str.Contains("|"))
            {
                var parts = str.Split('|');
                return value?.ToString() == parts[1];
            }
            return false;
        }
    }
}
