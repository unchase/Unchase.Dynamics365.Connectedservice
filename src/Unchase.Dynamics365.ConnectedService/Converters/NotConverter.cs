using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Unchase.Dynamics365.ConnectedService.Converters
{
    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
                return (object)(Visibility)((Visibility)new VisibilityConverter().Convert(value, targetType, parameter, culture) == Visibility.Visible ? 2 : 0);
            if (!(targetType == typeof(bool)))
                return (object)null;
            if (value == null)
                return (object)true;
            return (object)!(bool)value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is bool val && targetType == typeof(bool))
                return (object)!val;
            throw new NotImplementedException();
        }
    }
}
