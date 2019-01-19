using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;

namespace ChameleonLib.Converter
{
    public class DayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string[] dayNames = DateTimeFormatInfo.InvariantInfo.AbbreviatedDayNames;
                string dayName = value.ToString();

                for (int i = 0; i < dayNames.Length; i++)
                {
                    if (dayNames[i].ToLower() == dayName.ToLower())
                    {
                        return culture.DateTimeFormat.AbbreviatedDayNames[i];
                    }
                }

                return value;
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
