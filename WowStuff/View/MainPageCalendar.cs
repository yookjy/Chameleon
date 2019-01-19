using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ChameleonLib.Api.Calendar;
using ChameleonLib.Api.Calendar.Model;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace Chameleon
{
    public class Test
    {
        public string Day { get; set; }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        private string currentMonth;
        public string CurrentMonth
        {
            get
            {
                return currentMonth;
            }
            set
            {
                if (currentMonth != value)
                {
                    currentMonth = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private ApplicationBar IAppBarCalendar;

        //현재 달력에 쓰이는 datetime
        private DateTime CurrentCalendar;

        private bool IsShowAppointment;

        private Appointments appointments;

        private Uri appointmentPageUri;
        //날짜 선택 컨텍스트 메뉴 이벤트 처리 객체
        //public ContextMenuItemCommand AddApointmentCommand { get; set; }

        public void MainPageCalendar()
        {
            CreateCalendarAppBar();

            appointmentPageUri = new Uri("/View/AppointmentListPage.xaml", UriKind.Relative);
            appointments = new Appointments();
            appointments.SearchCompleted += appointments_SearchCompleted;

            BitmapImage bi = new BitmapImage();
            bi.UriSource = PathHelper.GetPath("calendar.refresh.png");
            BitmapImage bip = new BitmapImage();
            bip.UriSource = PathHelper.GetPath("calendar.refresh.pressed.png");

            CurrentMonth = DateTime.Now.Month.ToString();

            CalendarSelector.ManipulationDelta += CalendarSelector_ManipulationDelta;
            CalendarSelector.ManipulationCompleted += CalendarSelector_ManipulationCompleted;
        }

        public void LoadConfigCalendar()
        {
            //일정 표시 여부 캐싱
            IsShowAppointment = (bool)SettingHelper.Get(Constants.CALENDAR_SHOW_APPOINTMENT);
            //요일 헤더 로딩
            CalendarHeader.ItemsSource = VsCalendar.GetDayNames(false);
        }

        //앱바 생성
        private void CreateCalendarAppBar()
        {
            IAppBarCalendar = new ApplicationBar();
            IAppBarCalendar.Mode = ApplicationBarMode.Default;
            IAppBarCalendar.Opacity = 0.8;
            IAppBarCalendar.IsVisible = false;
            IAppBarCalendar.IsMenuEnabled = true;
            CopyMenuItem(IAppBarCalendar.MenuItems);

            ApplicationBarIconButton addAppBarIconBtn = new ApplicationBarIconButton();
            addAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.add.png");
            addAppBarIconBtn.Text = AppResources.AddAppointment;
            addAppBarIconBtn.Click += appAppointmentBarIconBtn_Click;

            ApplicationBarIconButton appBarIconBtnSettings = new ApplicationBarIconButton();
            appBarIconBtnSettings.IconUri = PathHelper.GetPath("appbar.settings.png");
            appBarIconBtnSettings.Text = AppResources.Settings;
            appBarIconBtnSettings.Click += settingMI_Click;

            IAppBarCalendar.Buttons.Add(addAppBarIconBtn);
            IAppBarCalendar.Buttons.Add(appBarIconBtnSettings);
        }

        void appAppointmentBarIconBtn_Click(object sender, EventArgs e)
        {
            Day day = CalendarSelector.SelectedItems[0] as Day;
            if (day != null)
            {
                SaveAppointmentTask saveAppointmentTask = new SaveAppointmentTask();
                saveAppointmentTask.StartTime = day.DateTime;
                saveAppointmentTask.EndTime = day.DateTime.AddHours(1);
                saveAppointmentTask.Show();
            }
        }

        void CalendarSelector_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            Point deltaPoint = e.TotalManipulation.Translation;

            if (Math.Abs(deltaPoint.X) < Math.Abs(deltaPoint.Y))
            {
                if (e.FinalVelocities.LinearVelocity.Y == 0)
                {
                    CalendarSelector.Opacity = 1;
                    return;
                }

                DateTime newDt;
                if (e.FinalVelocities.LinearVelocity.Y < 0)
                {
                    newDt = CurrentCalendar.AddMonths(1);
                    CurrentCalendar = new DateTime(1, 1, 1);
                }
                else 
                {
                    newDt = CurrentCalendar.AddMonths(-1);
                    CurrentCalendar = new DateTime(1, 1, 1);
                }

                Storyboard sb = GetFadeIn(CalendarSelector);
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, be) =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        LoadCalendar(newDt);
                        sb.SkipToFill();
                    });
                };
                worker.RunWorkerAsync();
                sb.Begin();
            }
        }

        public Storyboard GetFadeIn(FrameworkElement obj)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimationUsingKeyFrames timeline = new DoubleAnimationUsingKeyFrames();
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0)), Value = obj.Opacity });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000)), Value = 1 });
            sb.Children.Add(timeline);

            Storyboard.SetTarget(timeline, obj);
            Storyboard.SetTargetProperty(timeline, new PropertyPath("Opacity"));
            return sb;
        }

        void CalendarSelector_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation == null)
            {
                CalendarSelector.Opacity -= Math.Abs(e.DeltaManipulation.Translation.Y) / CalendarSelector.ActualHeight * 1.5;

                if (CalendarSelector.Opacity < 0.15)
                {
                    CalendarSelector.Opacity = 0.15;
                }
            }
        }
        
        void appointments_SearchCompleted(object sender, AppointmentsSearchEventArgs e)
        {
            Dictionary<string, object> state = e.State as Dictionary<string, object>;
            List<Day> dayList = state["dayList"] as List<Day>;
            DateTime selectDay = (DateTime)state["selectDay"];

            //달력과 약속 데이터 합침
            dayList = VsCalendar.MergeCalendar(dayList, e.Results);
            CalendarSelector.ItemsSource = dayList;
            
            //날짜 선택
            IEnumerable<Day> days = from Day day in dayList
                                    where day.DateTime.ToLongDateString() == selectDay.ToLongDateString()
                                    select day;

            if (days.Any())
            {
                //오늘 날짜를 디폴트 선택값으로 설정
                CalendarSelector.SelectedItem = days.First();
                //오늘 날짜에 약속이 있으면 약속을 표시
                if (days.First().AppointmentList != null)
                {
                    IOrderedEnumerable<Appointment> appointments = days.First().AppointmentList.OrderBy(x => x.StartTime);
                    CalendarAppointmentSelector.ItemsSource = appointments.ToList();
                    CalendarAppointmentSelector.SelectedItem = days.First();

                    var item = appointments.Where(x =>
                    {
                        if (x.StartTime.CompareTo(DateTime.Now) <= 1 && x.EndTime.CompareTo(DateTime.Now) >= 1)
                        {
                            return true;
                        }
                        return false;
                    });
                    //현재 시간에 걸린 아이템으로 스크롤을 옮긴다.
                    if (item.Any() && item.Count() > 2)
                    {
                        CalendarAppointmentSelector.ScrollTo(item.First());
                    }
                }
            }
            else
            {
                CalendarAppointmentSelector.ItemsSource = null;
            }
        }

        public void LoadCalendar(DateTime dt)
        {
            //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            //선택할 날짜
            DateTime selectDt = CurrentCalendar;
            if (CurrentCalendar.Year == 1)
            {
                selectDt = DateTime.Now;
            }

            //이전값 복원
            if (CalendarSelector.SelectedItem != null)
            {
                selectDt = (CalendarSelector.SelectedItem as Day).DateTime;
            }

            CurrentCalendar = dt;
            PICalendar.Header = dt.ToString(DateTimeFormatInfo.CurrentInfo.YearMonthPattern.Replace("MMMM", "MMM"));

            int dim = DateTimeFormatInfo.CurrentInfo.Calendar.GetDaysInMonth(dt.Year, dt.Month);
            
            List<Day> dayList = VsCalendar.GetCalendarOfMonth(dt, new DateTime(), false, false);
            CalendarSelector.ItemsSource = dayList;
            
            if (IsShowAppointment)
            {
                Dictionary<string, object> state = new Dictionary<string, object>();
                state.Add("dayList", dayList);
                state.Add("selectDay", selectDt);

                appointments.SearchAsync(dayList[0].DateTime, dayList[dayList.Count - 1].DateTime, state);
            }
            
            //watch.Stop();
            //System.Diagnostics.Debug.WriteLine(watch.ElapsedMilliseconds);
        }

        private void RefreshCalendar()
        {
            if (CurrentCalendar.Year != DateTime.Now.Year || CurrentCalendar.Month != DateTime.Now.Month)
            {
                CurrentCalendar = new DateTime(1, 1, 1);
                LoadCalendar(DateTime.Now);
                //CalendarFadeIn.Begin();
                GetFadeIn(CalendarSelector).Begin();
            }
        }

        private void CalendarSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalendarSelector.SelectedItem == null)
            {
                CalendarAppointmentSelector.ItemsSource = null;
                return;
            }
            if (e.AddedItems.Count > 0)
            {
                Day selectedDay = e.AddedItems[0] as Day;
                if (selectedDay != null)
                {
                    selectedDay.FontWeight = FontWeights.Bold;
                    selectedDay.ForegroundBrush = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
                    CalendarAppointmentSelector.ItemsSource = selectedDay.AppointmentList;
                }
            }

            if (e.RemovedItems.Count > 0)
            {
                Day deselectedDay = e.RemovedItems[0] as Day;
                if (deselectedDay != null)
                {
                    deselectedDay.FontWeight = FontWeights.Normal;
                    /*
                    bool useSundayColor = true;
                    bool useSaturdayColor = true;

                    if (useSundayColor && deselectedDay.DateTime.DayOfWeek == DayOfWeek.Sunday)
                    {
                        //일요일 색상 원복
                        deselectedDay.ForegroundBrush = new SolidColorBrush(Colors.Red);
                    }
                    else if (useSaturdayColor && deselectedDay.DateTime.DayOfWeek == DayOfWeek.Saturday)
                    {
                        //토요일 색상 원복
                        deselectedDay.ForegroundBrush = new SolidColorBrush(Colors.Blue);
                    }
                    else
                     */ 
                    {
                        if (deselectedDay.DateTime.ToLongDateString() != DateTime.Today.ToLongDateString())
                        {
                            if ((CalendarSelector.ItemsSource as List<Day>)[(CalendarSelector.ItemsSource as List<Day>).Count / 2].DateTime.Month != deselectedDay.DateTime.Month)
                            {
                                deselectedDay.ForegroundBrush = App.Current.Resources["PhoneSubtleBrush"] as SolidColorBrush;
                            }
                            else
                            {
                                deselectedDay.ForegroundBrush = App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
                            }
                        }
                        else
                        {
                            SolidColorBrush foregroundBrush = (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
                            //deselectedDay.ForegroundBrush = App.Current.Resources["PhoneSubtleBrush"] as SolidColorBrush;
                            deselectedDay.ForegroundBrush = new SolidColorBrush(
                               Color.FromArgb((byte)(foregroundBrush.Color.A * 0.6), (byte)(foregroundBrush.Color.R * 0.6),
                               (byte)(foregroundBrush.Color.G * 0.6), (byte)(foregroundBrush.Color.B * 0.6)));
                        }
                    }
                }
            }
        }
    }

}
