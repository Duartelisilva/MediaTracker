using MediaTracker.Domain;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaTracker.Converters
{
    public class MovieDateTupleConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return null;
            if (values[0] is Movie movie && values[1] is DateTime dt)
                return Tuple.Create(movie, dt);
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
