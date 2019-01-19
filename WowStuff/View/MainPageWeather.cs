using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.Devices.Geolocation;
using ChameleonLib.Helper;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Converter;
using ChameleonLib.Resources;
using ChameleonLib.Model;
using Microsoft.Phone.Net.NetworkInformation;

namespace Chameleon
{
    public partial class MainPage : PhoneApplicationPage
    {
        private WeatherBug weatherBug;

        private IApplicationBar iAppBarWeather;

        private Storyboard sbWeatherForecastDesc = new Storyboard();

        private void MainPageWeather()
        {
            //앱바 생성
            CreateWeatherAppBar();

            weatherBug = new WeatherBug();
            weatherBug.DefaultUnitType = (DisplayUnit)SettingHelper.Get(Constants.WEATHER_UNIT_TYPE);
            weatherBug.RequestFailed += weatherBug_RequestFailed;
            weatherBug.LiveWeatherBeforeLoad += weatherBug_LiveWeatherBeforeLoad;
            weatherBug.LiveWeatherCompletedLoad += weatherBug_LiveWeatherCompleted;
        }

        private void LoadLiveWeather()
        {
            GridLiveWeather.Visibility = System.Windows.Visibility.Collapsed;
            ListBoxForecasts.Visibility = System.Windows.Visibility.Collapsed;
            TxtWeatherNoCity.Visibility = System.Windows.Visibility.Visible;           
            
            //날씨를 조회한다.
            RetrieveWeather();
        }

        private void RetrieveWeather()
        {
            WeatherCity city = SettingHelper.Get(Constants.WEATHER_MAIN_CITY) as WeatherCity;

            if (SettingHelper.ContainsKey(Constants.WEATHER_LIVE_RESULT))
            {
                LiveWeather liveWeather = SettingHelper.Get(Constants.WEATHER_LIVE_RESULT) as LiveWeather;

                System.DateTime updDttm = GetDateTime(liveWeather.ObDate);
                //System.DateTime updDttm = liveWeather.UpdatedDateTime;

                //업데이트 후 1시간이 지나지 않았다면
                if (DateTime.Now.Subtract(updDttm).TotalMinutes < 60)
                {
                    //도시 설정
                    weatherBug.DefaultWeatherCity = city;
                    //화면 표시
                    RenderLiveWeather(liveWeather);

                    //업데이트 불필요 기존 데이터를 다시 로드하여 보여줌
                    if (SettingHelper.ContainsKey(Constants.WEATHER_FORECAST_RESULT))
                    {
                        Forecasts forecasts = SettingHelper.Get(Constants.WEATHER_FORECAST_RESULT) as Forecasts;

                        //주간 예보 표시
                        RenderForecast(forecasts);
                        return;
                    }
                }
            }

            if (city != null)
            {
                RetrieveWeather(city);
            }
        }

        private void RetrieveWeather(WeatherCity city)
        {
            DisplayUnit unitType = (DisplayUnit)SettingHelper.Get(Constants.WEATHER_UNIT_TYPE);
            weatherBug.LiveWeather(city, unitType);
        }

        void weatherBug_RequestFailed(object sender, object result)
        {
            //로딩 레이어 제거
            HideLoadingPanel();

            if (App.CheckNetworkStatus())
            {
                MessageBox.Show(AppResources.MsgErrorWeatherServerMsg);
            }
        }

        void weatherBug_LiveWeatherBeforeLoad(object sender, object result)
        {
            ShowLoadingPanel(AppResources.MsgQueryLiveWeather);
        }

        void weatherBug_LiveWeatherCompleted(object sender, LiveWeather liveWeather, Forecasts forecasts)
        {
            //도시검색을 통해서 들어온 경우라면 폰상태 정보의 도시정보를 설정에 저장하고 삭제
            if (PhoneApplicationService.Current.State.ContainsKey(Constants.WEATHER_MAIN_CITY))
            {
                SettingHelper.Set(Constants.WEATHER_MAIN_CITY, PhoneApplicationService.Current.State[Constants.WEATHER_MAIN_CITY], true);
                PhoneApplicationService.Current.State.Remove(Constants.WEATHER_MAIN_CITY);
            }

            if (liveWeather == null || !(liveWeather is LiveWeather) || forecasts == null || !(forecasts is Forecasts))
            {
                WeatherCity city = SettingHelper.Get(Constants.WEATHER_MAIN_CITY) as WeatherCity;

                PIWeather.Header = city.IsGpsLocation ? AppResources.Refresh : city.CityName;
                GridLiveWeather.Visibility = System.Windows.Visibility.Collapsed;
                ListBoxForecasts.Visibility = System.Windows.Visibility.Collapsed;
                TxtWeatherNoCity.Visibility = System.Windows.Visibility.Visible;
                TxtWeatherNoCity.Text = AppResources.MsgNotSupportWeatherLocation;
                return;
            }
            //검색된 결과값 저장
            SettingHelper.Set(Constants.WEATHER_LIVE_RESULT, liveWeather, true);
            //화면에 검색된 결과를 표시
            RenderLiveWeather(liveWeather);
            //조회된 주간예보 정보를 저장
            SettingHelper.Set(Constants.WEATHER_FORECAST_RESULT, forecasts, true);
            //조회된 주간예보 정보를 화면에 표시
            RenderForecast(forecasts);
            //날씨 라이브타일 다시 로드
            CreateWeatherLivetileImage();

            //로딩 레이어 제거
            HideLoadingPanel();
        }

        private void RenderLiveWeather(LiveWeather result)
        {
            WeatherCity city = SettingHelper.Get(Constants.WEATHER_MAIN_CITY) as WeatherCity;

            TxtWeatherNoCity.Visibility = System.Windows.Visibility.Collapsed;
            GridLiveWeather.Visibility = System.Windows.Visibility.Visible;

            SystemTray.SetIsVisible(this, false);
            if (SystemTray.ProgressIndicator != null)
            {
                SystemTray.ProgressIndicator.IsVisible = false;
            }

            BitmapImage imageSource = new BitmapImage();
            imageSource.UriSource = new WeatherImageIconConverter().Convert(result.CurrentConditionIcon, null, "205x172", null) as Uri;
            //업데이트 시간
            System.DateTime dateTime = GetDateTime(result.ObDate);
            //지역
            string[] langs = CultureInfo.CurrentCulture.Name.Split('-');
            if (langs[0] == "ko" || langs[0] == "ja" || langs[1] == "zh")
            {
                PIWeather.Header = (string.IsNullOrEmpty(result.Station.State) ? string.Empty : result.Station.State + " ") + result.Station.City;
            }
            else
            {
                PIWeather.Header = result.Station.City + (string.IsNullOrEmpty(result.Station.State) ? string.Empty : " ," + result.Station.State);
            }

            //업데이트 시간
            TxtLiveWeatherDtUpdated.Text = dateTime.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern);
            TxtLiveWeatherTmUpdated.Text = string.Format(AppResources.WeatherUpdatedTime, dateTime.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern));
            //날씨 이미지
            ImgLiveWeatherIcon.Source  = imageSource;
            //날씨 텍스트
            TxtLiveWeatherCondition.Text = result.CurrentCondition;
            //기온
            string[] temp = result.Temp.Value.Value.Split('.');
            TxtLiveWeatherTemp.Text = temp[0];
            //TxtLiveWeatherTemp.Text = "-50";

            double orgFontSize = TxtLiveWeatherTemp.FontSize;
            //온도 표시 문자열의 길이에 따라 폰트 사이즈 변경
            while(TxtLiveWeatherTemp.Width < TxtLiveWeatherTemp.ActualWidth)
            {
                TxtLiveWeatherTemp.FontSize--;
            }

            if (temp.Length > 1)
            {
                TxtLiveWeatherTempFloat.Text = "." + temp[1];
            }
            TxtLiveWeatherTempUnits.Text = (WeatherUnitsConverter.ConvertOnlyUnit(result.Temp.Value) as ValueUnits).Units;
            //체감온도 
            string feelsLikeLabel = string.IsNullOrEmpty(result.FeelsLikeLabel) ? AppResources.WeatherLiveFeelsLike : result.FeelsLikeLabel + " {0}";
            TxtLiveWeatherFeelTemp.Text = string.Format(feelsLikeLabel, WeatherUnitsConverter.Convert(result.FeelsLike));
            //습도
            TxtLiveWeatherHumidity.Text = string.Format(AppResources.WeatherLiveHumidity, result.Humidity.Value.Value, result.Humidity.Value.Units);
            //바람
            TxtLiveWeatherWind.Text = string.Format(AppResources.WeatherLiveWind, result.WindDirection, result.WindSpeed.Value, result.WindSpeed.Units);
            //최고/최저 기온
            TxtLiveWeatherTempRange.Text = WeatherRangeConverter.RangeText(
                new ValueUnits[2] 
                {
                    result.Temp.Low,
                    result.Temp.High
                }, true);

            double dTemp = 0;
            double dFeel = 0;

            try
            {
                dTemp = Double.Parse(result.Temp.Value.Value);
                dFeel = Double.Parse(result.FeelsLike.Value);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("온도 파싱 에러" + e.Message);
            }

            if (string.IsNullOrEmpty(result.FeelsLike.Value) 
                || string.Format("{0:F1}", dTemp) == string.Format("{0:F1}", dFeel))
            {
                BrdLiveWeatherFeelTemp.Visibility = System.Windows.Visibility.Collapsed;
            }

            //현재 기온의 텍스트 크기가 변경이 되었다면... 단위와 소수점 크기도 변경
            double rt = TxtLiveWeatherTemp.FontSize / orgFontSize;
            Thickness margin = GrdTempSub.Margin;
            margin.Top *= rt;
            GrdTempSub.Margin = margin;
            GrdTempSub.Height *= rt;
            TxtLiveWeatherTempUnits.FontSize *= rt;
            TxtLiveWeatherTempFloat.FontSize *= rt;
        }

        private void RenderForecast(Forecasts forecasts)
        {
            if (forecasts != null && forecasts.Items.Count > 0)
            {
                List<Forecast> items = forecasts.Items.ToList<Forecast>();
                items.Insert(0, forecasts.Today);

                ListBoxForecasts.ItemsSource = items;
                ListBoxForecasts.Visibility = System.Windows.Visibility.Visible;

                ListBoxNightForecasts.ItemsSource = items;
                ListBoxNightForecasts.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ListBoxForecasts.ItemsSource = null;
                ListBoxForecasts.Visibility = System.Windows.Visibility.Collapsed;

                ListBoxNightForecasts.ItemsSource = null;
                ListBoxNightForecasts.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void CreateWeatherAppBar()
        {
            iAppBarWeather = new ApplicationBar();
            iAppBarWeather.Mode = ApplicationBarMode.Default;
            iAppBarWeather.Opacity = 0.8;
            iAppBarWeather.IsVisible = false;
            iAppBarWeather.IsMenuEnabled = true;
            CopyMenuItem(iAppBarWeather.MenuItems);

            ApplicationBarIconButton appBarIconBtnForecast = new ApplicationBarIconButton();
            appBarIconBtnForecast.IconUri = PathHelper.GetPath("appbar.moon.png");
            appBarIconBtnForecast.Text = AppResources.AppbarMenuNight;
            appBarIconBtnForecast.Click += appBarIconBtnForecast_Click;

            ApplicationBarIconButton appBarIconBtnSearch = new ApplicationBarIconButton();
            appBarIconBtnSearch.IconUri = PathHelper.GetPath("appbar.magnify.png");
            appBarIconBtnSearch.Text = AppResources.WeatherSearchCity;
            appBarIconBtnSearch.Click += appBarIconBtnSearch_Click;

            ApplicationBarIconButton appBarIconBtnGps = new ApplicationBarIconButton();
            appBarIconBtnGps.IconUri = PathHelper.GetPath("appbar.location.png");
            appBarIconBtnGps.Text = AppResources.WeatherCurrentLocation;
            appBarIconBtnGps.Click += appBarIconBtnGps_Click;
            
            ApplicationBarIconButton appBarIconBtnSettings = new ApplicationBarIconButton();
            appBarIconBtnSettings.IconUri = PathHelper.GetPath("appbar.settings.png");
            appBarIconBtnSettings.Text = AppResources.Settings;
            appBarIconBtnSettings.Click += settingMI_Click;

            iAppBarWeather.Buttons.Add(appBarIconBtnForecast);
            iAppBarWeather.Buttons.Add(appBarIconBtnSearch);
            iAppBarWeather.Buttons.Add(appBarIconBtnGps);
            iAppBarWeather.Buttons.Add(appBarIconBtnSettings);
        }

        private void appBarIconBtnForecast_Click(object sender, EventArgs e)
        {
            HideForecastDescriptionMarquee();
            ApplicationBarIconButton btn = iAppBarWeather.Buttons[0] as ApplicationBarIconButton;
            bool isNight = btn.Text == AppResources.AppbarMenuNight;
            if (isNight)
            {
                btn.Text = AppResources.AppbarMenuDay;
                btn.IconUri = PathHelper.GetPath("appbar.weather.sun.png");
                ListBoxNightForecasts.Visibility = System.Windows.Visibility.Visible;
                ListBoxForecasts.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                btn.Text = AppResources.AppbarMenuNight;
                btn.IconUri = PathHelper.GetPath("appbar.moon.png");
                ListBoxNightForecasts.Visibility = System.Windows.Visibility.Collapsed;
                ListBoxForecasts.Visibility = System.Windows.Visibility.Visible;
            }



            Color newColor;
            Color dark = Color.FromArgb(255, 64, 64, 64);

            if ((bool)SettingHelper.Get(Constants.CHAMELEON_USE_PROTECTIVE_COLOR))
            {
                ColorAnimation protectiveColorAnimation = gradientStopAnimationStoryboard.Children[0] as ColorAnimation;
                newColor = (Color)protectiveColorAnimation.To;
                if (protectiveColorAnimation != null)
                {
                    //gradientStopAnimationStoryboard.Stop();
                    //gradientStopAnimationStoryboard.Children.Clear();
                    //gradientStopAnimationStoryboard.Children.Add(protectiveColorAnimation);

                    if (isNight)
                    {
                        newColor = dark;
                    }
                    else
                    {
                        //계속 다른 색상을 생성
                        do
                        {
                            int index = new Random().Next(0, ColorItem.UintColors.Length);
                            newColor = ColorItem.ConvertColor(ColorItem.UintColors[index]);
                        } while (newColor.Equals(protectiveColorAnimation.To));

                    }
                    protectiveColorAnimation.To = newColor;
                    gradientStopAnimationStoryboard.Begin();
                }
            }
            else
            {
                if (isNight)
                {
                    newColor = dark;
                }
                else
                {
                    newColor = (SettingHelper.Get(Constants.CHAMELEON_SKIN_BACKGROUND_COLOR) as ColorItem).Color;
                }
                ChangeBackgroundColor(newColor);
                //    DoubleAnimation fixedColorAnimation = gradientStopAnimationStoryboard.Children[0] as DoubleAnimation;

                //    if (fixedColorAnimation == null)
                //    {
                //        gradientStopAnimationStoryboard.Stop();
                //        gradientStopAnimationStoryboard.Children.Clear();
                //        gradientStopAnimationStoryboard.Children.Add(offsetAnimation);
                //    }
                //    gradientStopAnimationStoryboard.Begin();
            }

            SetAppbarColor(ApplicationBar, newColor);
        }

        private void appBarIconBtnSearch_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/View/SearchCityPage.xaml?pi={0}", PIWeather.Name), UriKind.Relative));
        }

        private void appBarIconBtnGps_Click(object sender, EventArgs e)
        {
            if (!(bool)SettingHelper.Get(Constants.WEATHER_USE_LOCATION_SERVICES))
            {
                if (MessageBox.Show(AppResources.MsgUseLocationServices, AppResources.Confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    NavigationService.Navigate(new Uri(string.Format("/View/SettingPage.xaml?piName={0}", "PIWeather"), UriKind.Relative));
                }
                else
                {
                    return;
                }
            }
            //활성화 되어 있는 상태에서의 처리
            if ((bool)SettingHelper.Get(Constants.WEATHER_USE_LOCATION_SERVICES))
            {
                //로딩 패널 띄움
                ShowLoadingPanel(AppResources.MsgCheckingLocation);
                //위치 찾기
                FindLocation();
            }
        }
        
        private async void FindLocation()
        {
            if (!SettingHelper.ContainsKey(Constants.WEATHER_LOCATION_CONSENT))
            {
                MessageBoxResult result =
                MessageBox.Show(AppResources.MsgConfirmUseLocation,
                    AppResources.AgreeUseLocation,
                    MessageBoxButton.OKCancel);

                SettingHelper.Set(Constants.WEATHER_LOCATION_CONSENT, (result == MessageBoxResult.OK), false);
            }

            if (!SettingHelper.ContainsKey(Constants.WEATHER_LOCATION_CONSENT))
            {
                // The user has opted out of Location.
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                DisplayUnit unitType = (DisplayUnit)SettingHelper.Get(Constants.WEATHER_UNIT_TYPE);

                //날씨 조회 모드 설정 변경
                WeatherCity city = new WeatherCity();
                city.IsGpsLocation = true;
                city.Latitude = geoposition.Coordinate.Latitude.ToString();
                city.Longitude = geoposition.Coordinate.Longitude.ToString();
                //검색조건 저장
                SettingHelper.Set(Constants.WEATHER_MAIN_CITY, city, false);

                //좌표로 날씨 조회
                RetrieveWeather(city);
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    //로딩 패널 제거
                    HideLoadingPanel();

                    // the application does not have the right capability or the location master switch is off
                    //StatusTextBlock.Text = "location  is disabled in phone settings.";
                    if (MessageBox.Show(AppResources.MsgFailLocationService, AppResources.Confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        var navigate = Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
                    }
                }
                //else
                {
                    // something else happened acquring the location
                }
            }
        }
        
        private void ListBoxForecasts_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedItem != null)
            {
                Forecast forecast = listBox.SelectedItem as Forecast;
                ShowForecastDescriptionMarquee(string.Format("{0} ({1})", 
                    forecast.Prediction, 
                    string.Format(AppResources.WeatherUpdatedTime, 
                    string.Format("{0} {1}", forecast.DateTime.ToLongDateString(), forecast.DateTime.ToLongTimeString()))));
            }
        }

        private void ListBoxNightForecasts_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedItem != null)
            {
                Forecast forecast = listBox.SelectedItem as Forecast;
                ShowForecastDescriptionMarquee(string.Format("{0} ({1})",
                    forecast.PredictionForNight,
                    string.Format(AppResources.WeatherUpdatedTime,
                    string.Format("{0} {1}", forecast.DateTime.ToLongDateString(), forecast.DateTime.ToLongTimeString()))));
            }
        }

        private void LiveWeather_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ListBoxForecasts.SelectedIndex = -1;
            ListBoxNightForecasts.SelectedIndex = -1;
            HideForecastDescriptionMarquee();
        }

        //슬라이드 애니메이션
        private DoubleAnimationUsingKeyFrames GetForecastDescriptionAnimation()
        {
            TxtForecastDescription.Visibility = System.Windows.Visibility.Visible;

            EasingDoubleKeyFrame sKeyFrame = new EasingDoubleKeyFrame();
            EasingDoubleKeyFrame eKeyFrame = new EasingDoubleKeyFrame();
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();

            double txtWidth = TxtForecastDescription.ActualWidth;

            sKeyFrame.KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0));
            sKeyFrame.Value = PIWeather.ActualWidth;
            eKeyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(txtWidth * 10));
            eKeyFrame.Value = -txtWidth;
            
            animation.KeyFrames.Add(sKeyFrame);
            animation.KeyFrames.Add(eKeyFrame);
            
            Storyboard.SetTarget(animation, TxtForecastDescription);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Left)"));

            return animation;
        }

        private void ShowForecastDescriptionMarquee(string marquee)
        {
            if ((PanoramaMainView.SelectedItem as PanoramaItem).Name == PIWeather.Name)
            {
                TxtForecastDescription.Text = WeatherUnitsConverter.ConvertDeg(marquee);
                Canvas canvas = TxtForecastDescription.Parent as Canvas;
                if (canvas.Clip == null)
                {
                    canvas.Clip = new RectangleGeometry();
                    (canvas.Clip as RectangleGeometry).Rect = new Rect(0, 0, PIWeather.ActualWidth, TxtForecastDescription.ActualHeight);
                }

                if (sbWeatherForecastDesc.GetCurrentState() != ClockState.Stopped)
                {
                    sbWeatherForecastDesc.Stop();
                }

                sbWeatherForecastDesc.Children.Clear();
                sbWeatherForecastDesc.Children.Add(GetForecastDescriptionAnimation());
                sbWeatherForecastDesc.Completed += new EventHandler((object obj, EventArgs ev) =>
                {
                    TxtForecastDescription.Text = string.Empty;
                    TxtForecastDescription.Visibility = System.Windows.Visibility.Collapsed;
                });
                sbWeatherForecastDesc.Begin();
            }
        }

        private void HideForecastDescriptionMarquee()
        {
            sbWeatherForecastDesc.Stop();
            TxtForecastDescription.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void RefreshLiveWeather()
        {
            DisplayUnit weatherUnit = (DisplayUnit)SettingHelper.Get(Constants.WEATHER_UNIT_TYPE);
            weatherBug.RefreshLiveWeather(weatherUnit);
        }

        private System.DateTime GetDateTime(ChameleonLib.Api.Open.Weather.Model.WeatherDateTime dateTime)
        {
            int year = Int32.Parse(dateTime.Year);
            int month = Int32.Parse(dateTime.Month.Number);
            int day = Int32.Parse(dateTime.Day.Number);
            int hour = Int32.Parse(dateTime.Hour24);
            int minute = Int32.Parse(dateTime.Minute);
            int second = Int32.Parse(dateTime.Second);

            return new System.DateTime(year, month, day, hour, minute, second);
        }
    }
}
