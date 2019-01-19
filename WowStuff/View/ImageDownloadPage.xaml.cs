using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ChameleonLib.Resources;
using System.Collections;
using ChameleonLib.Model;
using System.Collections.ObjectModel;
using Microsoft.Phone.Net.NetworkInformation;
using System.Windows.Media.Imaging;
using System.Net.Http;
using ChameleonLib.Helper;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Animation;

namespace ChameleonLib.View
{
    public partial class ImageDownloadPage : PhoneApplicationPage
    {
        private BitmapImage imageDownloader;

        private WebBrowser redirectDownloader;

        private DownloadItem downloadingItem;

        private string searchQuery;

        private bool isCanceledDownload;

        private BackgroundWorker addWorker;

        public ImageDownloadPage()
        {
            InitializeComponent();

            DeviceNetworkInformation.NetworkAvailabilityChanged += DeviceNetworkInformation_NetworkAvailabilityChanged;
            
            imageDownloader = new BitmapImage()
            {
                CreateOptions = BitmapCreateOptions.BackgroundCreation
            };
            imageDownloader.ImageFailed += imageDownloader_ImageFailed;
            imageDownloader.ImageOpened += imageDownloader_ImageOpened;
            imageDownloader.DownloadProgress += imageDownloader_DownloadProgress;

            redirectDownloader = new WebBrowser();
            redirectDownloader.Navigated += redirectDownloader_Navigated;
            redirectDownloader.NavigationFailed += redirectDownloader_NavigationFailed;

            addWorker = new BackgroundWorker();
            addWorker.WorkerReportsProgress = true;
            addWorker.WorkerSupportsCancellation = true;
            addWorker.ProgressChanged += OnProgressChangedAddWorker;
            addWorker.DoWork += OnDoWorkAddWorker;
            addWorker.RunWorkerCompleted += OnRunWorkerCompletedAddWorker;
        }
        
        void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            if (downloadingItem != null)
            {
                downloadingItem.DownloadNetwork = GetNetworkTypeName(e.NotificationType, e.NetworkInterface.InterfaceType);
            }
        }

        private string GetNetworkTypeName(NetworkNotificationType nft, NetworkInterfaceType nit)
        {
            string typeName = string.Empty;
            switch (nft)
            {
                case NetworkNotificationType.CharacteristicUpdate:
                    typeName = GetNetworkInterfaceTypeName(nit);
                    break;
                case NetworkNotificationType.InterfaceConnected:
                    typeName = GetNetworkInterfaceTypeName(nit);
                    break;
                case NetworkNotificationType.InterfaceDisconnected:
                    typeName = AppResources.NetworkStatusNameDisconnect;   //Disconnected Network
                    break;
            }
            return typeName;
        }

        private string GetNetworkInterfaceTypeName(NetworkInterfaceType nit)
        {
            string typeName = string.Empty;

            switch (nit)
            {
                case NetworkInterfaceType.Ethernet:
                    typeName = AppResources.NetworkStatusNamePC; //Desktop Passthru
                    break;
                case NetworkInterfaceType.MobileBroadbandCdma:
                case NetworkInterfaceType.MobileBroadbandGsm:
                    typeName = AppResources.NetworkStatusNameCellular; //Cellular Network
                    break;
                case NetworkInterfaceType.None:
                    typeName = AppResources.NetworkStatusNameNone;     //No Network
                    break;
                case NetworkInterfaceType.Wireless80211:
                    typeName = AppResources.NetworkStatusNameWiFi; //Wi-Fi Network
                    break;

            }
            return typeName;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            ObservableCollection<DownloadItem> items = (ObservableCollection<DownloadItem>)PhoneApplicationService.Current.State[Constants.DOWNLOAD_IMAGE_LIST];

            if (NavigationContext.QueryString.ContainsKey("searchKeyword"))
            {
                searchQuery = NavigationContext.QueryString["searchKeyword"];
            }

            NoDownloadItem.Visibility = System.Windows.Visibility.Collapsed;
            LLSDownload.Visibility = System.Windows.Visibility.Visible;

            if (items[0].SourceOrigin != SourceOrigin.Phone)
            {
                LLSDownload.ItemsSource = items;
                //다운로드 시작
                StartDownload();
            }
            else
            {
                using (MediaLibrary mediaLibrary = new MediaLibrary())
                {
                    using (var album = mediaLibrary.RootPictureAlbum.Albums.First(x => x.Name == items[0].AlumName))
                    {
                        foreach (DownloadItem item in items)
                        {
                            BitmapImage bi = new BitmapImage();
                            bi.SetSource(album.Pictures.First(x => x.Name == item.FileName).GetThumbnail());
                            item.ThumbnailPath = bi;
                        }
                    }
                }

                LLSDownload.ItemsSource = items;
                //저장 시작
                addWorker.RunWorkerAsync(items);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            
            //뒤로 돌아갈때 다운로드 중이면 나머지를 취소하겠냐고 묻는다.
            if ((from element in LLSDownload.ItemsSource as ObservableCollection<DownloadItem>
                 where element.DownloadStatusCode != DownloadStatus.DownloadFailed
                 && element.DownloadStatusCode != DownloadStatus.SaveFailed
                 && element.Visibility == System.Windows.Visibility.Visible
                 select element).Any())
            {
                if ((LLSDownload.ItemsSource[0] as DownloadItem).SourceOrigin != SourceOrigin.Phone)
                {
                    if (!isCanceledDownload)
                    {
                        //일단 무조건 정지 (왜냐하면 UI쓰레드와 다운로드 쓰레드가 다르므로 제어 불가)
                        isCanceledDownload = true;

                        //웹이미지의 취소 프로세스
                        if (MessageBox.Show(AppResources.MsgIsCancleDownloadImage, AppResources.Confirm, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            //취소 상태 변경
                            isCanceledDownload = false;
                        }
                        e.Cancel = true;
                    }
                }
                else
                {
                    //폰이미지의 취소 프로세스
                    if (addWorker.IsBusy)
                    {
                        if (MessageBox.Show(AppResources.MsgIsCancleAddImage, AppResources.Confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            addWorker.CancelAsync();
                        }
                        e.Cancel = true;
                    }
                }
            }
        }
        
        private void GoBackPage()
        {
            NavigationService.GoBack();
        }

        #region AddWorker Backgound Event Handler
        private void OnProgressChangedAddWorker(object worker, ProgressChangedEventArgs changed)
        {
            DownloadItem item = changed.UserState as DownloadItem;
            string prgsMsg = string.Empty;
            int percent = item.DownloadStatusCode == DownloadStatus.SaveFailed ? -1 : changed.ProgressPercentage;

            switch (percent)
            {
                case -1:
                    prgsMsg = AppResources.ImageStatusFailSave;
                    item.DownloadStatusCode = DownloadStatus.SaveFailed;
                    break;
                case -2:
                    prgsMsg = AppResources.ImageStatusTimeOut;
                    item.DownloadStatusCode = DownloadStatus.SaveFailed;
                    break;
                case 1:
                    prgsMsg = AppResources.ImageStatusAdd;
                    item.DownloadStatusCode = DownloadStatus.Pending;
                    break;
                case 10:
                case 20:
                case 50:
                case 65:
                case 80:
                case 90:
                    prgsMsg = AppResources.ImageStatusAdding;
                    item.DownloadStatusCode = DownloadStatus.Saving;
                    break;
                case 100:
                    prgsMsg = AppResources.ImageStatusAdded;
                    item.DownloadStatusCode = DownloadStatus.Completed;
                    break;
                default:
                    prgsMsg = AppResources.ImageStatusFailAdd;
                    item.DownloadStatusCode = DownloadStatus.SaveFailed;
                    break;
            }
            item.DownloadStatus = prgsMsg;
            item.DownloadRate = changed.ProgressPercentage;

        }

        private void OnDoWorkAddWorker(object worker, DoWorkEventArgs work)
        {
            ManualResetEvent done = new ManualResetEvent(false);
            ObservableCollection<DownloadItem> downloadItemList = work.Argument as ObservableCollection<DownloadItem>;

            using (MediaLibrary lib = new MediaLibrary())
            {
                using (var picAlbum = lib.RootPictureAlbum.Albums.First(x => x.Name == downloadItemList[0].AlumName))
                {
                    for (int i = 0; i < downloadItemList.Count; i++)
                    {
                        if (addWorker.CancellationPending)
                        {
                            work.Cancel = true;
                            return;
                        }

                        addWorker.ReportProgress(1, downloadItemList[i]);

                        using (MemoryStream ms = new MemoryStream())
                        {
                            //저장할 파일명 생성
                            string name = FileHelper.GetUniqueFileName(Constants.LOCKSCREEN_IMAGE_POSTFIX);
                            WriteableBitmap bitmap = null;

                            //복사본 생성 => 10%
                            done.Reset();
                            Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    using (var libPic = picAlbum.Pictures.First(x => x.Name == downloadItemList[i].FileName))
                                    {
                                        using (Stream stream = libPic.GetImage())
                                        {
                                            stream.CopyTo(ms);
                                            bitmap = BitmapFactory.New(0, 0).FromStream(ms);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    downloadItemList[i].DownloadStatusCode = DownloadStatus.SaveFailed;
                                    System.Diagnostics.Debug.WriteLine(ex.Message);
                                }
                                finally
                                {
                                    done.Set();
                                }
                            });
                            done.WaitOne();
                            addWorker.ReportProgress(10, downloadItemList[i]);

                            //축소 가능한 이미지라면 축소 => 20%
                            Resize(bitmap, ms, ResolutionHelper.CurrentResolution, false, downloadItemList[i], done);
                            addWorker.ReportProgress(20, downloadItemList[i]);

                            //이미지 파일 저장 => 50%
                            Save(name, ms, downloadItemList[i], done);
                            addWorker.ReportProgress(50, downloadItemList[i]);
                            //세로 이미지이면 
                            if (bitmap.PixelWidth < bitmap.PixelHeight)
                            {
                                //락스크린용 이미지 축소 => 65%
                                Resize(bitmap, ms, LockscreenHelper.Size, true, downloadItemList[i], done);
                                addWorker.ReportProgress(65, downloadItemList[i]);

                                //락스크린용 이미지 저장 => 80%
                                Save(name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_READY_POSTFIX), ms, downloadItemList[i], done);
                                addWorker.ReportProgress(80, downloadItemList[i]);
                            }

                            //썸네일 만들기 => 90%
                            Resize(bitmap, ms, LockscreenHelper.ThumnailSize, true, downloadItemList[i], done);
                            addWorker.ReportProgress(90, downloadItemList[i]);

                            //썸네일 저장 =>100%
                            if (bitmap.PixelWidth > LockscreenHelper.ThumnailSize.Width || bitmap.PixelHeight > LockscreenHelper.ThumnailSize.Height)
                            {
                                Save(name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_THUMNAIL_POSTFIX), ms, downloadItemList[i], done);
                            }
                            addWorker.ReportProgress(100, downloadItemList[i]);

                            done.Reset();
                            Dispatcher.BeginInvoke(async () =>
                            {
                                await Task.Delay(200);
                                downloadItemList[i].Visibility = System.Windows.Visibility.Collapsed;
                                done.Set();
                            });
                            done.WaitOne();

                        }
                    }
                }
            }
        }

        private void OnRunWorkerCompletedAddWorker(object worker, RunWorkerCompletedEventArgs completed)
        {
            if (completed.Cancelled)
            {
                ChangeCancelStatus();
                MessageBox.Show(AppResources.MsgCanceledAddImage);
            }
            toggleNoMsg();
        }

        private void toggleNoMsg()
        {
            if (!(LLSDownload.ItemsSource as ObservableCollection<DownloadItem>).Any(x => x.Visibility == System.Windows.Visibility.Visible))
            {
                NoDownloadItem.Visibility = System.Windows.Visibility.Visible;
                LLSDownload.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private bool Resize(WriteableBitmap wb, Stream stream, Size size, bool isCenterCrop, DownloadItem item, ManualResetEvent done)
        {
            bool isResized = false;
            done.Reset();
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    isResized = JpegHelper.Resize(wb, stream, size, isCenterCrop);
                }
                catch (Exception ex)
                {
                    item.DownloadStatusCode = DownloadStatus.SaveFailed;
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {
                    done.Set();
                }
            });
            done.WaitOne();
            return isResized;
        }

        private void Save(string name, Stream stream, DownloadItem item, ManualResetEvent done)
        {
            done.Reset();
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    FileHelper.SaveImage(name, stream);
                }
                catch (Exception ex)
                {
                    item.DownloadStatusCode = DownloadStatus.SaveFailed;
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {
                    done.Set();
                }
            });
            done.WaitOne();
            GC.Collect();
        }
        #endregion

        private void ChangeCancelStatus()
        {
            foreach (DownloadItem item in LLSDownload.ItemsSource)
            {
                if (item.DownloadStatusCode == DownloadStatus.Pending)
                {
                    if (item.SourceOrigin == SourceOrigin.Phone ||
                        (item.SourceOrigin != SourceOrigin.Phone && item.Guid != downloadingItem.Guid))
                    {
                        item.DownloadStatus = AppResources.ImageStatusCanceled;
                    }
                }
            }
        }

        private async void StartDownload()
        {
            //취소되었다면 진행하지 않음.
            if (isCanceledDownload)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ChangeCancelStatus();
                    toggleNoMsg();
                    MessageBox.Show(AppResources.MsgCanceledDownloadImage);
                });
                return;
            }

            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                Dispatcher.BeginInvoke(() => 
                {
                    if (MessageBox.Show(AppResources.MsgRequiredDataNetwork, AppResources.Confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        new Microsoft.Phone.Tasks.ConnectionSettingsTask().Show();
                    }
                });
                return;
            }

            //상태코드가 pending인 것중 첫번째 아이템을 다운로드 대상으로 한다. 없으면 종료
            var downloadItems = from element in LLSDownload.ItemsSource as ObservableCollection<DownloadItem>
                               where element.DownloadStatusCode == DownloadStatus.Pending
                               select element;

            if (!downloadItems.Any())
            {
                toggleNoMsg();
                //더이상 다운로드할 데이터가 없으므로 다운로드를 종료
                return;
            }

            downloadingItem = downloadItems.First();
                        
            //네트워크 명 설정
            downloadingItem.DownloadNetwork = GetNetworkInterfaceTypeName(NetworkInterface.NetworkInterfaceType);

            string target = downloadingItem.DownloadPath as string;
            //문제의 url들 1. 리다이렉트, 2.첨부파일방식
            //string target = "http://wagle.joinsmsn.com/app/files/attach/images/197089/169/211/" + System.Net.HttpUtility.UrlEncode("신민아~1.JPG");
            //string target = "http://wagle.joinsmsn.com/app/files/attach/images/197089/169/211/신민아~1.JPG";
            //string target = "http://dcimg1.dcinside.com/viewimage.php?id=3ea8ca3f&no=29bcc427b68577a16fb3dab004c86b6ff17ca063309f3ddcd2c4de2037a14c10cba6af693ab88cbb344704ace267746918fce8bb6b9369381e14a81da76286fa332332446a2bb353b91b35f5b338c95aa69b68b054";

            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                };
                
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    using (var picResponse = await httpClient.GetAsync(target, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (picResponse.IsSuccessStatusCode)
                        {
                            //정상적인 경우이므로 다운로드 시작
                            imageDownloader.UriSource = new Uri(target);
                        }
                        else if (picResponse.StatusCode == HttpStatusCode.Redirect
                                || picResponse.StatusCode == HttpStatusCode.RedirectKeepVerb
                                || picResponse.StatusCode == HttpStatusCode.RedirectMethod)
                        {
                            //리다이렉트인 경우로 한글의 경우 처리가 안되는 곳들이 있다. 그렇기 때문에 웹브라우저 컨트롤을 사용하여 변경된 주소를 얻어 온다.
                            redirectDownloader.Source = new Uri(target);
                        }
                        else if (picResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            //못찾는 경우 블랙 리스트 추가
                            //경찰청 차단의 경우 테스트 해 볼필요또한 있다.... 
                            DownloadFailHandler();
                        }
                        else
                        {
                            //실패시 다음 다운로드 항목으로 넘어간다.
                            DownloadFailHandler();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                //실패시 다음 다운로드 항목으로 넘어간다.
                DownloadFailHandler();
            }
        }

        private void DownloadFailHandler()
        {
            downloadingItem.DownloadStatus = AppResources.ImageStatusFailDownload;
            downloadingItem.DownloadStatusCode = DownloadStatus.DownloadFailed;

            if (downloadingItem.SourceOrigin == SourceOrigin.Search)
            {
                downloadingItem.DownloadStatus = string.Format("{0}. {1}", downloadingItem.DownloadStatus, AppResources.MsgAddDomainFilter);
                downloadingItem.IsActiveDomainFilter = true;
            }

            //다음 다운로드 시작
            StartDownload();
        }

        private async void AsyncCollapseDownloadItem()
        {
            await Task.Delay(200);
            downloadingItem.Visibility = System.Windows.Visibility.Collapsed;
        }
        
        private async void DelayStartDownload()
        {
            await Task.Delay(300);
            StartDownload();
        }

        private void imageDownloader_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //실패시 다음 다운로드 항목으로 넘어간다.
            DownloadFailHandler();
        }

        private void imageDownloader_ImageOpened(object sender, RoutedEventArgs e)
        {           
            try
            {
                WriteableBitmap wb = new WriteableBitmap(sender as BitmapImage);
                JpegHelper.Save(FileHelper.GetUniqueFileName(Constants.LOCKSCREEN_IMAGE_POSTFIX), wb);
                downloadingItem.DownloadStatus = AppResources.ImageStatusAdded;
                downloadingItem.DownloadStatusCode = DownloadStatus.Completed;
                //다운로드 완료된 파일을 숨김 처리
                AsyncCollapseDownloadItem();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                downloadingItem.DownloadStatus = AppResources.ImageStatusFailAdd;
                downloadingItem.DownloadStatusCode = DownloadStatus.SaveFailed;
            }
            finally
            {
                downloadingItem.DownloadNetwork = string.Empty;
                DelayStartDownload();
            }
        }

        private void imageDownloader_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            downloadingItem.DownloadRate = e.Progress;
            if (downloadingItem.DownloadRate < 100)
            {
                if (downloadingItem.DownloadStatusCode == DownloadStatus.Pending)
                {
                    downloadingItem.DownloadStatus = AppResources.ImageStatusDownloading;
                }
            }
            else
            {
                if (downloadingItem.DownloadStatusCode == DownloadStatus.Downloading)
                {
                    downloadingItem.DownloadStatus = AppResources.ImageStatusDownloaded;
                    downloadingItem.DownloadStatusCode = DownloadStatus.Downloaded;
                    downloadingItem.DownloadNetwork = string.Empty;
                }
                else
                {
                    downloadingItem.DownloadStatus = AppResources.ImageStatusAdding;
                    downloadingItem.DownloadStatusCode = DownloadStatus.Saving;
                }
            }
        }

        void redirectDownloader_Navigated(object sender, NavigationEventArgs e)
        {
            imageDownloader.UriSource = e.Uri;
        }

        void redirectDownloader_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            //실패시 다음 다운로드 항목으로 넘어간다.
            DownloadFailHandler();
        }

        private void LLSDownload_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DownloadItem item = (sender as LongListSelector).SelectedItem as DownloadItem;
            if (item != null && item.IsActiveDomainFilter)
            {
                if (MessageBox.Show(AppResources.MsgWrongImage + AppResources.MsgAllowDomainFilter, AppResources.DomainFilter, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    BlackList blackList = new BlackList();
                    blackList.Add(new BlackDomain()
                    {
                        AddedDateTime = DateTime.Now,
                        BlackListMode = BlackListMode.Domain,
                        Host = DomainHelper.GetDomainFromUrl(downloadingItem.DownloadPath as string),
                        SearchKeyword = searchQuery
                    });
                    blackList.SaveData();
                    //화면에서 삭제
                    AsyncCollapseDownloadItem();
                    item.BlackListMode = BlackListMode.Domain;
                }
            }
        }
    }
}