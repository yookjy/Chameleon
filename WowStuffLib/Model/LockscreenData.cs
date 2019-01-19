using ChameleonLib.Helper;
using ChameleonLib.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChameleonLib.Model
{
    public class LockscreenData : LiveData
    {
        public LockscreenData(bool isWVGA)
        {
            Canvas = new Canvas();
            IsWVGA = isWVGA;
            FontRatio = LockscreenHelper.GetFontSizeRatio(IsWVGA);
            FontWeight = (FontWeight)typeof(FontWeights).GetProperty(SettingHelper.GetString(Constants.LOCKSCREEN_FONT_WEIGHT)).GetValue(null);
            
            if (!UseBackgroundSeparation && Items.Length > 0)
            {
                BackgroundPanel = new Canvas()
                {
                    Opacity = BackgroundOpacity,
                    Background = BackgroundBrush
                };
                Canvas.Children.Add(BackgroundPanel);
                //Canvas.Opacity = BackgroundOpacity;
                //Canvas.Background = BackgroundBrush;
            }
        }

        public Canvas Canvas { get; set; }

        public Canvas BackgroundPanel { get; set; }

        public WriteableBitmap BackgroundBitmap { get; set; }

        public bool IsWVGA { get; private set; }

        public double FontRatio { get; private set; }
                
        public SolidColorBrush ForegroundBrush 
        {
            get
            {
                //return Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
                return new SolidColorBrush(Colors.White);
            }
        }
        //72px
        public double FontSizeExtraExtraLarge 
        {
            get
            {
                return (double)Application.Current.Resources["PhoneFontSizeExtraExtraLarge"] * FontRatio;
            }
        }
        //32px
        public double FontSizeLarge 
        {
            get
            {
                return (double)Application.Current.Resources["PhoneFontSizeLarge"] * FontRatio;
            }
        }
        //24px
        public double FontSizeMedium
        {
            get
            {
                return (double)Application.Current.Resources["PhoneFontSizeMedium"] * FontRatio; ;
            }
        }

        public SolidColorBrush BackgroundBrush
        {
            get
            {
                return new SolidColorBrush((SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_COLOR) as ColorItem).Color);
            }
        }

        public double BackgroundOpacity
        {
            get
            {
                return (double)(int)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_OPACITY) / 100;
            }
        }

        public LockscreenItemInfo[] Items
        {
            get
            {
                return (SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_TEMPLATE) as LockscreenTemplateItem).LockscreenItemInfos;
            }
        }

        public bool UseBackgroundSeparation
        {
            get
            {
                return (bool)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION);
            }
        }
    }
}
