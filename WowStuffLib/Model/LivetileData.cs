using ChameleonLib.Helper;
using ChameleonLib.Resources;
using System;
using System.Windows;
using System.Windows.Media;

namespace ChameleonLib.Model
{
    public class LivetileData : LiveData
    {
        public LivetileData()
        {
            AreaSize = new Size(336, 336);
            FontWeight = (FontWeight)typeof(FontWeights).GetProperty(SettingHelper.GetString(Constants.LIVETILE_FONT_WEIGHT)).GetValue(null);
        }

        public Size AreaSize { get; private set; }

        public SolidColorBrush ForegroundBrush 
        {
            get
            {
                return Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
            }
        }
        //72px
        public double FontSizeExtraExtraLarge 
        {
            get
            {
                return (double)Application.Current.Resources["PhoneFontSizeExtraExtraLarge"];
            }
        }
        //32px
        public double FontSizeLarge 
        {
            get
            {
                return (double)Application.Current.Resources["PhoneFontSizeLarge"];
            }
        }
        //24px
        public double FontSizeMedium
        {
            get
            {
                return (double)Application.Current.Resources["PhoneFontSizeMedium"];
            }
        }

        public SolidColorBrush GetBackgroundBrush(LiveItems item)
        {
            string key = string.Empty;

            if ((bool)SettingHelper.Get(Constants.LIVETILE_RANDOM_BACKGROUND_COLOR))
            {
                int index = new Random().Next(0, ColorItem.UintColors.Length - 1);
                return new SolidColorBrush(ColorItem.ConvertColor(ColorItem.UintColors[index]));
            }
            else
            {
                switch (item)
                {
                    case LiveItems.Calendar:
                        key = Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR;
                        break;
                    case LiveItems.Weather:
                        key = Constants.LIVETILE_WEATHER_BACKGROUND_COLOR;
                        break;
                    case LiveItems.Battery:
                        key = Constants.LIVETILE_BATTERY_BACKGROUND_COLOR;
                        break;
                }

                return new SolidColorBrush((SettingHelper.Get(key) as ColorItem).Color);
            }
        }
    }
}
