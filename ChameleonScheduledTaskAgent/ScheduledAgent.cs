using ChameleonLib.Api.Calendar;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Helper;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using ChameleonLib.Storages;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Phone.System.UserProfile;

namespace ChameleonScheduledTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private ScheduleSettings scheduleSettings;

        /// <remarks>
        /// ScheduledAgent 생성자는 UnhandledException 처리기를 초기화합니다.
        /// </remarks>
        static ScheduledAgent()
        {
            // 관리되는 예외 처리기에 알림을 신청합니다.
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// 처리되지 않은 예외에 대해 실행할 코드입니다.
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // 처리되지 않은 예외가 발생했습니다. 중단하고 디버거를 실행합니다.
                Debugger.Break();
            }
        }

        /// <summary>
        /// 예약된 작업을 실행하는 에이전트입니다.
        /// </summary>
        /// <param name="task">
        /// 호출한 작업입니다.
        /// </param>
        /// <remarks>
        /// 이 메서드는 정기적 작업 또는 리소스를 많이 사용하는 작업이 호출될 때 호출됩니다.
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            bool isLiveTileTurn = true;
            scheduleSettings = MutexedIsoStorageFile.Read<ScheduleSettings>("ScheduleSettings", Constants.MUTEX_DATA);

            //스케쥴러는 30분마다 들어온다.
            //이번이 누구 차례인가를 생성해야 한다.
            //라이브타일과 락스크린은 각각의 인터벌이 있고, 그 인터벌은 어느 순간 중복될 수 있다.
            //중복되면 라이브타일에 우선권을 부여하여 실행하며, 락스크린은 그 이후 스케줄로 밀린다.
            //판별에 사용될 변수는 1.인터벌, 2.실행대상, 3.실행대상의 최종 실행시간
#if !DEBUG_AGENT
            {
                DateTime LastRunForLivetile, LastRunForLockscreen;
                IsolatedStorageSettings.ApplicationSettings.TryGetValue<DateTime>(Constants.LAST_RUN_LIVETILE, out LastRunForLivetile);
                IsolatedStorageSettings.ApplicationSettings.TryGetValue<DateTime>(Constants.LAST_RUN_LOCKSCREEN, out LastRunForLockscreen);
                bool useLockscreenRotator;
                IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(Constants.LOCKSCREEN_USE_ROTATOR, out useLockscreenRotator);

                double lockscreenTerm = DateTime.Now.Subtract(LastRunForLockscreen).TotalMinutes - scheduleSettings.LockscreenUpdateInterval;
                //한번도 락스크린 스케쥴러를 실행한적이 없고 락스크린이 스케쥴에서 사용되지 않는 경우는 -1로 설정하여 락스크린으로 분기되지 않도록 처리
                if (LastRunForLockscreen.Year == 1 && LastRunForLockscreen.Month == 1 && LastRunForLockscreen.Day == 1 && !useLockscreenRotator)
                {
                    lockscreenTerm = -1;
                }

                if (DateTime.Now.Subtract(LastRunForLivetile).TotalMinutes < scheduleSettings.LivetileUpdateInterval
                    && lockscreenTerm < 0)
                {
                    System.Diagnostics.Debug.WriteLine("Too soon, stopping.");
                    NotifyComplete();
                    return;
                }
                else if (lockscreenTerm >= 0)
                {
                    isLiveTileTurn = false;
                }
            }
#else
            isLiveTileTurn = false;
#endif
            if (isLiveTileTurn)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine("1 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                    LivetileData data = new LivetileData()
                    {
                        DayList = VsCalendar.GetCalendarOfMonth(DateTime.Now, DateTime.Now, true, true),
                    };

                    try
                    {
                        System.Diagnostics.Debug.WriteLine("타일 전 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);

                        bool hasWeather = ShellTile.ActiveTiles.Any(x => x.NavigationUri.ToString().Contains("weather"));
                        bool hasCalendar = ShellTile.ActiveTiles.Any(x => x.NavigationUri.ToString().Contains("calendar"));

                        if (hasWeather)
                        {
                            //1. 날씨 타일이 핀되어 있다. 
                            WeatherCity city = SettingHelper.Get(Constants.WEATHER_MAIN_CITY) as WeatherCity;
                            DisplayUnit unit = (DisplayUnit)SettingHelper.Get(Constants.WEATHER_UNIT_TYPE);
                            WeatherBug weather = new WeatherBug();
                            weather.LiveWeatherCompletedLoad += (s, r, f) =>
                            {
                                //조회된 데이터를 셋팅(없으면 저장된 날씨를 사용함)
                                SetWeatherResult(data, r, f);
                                //달력 적용 또는 직접 렌더링
                                DelegateUpdateProcess(task, data, hasCalendar);
                            };
                            weather.RequestFailed += (s, r) =>
                            {
                                //데이터를 얻는데 실패 한 경우 네트워크가 연결되지 않았다면, 이전 저장된 데이터를 사용
                                if (!DeviceNetworkInformation.IsNetworkAvailable)
                                {
                                    data.LiveWeather = (LiveWeather)SettingHelper.Get(Constants.WEATHER_LIVE_RESULT);
                                    data.Forecasts = (Forecasts)SettingHelper.Get(Constants.WEATHER_FORECAST_RESULT);
                                }

                                //달력 적용 또는 직접 렌더링
                                DelegateUpdateProcess(task, data, hasCalendar);
                            };

                            if (city != null)
                            {
                                weather.LiveWeather(city, unit);
                            }
                            else
                            {
                                //달력 적용 또는 직접 렌더링
                                DelegateUpdateProcess(task, data, hasCalendar);
                            }
                        }
                        else
                        {
                            //달력 적용 또는 직접 렌더링
                            DelegateUpdateProcess(task, data, hasCalendar);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                });
            }
            else
            {
                if (!LockScreenManager.IsProvidedByCurrentApplication)
                {
                    System.Diagnostics.Debug.WriteLine("잠금화면 제공앱이 아니므로 변경할수 없음.");
                    NotifyComplete();
                    return;
                }

                if (!IsolatedStorageSettings.ApplicationSettings.Contains(Constants.LOCKSCREEN_USE_ROTATOR)
                    || !(bool)IsolatedStorageSettings.ApplicationSettings[Constants.LOCKSCREEN_USE_ROTATOR])
                {
                    System.Diagnostics.Debug.WriteLine("로테이터 사용안함.");
                    NotifyComplete();
                    return;
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine("1 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                    LockscreenData data = new LockscreenData(true)
                    {
                        DayList = VsCalendar.GetCalendarOfMonth(DateTime.Now, DateTime.Now, true, true),
                    };

                    bool hasWeather = false;
                    bool hasCalendar = false;

                    LockscreenItemInfo[] items = data.Items;

                    for (int i = 0; i < items.Length; i++)
                    {
                        switch (items[i].LockscreenItem)
                        {
                            case LiveItems.Weather:
                            case LiveItems.NoForecast:
                                hasWeather = true;
                                break;
                            case LiveItems.Calendar:
                                hasCalendar = true;
                                break;
                        }
                    }

                    if (hasWeather)
                    {
                        WeatherBug weather = new WeatherBug();
                        WeatherCity city = SettingHelper.Get(Constants.WEATHER_MAIN_CITY) as WeatherCity;
                        DisplayUnit unit = (DisplayUnit)SettingHelper.Get(Constants.WEATHER_UNIT_TYPE);

                        weather.LiveWeatherCompletedLoad += (s, r, f) =>
                        {
                            //조회된 데이터를 셋팅(없으면 저장된 날씨를 사용함)
                            SetWeatherResult(data, r, f);
                            //달력 적용 또는 직접 렌더링
                            DelegateUpdateProcess(task, data, hasCalendar);
                        };

                        weather.RequestFailed += (s, r) =>
                        {
                            if (!DeviceNetworkInformation.IsNetworkAvailable)
                            {
                                data.LiveWeather = (LiveWeather)SettingHelper.Get(Constants.WEATHER_LIVE_RESULT);
                                data.Forecasts = (Forecasts)SettingHelper.Get(Constants.WEATHER_FORECAST_RESULT);
                            }
                            //달력 적용 또는 직접 렌더링
                            DelegateUpdateProcess(task, data, hasCalendar);
                        };

                        if (city != null)
                        {
                            weather.LiveWeather(city, unit);
                        }
                        else
                        {
                            //달력 적용 또는 직접 렌더링
                            DelegateUpdateProcess(task, data, hasCalendar);
                        }
                    }
                    else
                    {
                        //달력 적용 또는 직접 렌더링
                        DelegateUpdateProcess(task, data, hasCalendar);
                    }
                });
            }
        }

        private void SetWeatherResult(LiveData data, LiveWeather liveWeather, Forecasts forecasts)
        {
            if (liveWeather != null)
            {
                data.LiveWeather = liveWeather;
            }
            else
            {
                data.LiveWeather = (LiveWeather)SettingHelper.Get(Constants.WEATHER_LIVE_RESULT);
            }

            if (forecasts != null)
            {
                data.Forecasts = forecasts;
            }
            else
            {
                data.Forecasts = (Forecasts)SettingHelper.Get(Constants.WEATHER_FORECAST_RESULT);
            }
        }

        private void DelegateUpdateProcess(ScheduledTask task, LiveData data, bool hasCalendar)
        {
            if (hasCalendar && (bool)SettingHelper.Get(Constants.CALENDAR_SHOW_APPOINTMENT))
            {
                //일정이 몇개 있는지를 백타일에 표시
                Appointments ap = new Appointments();
                ap.SearchCompleted += (so, se) =>
                {
                    VsCalendar.MergeCalendar(data.DayList, se.Results);
                    if (data is LockscreenData)
                        SetLockscreenImage(task, (LockscreenData)data);
                    else
                        SetLivetileImage(task, (LivetileData)data);
                    //완료됨을 OS에 알림
                    NotifyComplete();
                };
                ap.SearchAsync(data.DayList[7].DateTime, data.DayList[data.DayList.Count - 1].DateTime.AddDays(1), null);
            }
            else
            {
                //백타일에 일정 표시하지 않음
                if (data is LockscreenData)
                    SetLockscreenImage(task, (LockscreenData)data);
                else
                    SetLivetileImage(task, (LivetileData)data);
                //완료됨을 OS에 알림
                NotifyComplete();
            }
        }

        private void SetLivetileImage(ScheduledTask task, LivetileData data)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("타일 전 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);

                for (int i = 0; i < ShellTile.ActiveTiles.Count(); i++)
                {
                    ShellTile tile = ShellTile.ActiveTiles.ElementAt(i);

                    if (tile.NavigationUri.ToString().Contains("calendar"))
                    {
                        List<Appointment> appList = data.DayList.Find(x => x.DateTime.ToLongDateString() == DateTime.Today.ToLongDateString()).AppointmentList;
                        int count = appList == null ? 0 : appList.Count;

                        LivetileHelper.CreateLivetileImage(data, LiveItems.Calendar);
                        tile.Update(LivetileHelper.GetCalendarLivetileData(count));
                    }
                    else if (tile.NavigationUri.ToString().Contains("weather"))
                    {
                        LivetileHelper.CreateLivetileImage(data, LiveItems.Weather);
                        tile.Update(LivetileHelper.GetWeatherLivetileData());
                    }
                    else if (tile.NavigationUri.ToString().Contains("battery"))
                    {
                        LivetileHelper.CreateLivetileImage(data, LiveItems.Battery);
                        tile.Update(LivetileHelper.GetBatteryLivetileData());
                    }
                }

                System.Diagnostics.Debug.WriteLine("타일 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                //배터리 잠금화면 업데이트
                UpdateBatteryStatus();
                System.Diagnostics.Debug.WriteLine("배터리 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
#if DEBUG_AGENT
            ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(30));
            System.Diagnostics.Debug.WriteLine(Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
#else
            IsolatedStorageSettings.ApplicationSettings[Constants.LAST_RUN_LIVETILE] = DateTime.Now;
            IsolatedStorageSettings.ApplicationSettings.Save();
#endif
        }

        private void SetLockscreenImage(ScheduledTask task, LockscreenData data)
        {
            try
            {
                bool isProvider = LockScreenManager.IsProvidedByCurrentApplication;

                if (isProvider)
                {
                    int itemCount = data.Items == null ? 0 : data.Items.Length;

                    //1. 이번에 변경할 파일을 알아야 한다. 그러기 위해서 먼저 현재 락스크린 파일명을 구한다.
                    Uri lockscreenFileUri = null;
                    string lockscreenFileName = null;

                    try
                    {
                        lockscreenFileUri = LockScreen.GetImageUri();
                    }
                    catch (Exception)
                    {
                        ShellToast toast = new ShellToast();
                        toast.Title = AppResources.ApplicationTitle;
                        toast.Content = AppResources.MsgFailShortGetLockscreen;
                        toast.Show();
                        return;
                    }

                    if (lockscreenFileUri != null)
                    {
                        lockscreenFileName = lockscreenFileUri.ToString()
                            .Replace(Constants.PREFIX_APP_DATA_FOLDER, string.Empty)
                            .Replace(Constants.LOCKSCREEN_IMAGE_A_POSTFIX, Constants.LOCKSCREEN_IMAGE_POSTFIX)
                            .Replace(Constants.LOCKSCREEN_IMAGE_B_POSTFIX, Constants.LOCKSCREEN_IMAGE_POSTFIX);
                    }

                    //2. 이번에 변경할 파일명을 구한다.
                    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!string.IsNullOrEmpty(lockscreenFileName))
                        {
                            lockscreenFileName = lockscreenFileName.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_READY_POSTFIX);
                        }

                        var imgNames = from element in isoStore.GetFileNames()
                                       where element.Contains(Constants.LOCKSCREEN_IMAGE_READY_POSTFIX)
                                            && element.CompareTo(lockscreenFileName) > 0
                                       orderby element ascending
                                       select element;

                        //이번에 업데이트 할 이미지 이름을 임시 저장
                        lockscreenFileName = imgNames.Any() ? imgNames.First() :
                            isoStore.GetFileNames(string.Format("*{0}", Constants.LOCKSCREEN_IMAGE_READY_POSTFIX))[0];
                        
                        using (IsolatedStorageFileStream sourceStream = isoStore.OpenFile(lockscreenFileName, FileMode.Open, FileAccess.Read))
                        {
                            data.BackgroundBitmap = BitmapFactory.New(0, 0).FromStream(sourceStream);
                        }

                        //3. 표시할 아이템들과 이미지를 합성한다. (이미지 리사이징 포함)
                        LockscreenHelper.RenderLayoutToBitmap(data);

                        GC.Collect();
                        System.Threading.Thread.Sleep(1500);
                        System.Diagnostics.Debug.WriteLine("이미지 생성 직후" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);

                        //4. 새로운 이름으로 저장하기 위해 전환 논리를 이용하여 새로운 이름을 구한다.
                        if (lockscreenFileUri != null  && lockscreenFileUri.ToString().EndsWith(Constants.LOCKSCREEN_IMAGE_A_POSTFIX))
                        {
                            lockscreenFileName = lockscreenFileName.Replace(Constants.LOCKSCREEN_IMAGE_READY_POSTFIX, Constants.LOCKSCREEN_IMAGE_B_POSTFIX);
                        }
                        else 
                        {
                            lockscreenFileName = lockscreenFileName.Replace(Constants.LOCKSCREEN_IMAGE_READY_POSTFIX, Constants.LOCKSCREEN_IMAGE_A_POSTFIX);
                        }

                        //5. 새로운 이름으로 이미지를 저장
                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile(lockscreenFileName, FileMode.Create, FileAccess.Write))
                        {
                            data.BackgroundBitmap.SaveJpeg(targetStream, data.BackgroundBitmap.PixelWidth, data.BackgroundBitmap.PixelHeight, 0, 100);
                            //using (MemoryStream ms = new MemoryStream())
                            //{
                            //    data.BackgroundBitmap.SaveJpeg(ms, data.BackgroundBitmap.PixelWidth, data.BackgroundBitmap.PixelHeight, 0, 100);
                            //    System.Diagnostics.Debug.WriteLine("{0}x{1}", data.BackgroundBitmap.PixelWidth, data.BackgroundBitmap.PixelHeight);
                            //    byte[] readBuffer = new byte[4096];
                            //    int bytesRead = -1;
                            //    ms.Position = 0;
                            //    targetStream.Position = 0;

                            //    while ((bytesRead = ms.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            //    {
                            //        targetStream.Write(readBuffer, 0, bytesRead);
                            //    }
                            //}
                        }

                        //6. 새로운 이미지로 락스크린 업데이트
                        LockScreen.SetImageUri(new Uri(string.Format("{0}{1}", Constants.PREFIX_APP_DATA_FOLDER, lockscreenFileName), UriKind.Absolute));
                        Uri newLockscreenFileUri = LockScreen.GetImageUri();
                        
                        //7.변경이 정상적으로 이루어진 경우 기존 파일 삭제
                        if (newLockscreenFileUri.ToString() != lockscreenFileUri.ToString())
                        {
                            if (lockscreenFileUri.ToString().Contains(Constants.PREFIX_APP_DATA_FOLDER)) 
                            {
                                lockscreenFileName = lockscreenFileUri.ToString().Replace(Constants.PREFIX_APP_DATA_FOLDER, string.Empty);
                                if (isoStore.FileExists(lockscreenFileName))
                                {
                                    isoStore.DeleteFile(lockscreenFileName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggingError("error case 3 : \n" + e.StackTrace);
            }

            // If debugging is enabled, launch the agent again in one minute.
#if DEBUG_AGENT
            ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(30));
            System.Diagnostics.Debug.WriteLine(Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
            /*
            ShellToast toast = new ShellToast();
            toast.Title = AppResources.ApplicationTitle;
            toast.Content = AppResources.MsgSuccessChangeLockscreen;
            toast.Show();
            */
#else
            IsolatedStorageSettings.ApplicationSettings[Constants.LAST_RUN_LOCKSCREEN] = DateTime.Now;
            IsolatedStorageSettings.ApplicationSettings.Save();
#endif
        }

        public static void LoggingError(string msg)
        {
            if (Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }

            List<string> list = null;
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LockscreenUpdateErrorRecord"))
            {
                list = IsolatedStorageSettings.ApplicationSettings["LockscreenUpdateErrorRecord"] as List<string>;
            }
            else
            {
                list = new List<string>();
            }
            list.Add(msg);
            IsolatedStorageSettings.ApplicationSettings["LockscreenUpdateErrorRecord"] = list;
        }

        public static void UpdateBatteryStatus()
        {
            FlipTileData primaryTileData = new FlipTileData();
            primaryTileData.Count = BatteryHelper.BateryLevel;
            //primaryTileData.BackContent = content;
            
            ShellTile primaryTile = ShellTile.ActiveTiles.FirstOrDefault();
            primaryTile.Update(primaryTileData);
        }

    }
}