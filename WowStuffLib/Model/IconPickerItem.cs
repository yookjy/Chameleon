using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace ChameleonLib.Model
{
    public class IconPickerItem
    {
        public WeatherIconType WeatherIconType { get; set; }

        public BitmapSource Icon
        {
            get
            {
                string forecastPath = PathHelper.GetFullPath(string.Format("weather.iconpack.{0}.png", WeatherIconType.ToString().ToLower())).Substring(1);
                WriteableBitmap icon = BitmapFactory.New(0, 0).FromContent(forecastPath);
                return icon;
            }
        }

        public string Name
        {
            get
            {
                return AppResources.ResourceManager.GetString("WeatherIconPack" + WeatherIconType.ToString());
            }
        }
    }
}
