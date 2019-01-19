using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ChameleonLib.Helper;

namespace ChameleonLib.Converter
{
    public class ThemePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = PathHelper.GetFullPath(value.ToString());
            WriteableBitmap wb = BitmapFactory.New(0, 0).FromContent(path.Substring(1));
            return wb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
