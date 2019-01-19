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
    public class WeatherRangeConverter : IValueConverter
    {
        public static string RangeText(ValueUnits[] val, bool isSpace)
        {
            string sPattern = "{0}{1}";
            string dPattern = isSpace ? "{0} ~ {1}{2}" : "{0}~{1}{2}";
            string rangeStr = string.Empty;
            string unit = string.Empty;
            List<string> valList = new List<string>();

            for (int i = 0; i < val.Length; i++)
            {
                if (!string.IsNullOrEmpty(val[i].Value) && val[i].Value != "--")
                {
                    if (string.IsNullOrEmpty(unit))
                    {
                        ValueUnits vu = WeatherUnitsConverter.ConvertOnlyUnit(val[i]);
                        unit = vu.Units;
                    }
                    valList.Add(val[i].Value);
                }
            }

            if (valList.Count == 1)
            {
                return string.Format(sPattern, valList[0], unit);
            }
            else if (valList.Count == 2)
            {
                return string.Format(dPattern, valList[0], valList[1], unit);
            }
                        
            return rangeStr;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return WeatherRangeConverter.RangeText(value as ValueUnits[], false);
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
