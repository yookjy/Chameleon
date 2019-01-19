using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Resources;

namespace ChameleonLib.Converter
{
    public class WeatherImageIconConverter : IValueConverter
    {
        public static string GetIamgetName(string url)
        {
            int s = url.LastIndexOf("/") + 1;
            int e = url.LastIndexOf(".");

            url = url.Replace(".gif", string.Empty);
            url = url.Replace(".png", string.Empty);

            if (s == 0 || e == -1) return WeatherIconMap.Instance.GetImageName(url);

            StringBuilder cond = new StringBuilder();

            for (int i = s; i < e; i++)
            {
                cond.Append(url[i]);
            }
            return WeatherIconMap.Instance.GetImageName(cond.ToString());
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                WeatherIconType type = WeatherIconMap.Instance.WeatherIconType;
                //Uri uri = new Uri(string.Format(WeatherBug.ICON_URL, parameter.ToString(), "trans", cond.ToString()), UriKind.Absolute);
                //로컬로 전환...(속도 및 락스크린 렌더링을 위해 로컬에 있는게 유리하다..)
                Uri uri = new Uri(string.Format(WeatherBug.ICON_LOCAL_PATH, (type).ToString().ToLower(), parameter.ToString(), WeatherImageIconConverter.GetIamgetName(value.ToString())), UriKind.Relative);
                
                return uri;
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
