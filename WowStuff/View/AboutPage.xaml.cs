using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Reflection;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Info;
using ChameleonLib.Resources;
using ChameleonLib.Model;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Chameleon.View
{
    public partial class AboutPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private SolidColorBrush _LinkColor;

        public SolidColorBrush LinkColor
        {
            get
            {
                return _LinkColor;
            }
            set
            {
                if (_LinkColor != value)
                {
                    _LinkColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private WriteableBitmap _LauncherIcon;

        public WriteableBitmap LauncherIcon
        {
            get
            {
                return _LauncherIcon;
            }
            set
            {
                if (_LauncherIcon != value)
                {
                    _LauncherIcon = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public AboutPage()
        {
            InitializeComponent();

            DataContext = new About();

            foreach (PivotItem pi in AboutPivot.Items)
            {
                AttachEventTextBlock(pi.Content as Panel);
            }

            LauncherIcon = BitmapFactory.New(0, 0).FromContent("Chameleon_336.png");
            //Color bgCol = Color.FromArgb(255, 73, 143, 225); => 159 배경색
            Color bgCol = Color.FromArgb(255, 70, 146, 225);

            for (int x = 0; x < LauncherIcon.PixelWidth; x++)
            {
                for (int y = 0; y < LauncherIcon.PixelHeight; y++)
                {
                    if (LauncherIcon.GetPixel(x, y) == bgCol)
                    {
                        LauncherIcon.SetPixel(x, y, Colors.Transparent);
                    }
                }
            }
        }

        private void AttachEventTextBlock(Panel panel)
        {
            if (panel == null) return;

            foreach (UIElement elem in panel.Children)
            {
                if (elem is TextBlock && (elem as TextBlock).Tag as string == "link")
                {
                    elem.Tap += Link_Tap;
                    LinkColor = new SolidColorBrush(ColorItem.GetColorByName("Green"));
                }
                else if (elem is Panel)
                {
                    AttachEventTextBlock(elem as Panel);
                }
            }
        }

        void Link_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string name = (sender as TextBlock).Name;

            if (name == DeveloperContact.Name)
            {
                ShowWebBrowserTask("https://www.facebook.com/yookjy");
            }
            else if (name == DesignerContact.Name)
            {
                ShowWebBrowserTask("https://www.facebook.com/yookjy");
            }
            else if (name == RateReview.Name)
            {
                new MarketplaceReviewTask().Show();
            }
            else if (name == Facebook.Name)
            {
                ShowWebBrowserTask(Constants.FACEBOOK_SUPPORT);
            }
            else if (name == Feedback.Name)
            {
                var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);

                EmailComposeTask emailComposeTask = new EmailComposeTask();
                emailComposeTask.To = Constants.EMAIL_SUPPORT;
                emailComposeTask.Subject = "Support";
                emailComposeTask.Body =
                    new System.Text.StringBuilder().Append(nameHelper.Name).Append(" ")
                    .AppendFormat("{0}.{1}.{2}", nameHelper.Version.Major, nameHelper.Version.Minor, nameHelper.Version.Build)
                    .AppendLine()
                    //.AppendLine(nameHelper.Version.ToString())
                    .AppendLine(System.Globalization.CultureInfo.CurrentCulture.EnglishName)
                    .AppendLine(DeviceStatus.DeviceName)
                    .AppendLine(DeviceStatus.DeviceFirmwareVersion)
                    .AppendLine(Environment.OSVersion.Version.ToString()).ToString();
                emailComposeTask.Show();
            }
            else if (name == UserVoice.Name)
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();
                emailComposeTask.To = Constants.EMAIL_SUPPORT;
                emailComposeTask.Subject = "Suggest";
                emailComposeTask.Show();
            }
            else if (name == ShareApp.Name)
            {
                ShareLinkTask slt = new ShareLinkTask();
                slt.LinkUri = new Uri("http://www.windowsphone.com/s?appid=d9561d4e-b2c1-42b0-8701-a7bb2f883605");
                slt.Message = "This app is very useful!";
                slt.Title = "Share Chameleon app!";
                slt.Show();
            }
            else if (name == LibraryCreator1.Name)
            {
                ShowWebBrowserTask("http://phone.codeplex.com/");
            }
            else if (name == LibraryCreator2.Name)
            {
                ShowWebBrowserTask("http://writeablebitmapex.codeplex.com/");
            }
            else if (name == LibraryCreator3.Name)
            {
                ShowWebBrowserTask("http://james.newtonking.com/json");
            }
            else if (name == LibraryCreator4.Name)
            {
                ShowWebBrowserTask("http://www.icsharpcode.net/");
            }
            else if (name == SpecialPeople1.Name)
            {
            }
            else if (name == SpecialPeople2.Name)
            {
                ShowWebBrowserTask("https://www.facebook.com/100001136649926");
            }
            //else if (name == SpecialPeople3.Name)
            //{
            //    ShowWebBrowserTask("http://dribbble.com/gilhyun");
            //}
        }

        private void ShowWebBrowserTask(string url)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = new Uri(url, UriKind.Absolute);
            wbt.Show();
        }
    }
}