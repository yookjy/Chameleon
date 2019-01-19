using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace ChameleonLib.Model
{
    public class LivetileTemplateItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public LivetileTemplateItem(LiveItems liveItem)
        {
            visibility = System.Windows.Visibility.Collapsed;
            LiveItem = liveItem;
            pinIconOpacity = 0.3;
        }

        public Uri PinIconUri
        {
            get
            {
                return PathHelper.GetPath("appbar.pin.png");
            }
        }

        public LiveItems LiveItem;

        private double pinIconOpacity;

        private BitmapSource frontSide;

        private BitmapSource backSide;

        private Brush background;
        
        private string liveBackTileContent;

        private Visibility visibility;

        public Brush Background
        {
            get
            {
                return background;
            }
            set
            {
                if (background != value)
                {
                    background = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility Visibility
        {
            get
            {
                return visibility;
            }
            set
            {
                if (visibility != value)
                {
                    visibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string BackTitle { get; set; }

        public string Title
        {
            get
            {
                switch (LiveItem)
                {
                    case LiveItems.Calendar:
                        //return "Calendar";
                        return AppResources.Calendar;
                    case LiveItems.Weather:
                        return AppResources.Weather;
                    case LiveItems.Battery:
                        return AppResources.Battery;
                    //return "Battery";
                }
                return string.Empty;
            }
        }

        public string Subtitle
        {
            get
            {
                switch (LiveItem)
                {
                    case LiveItems.Calendar:
                        return string.Empty;
                    case LiveItems.Weather:
                        return string.Empty;
                    case LiveItems.Battery:
                        return string.Format("[{0}]", AppResources.Flashlight);
                }
                return string.Empty;
            }
        }

        public BitmapSource FrontSide
        {
            get
            {
                return frontSide;
            }
            set
            {
                if (frontSide != value)
                {
                    frontSide = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BitmapSource BackSide
        {
            get
            {
                return backSide;
            }
            set
            {
                if (backSide != value)
                {
                    backSide = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double PinIconOpacity
        {
            get
            {
                return pinIconOpacity;
            }
            set
            {
                if (pinIconOpacity != value)
                {
                    pinIconOpacity = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string LiveBackTileContent
        {
            get
            {
                return liveBackTileContent;
            }
            set
            {
                if (liveBackTileContent != value)
                {
                    liveBackTileContent = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
