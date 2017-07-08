using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SecretHitler.App.Converters
{
    public class VisibleIfGreaterThanOrEqualConverter : IValueConverter
    {
        /// <summary>
        /// Set visibility to visible if greater than or equal to the provided integer parameter.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter"></param>
        /// <param name="culture">Not used.</param>
        /// <returns>Visible if value >= parameter, otherwise collapsed.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Int32.TryParse(value.ToString(), out int left) && Int32.TryParse(parameter.ToString(), out int right))
                return left >= right ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
