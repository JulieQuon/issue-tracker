using Case.IssueTracker.Data;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Case.IssueTracker.Converters
{
    [ValueConversion(typeof(User), typeof(string))]
    public class AssigneeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            User c = (User)value;
            return (null == c || null == c.displayName || c.displayName == "") ? "none" : c.displayName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }

    }
}
