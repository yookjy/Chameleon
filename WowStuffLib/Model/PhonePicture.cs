using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace ChameleonLib.Model
{
    public class PhonePicture : AbstractPicture
    {
        private Thickness margin;

        [IgnoreDataMemberAttribute]
        private ImageSource _ThumbnailImageSource;

        [IgnoreDataMemberAttribute]
        public ImageSource ThumbnailImageSource { 
            get
            {
                return _ThumbnailImageSource;
            }
            set
            {
                if (_ThumbnailImageSource != value)
                {
                    _ThumbnailImageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [IgnoreDataMemberAttribute]
        public ImageSource ImageSource { get; set; }

        public DateTimeOffset DateTimeOffset { get; set; }

        public Thickness Margin 
        {
            get
            {
                return margin;
            }
            set
            {
                if (margin != value)
                {
                    margin = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        private Uri currentLockscreen;
        public Uri CurrentLockscreen
        {
            get
            {
                return currentLockscreen;
            }
            set
            {
                if (currentLockscreen != value)
                {
                    currentLockscreen = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Uri warnning;
        public Uri Warnning
        {
            get
            {
                return warnning;
            }
            set
            {
                if (warnning != value)
                {
                    warnning = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double opacity;
        public double Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                if (opacity != value)
                {
                    opacity = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
