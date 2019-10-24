using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Unchase.Dynamics365.ConnectedService.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = value != null;
            if (value is bool val)
                flag = val;
            if (value is string stringVal)
                flag = !string.IsNullOrEmpty(stringVal);
            if (value is IList iListVal)
            {
                var list = iListVal;
                if (list.Count == 0)
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                    if (parameter is string)
                    {
                        var str = parameter.ToString();
                        if (str.StartsWith("CheckAll:"))
                        {
                            var name = str.Substring(9);
                            foreach (var obj in (IEnumerable)list)
                            {
                                var property = obj.GetType().GetProperty(name);
                                if (property != (PropertyInfo)null && !(bool)this.Convert(property.GetValue(obj, (object[])null), typeof(bool), (object)null, (CultureInfo)null))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (value is int intVal)
                flag = intVal > 0;
            if (value is Visibility visibilityVal)
                flag = visibilityVal == Visibility.Visible;
            if (targetType == typeof(Visibility))
                return (object)(Visibility)(flag ? 0 : 2);
            return (object)flag;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
