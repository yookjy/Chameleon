using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.Phone.System.UserProfile;
using ChameleonLib.Api.Calendar;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Helper;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using ChameleonLib.Storages;
using Microsoft.Phone.Net.NetworkInformation;


namespace Chameleon
{
    public partial class MainPage : PhoneApplicationPage ,INotifyPropertyChanged
    {
        private string lockscreenCount;

        private bool lockscreenEditWarnning;

        //셋팅에서 불러올때 이벤트 바인딩 될때 호출되는걸 피하기 위해 요녀석을 설정
        private bool ignoreCheckBoxEvents = false;

        public string LockscreenCount
        {
            get
            {
                return lockscreenCount;
            }
            set
            {
                if (lockscreenCount != value)
                {
                    lockscreenCount = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        public bool LockscreenEditWarnning
        {
            get
            {
                return lockscreenEditWarnning;
            }
            set
            {
                if (lockscreenEditWarnning != value)
                {
                    lockscreenEditWarnning = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private IApplicationBar IAppBarLockscreen;

        private IList appBarIconButtons;

        private Uri currentLockscreenUri =  new Uri("/Images/lockscreen/lockscreen.current.png", UriKind.Relative);

        private Uri warnningUri = new Uri("/Images/lockscreen/appbar.transit.hazard.png", UriKind.Relative);
        
        private Thickness currentLockscreenMargin = new Thickness(7, 13, 7, 22);

        private ScheduleSettings scheduleSettings;
        
        //락스크린 초기화
        private void MainPageLockscreen()
        {
            //앱바 생성
            CreateLockscreenAppBar();
                        
            //잠금화면 공급자 설정
            //LockscreenHelper.SetLockscreenProvider();
            
            //락스크린 사용여부 설정
            ignoreCheckBoxEvents = true;
            if (SettingHelper.ContainsKey(Constants.LOCKSCREEN_USE_ROTATOR))
            {
                //잠금화면 공급자가 아니면 스케쥴러 및 락스크린 사용여부 삭제
                if (!LockScreenManager.IsProvidedByCurrentApplication)
                {
                    NoUseLockscreen();
                }
                else
                {
                    //잠금화면 공급자인데 스케쥴러가 없으면 시작시킴
                    UseLockscreen.IsChecked = true;
                    //로딩시 락스크린 사용인데 스케줄러에 없으면 시작시킴
                    if (ScheduledActionService.Find(Constants.PERIODIC_TASK_NAME) == null)
                    {
                        StartPeriodicAgent();
                    }
                }
            }
            ignoreCheckBoxEvents = false;
            //경고 숨김
            LockscreenEditWarnning = false;
        }

        //앱바 생성
        private void CreateLockscreenAppBar()
        {
            IAppBarLockscreen = new ApplicationBar();
            IAppBarLockscreen.Mode = ApplicationBarMode.Default;
            IAppBarLockscreen.Opacity = 0.8;
            IAppBarLockscreen.IsVisible = false;
            IAppBarLockscreen.IsMenuEnabled = true;
            CopyMenuItem(IAppBarLockscreen.MenuItems);

            ApplicationBarIconButton selectAppBarIconBtn = new ApplicationBarIconButton();
            selectAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.list.check.png");
            selectAppBarIconBtn.Text = AppResources.AppbarMenuSelect;
            selectAppBarIconBtn.IsEnabled = false;
            selectAppBarIconBtn.Click += selectAppBarIconBtn_Click;

            ApplicationBarIconButton addAppBarIconBtn = new ApplicationBarIconButton();
            addAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.add.png");
            addAppBarIconBtn.Text = AppResources.AppbarMenuAddSingleImage;
            addAppBarIconBtn.Click += appBarIconBtn_Click;

            ApplicationBarIconButton addMultiAppBarIconBtn = new ApplicationBarIconButton();
            addMultiAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.layer.add.png");
            addMultiAppBarIconBtn.Text = AppResources.AppbarMenuAddMultipleImage;
            addMultiAppBarIconBtn.Click += appMultiBarIconBtn_Click;

            ApplicationBarIconButton appBarIconBtnSettings = new ApplicationBarIconButton();
            appBarIconBtnSettings.IconUri = PathHelper.GetPath("appbar.settings.png");
            appBarIconBtnSettings.Text = AppResources.Settings;
            appBarIconBtnSettings.Click += settingMI_Click;

            IAppBarLockscreen.Buttons.Add(selectAppBarIconBtn);
            IAppBarLockscreen.Buttons.Add(addAppBarIconBtn);
            IAppBarLockscreen.Buttons.Add(addMultiAppBarIconBtn);
            IAppBarLockscreen.Buttons.Add(appBarIconBtnSettings);

            ApplicationBarIconButton delAppBarIconBtn = new ApplicationBarIconButton();
            delAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.delete.png");
            delAppBarIconBtn.Text = AppResources.AppbarMenuRemove;
            delAppBarIconBtn.Click += delAppBarIconBtn_Click;

            appBarIconButtons = new List<ApplicationBarIconButton>();
            appBarIconButtons.Add(delAppBarIconBtn);
        }

        //락스크린 리스트 로딩
        public void LoadLockscreenList()
        {
            //락스크린 썸네일 이미지 크기  설정
            LockscreenSelector.GridCellSize = LockscreenHelper.ThumnailSize;
            ChameleonAlbum phoneAlbum = LockscreenSelector.ItemsSource as ChameleonAlbum;

            if (phoneAlbum == null)
            {
                phoneAlbum = new ChameleonAlbum();
                LockscreenSelector.ItemsSource = phoneAlbum;
            }

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var imgNames = from element in isoStore.GetFileNames()
                               where element.Contains(Constants.LOCKSCREEN_IMAGE_POSTFIX)
                               orderby element.Substring(0, element.IndexOf("_")) ascending
                               select element;

                Uri lockscreenFileUri = null;
                
                try
                {
                    lockscreenFileUri = LockScreen.GetImageUri();
                }
                catch (Exception)
                {
                }

                string lockscreenFileName = null;

                if (lockscreenFileUri != null)
                {
                    lockscreenFileName = lockscreenFileUri.ToString()
                        .Replace(Constants.PREFIX_APP_DATA_FOLDER, string.Empty)
                        .Replace(Constants.LOCKSCREEN_IMAGE_A_POSTFIX, Constants.LOCKSCREEN_IMAGE_POSTFIX)
                        .Replace(Constants.LOCKSCREEN_IMAGE_B_POSTFIX, Constants.LOCKSCREEN_IMAGE_POSTFIX);
                }

                foreach (string imgName in imgNames)
                {
                    bool isReady = isoStore.GetFileNames(imgName.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_READY_POSTFIX)).Any();
                    var pic = from element in phoneAlbum
                               where element.Name == imgName
                               select element;

                    if (pic.Any())
                    {
                        //원래 존재하는 이름이므로 해당 파일에 대한 정보를 업데이트 처리
                        PhonePicture curPic = pic.First() as PhonePicture;

                        if (curPic.Name == lockscreenFileName)
                        {
                            curPic.CurrentLockscreen = currentLockscreenUri;
                            curPic.Margin = currentLockscreenMargin;
                        }
                        else
                        {
                            curPic.CurrentLockscreen = null;
                            curPic.Margin = new Thickness();
                        }

                        //이미지 변경 시간을 체크해서, 이미지가 편집이 되었다면 이미지를 다시 로드
                        DateTimeOffset offset = isoStore.GetLastWriteTime(imgName);
                        if (offset.Subtract(curPic.DateTimeOffset).Milliseconds != 0)
                        {
                            curPic.DateTimeOffset = offset;

                            WriteableBitmap bitmap = null;
                            string thumbName = imgName.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_THUMNAIL_POSTFIX);
                            string[] thumbs = isoStore.GetFileNames(thumbName);
                            
                            //썸네일이 없는 경우면 원본 이름을 셋팅
                            if (thumbs == null || thumbs.Length == 0)
                            {
                                thumbName = imgName;
                            }

                            //이미지 로드
                            using (IsolatedStorageFileStream sourceStream = isoStore.OpenFile(thumbName, FileMode.Open, FileAccess.Read))
                            {
                                //썸네일이 없는 경우면 원본을 리사이징
                                if (thumbs == null || thumbs.Length == 0)
                                {
                                    bitmap = JpegHelper.Resize(sourceStream, LockscreenHelper.ThumnailSize, true);
                                }
                                else
                                {
                                    bitmap = BitmapFactory.New(0, 0).FromStream(sourceStream);
                                }
                            }
                            //썸네일 이미지 교체
                            curPic.ThumbnailImageSource = bitmap;
                        }

                        //편집 페이지에서 준비상태로 편집이 완료되었으면 편집 경고 삭제
                        if (isReady)
                        {
                            curPic.Warnning = null;
                        }
                    }
                    else
                    {
                        //존재하지 않는 파일이므로 새롭게 리스트에 추가
                        WriteableBitmap bitmap = null;
                        string thumbName = imgName.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_THUMNAIL_POSTFIX);
                        string[] thumbs = isoStore.GetFileNames(thumbName);
                        Uri uri = null;
                        Thickness margin = new Thickness();
                        //썸네일이 없는 경우면 원본 이름을 셋팅
                        if (thumbs == null || thumbs.Length == 0)
                        {
                            thumbName = imgName;
                        }

                        //이미지 로드
                        using (IsolatedStorageFileStream sourceStream = isoStore.OpenFile(thumbName, FileMode.Open, FileAccess.Read))
                        {
                            //썸네일이 없는 경우면 원본을 리사이징
                            if (thumbs == null || thumbs.Length == 0)
                            {
                                bitmap = JpegHelper.Resize(sourceStream, LockscreenHelper.ThumnailSize, true);
                            }
                            else
                            {
                                bitmap = BitmapFactory.New(0, 0).FromStream(sourceStream);
                            }
                        }
                        //현재 락스크린 지정 이미지인 경우 처리
                        if (lockscreenFileName == imgName)
                        {
                            uri = currentLockscreenUri;
                            margin = currentLockscreenMargin;
                        }
                        //락스크린 이미지 객체 생성
                        phoneAlbum.Add(new PhonePicture()
                        {
                            Guid = Guid.NewGuid(),
                            Name = imgName,
                            ThumnailName = thumbName,
                            ThumbnailImageSource = bitmap,
                            Margin = margin,
                            CurrentLockscreen = uri,
                            Warnning = isReady ? null : warnningUri,
                            Opacity = 1,
                            DateTimeOffset = isoStore.GetLastWriteTime(thumbName)
                        });
                    }
                    
                    //경고 안내문구 표시
                    if (!LockscreenEditWarnning && !isReady)
                    {
                        LockscreenEditWarnning = true;
                    }
                }
            }

            (IAppBarLockscreen.Buttons[0] as ApplicationBarIconButton).IsEnabled = !(phoneAlbum.Count == 0 && IAppBarLockscreen.Buttons.Count > 1) ;
            //이미지 갯수 표시
            LockscreenCount = string.Format("({0})", phoneAlbum.Count);
            //에디팅 경고 표시
            LockscreenEditWarnning = phoneAlbum.Any(x => (x as PhonePicture).Warnning != null);

            if (phoneAlbum.Count == 0)
            {
                //도움말 표시 토글
                TxtLockscreen.Visibility = Visibility.Visible;
                //이미지가 없고 활성화된 라이브타일이 없으면 스케쥴러 정저
                if (!ExistsActiveTile)
                {
                    RemoveAgent(Constants.PERIODIC_TASK_NAME);
                }
                UseLockscreen.IsChecked = false;
                UseLockscreen.IsEnabled = false;
            }
            else
            {
                UseLockscreen.IsEnabled = true;
                TxtLockscreen.Visibility = Visibility.Collapsed;
            }
        }
        
        //앱바 선택 버튼 이벤트 핸들러
        void selectAppBarIconBtn_Click(object sender, EventArgs e)
        {
            LockscreenSelector.EnforceIsSelectionEnabled = true;
            ChangeAppbarIconButtons(true);
        }

        //앱바 변경
        private void ChangeAppbarIconButtons(bool isToDeleteAppBar)
        {
            if ((appBarIconButtons.Count == 1 && isToDeleteAppBar) || (appBarIconButtons.Count > 1 && !isToDeleteAppBar))
            {
                List<ApplicationBarIconButton> tmpList = new List<ApplicationBarIconButton>(IAppBarLockscreen.Buttons.Count);
                //현재 앱바를 백업
                while (IAppBarLockscreen.Buttons.Count > 0)
                {
                    ApplicationBarIconButton button = IAppBarLockscreen.Buttons[0] as ApplicationBarIconButton;
                    tmpList.Add(button);
                    IAppBarLockscreen.Buttons.RemoveAt(0);
                }
                //새로운 앱바 로드
                for (int i = 0; i < appBarIconButtons.Count; i++)
                {
                    ApplicationBarIconButton button = appBarIconButtons[i] as ApplicationBarIconButton;
                    button.IsEnabled = !(button.Text == AppResources.AppbarMenuSelect && (LockscreenSelector.ItemsSource == null || LockscreenSelector.ItemsSource.Count == 0));
                    IAppBarLockscreen.Buttons.Add(button);
                }

                appBarIconButtons = tmpList;
            }
        }

        //앱바 삭제 버튼 이벤트 핸들러
        void delAppBarIconBtn_Click(object sender, EventArgs e)
        {
            for (int i = LockscreenSelector.SelectedItems.Count -1; i >=0 ; i--)
            {
                RemoveLockscreenImage(LockscreenSelector.SelectedItems[i] as PhonePicture);
            }
        }

        //단건 폰사진 추가 이벤트 핸들러
        private void appBarIconBtn_Click(object sender, EventArgs e)
        {
            PhotoChooserTask photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += photoChooserTask_Completed;
            photoChooserTask.Show();
        }
        //단건 폰사진 추가 완료 이벤트 핸들러
        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                if (LockscreenSelector.ItemsSource.Count < Constants.LOCKSCREEN_MAX_COUNT)
                {
                    using (Stream stream = e.ChosenPhoto)
                    {
                        JpegHelper.Save(FileHelper.GetUniqueFileName(Constants.LOCKSCREEN_IMAGE_POSTFIX), stream);
                    }
                }
                else
                {
                    MessageBox.Show(string.Format(AppResources.MsgFailAddMaxCount, Constants.LOCKSCREEN_MAX_COUNT));
                }
            }
        }

        //복수의 폰 및 빙사진 추가 이벤트 핸들러
        private void appMultiBarIconBtn_Click(object sender, EventArgs e)
        {
            App.CheckNetworkStatus();
            NavigationService.Navigate(new Uri("/View/LockscreenSelectionPage.xaml", UriKind.Relative));
        }
       
        private void RemoveLockscreenImage(PhonePicture picture)
        {
            var container = LockscreenSelector.ContainerFromItem(picture);
            Border border = FindFirstElementInVisualTree<Border>((DependencyObject)container);

            Storyboard sb = new Storyboard();
            DoubleAnimationUsingKeyFrames timeline = new DoubleAnimationUsingKeyFrames();
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0)), Value = 1 });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100)), Value = 0.1 });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100)), Value = 0.01 });
            
            sb.Children.Add(timeline);

            border.RenderTransformOrigin = new Point(0.5, 0.5);
            border.RenderTransform = new CompositeTransform();
            Storyboard.SetTarget(timeline, border.RenderTransform);
            Storyboard.SetTargetProperty(timeline, new PropertyPath("CompositeTransform.ScaleY"));

            sb.Begin();
            sb.Completed += (s, e) =>
            {
                //다음 추가될 이미지를 위해 트랜스폼 복구
                (border.RenderTransform as CompositeTransform).ScaleY = 1;
                //리스트에서 실제 삭제 처리
                LockscreenSelector.ItemsSource.Remove(picture);
                //스토리지에서 파일 삭제
                FileHelper.RemoveImage(picture.Name);
                FileHelper.RemoveImage(picture.ThumnailName);
                FileHelper.RemoveImage(picture.Name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_READY_POSTFIX));
                //UI 변경
                if (LockscreenSelector.ItemsSource.Count == 0)
                {
                    TxtLockscreen.Visibility = System.Windows.Visibility.Visible;
                    LockscreenSelector.EnforceIsSelectionEnabled = false;
                    ChangeAppbarIconButtons(false);

                    //이미지가 없고 활성화된 라이브타일이 없으면 스케쥴러 정저
                    if (!ExistsActiveTile)
                    {
                        RemoveAgent(Constants.PERIODIC_TASK_NAME);
                    }
                    SettingHelper.Remove(Constants.LOCKSCREEN_USE_ROTATOR);
                    UseLockscreen.IsChecked = false;
                    UseLockscreen.IsEnabled = false;
                }

                if (IAppBarLockscreen.Buttons.Count > 1 && LockscreenSelector.ItemsSource.Count == 0)
                {
                    (IAppBarLockscreen.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
                }

                LockscreenEditWarnning = false;
                //편집 경고 표시
                for (int i = 0; i < LockscreenSelector.ItemsSource.Count; i++)
                {
                    PhonePicture pic = LockscreenSelector.ItemsSource[i] as PhonePicture;

                    if (pic.Warnning != null)
                    {
                        LockscreenEditWarnning = true;
                        break;
                    }
                }
                //락스크린 카운드
                LockscreenCount = string.Format("({0})", LockscreenSelector.ItemsSource.Count);
            };
        }

        private T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var result = FindFirstElementInVisualTree<T>(child);
                    if (result != null)
                        return result;

                }
            }
            return null;
        }

        private async Task SelectLockscreen(PhonePicture picture)
        {
           await Task.Run(async () =>
            {
                if (picture != null)
                {
                    try
                    {
                        var isProvider = LockScreenManager.IsProvidedByCurrentApplication;
                        var op = isProvider ? LockScreenRequestResult.Granted : LockScreenRequestResult.Denied;

                        if (!isProvider)
                        {
                            // If you're not the provider, this call will prompt the user for permission.
                            // Calling RequestAccessAsync from a background agent is not allowed.
                            op = await LockScreenManager.RequestAccessAsync();
                        }

                        if (op == LockScreenRequestResult.Granted)
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                //로딩 패널 띄움
                                ShowLoadingPanel(AppResources.MsgApplyingLockscreen);
                            
                                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    using (IsolatedStorageFileStream sourceStream = isoStore.OpenFile(picture.Name, FileMode.Open, FileAccess.Read))
                                    {
                                        WriteableBitmap wb = BitmapFactory.New(0, 0).FromStream(sourceStream);
                                        Size rSize = ResolutionHelper.CurrentResolution;
                                        MemoryStream ms = null;

                                        LockscreenData data = new LockscreenData(false)
                                        {
                                            DayList = VsCalendar.GetCalendarOfMonth(DateTime.Now, DateTime.Now, true, true),
                                            LiveWeather = SettingHelper.Get(Constants.WEATHER_LIVE_RESULT) as LiveWeather,
                                            Forecasts = SettingHelper.Get(Constants.WEATHER_FORECAST_RESULT) as Forecasts,
                                            BackgroundBitmap = wb.Crop(new Rect((wb.PixelWidth - rSize.Width) / 2, (wb.PixelHeight - rSize.Height) / 2, rSize.Width, rSize.Height))
                                        };

                                        //편집이 필요한 이미지라면 스트림 생성 및 이미지 복사
                                        if (picture.Warnning != null)
                                        {
                                            //메모리 스트림 생성 (close 처리는 SetLockscreen에서 한다.)
                                            ms = new MemoryStream();
                                            //잘라내기가 된 이미지를 스트림에 저장
                                            data.BackgroundBitmap.SaveJpeg(ms, data.BackgroundBitmap.PixelWidth, data.BackgroundBitmap.PixelHeight, 0, 100);
                                        }

                                        if ((bool)SettingHelper.Get(Constants.CALENDAR_SHOW_APPOINTMENT))
                                        {
                                            Appointments appointments = new Appointments();
                                            appointments.SearchCompleted += (s, se) =>
                                            {
                                                VsCalendar.MergeCalendar(data.DayList, se.Results);
                                                LockscreenHelper.RenderLayoutToBitmap(data);
                                                SetLockscreen(picture, data.BackgroundBitmap, ms);

                                            };
                                            appointments.SearchAsync(data.DayList[7].DateTime, data.DayList[data.DayList.Count - 1].DateTime, null);
                                        }
                                        else
                                        {
                                            LockscreenHelper.RenderLayoutToBitmap(data);
                                            SetLockscreen(picture, data.BackgroundBitmap, ms);
                                        }
                                    }
                                }
                            });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            });
        }

        private void SetLockscreen(PhonePicture picture, WriteableBitmap wb, Stream beforeImageStream)
        {
            string fileName = picture.Name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_A_POSTFIX);
            Uri currentImage = null;

            try
            {
                currentImage = LockScreen.GetImageUri();
            }
            catch (Exception)
            {
                MessageBox.Show(AppResources.MsgFailGetLockscreen);
                HideLoadingPanel();
                return;
            }

            if (currentImage != null && currentImage.ToString().EndsWith(Constants.LOCKSCREEN_IMAGE_A_POSTFIX))
            {
                fileName = picture.Name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_B_POSTFIX);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                //축소/확대 가능한 이미지라면 축소/확대
                JpegHelper.Resize(wb, ms, ResolutionHelper.CurrentResolution, true);
                FileHelper.SaveImage(fileName, ms);
                
                LockscreenHelper.SetLockscreen(fileName, false, (result) =>
                {
                    if (result.AsyncState is string)
                    {
                        MessageBox.Show(AppResources.MsgFailChangeLockscreen);
                    }
                    else if (result.AsyncState is bool && (bool)result.AsyncState == true)
                    {
                        if (currentImage != null && currentImage.ToString().StartsWith(Constants.PREFIX_APP_DATA_FOLDER))
                        {
                            fileName = currentImage.ToString().Replace(Constants.PREFIX_APP_DATA_FOLDER, string.Empty);
                            FileHelper.RemoveImage(fileName);
                        }

                        for (int i = 0; i < LockscreenSelector.ItemsSource.Count; i++ )
                        {
                            PhonePicture pic = (LockscreenSelector.ItemsSource as ChameleonAlbum)[i] as PhonePicture;
                            if (pic.CurrentLockscreen != null)
                            {
                                pic.CurrentLockscreen = null;
                                pic.Margin = new Thickness();
                            }
                            //자기자신으로 변경한 경우 및 새롭게 변경된 아이템
                            if (pic.Guid.CompareTo(picture.Guid) == 0)
                            {
                                pic.CurrentLockscreen = currentLockscreenUri;
                                pic.Margin = currentLockscreenMargin;
                            }
                        }

                        //표시항목들이 적용되지 않은 잘라내기가 수행된 이미지 데이터
                        if (picture.Warnning != null && beforeImageStream != null)
                        {
                            if (MessageBox.Show(AppResources.MsgEditedAutomatically, AppResources.Confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            {
                                //ready 파일 저장
                                //락스크린용 이미지로 축소
                                WriteableBitmap rwb = JpegHelper.Resize(beforeImageStream, LockscreenHelper.Size, true);
                                using (MemoryStream rms = new MemoryStream())
                                {
                                    rwb.SaveJpeg(rms, rwb.PixelWidth, rwb.PixelHeight, 0, 100);
                                    FileHelper.SaveImage(picture.Name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_READY_POSTFIX), rms);
                                }
                                
                                //상태 변경
                                picture.Warnning = null;
                                //경고 메세지 제어
                                bool isShowWarn = false;
                                foreach (PhonePicture pic in LockscreenSelector.ItemsSource as ChameleonAlbum)
                                {
                                    if (pic.Warnning != null)
                                    {
                                        isShowWarn = true;
                                        break;
                                    }
                                }
                                LockscreenEditWarnning = isShowWarn;

                                SetSchedulerChagendTime(false);
                                //MessageBox.Show(AppResources.MsgSuccessChangeLockscreen);
                            }
                        }
                        else
                        {
                            SetSchedulerChagendTime(false);
                            //MessageBox.Show(AppResources.MsgSuccessChangeLockscreen);
                        }

                        if (beforeImageStream != null)
                        {
                            //스트림 종료
                            beforeImageStream.Close();
                        }
                    }
                    HideLoadingPanel();
                });
            }
        }

        private async void OnCheckedLockscreen(object sender, RoutedEventArgs e)
        {
            if (ignoreCheckBoxEvents)
                return;

            if (MessageBoxResult.OK == MessageBox.Show(AppResources.MsgConfirmLockscreenUpdate, AppResources.Confirm, MessageBoxButton.OKCancel))
            {
                if (LockscreenSelector.ItemsSource != null && LockscreenSelector.ItemsSource.Count > 0)
                {
                    //락스크린 프로바이더가 아닌경우 설정 창 띄움.
                    var op = await LockscreenHelper.SetLockscreenProvider();

                    if (op == LockScreenRequestResult.Granted)
                    {
                        var exists = (LockscreenSelector.ItemsSource as ChameleonAlbum).Any(p =>
                        {
                            return (p as PhonePicture).CurrentLockscreen != null;
                        });

                        if (!exists)
                        {
                            await SelectLockscreen(LockscreenSelector.ItemsSource[0] as PhonePicture);
                        }

                        if (ScheduledActionService.Find(Constants.PERIODIC_TASK_NAME) == null)
                        {
                            StartPeriodicAgent();
                        }
                        SettingHelper.Set(Constants.LOCKSCREEN_USE_ROTATOR, true, true);
                    }
                    else
                    {
                        UseLockscreen.IsChecked = false;
                    }
                }
            }
            else
            {
                UseLockscreen.IsChecked = false;
                //이미지가 없고 활성화된 라이브타일이 없으면 스케쥴러 정저
                if (!ExistsActiveTile)
                {
                    RemoveAgent(Constants.PERIODIC_TASK_NAME);
                }
            }
        }

        private void OnUncheckedLockscreen(object sender, RoutedEventArgs e)
        {
            if (ignoreCheckBoxEvents)
                return;

            NoUseLockscreen();
        }

        private void NoUseLockscreen()
        {
            if (!ExistsActiveTile)
            {
                RemoveAgent(Constants.PERIODIC_TASK_NAME);
            }

            SettingHelper.Remove(Constants.LOCKSCREEN_USE_ROTATOR);
        }

        /**
         * 락스크린 이미지 삭제 이벤트 (컨텍스트 메뉴 : 터치 & 홀드)
         * 
         **/
        private void OnRemoveLockscreenImage(object sender, RoutedEventArgs e)
        {
            PhonePicture picture = (sender as MenuItem).DataContext as PhonePicture;
            if (picture != null)
            {
                RemoveLockscreenImage(picture);
            }
        }

        /**
         * 락스크린 설정 이벤트 (컨텍스트 메뉴 : 터치 & 홀드)
         * 
         **/
        private async void OnSetLockscreenImage(object sender, RoutedEventArgs e)
        {
            PhonePicture picture = (sender as MenuItem).DataContext as PhonePicture;
            await SelectLockscreen(picture);
        }

        /**
         * 편집 및 미리보기 (컨텍스트 메뉴)
         * */
        private void OnLockscreenPreviewTap(object sender, RoutedEventArgs e)
        {
            PhonePicture picture = (sender as MenuItem).DataContext as PhonePicture;
            NavigationService.Navigate(new Uri(string.Format("/View/PictureEditPage.xaml?imgName={0}", picture.Name), UriKind.Relative));
        }

        /**
         * 컨텍스트 메뉴 강제 로드
         * */
        private void LockscreenImage_Tap(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            ContextMenu contextMenu = ContextMenuService.GetContextMenu(grid);
            if (contextMenu.Parent == null)
            {
                contextMenu.IsOpen = true;
                (grid.DataContext as PhonePicture).Opacity = 0.6;
                //멀티 셀렉트 삭제와 일관성이 떨어진다.
                //MenuItem rmItem = (contextMenu.Items as System.Windows.Controls.ItemCollection).First(x => (x as Microsoft.Phone.Controls.MenuItem).Name == "RemoveMenu") as MenuItem;
                //rmItem.IsEnabled = (grid.DataContext as PhonePicture).CurrentLockscreen == null;
            }
        }

        /**
         * 홀드 이벤트 방지
         */
        private void Grid_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
        }

        /**
         * 이전 데이터가 로드 되는 것을 방지함
         * */
        private void OnUnloadLockscreenContextMenu(object sender, RoutedEventArgs e)
        {
            ContextMenu conmen = (sender as ContextMenu);
            (conmen.DataContext as PhonePicture).Opacity = 1;
            conmen.ClearValue(FrameworkElement.DataContextProperty);
        }

    }
}
