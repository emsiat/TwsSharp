using System;
using System.Windows;
using System.Windows.Data;

namespace TwsSharpApp
{
    [ValueConversion(typeof(bool), typeof(double))]
    public class BoolToOpacity_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return 1;
            bool v = (bool)value;

            if (v == false) return 1;

            return 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
