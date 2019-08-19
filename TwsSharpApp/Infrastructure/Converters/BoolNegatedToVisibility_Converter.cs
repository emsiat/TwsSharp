﻿using System;
using System.Windows;
using System.Windows.Data;

namespace TwsSharpApp
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolNegatedToVisibility_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Visible;
            bool v = (bool)value;

            if (v == false) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
