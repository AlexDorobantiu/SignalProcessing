using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SignalProcessing.Logic.Util.Converters
{
    class StringToInt32Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int i = (int)value;
            return i.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int ret;
            if (int.TryParse(value.ToString(), out ret))
            {
                return ret;
            }
            else return 0;
        }
    }
}
