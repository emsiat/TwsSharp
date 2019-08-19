﻿using System;
using System.Windows;
using System.Windows.Data;

namespace TwsSharpApp
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibility_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            bool v = (bool)value;

            if (v == false) return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
