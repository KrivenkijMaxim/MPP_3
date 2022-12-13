using DirectoryScanner.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DirectoryScanner.View
{
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToStringConverter : MarkupExtension, IValueConverter
    {

        private static DoubleToStringConverter _converter = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleVal = (double)value;
            return doubleVal.ToString("0.00");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(_converter == null)
                _converter = new DoubleToStringConverter();
            return _converter;
        }
    }
}
