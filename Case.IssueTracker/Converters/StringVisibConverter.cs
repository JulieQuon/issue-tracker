using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace Case.IssueTracker.Converters
{
    [ValueConversion(typeof(String), typeof(Visibility))]
    public class StringVisinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string c = (string)value;
            return (string.IsNullOrWhiteSpace(c)) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
