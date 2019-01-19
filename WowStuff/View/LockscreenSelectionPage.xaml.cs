using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ChameleonLib.Helper;
using ChameleonLib.Api.Open.Bing;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Resources;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Collections.Generic;
using System.Threading;
using ChameleonLib.Model;
using System.Collections;
using ChameleonLib.Api.Open.Today;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Chameleon.View.Helper.Helper;

namespace Chameleon.View
{
    public class HeaderItem
    {
        public HeaderItem(string url, string header)
        {
            HeaderTitle = header;
            if (url != null)
            {
                HeaderImage = new Uri(url, UriKind.Relative);
                Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public Uri HeaderImage { get; private set; }

        public string HeaderTitle { get; private set; }

        public Visibility Visibility { get; private set; }
    }

    public partial class LockscreenSelectionPage : PhoneApplicationPage
    {
        private Finder finder;
        
        private BingToday bingToday;

        private NasaToday nasaToday;
        
        private BackgroundWorker bgPhoneLoader;
        
        private BackgroundWorker bgBingLoader;

        private BackgroundWorker bgNasaLoader;

        private ApplicationBarIconButton appBarIconBtnSelect;

        private ApplicationBarIconButton appBarIconBtnAdd;

        private ApplicationBarIconButton appBarIconBtnSettings;

        private string searchQuery;

        public LockscreenSelectionPage()
        {
            InitializeComponent();

            //페이지의 ApplicationBar를 ApplicationBar의 새 인스턴스로 설정합니다.
            BuildLocalizedApplicationBar();
                        
            bgPhoneLoader = new BackgroundWorker();
            bgPhoneLoader.WorkerReportsProgress = true;
            bgPhoneLoader.DoWork += bgPhoneLoader_DoWork;
            bgPhoneLoader.ProgressChanged += bgPhoneLoader_ProgressChanged;

            bgBingLoader = new BackgroundWorker();
            bgBingLoader.DoWork += bgBingLoader_DoWork;

            bgNasaLoader = new BackgroundWorker();
            bgNasaLoader.DoWork += bgNasaLoader_DoWork;

            LockScreenPivot.Title = string.Format("{0} - {1}", AppResources.ApplicationTitle, AppResources.SelectPicture);

            (LockScreenPivot.Items[0] as PivotItem).Header = new HeaderItem(null, AppResources.Album);
            (LockScreenPivot.Items[1] as PivotItem).Header = new HeaderItem("/Images/search/bing.png", AppResources.Today);
            (LockScreenPivot.Items[2] as PivotItem).Header = new HeaderItem("/Images/search/nasa.png", AppResources.Universe);
            (LockScreenPivot.Items[3] as PivotItem).Header = new HeaderItem("/Images/search/bing.png", AppResources.Search);
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.8;
            ApplicationBar.IsVisible = false;
            ApplicationBar.IsMenuEnabled = true;

            appBarIconBtnSelect = new ApplicationBarIconButton();
            appBarIconBtnSelect.IconUri = PathHelper.GetPath("appbar.list.check.png");
            appBarIconBtnSelect.Text = AppResources.AppbarMenuSelect;
            appBarIconBtnSelect.Click += appBarIconBtnSelect_Click;

            appBarIconBtnAdd = new ApplicationBarIconButton();
            appBarIconBtnAdd.IconUri = PathHelper.GetPath("appbar.list.add.below.png");
            appBarIconBtnAdd.Text = AppResources.AppbarMenuAddLockscreen;
            appBarIconBtnAdd.Click += appBarIconBtnAdd_Click;

            appBarIconBtnSettings = new ApplicationBarIconButton();
            appBarIconBtnSettings.IconUri = PathHelper.GetPath("appbar.settings.png");
            appBarIconBtnSettings.Text = AppResources.ImageSettings;
            appBarIconBtnSettings.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/View/SettingImagePage.xaml", UriKind.Relative));
            };

            ApplicationBar.Buttons.Add(appBarIconBtnSelect);
            ApplicationBar.Buttons.Add(appBarIconBtnSettings);
        }

        private void appBarIconBtnAdd_Click(object sender, EventArgs e)
        {
            LongListMultiSelector selector = GetSelectorByPivotName((LockScreenPivot.SelectedItem as PivotItem).Name);

            if (selector != PhonePictureSelector && selector.SelectedItems.Count > 0)
            {
                if (LockscreenHelper.CurrentListCount + selector.SelectedItems.Count > Constants.LOCKSCREEN_MAX_COUNT)
                {
                    MessageBox.Show(string.Format(AppResources.MsgFailAddMaxCount, Constants.LOCKSCREEN_MAX_COUNT));
                    return;
                }
                PageHelper.SetDownloadImageList(selector.SelectedItems);
                NavigationService.Navigate(new Uri(string.Format("/View/ImageDownloadPage.xaml?searchKeyword={0}", searchQuery), UriKind.Relative));
            }
        }
        
        void bgBingLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadBingTodayPictureAlbum();
        }

        void bgPhoneLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            ChameleonAlbum phoneAlbum = e.Argument as ChameleonAlbum;
            BackgroundWorker worker = sender as BackgroundWorker;
            
            using (MediaLibrary mediaLibrary = new MediaLibrary())
            {
                foreach (var pa in mediaLibrary.RootPictureAlbum.Albums)
                {
                    if (pa.Pictures.Count > 0)
                    {

                        var pic = (from picture in pa.Pictures
                                       orderby picture.Date descending
                                       select picture).First();
                        
                        worker.ReportProgress(0, new object[] { phoneAlbum, pic });
                        //System.Threading.Thread.Sleep(50);
                    }
                }
            }           
        }

        void bgNasaLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadNasaTodayPictureAlbum();
        }

        void bgPhoneLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            object[] args = e.UserState as object[];
            ChameleonAlbum phoneAlbum = args[0] as ChameleonAlbum;
            Picture picture = args[1] as Picture;

            using (Stream stream = picture.GetImage())
            {
                WriteableBitmap bitmap = JpegHelper.Resize(stream, new Size(200, 200), true);
                phoneAlbum.Add(new PhonePicture()
                {
                    SourceOrigin = SourceOrigin.Phone,
                    AlbumName = picture.Album.Name,
                    ImageSource = bitmap,
                    Name = picture.Name
                });
            }
        }

        void appBarIconBtnSelect_Click(object sender, EventArgs e)
        {
            PivotItem pivotItem = LockScreenPivot.SelectedItem as PivotItem;
            LongListMultiSelector selector = GetSelectorByPivotName(pivotItem.Name);
            selector.EnforceIsSelectionEnabled = !selector.EnforceIsSelectionEnabled;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
                        
            PivotItem pivotItem = LockScreenPivot.SelectedItem as PivotItem;
            LongListMultiSelector selector = GetSelectorByPivotName(pivotItem.Name);
            if (selector != PhonePictureSelector && selector.EnforceIsSelectionEnabled)
            {
                selector.EnforceIsSelectionEnabled = false;
                e.Cancel = true;
            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                ChameleonAlbum album = new ChameleonAlbum();
                PhonePictureSelector.ItemsSource = album;

                bgPhoneLoader.RunWorkerAsync(album);
                bgBingLoader.RunWorkerAsync();
                bgNasaLoader.RunWorkerAsync();
                LoadWebPictureAlbum();
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (PhoneApplicationService.Current.State.ContainsKey(Constants.DOWNLOAD_IMAGE_LIST))
                {
                    ObservableCollection<DownloadItem> downloadList = PhoneApplicationService.Current.State[Constants.DOWNLOAD_IMAGE_LIST] as ObservableCollection<DownloadItem>;
                    PageHelper.ProcessDownloadImageResult(downloadList, GetSelectorBySourceOrigin(downloadList[0].SourceOrigin));
                }
            }
        }       

        private void LoadBingTodayPictureAlbum()
        {
            bingToday = new BingToday();
            bingToday.LoadCompleted += bingToday_LoadCompleted;
            bingToday.Load(0);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                BingTodayProgressBar.Visibility = System.Windows.Visibility.Visible;
            });
        }


        private void LoadNasaTodayPictureAlbum()
        {
            nasaToday = new NasaToday();
            nasaToday.LoadCompleted += nasaToday_LoadCompleted;
            nasaToday.Load();
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NasaTodayProgressBar.Visibility = System.Windows.Visibility.Visible;
            });
        }

        void bingToday_LoadCompleted(object sender, TodayImageResult result)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ChameleonAlbum album = BingTodayPictureSelector.ItemsSource as ChameleonAlbum;
                if (album == null)
                {
                    album = new ChameleonAlbum();
                    BingTodayPictureSelector.ItemsSource = album;
                }

                bool loadBreak = false;

                foreach (var pic in result.Album)
                {
                    bool exists = album.Any(x => {
                        return ((WebPicture) x).Path == ((WebPicture) pic).Path;
                    });

                    if (!exists) album.Add(pic);
                    else loadBreak = true;
                }

                if (!loadBreak)
                {
                    bingToday.Load(result.Index);
                }

                if (result.Index > 0)
                {
                    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = result.Album.Count > 0;
                }
                BingTodayProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            });
        }

        void nasaToday_LoadCompleted(object sender, TodayImageResult result)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ChameleonAlbum album = new ChameleonAlbum();
                NasaTodayPictureSelector.ItemsSource = album;

                foreach (var pic in result.Album)
                {
                    album.Add(pic);
                }

                if (result.Index > 0)
                {
                    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = result.Album.Count > 0;
                }
                NasaTodayProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            });
        }

        private void LoadWebPictureAlbum()
        {
            finder = new Finder();
            finder.FindCompleted += finder_FindCompleted;
        }

        private void finder_FindCompleted(object sender, ChameleonAlbum webAlbum)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                SearchingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                WebPictureSelector.ItemsSource = webAlbum;

                (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = webAlbum.Count > 0;
                if (webAlbum.Count > 0)
                {
                    TxtSearchBingNoData.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    TxtSearchBingNoData.Visibility = System.Windows.Visibility.Visible;
                }
            });
        }

        private void PhonePicture_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            bool isFound = false;
            String albumName = (((sender as Image).Parent as Grid).Children[1] as TextBlock).Text;
            
            using (MediaLibrary mediaLib = new MediaLibrary())
            {
                foreach (Album album in mediaLib.Albums)
                {
                    if (album.Name == albumName)
                    {
                        isFound = true;
                        break;
                    }
                }
            }

            if (!isFound)
            {
                for (int i = 0; i < PhonePictureSelector.ItemsSource.Count; i++)
                {
                    PhonePicture pic = (PhonePicture)PhonePictureSelector.ItemsSource[i];

                    if (pic.AlbumName == albumName)
                    {
                        PhonePictureSelector.ItemsSource.Remove(pic);// 
                    }
                }
            }
        }

        private void WebPicture_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Image image = sender as Image;
            WebPicture picture = image.DataContext as WebPicture;
            LongListMultiSelector selector = picture.SourceOrigin == SourceOrigin.BingToday ? BingTodayPictureSelector : (picture.SourceOrigin == SourceOrigin.Search ? WebPictureSelector : NasaTodayPictureSelector);

            WebImageLoadFailed(image, selector);
        }

        //웹이미지 로딩이 실패한 경우에 대한 처리
        private void WebImageLoadFailed(Image image, LongListMultiSelector selector)
        {
            BitmapImage img = image.Source as BitmapImage;
            string errorUrl = img.UriSource.OriginalString;
            string decodedErrorUrl = HttpUtility.UrlDecode(errorUrl);
            
            for (int i = 0; i < selector.ItemsSource.Count; i++)
            {
                WebPicture pic = (WebPicture)selector.ItemsSource[i];

                if (pic.Path == errorUrl || pic.Path == decodedErrorUrl)
                {
                    selector.ItemsSource.Remove(pic);// 
                }
            }

            image.Source = null;
            img.UriSource = null;
        }

        private void OnBingTodayPictureSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeAppbarIconButton(BingTodayPictureSelector, e.AddedItems.Count, false);
        }

        private void OnNasaTodayPictureSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeAppbarIconButton(NasaTodayPictureSelector, e.AddedItems.Count, false);
        }

        private void OnWebPictureSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeAppbarIconButton(WebPictureSelector, e.AddedItems.Count, false);
        }

        private void OnPhonePictureItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                int count = 0;
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    count = isoStore.GetFileNames().Count(x => x.Contains(Constants.LOCKSCREEN_IMAGE_POSTFIX));
                }
                
                var CurrentPicture = fe.DataContext as PhonePicture;
                NavigationService.Navigate(new Uri(string.Format("/View/AlbumPage.xaml?album={0}&currCnt={1}", 
                    CurrentPicture.AlbumName, count), UriKind.Relative));
            }
        }

        //웹이미지를 전체보기로 실행한다.
        private void OnWebPictureItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            IList downloadList = new List<WebPicture>();
            downloadList.Add(((sender as Grid).DataContext) as WebPicture);
            PageHelper.SetDownloadImageList(downloadList);
            NavigationService.Navigate(new Uri(string.Format("/View/PicturePage.xaml?searchKeyword={0}", searchQuery), UriKind.Relative));
        }
            
        private void btnSearch_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (txtSearch.Text.Trim() == string.Empty)
            {
                MessageBox.Show(AppResources.MsgEnterSearchKeyword);
            }
            else
            {
                string key = "TodaySearchCount";
                string today = DateTime.Today.ToString("yyyyMMdd");

                if (SettingHelper.ContainsKey(key))
                {
                    string ymdCnt = SettingHelper.GetString(key);
                    string[] val = ymdCnt.Split('_');

                    if (val[0] == today)
                    {
                        int cnt = 0;
                        Int32.TryParse(val[1], out cnt);
                        cnt++;

                        SettingHelper.Set(key, string.Format("{0}_{1}", today, cnt), true);

                        if (cnt > 5)
                        {
                            MessageBox.Show(AppResources.MsgSearchMaxCount);
                            return;
                        }
                    }
                    else
                    {
                        SettingHelper.Set(key, string.Format("{0}_{1}", today, 1), true);
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine(DateTime.Today.ToString("yyyyMMdd"));
                    SettingHelper.Set(key, string.Format("{0}_{1}", today, 1), true);
                }

                searchQuery = txtSearch.Text.Trim();
                finder.FindImageUrlsFor(searchQuery);
                SearchingProgressBar.Visibility = System.Windows.Visibility.Visible;
            }
        }
       
        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String pivotName = (e.AddedItems[0] as PivotItem).Name;
            ApplicationBar.IsVisible = false;

            LongListMultiSelector selector = GetSelectorByPivotName(pivotName);
            if (selector != PhonePictureSelector)
            {
                ApplicationBar.IsVisible = true;
                ChameleonAlbum album = selector.ItemsSource as ChameleonAlbum;
                ChangeAppbarIconButton(selector, 0, true);
                (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = (album != null && album.Count > 0);
            }
        }

        private void ChangeAppbarIconButton(LongListMultiSelector selector, int addedCount, bool isPivotChanged)
        {
            if (selector.SelectedItems.Count == 0)
            {
                ApplicationBar.Buttons.Clear();
                ApplicationBar.Buttons.Add(appBarIconBtnSelect);
                ApplicationBar.Buttons.Add(appBarIconBtnSettings);
                return;
            }

            if (selector.SelectedItems.Count <= addedCount || isPivotChanged)
            {
                ApplicationBar.Buttons.Clear();
                ApplicationBar.Buttons.Add(appBarIconBtnAdd);
            }
        }

        #region Utils

        private LongListMultiSelector GetSelectorByPivotName(string pivotName)
        {
            LongListMultiSelector selector = null;
            switch (pivotName)
            {
                case "Bing":
                    selector = BingTodayPictureSelector;
                    break;
                case "Nasa":
                    selector = NasaTodayPictureSelector;
                    break;
                case "Search":
                    selector = WebPictureSelector;
                    break;
                default:
                    selector = PhonePictureSelector;
                    break;
            }
            return selector;
        }

        private LongListMultiSelector GetSelectorBySourceOrigin(SourceOrigin source)
        {
            LongListMultiSelector selector = null;

            switch (source)
            {
                case SourceOrigin.BingToday:
                    selector = BingTodayPictureSelector;
                    break;
                case SourceOrigin.NasaToday:
                    selector = NasaTodayPictureSelector;
                    break;
                case SourceOrigin.Search:
                    selector = WebPictureSelector;
                    break;
                default:
                    selector = PhonePictureSelector;
                    break;
            }
            return selector;
        }
        #endregion
    }
}