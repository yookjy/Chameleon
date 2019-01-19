using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Chameleon;
using ChameleonLib.Resources;
using ChameleonLib.Helper;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Windows.Phone.Media.Capture;
using System.Threading;
using Windows.Phone.Devices.Power;

namespace ChameleonLib.View
{
    public partial class FlashlightPage : PhoneApplicationPage
    {
        public string PageTitle { get; set; }

        private Thread flickeringThread;

        private volatile int frequency;

        private volatile bool isTurnOn;

        private volatile bool exitThread;

        public FlashlightPage()
        {
            InitializeComponent();
            //페이지 타이틀
            PageTitle = string.Format("{0} - {1}", AppResources.ApplicationTitle, AppResources.Flashlight);

            ApplicationBarIconButton appbarClose = new ApplicationBarIconButton();
            appbarClose.Text = AppResources.Home;
            appbarClose.IconUri = PathHelper.GetPath("appbar.home.variant.enter.png");
            appbarClose.Click += appbarClose_Click;

            ApplicationBarIconButton appbarExit = new ApplicationBarIconButton();
            appbarExit.Text = AppResources.Exit;
            appbarExit.IconUri = PathHelper.GetPath("appbar.door.leave.png");
            appbarExit.Click += appbarExit_Click;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(appbarClose);
            ApplicationBar.Buttons.Add(appbarExit);

            WriteableBitmap source = BitmapFactory.New(0, 0).FromContent(PathHelper.GetFullPath("appbar.power.big.png").Substring(1));
            PowerButtonImage.Source = source;

            var sensorLocation = CameraSensorLocation.Back;
            var supportedCameraModes = AudioVideoCaptureDevice.GetSupportedPropertyValues(sensorLocation, KnownCameraAudioVideoProperties.VideoTorchMode);
            if (supportedCameraModes.ToList().Contains((UInt32)VideoTorchMode.On))
            {
                //플래시 지원 기기
                PowerButton.MouseEnter += PowerButton_MouseEnter;
                PowerButton.MouseLeave += PowerButton_MouseLeave;
                PowerButton.Tap += PowerButton_Tap;

                UseToggleButton.IsChecked = (bool)SettingHelper.Get(Constants.FLASHLIGHT_USE_TOGGLE_SWITCH);
                UseToggleButton.Checked += UseToggleButton_Checked;
                UseToggleButton.Unchecked += UseToggleButton_Unchecked;

                flickeringThread = new Thread(FlickeringThread);
                flickeringThread.Start();

                InitializeFlashlight();
            }
            else
            {
                //플래시 미지원 기기
                FlickeringFrequencyHeader.Visibility = System.Windows.Visibility.Collapsed;
                FlickeringFrequency.Visibility = System.Windows.Visibility.Collapsed;
                UseToggleButton.Visibility = System.Windows.Visibility.Collapsed;

                PowerButton.MouseEnter += ((s, e) =>
                {
                    TurnOnButton();
                });

                PowerButton.MouseLeave += ((s, e) =>
                {
                    TurnOffButton();
                });

                PowerButton.Tap += ((s, e) =>
                {
                    SystemTray.IsVisible = false;
                    ApplicationBar.IsVisible = false;
                    FlashlightPanel.Visibility = System.Windows.Visibility.Visible;
                });
            }

            Battery.GetDefault().RemainingChargePercentChanged += FlashlightPage_RemainingChargePercentChanged;
            BatteryRemainPercent.Text = Battery.GetDefault().RemainingChargePercent + "%";
        }

        void FlashlightPage_RemainingChargePercentChanged(object sender, object e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                BatteryRemainPercent.Text = Battery.GetDefault().RemainingChargePercent + "%";
            });
        }

        async void InitializeFlashlight()
        {
            if (App.AvDevice == null)
            {
                var sensorLocation = CameraSensorLocation.Back;
                // get the AudioViceoCaptureDevice
                App.AvDevice = await AudioVideoCaptureDevice.OpenAsync(sensorLocation,
                    AudioVideoCaptureDevice.GetAvailableCaptureResolutions(sensorLocation).First());
            }

            if (UseToggleButton.IsChecked == true)
                TurnOnButton();
        }

        public bool IsTurnOnFlashlight
        {
            get
            {
                if (App.AvDevice == null) return false;
                else
                {
                    return (UInt32)App.AvDevice.GetProperty(KnownCameraAudioVideoProperties.VideoTorchMode) == (UInt32)VideoTorchMode.On;
                }
            }
        }

        public void TurnOffFlashlight()
        {
            var sensorLocation = CameraSensorLocation.Back;
            try
            {
                // turn flashlight on
                var supportedCameraModes = AudioVideoCaptureDevice
                    .GetSupportedPropertyValues(sensorLocation, KnownCameraAudioVideoProperties.VideoTorchMode);
                if (supportedCameraModes.ToList().Contains((UInt32)VideoTorchMode.On))
                {
                    App.AvDevice.SetProperty(KnownCameraAudioVideoProperties.VideoTorchMode, VideoTorchMode.Off);
                }
            }
            catch (Exception)
            {
            }
        }

        public void TurnOnFlashlight()
        {
            var sensorLocation = CameraSensorLocation.Back;
            try
            {
                //turn flashlight on
                var supportedCameraModes = AudioVideoCaptureDevice
                    .GetSupportedPropertyValues(sensorLocation, KnownCameraAudioVideoProperties.VideoTorchMode);
                if (supportedCameraModes.ToList().Contains((UInt32)VideoTorchMode.On))
                {
                    if (App.AvDevice != null)
                    {
                        App.AvDevice.SetProperty(KnownCameraAudioVideoProperties.VideoTorchMode, VideoTorchMode.On);

                        // set flash power to maxinum
                        App.AvDevice.SetProperty(KnownCameraAudioVideoProperties.VideoTorchPower,
                            AudioVideoCaptureDevice.GetSupportedPropertyRange(sensorLocation, KnownCameraAudioVideoProperties.VideoTorchPower).Max);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void appbarExit_Click(object sender, EventArgs e)
        {
            exitThread = true;
            App.Current.Terminate();
        }

        void appbarClose_Click(object sender, EventArgs e)
        {
            GoMainPage();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            exitThread = true;
            base.OnBackKeyPress(e);
        }

        void GoMainPage()
        {
            exitThread = true;
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Uri("/View/MainPage.xaml?from=flashlight", UriKind.Relative));
            }
        }
        
        private void PowerButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (UseToggleButton.IsChecked == false)
            {
                TurnOnButton();
            }
        }

        private void PowerButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (UseToggleButton.IsChecked == false)
            {
                TurnOffButton();
            }
        }

        private void PowerButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (UseToggleButton.IsChecked == true)
            {
                if (isTurnOn)
                {
                    TurnOffButton();
                }
                else
                {
                    TurnOnButton();
                }
            }
        }

        private void UseToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            TurnOnButton();
            SettingHelper.Set(Constants.FLASHLIGHT_USE_TOGGLE_SWITCH, true, true);
        }

        private void UseToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            TurnOffButton();
            SettingHelper.Set(Constants.FLASHLIGHT_USE_TOGGLE_SWITCH, false, true);
        }

        private void FlickeringFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //0.1도 0은 아니므로 1로 처리해야 UI와 움직임이 일관성이 유지됨
            frequency = (int)Math.Ceiling(e.NewValue);
        }

        private void FlickeringThread()
        {
            while (!exitThread)
            {
                if (frequency > 0)
                {
                    if (isTurnOn)
                    {
                        if (!IsTurnOnFlashlight)
                        {
                            //라이트를 켠다.
                            TurnOnFlashlight();
                        }
                        else
                        {
                            //라이트를 끈다.
                            TurnOffFlashlight();
                        }
                        
                        if (frequency > 0)
                            Thread.Sleep(5000 / (int)frequency);
                    }
                    else
                    {
                        if (IsTurnOnFlashlight)
                        {
                            //라이트를 끈다.
                            TurnOffFlashlight();
                        }
                    }
                }
                else
                {
                    if (isTurnOn && !IsTurnOnFlashlight)
                    {
                        //라이트를 켠다.
                        TurnOnFlashlight();
                    }
                    else if (!isTurnOn && IsTurnOnFlashlight)
                    {
                        //라이트를 끈다.
                        TurnOffFlashlight();
                    }
                }
            }

            TurnOffFlashlight();
        }

        private void TurnOnButton()
        {
            ButtonHighlight();
            isTurnOn = true;
        }

        private void TurnOffButton()
        {
            isTurnOn = false;
            PowerButtonImage.Source = BitmapFactory.New(0, 0).FromContent(PathHelper.GetFullPath("appbar.power.big.png").Substring(1));
        }

        private void ButtonHighlight()
        {
            WriteableBitmap source = PowerButtonImage.Source as WriteableBitmap;
            Color accentColor = (Color)Application.Current.Resources["PhoneAccentColor"];

            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    Color pixel = source.GetPixel(x, y);
                    if (pixel.A != 0)
                    {
                        Color curColor = pixel;
                        accentColor.A = curColor.A;
                        source.SetPixel(x, y, accentColor);
                    }
                }
            }

            PowerButtonImage.Source = source;
        }

        private void FlashlightPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TurnOffButton();
            SystemTray.IsVisible = true;
            ApplicationBar.IsVisible = true;
            FlashlightPanel.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}