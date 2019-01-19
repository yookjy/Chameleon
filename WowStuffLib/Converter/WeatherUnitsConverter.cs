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
    public class WeatherUnitsConverter : IValueConverter
    {
        public static string ConvertDeg(string val)
        {
            return val.Replace("&deg;", "°");
        }

        public static ValueUnits ConvertOnlyUnit(ValueUnits value)
        {
            value.Units = WeatherUnitsConverter.ConvertDeg(value.Units);
            return value;
        }
        
        public static string Convert(ValueUnits value)
        {
            ValueUnits vu = WeatherUnitsConverter.ConvertOnlyUnit(value);
            return string.Format("{0}{1}", vu.Value, vu.Units);
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return WeatherUnitsConverter.Convert(value as ValueUnits);
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
