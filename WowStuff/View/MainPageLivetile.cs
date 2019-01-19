using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChameleonLib.Api.Calendar;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Helper;
using ChameleonLib.Model;
using ChameleonLib.Resources;

namespace Chameleon
{
    public partial class MainPage : PhoneApplicationPage
    {
        private IApplicationBar IAppBarLivetile;

        private void MainPageLivetile()
        {
            CreateLivetileAppBar();

            if (ScheduledActionService.Find(Constants.PERIODIC_TASK_NAME) == null 
                && (bool)ExistsActiveTile)
            {
                StartPeriodicAgent();
            }
        }

        private void LoadLivetile()
        {
            //잠금화면의 배터리 잔량 업데이트
            ShellTile primaryTile = ShellTile.ActiveTiles.FirstOrDefault();
            ShellTileData tileData = new FlipTileData() { Count = BatteryHelper.BateryLevel };
            primaryTile.Update(tileData);

            LivetileSelector.ItemsSource = new List<LivetileTemplateItem>();
            LivetileSelector.ItemsSource.Add(new LivetileTemplateItem(LiveItems.Weather));
            LivetileSelector.ItemsSource.Add(new LivetileTemplateItem(LiveItems.Calendar));
            LivetileSelector.ItemsSource.Add(new LivetileTemplateItem(LiveItems.Battery));
            
            LoadAllLivetile();
        }

        //앱바 생성
        private void CreateLivetileAppBar()
        {
            IAppBarLivetile = new ApplicationBar();
            IAppBarLivetile.Mode = ApplicationBarMode.Default;
            IAppBarLivetile.Opacity = 0.8;
            IAppBarLivetile.IsVisible = false;
            IAppBarLivetile.IsMenuEnabled = true;
            CopyMenuItem(IAppBarLivetile.MenuItems);

            ApplicationBarIconButton pinWeatherAppBarIconBtn = new ApplicationBarIconButton();
            pinWeatherAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.weather.pin.png");
            pinWeatherAppBarIconBtn.Text = AppResources.PinWeatherLivetile;
            pinWeatherAppBarIconBtn.Click += pinWeatherAppBarIconBtn_Click;
            pinWeatherAppBarIconBtn.IsEnabled = false;

            ApplicationBarIconButton pinCalendarAppBarIconBtn = new ApplicationBarIconButton();
            pinCalendarAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.calendar.pin.png");
            pinCalendarAppBarIconBtn.Text = AppResources.PinCalendarLivetile;
            pinCalendarAppBarIconBtn.Click += pinCalendarAppBarIconBtn_Click;
            pinCalendarAppBarIconBtn.IsEnabled = false;
            
            ApplicationBarIconButton pinBatteryAppBarIconBtn = new ApplicationBarIconButton();
            pinBatteryAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.battery.pin.png");
            pinBatteryAppBarIconBtn.Text = AppResources.PinBatteryLivetile;
            pinBatteryAppBarIconBtn.Click += pinBatteryAppBarIconBtn_Click;
            pinBatteryAppBarIconBtn.IsEnabled = false;

            ApplicationBarIconButton appBarIconBtnSettings = new ApplicationBarIconButton();
            appBarIconBtnSettings.IconUri = PathHelper.GetPath("appbar.settings.png");
            appBarIconBtnSettings.Text = AppResources.Settings;
            appBarIconBtnSettings.Click += settingMI_Click;

            IAppBarLivetile.Buttons.Add(pinWeatherAppBarIconBtn);
            IAppBarLivetile.Buttons.Add(pinCalendarAppBarIconBtn);
            IAppBarLivetile.Buttons.Add(pinBatteryAppBarIconBtn);
            IAppBarLivetile.Buttons.Add(appBarIconBtnSettings);
        }

        private void settingMI_Click(object sender, EventArgs e)
        {
            NavigateSettingPage();
        }
        
        private void LoadLivetileImage(LivetileTemplateItem item)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(string.Format("Shared/ShellContent/livetile.{0}.back.jpg", item.LiveItem.ToString().ToLower())))
                {
                    using (IsolatedStorageFileStream sourceStream = isoStore.OpenFile(string.Format("Shared/ShellContent/livetile.{0}.back.jpg", item.LiveItem.ToString().ToLower()), System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        item.BackSide = BitmapFactory.New(0, 0).FromStream(sourceStream);
                    }
                }

                if (isoStore.FileExists(string.Format("Shared/ShellContent/livetile.{0}.jpg", item.LiveItem.ToString().ToLower())))
                {
                    using (IsolatedStorageFileStream sourceStream = isoStore.OpenFile(string.Format("Shared/ShellContent/livetile.{0}.jpg", item.LiveItem.ToString().ToLower()), System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        item.FrontSide = BitmapFactory.New(0, 0).FromStream(sourceStream);
                    }
                }
            }
        }

        private void CreateCalendarLivetileImage()
        {
            LivetileTemplateItem calendarItem = LivetileSelector.ItemsSource[1] as LivetileTemplateItem;
            calendarItem.Background = new SolidColorBrush((SettingHelper.Get(Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR) as ColorItem).Color);
            calendarItem.BackTitle = AppResources.ApplicationTitle;

            //달력이미지를 무조건 새로 생성해서 화면 업데이트
            LivetileData data = new LivetileData()
            {
                DayList = VsCalendar.GetCalendarOfMonth(DateTime.Now, DateTime.Now, true, true)
            };

            if ((bool)SettingHelper.Get(Constants.CALENDAR_SHOW_APPOINTMENT))
            {
                Appointments appointments = new Appointments();
                appointments.SearchCompleted += (s, se) =>
                {
                    //달력 데이타
                    VsCalendar.MergeCalendar(data.DayList, se.Results);
                    
                    LivetileHelper.CreateLivetileImage(data, LiveItems.Calendar);
                    LoadLivetileImage(calendarItem);
                    //일정이 몇개 있는지를 백타일에 표시
                    List<Appointment> appList = data.DayList.Find(x => x.DateTime.ToLongDateString() == DateTime.Today.ToLongDateString()).AppointmentList;
                    int count = appList == null ? 0 : appList.Count;
                    calendarItem.LiveBackTileContent = LivetileHelper.GetCalendarBackTextContent(count);
                    calendarItem.Visibility = System.Windows.Visibility.Visible;
                    if (ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("calendar")) != null)
                    {
                        calendarItem.PinIconOpacity = 1;
                        GetAppBarButton(AppResources.PinCalendarLivetile).IsEnabled = false;
                        UpdateLiveTile(calendarItem, count);
                    }
                    else
                    {
                        calendarItem.PinIconOpacity = 0.3;
                        GetAppBarButton(AppResources.PinCalendarLivetile).IsEnabled = true;
                    }
                };
                appointments.SearchAsync(data.DayList[7].DateTime, data.DayList[data.DayList.Count - 1].DateTime.AddDays(1), null);
            }
            else
            {
                //전체 타일 이미지 생성
                LivetileHelper.CreateLivetileImage(data, LiveItems.Calendar);
                //달력 이미지 로드
                LoadLivetileImage(calendarItem);
                //백타일에 일정 표시하지 않음
                calendarItem.LiveBackTileContent = LivetileHelper.GetCalendarBackTextContent(-1);
                calendarItem.Visibility = System.Windows.Visibility.Visible;
                if (ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("calendar")) != null)
                {
                    calendarItem.PinIconOpacity = 1;
                    GetAppBarButton(AppResources.PinCalendarLivetile).IsEnabled = false;
                    UpdateLiveTile(calendarItem, -1);
                }
                else
                {
                    calendarItem.PinIconOpacity = 0.3;
                    GetAppBarButton(AppResources.PinCalendarLivetile).IsEnabled = true;
                }
            }
        }

        private void CreateWeatherLivetileImage()
        {
            LivetileTemplateItem weatherItem = LivetileSelector.ItemsSource[0] as LivetileTemplateItem;
            //날씨 데이타
            LivetileData data = new LivetileData()
            {
                LiveWeather = SettingHelper.Get(Constants.WEATHER_LIVE_RESULT) as LiveWeather,
                Forecasts = SettingHelper.Get(Constants.WEATHER_FORECAST_RESULT) as Forecasts
            };

            //날씨 이미지 로드
            LivetileHelper.CreateLivetileImage(data, LiveItems.Weather);
            LoadLivetileImage(weatherItem);
            weatherItem.Visibility = System.Windows.Visibility.Visible;
            if (ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("weather")) != null)
            {
                weatherItem.PinIconOpacity = 1;
                GetAppBarButton(AppResources.PinWeatherLivetile).IsEnabled = false;
                UpdateLiveTile(weatherItem, 0);
            }
            else
            {
                weatherItem.PinIconOpacity = 0.3;
                GetAppBarButton(AppResources.PinWeatherLivetile).IsEnabled = true;
            }
        }

        private void CreateBatteryLivetileImage()
        {
            LivetileTemplateItem batteryItem = LivetileSelector.ItemsSource[2] as LivetileTemplateItem;
            batteryItem.Background = new SolidColorBrush((SettingHelper.Get(Constants.LIVETILE_BATTERY_BACKGROUND_COLOR) as ColorItem).Color);
            batteryItem.BackTitle = AppResources.ApplicationTitle;

            LivetileData data = new LivetileData();
            //배터리 이미지 로드
            LivetileHelper.CreateLivetileImage(data, LiveItems.Battery);
            LoadLivetileImage(batteryItem);
            //배터리 백타일 내용
            batteryItem.LiveBackTileContent = LivetileHelper.GetBatteryBackTextContent();
            batteryItem.Visibility = System.Windows.Visibility.Visible;
            if (ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("battery")) != null)
            {
                batteryItem.PinIconOpacity = 1;
                GetAppBarButton(AppResources.PinBatteryLivetile).IsEnabled = false;
                UpdateLiveTile(batteryItem, 0);
            }
            else
            {
                batteryItem.PinIconOpacity = 0.3;
                GetAppBarButton(AppResources.PinBatteryLivetile).IsEnabled = true;
            }
        }

        private void LoadAllLivetile()
        {
            CreateCalendarLivetileImage();
            CreateWeatherLivetileImage();
            CreateBatteryLivetileImage();
            //스케줄러에 변경됨을 통지
            SetSchedulerChagendTime(true);
        }

        private ApplicationBarIconButton GetAppBarButton(string buttonText)
        {
            foreach (ApplicationBarIconButton button in IAppBarLivetile.Buttons)
            {
                if (button.Text == buttonText)
                {
                    return button;
                }
            }
            return null;
        }

        #region Pin Event

        //리스트의 좌측 핀 아이콘 탭 이벤트 핸들러
        private void LivetilePin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            LivetileTemplateItem templateItem = (sender as FrameworkElement).DataContext as LivetileTemplateItem;
            if (IsEnabledButton(templateItem.LiveItem))
            {
                if (MessageBox.Show(string.Format(AppResources.MsgPinLivetile, GetLivetileTitle(templateItem)), AppResources.Confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    PinLivetile(templateItem.LiveItem);
                }
            }
            else
            {
                if (MessageBox.Show(string.Format(AppResources.MsgUnPinLivetile, GetLivetileTitle(templateItem)), AppResources.Confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    UnPinLivetile(templateItem);
                }
            }
        }

        //리스트의 타일 아이콘 탭 이벤트 핸들러
        private void LivetileItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            LivetileTemplateItem templateItem = (sender as FrameworkElement).DataContext as LivetileTemplateItem;
            switch(templateItem.LiveItem)
            {
                case LiveItems.Weather:
                    if (SettingHelper.Get(Constants.WEATHER_MAIN_CITY) == null)
                    {
                        NavigationService.Navigate(new Uri(string.Format("/View/SearchCityPage.xaml?pi={0}", PILivetile.Name), UriKind.Relative));
                    }
                    else
                    { 
                        PanoramaMainView.DefaultItem = PIWeather;
                    }
                    break;
                case LiveItems.Calendar:
                    PanoramaMainView.DefaultItem = PICalendar;
                    break;
                case LiveItems.Battery:
                    NavigationService.Navigate(new Uri("/View/FlashlightPage.xaml", UriKind.Relative));
                    break;
            }
        }

        public void CreateCalendarLivetile(int appointCount)
        {
            Uri tileUri = new Uri("/View/MainPage.xaml?pi=PICalendar&tile=calendar", UriKind.Relative);
            ShellTile.Create(tileUri, LivetileHelper.GetCalendarLivetileData(appointCount), false);
        }

        private void PinWeatherLivetile()
        {
            CreateWeatherLivetile();
            DisableItem(LiveItems.Weather);
        }

        private void PinCalendarLivetile()
        {
            if ((bool)SettingHelper.Get(Constants.CALENDAR_SHOW_APPOINTMENT))
            {
                //일정이 몇개 있는지를 백타일에 표시
                Appointments ap = new Appointments();
                ap.SearchCompleted += (s, se) =>
                {
                    CreateCalendarLivetile(se.Results.Count());
                    DisableItem(LiveItems.Calendar);
                };
                ap.SearchAsync(DateTime.Today, DateTime.Today.AddDays(1), null);
            }
            else
            {
                //백타일에 일정 표시하지 않음
                CreateCalendarLivetile(-1);
                DisableItem(LiveItems.Calendar);
            }
        }

        private void PinBatteryLivetile()
        {
            CreateBatteryLivetile();
            DisableItem(LiveItems.Battery);
        }

        private void PinLivetile(LiveItems item)
        {
            if (!ExistsActiveTile && Microsoft.Phone.Scheduler.ScheduledActionService.Find(Constants.PERIODIC_TASK_NAME) == null)
            {
                StartPeriodicAgent();
            }

            switch (item)
            {
                case LiveItems.Weather:
                    PinWeatherLivetile();
                    break;
                case LiveItems.Calendar:
                    PinCalendarLivetile();
                    break;
                case LiveItems.Battery:
                    PinBatteryLivetile();
                    break;
            }
        }

        private string GetLivetileTitle(LivetileTemplateItem item)
        {
            string title = string.Empty;
            switch (item.LiveItem)
            {
                case LiveItems.Weather:
                    title = AppResources.PinWeatherLivetile;
                    break;
                case LiveItems.Calendar:
                    title = AppResources.PinCalendarLivetile;
                    break;
                case LiveItems.Battery:
                    title = string.Format("{0} + {1}", AppResources.Flashlight, AppResources.PinBatteryLivetile);
                    break;
            }
            return title;
        }

        private void UnPinLivetile(LivetileTemplateItem item)
        {
            ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(item.LiveItem.ToString().ToLower()));
            if (oTile != null)
            {
                oTile.Delete();
                item.PinIconOpacity = 0.3;
                string buttonText = AppResources.PinCalendarLivetile;

                if (item.LiveItem == LiveItems.Weather)
                {
                    buttonText = AppResources.PinWeatherLivetile;
                }
                else if (item.LiveItem == LiveItems.Battery)
                {
                    buttonText = AppResources.PinBatteryLivetile;
                }

                foreach (ApplicationBarIconButton button in IAppBarLivetile.Buttons)
                {
                    if (button.Text == buttonText)
                    {
                        button.IsEnabled = true;
                        break;
                    }
                }
            }

            if (!ExistsActiveTile && !(bool)UseLockscreen.IsChecked)
            {
                RemoveAgent(Constants.PERIODIC_TASK_NAME);
            }
        }

        private void pinCalendarAppBarIconBtn_Click(object sender, EventArgs e)
        {
            if (IsEnabledButton(LiveItems.Calendar))
            {
                PinLivetile(LiveItems.Calendar);
            }
        }

        public static void CreateWeatherLivetile()
        {
            Uri tileUri = new Uri("/View/MainPage.xaml?pi=PIWeather&tile=weather", UriKind.Relative);
            ShellTile.Create(tileUri, LivetileHelper.GetWeatherLivetileData(), false);
        }

        private void pinWeatherAppBarIconBtn_Click(object sender, EventArgs e)
        {
            if (IsEnabledButton(LiveItems.Weather))
            {
                PinLivetile(LiveItems.Weather);
            }
        }

        public static void CreateBatteryLivetile()
        {
            //Uri tileUri = new Uri("/View/MainPage.xaml?pi=PILivetile&tile=battery", UriKind.Relative);
            Uri tileUri = new Uri("/View/FlashlightPage.xaml?pi=PILivetile&tile=battery", UriKind.Relative);
            ShellTile.Create(tileUri, LivetileHelper.GetBatteryLivetileData(), false);
        }

        private void pinBatteryAppBarIconBtn_Click(object sender, EventArgs e)
        {
            //만약에 라도... 이미 추가된 상태에서 눌리면 처리하지 않음.
            if (IsEnabledButton(LiveItems.Battery))
            {
                PinLivetile(LiveItems.Battery);
            }
        }
        #endregion

        #region Common method
        private bool IsEnabledButton(LiveItems item)
        {
            foreach (LivetileTemplateItem lti in LivetileSelector.ItemsSource as List<LivetileTemplateItem>)
            {
                if (lti.LiveItem == item)
                {
                    return lti.PinIconOpacity != 1;
                }
            }
            return false;
        }

        private void DisableItem(LiveItems item)
        {
            foreach (LivetileTemplateItem lti in LivetileSelector.ItemsSource as List<LivetileTemplateItem>)
            {
                if (lti.LiveItem == item)
                {
                    lti.PinIconOpacity = 1;
                }
            }

            foreach (ApplicationBarIconButton button in IAppBarLivetile.Buttons.AsQueryable())
            {
                if ((item == LiveItems.Weather && button.Text == AppResources.PinWeatherLivetile) 
                    || (item == LiveItems.Calendar && button.Text == AppResources.PinCalendarLivetile)
                    || (item == LiveItems.Battery && button.Text == AppResources.PinBatteryLivetile))
                {
                    button.IsEnabled = false;
                }
            }
        }

        /*
         * 타일을 업데이트 한다.
         */ 
        private void UpdateLiveTile(LivetileTemplateItem item, int appCount)
        {
            ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(item.LiveItem.ToString().ToLower().ToString()));
            if (oTile != null)
            {
                if (item.LiveItem == LiveItems.Weather)
                {
                    oTile.Update(LivetileHelper.GetWeatherLivetileData());
                }
                else if (item.LiveItem == LiveItems.Battery)
                {
                    oTile.Update(LivetileHelper.GetBatteryLivetileData());
                }
                else
                {
                    oTile.Update(LivetileHelper.GetCalendarLivetileData(appCount));
                }
            }
        }
        #endregion 
    }
}
