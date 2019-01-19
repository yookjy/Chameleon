using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;
using ChameleonLib.Helper;
using System.IO;
using System.Collections;
using System.ComponentModel;
using ChameleonLib.Resources;
using ChameleonLib.Model;
using System.Threading;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Chameleon.View.Helper.Helper;

namespace Chameleon.View
{
    public partial class AlbumPage : PhoneApplicationPage
    {
        private ApplicationBarIconButton appBarIconBtnSelect;

        private ApplicationBarIconButton appBarIconBtnAdd;

        private BackgroundWorker bgLoader;

        private string albumName;

        public AlbumPage()
        {
            InitializeComponent();
            
            //페이지의 ApplicationBar를 ApplicationBar의 새 인스턴스로 설정합니다.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.8;
            ApplicationBar.IsVisible = false;
            ApplicationBar.IsMenuEnabled = false;

            appBarIconBtnSelect = new ApplicationBarIconButton();
            appBarIconBtnSelect.IconUri = PathHelper.GetPath("appbar.list.check.png");
            appBarIconBtnSelect.Text = AppResources.AppbarMenuSelect;
            appBarIconBtnSelect.Click += appBarIconBtnSelect_Click;

            appBarIconBtnAdd = new ApplicationBarIconButton();
            appBarIconBtnAdd.IconUri = PathHelper.GetPath("appbar.list.add.below.png");
            appBarIconBtnAdd.Text = AppResources.AppbarMenuAddLockscreen;
            appBarIconBtnAdd.Click += appBarIconBtnAdd_Click;

            ApplicationBar.Buttons.Add(appBarIconBtnSelect);

            bgLoader = new BackgroundWorker();
            bgLoader.WorkerReportsProgress = true;
            bgLoader.DoWork += bgLoader_DoWork;
            bgLoader.ProgressChanged += bgLoader_ProgressChanged;
            bgLoader.RunWorkerCompleted += bgLoader_RunWorkerCompleted;
        }
        
        void bgLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string albumName = e.Argument as string;

            using (MediaLibrary mediaLibrary = new MediaLibrary())
            {
                using (var album = mediaLibrary.RootPictureAlbum.Albums.First(x => x.Name == albumName))
                {
                    foreach (var pics in album.Pictures)
                    {
                        worker.ReportProgress(0, pics);
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
        }

        void bgLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Picture pics = e.UserState as Picture;
            using (Stream stream = pics.GetThumbnail())
            {
                BitmapImage thumb = new BitmapImage();
                thumb.DecodePixelHeight = 110;
                thumb.SetSource(stream);

                AlbumSelector.ItemsSource.Add(new PhonePicture()
                {
                    Guid = Guid.NewGuid(),
                    SourceOrigin = SourceOrigin.Phone,
                    AlbumName = pics.Album.Name,
                    Name = pics.Name,
                    ThumbnailImageSource = thumb
                });
            }
        }

        void bgLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (AlbumSelector.ItemsSource.Count > 0)
            {
                ApplicationBar.IsVisible = true;
            }
        }

        void appBarIconBtnSelect_Click(object sender, EventArgs e)
        {
            AlbumSelector.EnforceIsSelectionEnabled = !AlbumSelector.IsSelectionEnabled;
        }

        void appBarIconBtnAdd_Click(object sender, EventArgs e)
        {
            if (AlbumSelector.SelectedItems.Count > 0)
            {
                if (LockscreenHelper.CurrentListCount + AlbumSelector.SelectedItems.Count > Constants.LOCKSCREEN_MAX_COUNT)
                {
                    MessageBox.Show(string.Format(AppResources.MsgFailAddMaxCount, Constants.LOCKSCREEN_MAX_COUNT));
                    return;
                }
                PageHelper.SetDownloadImageList(AlbumSelector.SelectedItems);
                NavigationService.Navigate(new Uri(string.Format("/View/ImageDownloadPage.xaml"), UriKind.Relative));
            }
        }
        
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (AlbumSelector.IsSelectionEnabled)
            {
                AlbumSelector.EnforceIsSelectionEnabled = false;
                e.Cancel = true;
            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                AlbumSelector.ItemsSource = new ChameleonAlbum();
                albumName = NavigationContext.QueryString["album"];
                txtAlbumName.Text = albumName;
                
                bgLoader.RunWorkerAsync(txtAlbumName.Text);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (PhoneApplicationService.Current.State.ContainsKey(Constants.DOWNLOAD_IMAGE_LIST))
                {
                    ObservableCollection<DownloadItem> downloadList = PhoneApplicationService.Current.State[Constants.DOWNLOAD_IMAGE_LIST] as ObservableCollection<DownloadItem>;
                    PageHelper.ProcessDownloadImageResult(downloadList, AlbumSelector);
                }
            }
        }

        private void OnAlbumPictureTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PhonePicture picture = ((sender as Grid).DataContext) as PhonePicture;
            ObservableCollection<PhonePicture> downloadList = new ObservableCollection<PhonePicture>();
            downloadList.Add(picture);

            PageHelper.SetDownloadImageList(downloadList);
            NavigationService.Navigate(new Uri("/View/PicturePage.xaml", UriKind.Relative));
        }

        private void OnAlbumSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlbumSelector.SelectedItems.Count > 0 && ApplicationBar.Buttons[0].Equals(appBarIconBtnSelect))
            {
                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBar.Buttons.Add(appBarIconBtnAdd);
                appBarIconBtnAdd.IsEnabled = true;
            }
            else if (AlbumSelector.SelectedItems.Count == 0 && ApplicationBar.Buttons[0].Equals(appBarIconBtnAdd))
            {
                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBar.Buttons.Add(appBarIconBtnSelect);
            }
        }

        
    }
}