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
using Path = System.IO.Path;

namespace DirectoryScanner.View
{
    [ValueConversion(typeof(ComponentType), typeof(BitmapImage))]
    public class TypeToSourceConverter : MarkupExtension, IValueConverter
    {
        private const string _path = @"..\..\..\images\";
        private const string _ext = ".png";

        private static TypeToSourceConverter _converter = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (ComponentType)value;
            var path = _path + type.ToString() +  _ext;
            var uri = new Uri(Path.GetFullPath(path));
            var source = new BitmapImage(uri);

            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(_converter == null)
                _converter = new TypeToSourceConverter();
            return _converter;
        }
    }
}
