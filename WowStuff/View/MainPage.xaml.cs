using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Linq;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Helper;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using ChameleonLib.Storages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Info;
using System.Reflection;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Scheduler;

namespace Chameleon
{
    public partial class MainPage : PhoneApplicationPage
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        private Storyboard gradientStopAnimationStoryboard;
        // 생성자
        public MainPage()
        {
            InitializeComponent();

            //업그레이드 관련 작업
            UpgradeVersion();

            DateTime LastRunForLivetile, LastRunForLockscreen;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<DateTime>(Constants.LAST_RUN_LIVETILE, out LastRunForLivetile);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<DateTime>(Constants.LAST_RUN_LOCKSCREEN, out LastRunForLockscreen);
            
            //스케줄러 복구 모드
            scheduleSettings = MutexedIsoStorageFile.Read<ScheduleSettings>("ScheduleSettings", Constants.MUTEX_DATA);
            if (DateTime.Now.Subtract(LastRunForLivetile).TotalMinutes > scheduleSettings.LivetileUpdateInterval + 60 //원래 업데이트 시간보다 60분 초과
                && DateTime.Now.Subtract(LastRunForLockscreen).TotalMinutes > scheduleSettings.LockscreenUpdateInterval + 60)
            {
                //스케줄러 초기화
                RemoveAgent(Constants.PERIODIC_TASK_NAME);
            }

            //위치 고정
            InitializeSystemTray();
            //보호색 설정
            InitializeProtectiveColor();
            
            //락스크린 초기화
            MainPageLockscreen();
            //라이브타일 초기화
            MainPageLivetile();
            //날씨 초기화
            MainPageWeather();
            //달력 초기화
            MainPageCalendar();

            //앱바 기본설정 설정
            BuildLocalizedApplicationBar();

#if(DEBUG_AGENT)
            RemoveAgent(Constants.PERIODIC_TASK_NAME);
            StartPeriodicAgent();
            ScheduledActionService.LaunchForTest(Constants.PERIODIC_TASK_NAME, TimeSpan.FromSeconds(30));
            //MessageBox.Show("스케줄러가 강제 시작됨");
#endif
        }

        private void UpgradeVersion()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("MigratedScheduler") &&  ScheduledActionService.Find("PeriodicAgent") as PeriodicTask != null)
            {
                //1.0.x대의 스케쥴러 삭제
                RemoveAgent("PeriodicAgent");
                IsolatedStorageSettings.ApplicationSettings["MigratedScheduler"] = true;
                //새로운 스케쥴러 등록
                StartPeriodicAgent();
            }

            //2.4 버전 색상값 변경 관련
            if (SettingHelper.ContainsKey(Constants.LOCKSCREEN_BACKGROUND_COLOR))
            {
                ColorItem colorItem = SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_COLOR) as ColorItem;
                if (colorItem.Color == Colors.Transparent)
                {
                    SettingHelper.Set(Constants.LOCKSCREEN_BACKGROUND_COLOR, new ColorItem()
                    {
                        Text = AppResources.ColorChrome,
                        Color = ColorItem.ConvertColor(0xFF1F1F1F)
                    }, false);
                    SettingHelper.Set(Constants.LOCKSCREEN_BACKGROUND_OPACITY, 0, false);
                    SettingHelper.Save();
                }
            }
        }

        private void InitializeSystemTray()
        {
            ProgressIndicator prog = new ProgressIndicator();
            prog.IsIndeterminate = true;
            prog.IsVisible = false;

            SystemTray.SetOpacity(this, 0);
            SystemTray.SetProgressIndicator(this, prog);
            SystemTray.SetIsVisible(this, false);
        }

        private void InitializeProtectiveColor()
        {
            //배경 색상 브러스 및 카멜레온 효과 스토리보드 생성
            LinearGradientBrush gradientBrush = new LinearGradientBrush() { EndPoint = new Point(0.5, 0), StartPoint = new Point(0.5, 1) };
            bool useProtectiveColor = (bool)SettingHelper.Get(Constants.CHAMELEON_USE_PROTECTIVE_COLOR);
            Color beginColor;
            
            if (useProtectiveColor)
            {
                int index = new Random().Next(0, ColorItem.UintColors.Length - 1);
                beginColor = ColorItem.ConvertColor(ColorItem.UintColors[index]);
            }
            else
            {
                beginColor = (SettingHelper.Get(Constants.CHAMELEON_SKIN_BACKGROUND_COLOR) as ColorItem).Color;
            }

            double rc = 0.1;
            GradientStop stop1 = new GradientStop() { Color = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible ?
                Color.FromArgb((byte)(beginColor.A * 0.8), (byte)(beginColor.R * rc), (byte)(beginColor.G * rc), (byte)(beginColor.B * rc)) : Colors.White, Offset = 1
            };
            //GradientStop stop2 = new GradientStop() { Color = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible ? Colors.Black : Colors.White, Offset = 0 };
            GradientStop stop2 = new GradientStop() { Color = beginColor, Offset = 0 };


            //GradientStop stop1 = new GradientStop() { Color = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible ? Colors.Black : Colors.White, Offset = 0 };
            //GradientStop stop2 = new GradientStop() { Color = initColor, Offset = 1 };

            gradientBrush.GradientStops.Add(stop1);
            gradientBrush.GradientStops.Add(stop2);

            PanoramaMainView.Background = gradientBrush;
            /*
            //
            // Animate the first gradient stop's offset from
            // 0.0 to 1.0 and then back to 0.0.
            //
            DoubleAnimation offsetAnimation = new DoubleAnimation();
            offsetAnimation.From = 0.0;
            offsetAnimation.To = 0.85;
            offsetAnimation.Duration = TimeSpan.FromSeconds(1.0);
            offsetAnimation.AutoReverse = true;
            Storyboard.SetTarget(offsetAnimation, stop1);
            Storyboard.SetTargetProperty(offsetAnimation,
                new PropertyPath(GradientStop.OffsetProperty));
            */
            //
            // Animate the second gradient stop's color from
            // Purple to Yellow and then back to Purple.
            //
            TimeSpan duration = TimeSpan.FromSeconds(2.5);
            ColorAnimation gradientStopColorAnimation = new ColorAnimation();
            //gradientStopColorAnimation.From = Colors.Green;
            gradientStopColorAnimation.To = Colors.Yellow;
            gradientStopColorAnimation.Duration = duration;
            //gradientStopColorAnimation.AutoReverse = true;
            Storyboard.SetTarget(gradientStopColorAnimation, stop2);
            Storyboard.SetTargetProperty(gradientStopColorAnimation,
                new PropertyPath(GradientStop.ColorProperty));
            
            // Set the animation to begin after the first animation
            // ends.
            gradientStopColorAnimation.BeginTime = TimeSpan.FromSeconds(0.5);

            // Create a Storyboard to apply the animations.
            gradientStopAnimationStoryboard = new Storyboard();
            if (useProtectiveColor)
            {
                gradientStopAnimationStoryboard.Children.Add(gradientStopColorAnimation);
            }
            else
            {
                //gradientStopAnimationStoryboard.Children.Add(offsetAnimation);
            }

            PanoramaMainView.SelectionChanged += (s, e) =>
            {
                //colorStoryboard.Begin();

                //날씨 설명 애니메이션 정지
                HideForecastDescriptionMarquee();

                String panoramaName = (e.AddedItems[0] as PanoramaItem).Name;

                if (panoramaName == PILivetile.Name)
                {
                    ApplicationBar = IAppBarLivetile;
                    ApplicationBar.IsVisible = true;
                }
                else if (panoramaName == PILockscreen.Name)
                {
                    ApplicationBar = IAppBarLockscreen;
                    ApplicationBar.IsVisible = true;

                    if (LockscreenSelector.ItemsSource != null)
                    {
                        foreach (PhonePicture pic in LockscreenSelector.ItemsSource as ChameleonAlbum)
                        {
                            if (pic.CurrentLockscreen != null)
                            {
                                LockscreenSelector.ScrollTo(pic);
                                break;
                            }
                        }
                    }
                }
                else if (panoramaName == PIWeather.Name)
                {
                    ApplicationBar = iAppBarWeather;
                    ApplicationBar.IsVisible = true;
                }
                else if (panoramaName == PICalendar.Name)
                {
                    ApplicationBar = IAppBarCalendar;
                    ApplicationBar.IsVisible = true;
                }
                else
                {
                    ApplicationBar = null;
                }           

                Color newColor;

                if ((bool)SettingHelper.Get(Constants.CHAMELEON_USE_PROTECTIVE_COLOR))
                {
                    ColorAnimation protectiveColorAnimation = gradientStopAnimationStoryboard.Children[0] as ColorAnimation;

                    if (protectiveColorAnimation == null)
                    {
                        gradientStopAnimationStoryboard.Stop();
                        gradientStopAnimationStoryboard.Children.Clear();
                        gradientStopAnimationStoryboard.Children.Add(gradientStopColorAnimation);
                        protectiveColorAnimation = gradientStopColorAnimation;
                    }

                    //계속 다른 색상을 생성
                    do
                    {
                        int index = new Random().Next(0, ColorItem.UintColors.Length);
                        newColor = ColorItem.ConvertColor(ColorItem.UintColors[index]);
                    } while (newColor.Equals(protectiveColorAnimation.To));
                    //System.Diagnostics.Debug.WriteLine("random : " + newColor.ToString());
                    protectiveColorAnimation.To = newColor;
                    gradientStopAnimationStoryboard.Begin();
                }
                else
                {
                    newColor = (SettingHelper.Get(Constants.CHAMELEON_SKIN_BACKGROUND_COLOR) as ColorItem).Color;
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
            };

            //gradientStopColorAnimation.Completed += (s, e) =>
            //{
            //    Color color = ApplicationBar.BackgroundColor;
            //    ApplicationBar.BackgroundColor = Color.FromArgb(255, color.R, color.G, color.B);
            //};
        }

        private void ChangeBackgroundColor(Color color)
        {
            GradientBrush gradientBrush = PanoramaMainView.Background as GradientBrush;
            GradientStop stop2 = gradientBrush.GradientStops[1] as GradientStop;

            if (!stop2.Color.Equals(color))
            {
                stop2.Color = color;
            }
        }

        private void SetAppbarColor(IApplicationBar appBar, Color color)
        {
            if (appBar != null && appBar.IsVisible)
            {
                double rc = 0.5;
                if ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible)
                {
                    appBar.BackgroundColor = Color.FromArgb((byte)(color.A * 0.8), (byte)(color.R * rc), (byte)(color.G * rc), (byte)(color.B * rc));
                }
                else
                {
                    appBar.BackgroundColor = Color.FromArgb((byte)(color.A * 0.5), 255, 255, 255);
                }
            }
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = IAppBarLivetile;
            ApplicationBar.IsVisible = true;
        }

        // ViewModel 항목에 대한 데이터를 로드합니다.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string from;
            NavigationContext.QueryString.TryGetValue("from", out from);
            if (from == "flashlight")
            {
                NavigationService.RemoveBackEntry();
            }

            //네비게이션 모드 
            bool isChangedSettings = e.NavigationMode == NavigationMode.Back && PhoneApplicationService.Current.State.ContainsKey(Constants.CHANGED_SETTINGS);
            //스케쥴 데이터 로드
            scheduleSettings = MutexedIsoStorageFile.Read<ScheduleSettings>("ScheduleSettings", Constants.MUTEX_DATA);
            
            if (e.NavigationMode == NavigationMode.New)
            {
                //달력 환경 설정 로딩
                LoadConfigCalendar();
                //라이브타일 로드
                LoadLivetile();
                //락스크린 리스트 로드
                LoadLockscreenList();
                //날씨 로드(비동기 처리로..)
                LoadLiveWeather();
                //달력 로드
                LoadCalendar(DateTime.Now);
            }
            else
            {
                if (PanoramaMainView != null && PanoramaMainView.SelectedItem != null)
                {
                    string panoramaName = (PanoramaMainView.SelectedItem as PanoramaItem).Name;

                    if (panoramaName == PILockscreen.Name)
                    {
                        //락스크린 리스트 로드
                        LoadLockscreenList();
                    }
                    else if (panoramaName == PIWeather.Name)
                    {
                        //if (!PhoneApplicationService.Current.State.ContainsKey(Constants.WEATHER_MAIN_CITY))
                        //{
                        //    PhoneApplicationService.Current.State[Constants.WEATHER_UNIT_TYPE] = true;
                        //}
                        //날씨 조회 지역을 찾아서 선택하고 들어온 경우는 날씨를 업데이트 해주고 락스크린을 갱신
                        if (PhoneApplicationService.Current.State.ContainsKey(Constants.WEATHER_MAIN_CITY))
                        {
                            WeatherCity city = PhoneApplicationService.Current.State[Constants.WEATHER_MAIN_CITY] as WeatherCity;
                            //위의 설정값은 RetriveWeather에서도 사용되며, 콜백 메소드에서 사용되므로 해당 콜백 메소드에서 지워야 함.
                            RetrieveWeather(city);
                        }
                    }
                    else if (panoramaName == PICalendar.Name)
                    {
                        //달력 환경 설정 로딩
                        LoadConfigCalendar();
                        //등록후 재검색..
                        LoadCalendar(CurrentCalendar);
                        //락스크린 갱신되도록 설정
                        PhoneApplicationService.Current.State[Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR] = true;
                    }
                }
            }

            string lockscreenKey = "WallpaperSettings";
            string piName = string.Empty;

            if (NavigationContext.QueryString.TryGetValue(lockscreenKey, out piName))
            {
                //잠금화면 설정에서 진입했는가를 체크
                piName = PILockscreen.Name;
            }
            else
            {
                //라이브 타일을 눌러서 진입했는가를 체크
                NavigationContext.QueryString.TryGetValue("pi", out piName);
                NavigationContext.QueryString.Remove("pi");
            }

            if (!string.IsNullOrEmpty(piName))
            {
                //해당 메뉴로 이동 시킴
                foreach (PanoramaItem pi in PanoramaMainView.Items)
                {
                    if (pi.Name == piName)
                    {
                        PanoramaMainView.DefaultItem = pi;

                        if (piName == PILivetile.Name)
                        {
                            ApplicationBar = IAppBarLivetile;
                            ApplicationBar.IsVisible = true;
                        }
                        else if (piName == PILockscreen.Name)
                        {
                            ApplicationBar = IAppBarLockscreen;
                            ApplicationBar.IsVisible = true;
                        }
                        else if (piName == PIWeather.Name)
                        {
                            ApplicationBar = iAppBarWeather;
                            ApplicationBar.IsVisible = true;
                            PhoneApplicationService.Current.State[Constants.WEATHER_UNIT_TYPE] = true;
                        }
                        else if (piName == PICalendar.Name)
                        {
                            ApplicationBar = IAppBarCalendar;
                            ApplicationBar.IsVisible = true;
                        }

                        break;
                    }
                }
            }

            bool isLoadedBatteryTile = false;
            bool isLoadedWeatherTile = false;
            bool isLoadedCalendarTile = false;

            if (ExistStatus(Constants.LIVETILE_RANDOM_BACKGROUND_COLOR))
            {
                //라이브타일 전체 다시 로드
                CreateCalendarLivetileImage();
                CreateWeatherLivetileImage();
                CreateBatteryLivetileImage();
                //타일 로딩 여부
                isLoadedCalendarTile = true;
                isLoadedBatteryTile = true;
            }
            else
            {
                if (ExistStatus(Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR))
                {
                    //달력 라이브타일 다시 로드
                    CreateCalendarLivetileImage();
                    isLoadedCalendarTile = true;
                }
                if (ExistStatus(Constants.LIVETILE_WEATHER_BACKGROUND_COLOR))
                {
                    //날씨 라이브타일 다시 로드
                    CreateWeatherLivetileImage();
                    isLoadedWeatherTile = true;
                }
                if (ExistStatus(Constants.LIVETILE_BATTERY_BACKGROUND_COLOR))
                {
                    //배터리 라이브타일 다시 로드
                    CreateBatteryLivetileImage();
                    isLoadedBatteryTile = true;
                }
            }

            if (ExistStatus(Constants.LIVETILE_FONT_WEIGHT))
            {
                if (!isLoadedWeatherTile)
                {
                    //날씨 라이브타일 다시 로드
                    CreateWeatherLivetileImage();
                }
                if (!isLoadedCalendarTile) 
                {
                    //달력 라이브타일 다시 로드
                    CreateCalendarLivetileImage();
                }
            }

            if (ExistStatus(Constants.LIVETILE_WEATHER_FONT_SIZE) && !isLoadedWeatherTile)
            {
                //날씨 라이브타일 다시 로드
                CreateWeatherLivetileImage();
            }

            if (ExistStatus(Constants.LIVETILE_BATTERY_FULL_INDICATION) && !isLoadedBatteryTile)
            {
                //배터리 라이브타일 다시 로드
                CreateBatteryLivetileImage();
            }

            if (ExistStatus(Constants.WEATHER_UNIT_TYPE) || ExistStatus(Constants.WEATHER_ICON_TYPE))
            {
                //날시 아이콘 다시 로딩
                if (ExistStatus(Constants.WEATHER_ICON_TYPE))
                {
                    WeatherIconMap.Instance.Load((WeatherIconType)SettingHelper.Get(Constants.WEATHER_ICON_TYPE));
                }
                //날씨 갱신 (무조건)
                RefreshLiveWeather();
            }

            if (ExistStatus(Constants.CHAMELEON_USE_PROTECTIVE_COLOR))
            {
                if (!(bool)SettingHelper.Get(Constants.CHAMELEON_USE_PROTECTIVE_COLOR))
                {
                    //바탕화면 색상 갱신
                    Color color = (SettingHelper.Get(Constants.CHAMELEON_SKIN_BACKGROUND_COLOR) as ColorItem).Color;
                    ChangeBackgroundColor(color);
                    SetAppbarColor(ApplicationBar, color);
                }
            }
            if (ExistStatus(Constants.CALENDAR_FIRST_DAY_OF_WEEK) || ExistStatus(Constants.CALENDAR_SHOW_APPOINTMENT))
            {
                LoadConfigCalendar();
                LoadCalendar(CurrentCalendar);
                //달력 타일 로드
                if (!isLoadedCalendarTile)
                {
                    CreateCalendarLivetileImage();
                }
            }
            
            PhoneApplicationService.Current.State.Remove(Constants.CHANGED_SETTINGS);
            PhoneApplicationService.Current.State.Remove(Constants.CHAMELEON_USE_PROTECTIVE_COLOR);
            PhoneApplicationService.Current.State.Remove(Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR);
            PhoneApplicationService.Current.State.Remove(Constants.LIVETILE_WEATHER_BACKGROUND_COLOR);
            PhoneApplicationService.Current.State.Remove(Constants.LIVETILE_BATTERY_BACKGROUND_COLOR);
            PhoneApplicationService.Current.State.Remove(Constants.LIVETILE_WEATHER_FONT_SIZE);
            PhoneApplicationService.Current.State.Remove(Constants.LIVETILE_BATTERY_FULL_INDICATION);
            PhoneApplicationService.Current.State.Remove(Constants.WEATHER_UNIT_TYPE);
            PhoneApplicationService.Current.State.Remove(Constants.WEATHER_ICON_TYPE);
            PhoneApplicationService.Current.State.Remove(Constants.CALENDAR_FIRST_DAY_OF_WEEK);
            PhoneApplicationService.Current.State.Remove(Constants.CALENDAR_SHOW_APPOINTMENT);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            string panoramaName = (PanoramaMainView.SelectedItem as PanoramaItem).Name;

            if (panoramaName == PILockscreen.Name)
            {
                if (IAppBarLockscreen.Buttons.Count > 1)
                {
                    base.OnBackKeyPress(e);
                }
                else if (LockscreenSelector.SelectedItems.Count == 0)
                {
                    ChangeAppbarIconButtons(false);
                    LockscreenSelector.EnforceIsSelectionEnabled = false;
                    e.Cancel = true;
                }
                else
                {
                    while(LockscreenSelector.SelectedItems.Count > 0)
                    {
                        LockscreenSelector.SelectedItems.RemoveAt(0);
                    }
                    e.Cancel = true;
                }
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
        
        private void OnTapPanoramaTitle(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigateSettingPage();
        }

        private void NavigateSettingPage()
        {
            PanoramaItem pi = PanoramaMainView.SelectedItem as PanoramaItem ;
            NavigationService.Navigate(new Uri(string.Format("/View/SettingPage.xaml?piName={0}", pi.Name), UriKind.Relative));
        }

        private void CopyMenuItem(IList menuItemList)
        {
            ApplicationBarMenuItem aboutMi = new ApplicationBarMenuItem(AppResources.ApplicationAbout);
            aboutMi.Click += aboutMi_Click;

            ApplicationBarMenuItem appListMi = new ApplicationBarMenuItem(AppResources.ManufacturerApps);
            appListMi.Click += appListMi_Click;

            menuItemList.Add(aboutMi);
            menuItemList.Add(appListMi);

            if (App.IsTrial)
            {
                ApplicationBarMenuItem buyAppMI = new ApplicationBarMenuItem(AppResources.BuyApp);
                buyAppMI.Click += buyAppMI_Click;
                menuItemList.Add(buyAppMI);
            }

            ApplicationBarMenuItem privacyPolicyMi = new ApplicationBarMenuItem(AppResources.PrivacyPolicy);
            privacyPolicyMi.Click += privacyPolicyMi_Click;
            menuItemList.Add(privacyPolicyMi);
        }

        private void privacyPolicyMi_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/PrivacyPolicyPage.xaml", UriKind.Relative));
        }

        void aboutMi_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/AboutPage.xaml", UriKind.Relative));
        }

        void appListMi_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/AppListPage.xaml", UriKind.Relative));
        }

        void buyAppMI_Click(object sender, EventArgs e)
        {
            new MarketplaceDetailTask().Show();
        }

        private void OnOpendLockscreenContext(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            Grid imgGrid = (sender as ContextMenu).Owner as Grid;

            double posY = App.Current.Host.Content.ActualHeight - (imgGrid.TransformToVisual(LayoutRoot).Transform(new Point()).Y + imgGrid.ActualHeight + menu.ActualHeight);

            if (posY > 0 && posY < 72) //72는 어플리케이션바 높이
            {
                //menu.VerticalOffset = -(menu.ActualHeight + imgGrid.ActualHeight);
                menu.VerticalOffset = -imgGrid.ActualHeight/2;
            }
            else
            {
                menu.VerticalOffset = 0;
            }
        }

        private void PIHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UIElementCollection elems = (sender as Grid).Children;
            foreach (UIElement elem in elems)
            {
                elem.Opacity = 0.5;
            }
        }

        private void PIHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            UIElementCollection elems = (sender as Grid).Children;
            foreach (UIElement elem in elems)
            {
                elem.Opacity = 1;
            }
        }

        private void PIHeader_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string name = (sender as Grid).Name;
            string imgName = string.Empty;

            switch (name)
            {
                case "LivetileHeader":
                    LoadAllLivetile();
                    break;
                case "LockscreenHeader":
                    //락스크린 리스트 로드
                    LoadLockscreenList();
                    break;
                case "WeatherHeader":
                    RefreshLiveWeather();
                    break;
                case "CalendarHeader":
                    RefreshCalendar();
                    break;
            }

        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            string name = (sender as Image).Name;
            string imgName = string.Empty;

            switch (name)
            {
                case "LivetileHeaderIcon":
                    imgName = "sync.png";
                    break;
                case "WeatherHeaderIcon":
                    imgName = "weather.refresh.png";
                    break;
                case "CalendarHeaderIcon":
                    imgName = "calendar.refresh.png";
                    break;
            }

            if (!string.IsNullOrEmpty(imgName))
            {
                string path = PathHelper.GetFullPath(imgName);
                WriteableBitmap wb = BitmapFactory.New(0, 0).FromContent(path.Substring(1));
                (sender as Image).Source = wb;

            }
        }

        private bool ExistStatus(string key)
        {
            bool result = false;
            if (PhoneApplicationService.Current.State.ContainsKey(key))
            {
                result = (bool)PhoneApplicationService.Current.State[key];
            }
            return result;
        }

        private void SetSchedulerChagendTime(bool isLivetile)
        {
            SettingHelper.Set(isLivetile ? Constants.LAST_RUN_LIVETILE : Constants.LAST_RUN_LOCKSCREEN, DateTime.Now, true);
        }

        private bool ExistsActiveTile
        {
            get
            {
                return ShellTile.ActiveTiles.Any(x =>
                        x.NavigationUri.ToString().Contains(LiveItems.Weather.ToString().ToLower())
                        || x.NavigationUri.ToString().Contains(LiveItems.Calendar.ToString().ToLower())
                        || x.NavigationUri.ToString().Contains(LiveItems.Battery.ToString().ToLower())
                    );
            }
        }

        private void ShowLoadingPanel(string loadingText)
        {
            LoadingText.Text = loadingText;
            LoadingProgressBar.Visibility = System.Windows.Visibility.Visible;
            LoadingProgressBar.UseOptimizedManipulationRouting = false;
            ApplicationBar.IsVisible = false;
        }

        private void HideLoadingPanel()
        {
            LoadingText.Text = string.Empty;
            LoadingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            LoadingProgressBar.UseOptimizedManipulationRouting = true;
            ApplicationBar.IsVisible = true;
        }
    }
}