using System;
using System.Globalization;
using System.Windows.Data;

namespace FileExplorer.Converters
{
    internal class ToSystemFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (!(value is DateTime))
            {
                throw new Exception("Not supported type");
            }
            return ((DateTime)value).ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern+" "+ CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}