﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace SecretHitler.App.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public bool NullResult { get; set; } = false;
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? NullResult : !NullResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
