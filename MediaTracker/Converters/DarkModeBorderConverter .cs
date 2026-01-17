using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;          
using System.Windows.Media;
using System.Globalization;

namespace MediaTracker.Converters
{
    public class DarkModeBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush && parameter is bool isDarkMode)
            {
                var color = brush.Color;
                if (isDarkMode)
                {
                    color = Color.FromRgb(
                        (byte)(color.R * 0.6),
                        (byte)(color.G * 0.6),
                        (byte)(color.B * 0.6));
                }
                return new SolidColorBrush(color);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
