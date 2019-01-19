using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
//using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowLib.UI
{
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        private string header;

        public string Header {
            get
            {
                return header;
            }
            set
            {
                if (header != value)
                {
                    header = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        public string Text { get; set; }

        public Color Color { get; set; }

        public ColorPicker()
        {
            InitializeComponent();
        }

        private void Rectangle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ColorDisplay.Width = e.NewSize.Height;
        }

        private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Uri uri = new Uri(string.Format("/WowLib;component/UI/ColorPickerPage.xaml?header={0}", header), UriKind.Relative);
            if (uri != (Application.Current.RootVisual as PhoneApplicationFrame).CurrentSource) //the uri you want to navigate to is not the current.
            {
                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
