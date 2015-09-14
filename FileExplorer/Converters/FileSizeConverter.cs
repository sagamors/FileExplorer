using System;
using System.Globalization;
using System.Windows.Data;
using static System.String;

namespace FileExplorer.Converters
{
    internal class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            if (!(value is long))
            {
                throw new Exception("Not supported type");
            }

            long v = (long) value;
            if (v == -1) return "";
            string[] sizes = {"B", "KB", "MB", "GB"};
            double len = (double)v;
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len/1024;
            }
            return Format("{0:0.##} {1}", len, sizes[order]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
