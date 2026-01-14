using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MediaTracker.Converters
{
    public class LightBackgroundToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                var c = brush.Color;

                // Include alpha in brightness estimation
                double alphaFactor = c.A / 255.0; // 0-1
                double brightness = ((0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255) * alphaFactor;

                // Adjust threshold for semi-transparent backgrounds
                return brightness > 0.4 ? Brushes.Black : Brushes.White;
            }
            return Brushes.White;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
