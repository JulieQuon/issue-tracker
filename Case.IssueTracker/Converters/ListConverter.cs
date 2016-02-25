
using System;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using Case.IssueTracker.Data;

namespace Case.IssueTracker.Converters
{
    public class ListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string components = "none";
            List<Component> list = (List<Component>)value;
            if (list != null && list.Any())
            {
                components = "";
                foreach (var c in list)
                    components += c.name + ", ";
                components = components.Remove(components.Count() - 2);
            }
            return components;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
