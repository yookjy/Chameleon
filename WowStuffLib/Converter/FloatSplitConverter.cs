using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;

namespace ChameleonLib.Converter
{
    public class FloatSplitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string[] values = value.ToString().Split('.');
                string param = parameter.ToString();

                int idx = Int32.Parse(param);
                return values[idx];
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
