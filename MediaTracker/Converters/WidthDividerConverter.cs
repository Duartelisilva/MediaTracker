using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
namespace MediaTracker.Converters
{
    public class WidthDividerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalWidth && int.TryParse(parameter?.ToString(), out int columns) && columns > 0)
            {
                double margin = 8; // same as ItemContainerStyle.Margin
                double totalSpacing = columns * margin * 2; // left + right per card
                return (totalWidth - totalSpacing) / columns;
            }
            return 250; // fallback width
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
