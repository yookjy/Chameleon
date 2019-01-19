using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ChameleonLib.Resources;


namespace ChameleonLib.Converter
{
    public class ResourceDateFormatter : IValueConverter
    {
         public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null)
            {
                string formatterString = parameter.ToString();

                if (!string.IsNullOrEmpty(formatterString) && !string.IsNullOrEmpty(value.ToString()))
                {
                    string pattern = "m";
                    if (System.Globalization.CultureInfo.CurrentUICulture.Name != "ko-KR")
                    {
                        pattern = "MMM d";     
                    }
                    string val = ((DateTime)value).ToString(pattern);
                    return string.Format(culture,
                        AppResources.ResourceManager.GetString(formatterString, System.Globalization.CultureInfo.CurrentUICulture), val);
                }
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
