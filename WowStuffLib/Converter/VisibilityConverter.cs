using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Resources;

namespace ChameleonLib.Converter
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string param = parameter as string;
                if (param == "Reverse")
                {
                    bool isHidden = (bool)value;

                    if (isHidden == true)
                    {
                        return System.Windows.Visibility.Collapsed;
                    }
                }
                else if (param == "Null")
                {
                    string val = value as string;
                    if (string.IsNullOrEmpty(val) || string.IsNullOrWhiteSpace(val))
                    {
                        return System.Windows.Visibility.Collapsed;
                    }
                }
                else if (param == "0")
                {
                    double val = (double)value;
                    if (val == 0)
                    {
                        return System.Windows.Visibility.Collapsed;
                    }
                }
                else
                {
                    bool isShow = (bool)value;

                    if (!isShow)
                    {
                        return System.Windows.Visibility.Collapsed;
                    }
                }

                return System.Windows.Visibility.Visible;
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
