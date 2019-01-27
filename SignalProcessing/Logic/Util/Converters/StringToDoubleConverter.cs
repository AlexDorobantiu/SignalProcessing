using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SignalProcessing.Logic.Util.Converters
{
    class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double d = (double)value;
            return Math.Round(d, 2).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double ret;
            if (double.TryParse(value.ToString(), out ret))
            {
                return ret;
            }
            else return 0;
        }
    }
}
