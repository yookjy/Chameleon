using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ChameleonLib.Helper;
using ChameleonLib.Resources;
using ChameleonLib.Model;
using System.Net.Http;
using System.Collections.ObjectModel;

namespace Chameleon.View
{
    public partial class PicturePage : PhoneApplicationPage
    {
        private BitmapImage imageDownloader;

        private WebBrowser redirectDownloader;

        const int SCALE_MAX = 3;

        private Point currentScale;

        private Size imageSize;

        private Rect innerRect;

        private DragDirection protectDirection;

        private bool canZoomOut;

        private string searchQuery;

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ImgDetail.Source = null;
        }

        public PicturePage()
        {
            InitializeComponent();

            currentScale = new Point(0, 0);
            canZoomOut = true;

            imageSize = new Size();
            imageSize.Width = (double)ResolutionHelper.CurrentResolution.Width / App.Current.Host.Content.ScaleFactor * 100;
            imageSize.Height = (double)ResolutionHelper.CurrentResolution.Height / App.Current.Host.Content.ScaleFactor * 100;

            WriteableBitmap wb = new WriteableBitmap((int)imageSize.Width, (int)imageSize.Height);
            ImgFrame.Source = wb;
            
            //페이지의 ApplicationBar를 ApplicationBar의 새 인스턴스로 설정합니다.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.8;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = false;

            ApplicationBarIconButton appBarIconBtnAdd = new ApplicationBarIconButton();
            appBarIconBtnAdd.IconUri = PathHelper.GetPath("appbar.list.add.below.png");
            appBarIconBtnAdd.Text = AppResources.AppbarMenuAddLockscreen;
            appBarIconBtnAdd.Click += appBarIconBtnAdd_Click;
            appBarIconBtnAdd.IsEnabled = false;

            ApplicationBarIconButton appBarIconBtnFit = new ApplicationBarIconButton();
            appBarIconBtnFit.IconUri = PathHelper.GetPath("appbar.corner.png");
            appBarIconBtnFit.Text = AppResources.AppbarMenuFitLockscreen;
            appBarIconBtnFit.Click += appBarIconBtnFit_Click;
            appBarIconBtnFit.IsEnabled = false;

            ApplicationBar.Buttons.Add(appBarIconBtnFit);
            ApplicationBar.Buttons.Add(appBarIconBtnAdd);

            //이미지 다운로더
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
        }

        private void appBarIconBtnFit_Click(object sender, EventArgs e)
        {
            FitImage();
        }

        void appBarIconBtnAdd_Click(object sender, EventArgs e)
        {
            if (Constants.LOCKSCREEN_MAX_COUNT - LockscreenHelper.CurrentListCount == 0)
            {
                MessageBox.Show(string.Format(AppResources.MsgFailAddMaxCount, Constants.LOCKSCREEN_MAX_COUNT));
                return;
            }

            try
            {
                WriteableBitmap bitmap = new WriteableBitmap(ImgDetail.Source as BitmapImage);
                JpegHelper.Save(FileHelper.GetUniqueFileName(Constants.LOCKSCREEN_IMAGE_POSTFIX), bitmap);
                MessageBox.Show(AppResources.MsgSuccessAddLockscreen);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(AppResources.MsgFailAddLockscreen, ex.Message));
            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (searchQuery == null && NavigationContext.QueryString.ContainsKey("searchKeyword"))
            {
                searchQuery = NavigationContext.QueryString["searchKeyword"];
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                ImgFrame.SizeChanged += OnSizeChangedImgFrame;
                ImgDetail.LayoutUpdated += OnLayoutUpdatedImgDetail;
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                LoadPicture();
            }
        }

        private void LoadPhonePicture(DownloadItem downloadItem)
        {
            using (MediaLibrary mediaLibrary = new MediaLibrary())
            {
                using (Picture picture = mediaLibrary.RootPictureAlbum.Albums.First(x =>
                    x.Name == downloadItem.AlumName).Pictures.First(p => p.Name == downloadItem.FileName))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(picture.GetImage());
                    ImgDetail.Source = bitmap;
                    imgName.Text = picture.Name;
                    EnableApplicationBar();
                }

                
                
                //var savedPictures = mediaLibrary.SavedPictures;

                //foreach (var album in picalbum.Albums)
                //{
                //    if (album.Name == picture.AlbumName)
                //    {
                //        foreach (var pic in album.Pictures)
                //        {
                //            if (pic.Name == picture.Name)
                //            {
                //                BitmapImage bitmap = new BitmapImage();
                //                bitmap.SetSource(pic.GetImage());
                //                ImgDetail.Source = bitmap;
                //                imgName.Text = pic.Name;
                //                EnableApplicationBar();
                //                break;
                //            }
                //        }
                //        break;
                //    }
                //}
            }

        }

        private async void LoadWebPicture(DownloadItem downloadItem)
        {
            try
            {
                string target = downloadItem.DownloadPath as string;
                //문제의 url들 1. 리다이렉트, 2.첨부파일방식
                //string target = "http://wagle.joinsmsn.com/app/files/attach/images/197089/169/211/" + System.Net.HttpUtility.UrlEncode("신민아~1.JPG");
                //string target = "http://dcimg1.dcinside.com/viewimage.php?id=3ea8ca3f&no=29bcc427b68577a16fb3dab004c86b6ff17ca063309f3ddcd2c4de2037a14c10cba6af693ab88cbb344704ace267746918fce8bb6b9369381e14a81da76286fa332332446a2bb353b91b35f5b338c95aa69b68b054";

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
            //화면 비활성화
            LoadingProgressBar.Visibility = System.Windows.Visibility.Visible;
            ApplicationBar.IsMenuEnabled = false;
        }

        private void DownloadFailHandler()
        {
            ObservableCollection<DownloadItem> downloadItemList
                = PhoneApplicationService.Current.State[Constants.DOWNLOAD_IMAGE_LIST] as ObservableCollection<DownloadItem>;
            DownloadItem downloadItem = downloadItemList[0];

            if (downloadItem.SourceOrigin == SourceOrigin.Search)
            {
                if (MessageBox.Show(AppResources.MsgWrongImage + AppResources.MsgAllowDomainFilter, AppResources.DomainFilter, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    BlackList blackList = new BlackList();
                    blackList.Add(new BlackDomain()
                    {
                        AddedDateTime = DateTime.Now,
                        BlackListMode = BlackListMode.Domain,
                        Host = DomainHelper.GetDomainFromUrl(downloadItem.DownloadPath as string),
                        SearchKeyword = searchQuery == null ? string.Empty : searchQuery
                    });
                    blackList.SaveData();
                }
            }
            else
            {
                MessageBox.Show(AppResources.MsgWrongImage);
            }
            NavigationService.GoBack();
        }

        private void imageDownloader_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //실패시 다음 다운로드 항목으로 넘어간다.
            DownloadFailHandler();
        }

        private void imageDownloader_ImageOpened(object sender, RoutedEventArgs e)
        {
            ImgDetail.Source = sender as BitmapImage;
            EnableApplicationBar();
        }

        private void imageDownloader_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            if (e.Progress < 100)
            {
                LoadingText.Text = AppResources.Loading + " " + e.Progress + "%";
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

        private void EnableApplicationBar()
        {
            //메뉴 활성화
            LoadingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            foreach (ApplicationBarIconButton button in ApplicationBar.Buttons)
            {
                button.IsEnabled = true;
            }
        }

        #region Gesture event handler


        private void ImgFrame_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            currentScale.X = transform.ScaleX;
            currentScale.Y = transform.ScaleY;
        }

        private void ImgFrame_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation == null)
            {
                //Drag
                double horizontalChange = e.DeltaManipulation.Translation.X;
                double verticalChange = e.DeltaManipulation.Translation.Y;
                //좌측으로
                if (horizontalChange < 0 && protectDirection != DragDirection.Left)
                {
                    transform.TranslateX += horizontalChange;
                    transform.TranslateY += verticalChange;
                    protectDirection = DragDirection.None;
                }
                //우측으로
                else if (horizontalChange > 0 && protectDirection != DragDirection.Right)
                {
                    transform.TranslateX += horizontalChange;
                    transform.TranslateY += verticalChange;
                    protectDirection = DragDirection.None;
                }
                else if (verticalChange < 0 && protectDirection != DragDirection.Top)
                {
                    //위로
                    transform.TranslateX += horizontalChange;
                    transform.TranslateY += verticalChange;
                    protectDirection = DragDirection.None;
                }
                else if (verticalChange > 0 && protectDirection != DragDirection.Bottom)
                {
                    //아래로
                    transform.TranslateX += horizontalChange;
                    transform.TranslateY += verticalChange;
                    protectDirection = DragDirection.None;
                }
                else
                {
                    return;
                }

                //일단 움직이면 축소 가능하도록 설정
                canZoomOut = true;

                GeneralTransform gtf = ImgDetail.TransformToVisual(ImgFrame);
                Point pt = gtf.Transform(new Point(ImgDetail.ActualWidth, ImgDetail.ActualHeight));
                //좌측선
                if (pt.X < innerRect.Left)
                {
                    transform.TranslateX += innerRect.Left - pt.X;
                    protectDirection = DragDirection.Left;
                    canZoomOut = false;
                }
                //상단선
                if (pt.Y < innerRect.Top)
                {
                    transform.TranslateY += innerRect.Top - pt.Y;
                    protectDirection = DragDirection.Top;
                    canZoomOut = false;
                }

                gtf = ImgFrame.TransformToVisual(ImgDetail);
                pt = gtf.Transform(new Point(innerRect.Left + innerRect.Width, innerRect.Top + innerRect.Height));

                //우측선
                if (pt.X < 0)
                {
                    transform.TranslateX += pt.X;
                    protectDirection = DragDirection.Right;
                    canZoomOut = false;
                }
                //하단선
                if (pt.Y < 0)
                {
                    transform.TranslateY += pt.Y;
                    protectDirection = DragDirection.Bottom;
                    canZoomOut = false;
                }
            }
            else
            {
                double distanceRatio = e.PinchManipulation.CumulativeScale;
                //Pinch
                Size scale = new Size(currentScale.X * distanceRatio, currentScale.Y * distanceRatio);
                bool isZoomIn = scale.Width > transform.ScaleX;

                if (isZoomIn)
                {
                    canZoomOut = true;
                    protectDirection = DragDirection.None;
                }

                if (scale.Width <= SCALE_MAX && (canZoomOut || isZoomIn))
                {
                    //축소일때 최소영역보다 이하이면 축소하지 않음
                    if (ImgDetail.ActualWidth * scale.Width >= innerRect.Width / 2
                        && ImgDetail.ActualHeight * scale.Height >= innerRect.Height / 2)
                    {
                        transform.ScaleX = scale.Width;
                        transform.ScaleY = scale.Height;

                        Point inPoint = new Point(innerRect.Left + innerRect.Width, innerRect.Top + innerRect.Height);
                        CompensationPinch(distanceRatio, ImgFrame, ImgDetail, inPoint, 0, true);
                        CompensationPinch(distanceRatio, ImgFrame, ImgDetail, inPoint, 0, false);

                        inPoint = new Point(ImgDetail.ActualWidth, ImgDetail.ActualHeight);
                        CompensationPinch(distanceRatio, ImgDetail, ImgFrame, inPoint, innerRect.Left, true);
                        CompensationPinch(distanceRatio, ImgDetail, ImgFrame, inPoint, innerRect.Top, false);
                    }
                }
            }
        }

        private void CompensationPinch(double zoonRatio, UIElement element, UIElement visualElement, Point inPoint, double compareValue, bool isHorizontal)
        {
            if (canZoomOut)
            {
                GeneralTransform gtf = element.TransformToVisual(visualElement);
                Point pt = gtf.Transform(inPoint);

                if ((isHorizontal ? pt.X : pt.Y) < compareValue)
                {
                    int dv = 100;
                    double dr = zoonRatio / dv;
                    for (int i = 0; i < dv; i++)
                    {
                        transform.ScaleX += currentScale.X * dr;
                        transform.ScaleY += currentScale.Y * dr;

                        gtf = element.TransformToVisual(visualElement);
                        pt = gtf.Transform(inPoint);

                        if ((isHorizontal ? pt.X : pt.Y) >= compareValue)
                        {
                            canZoomOut = false;
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        #region Image event handler

        private void OnSizeChangedImgFrame(object sender, SizeChangedEventArgs e)
        {
            WriteableBitmap wb = ImgFrame.Source as WriteableBitmap;
            imageSize = e.NewSize;
            Color color = Color.FromArgb((byte)(255 * 0.7), 0, 0, 0);
            double frameRation = 0.5;
            double innerWidth = imageSize.Width * frameRation;
            double innerHeight = imageSize.Height * frameRation;
            innerRect = new Rect(imageSize.Width * (1 - frameRation) / 2, imageSize.Height * (1 - frameRation) / 2, innerWidth, innerHeight);

            LoadPicture();
        }

        private void LoadPicture()
        {
            ObservableCollection<DownloadItem> downloadItemList 
                = PhoneApplicationService.Current.State[Constants.DOWNLOAD_IMAGE_LIST] as ObservableCollection<DownloadItem>;
            DownloadItem downloadItem = downloadItemList[0];
            switch (downloadItem.SourceOrigin)
            {
                case SourceOrigin.Search:
                case SourceOrigin.BingToday:
                case SourceOrigin.NasaToday:
                    LoadWebPicture(downloadItem);
                    break;
                default:
                    LoadPhonePicture(downloadItem);
                    break;
            }
        }

        private void OnLayoutUpdatedImgDetail(object sender, EventArgs e)
        {
            FitImage();
        }

        private void FitImage()
        {
            Size size = ImgDetail.RenderSize;

            if (size.Width != 0 && size.Height != 0)
            {
                transform.ScaleX = 1;
                transform.ScaleY = 1;

                //x축 좌표가 화면의 중앙에 오도록 설정
                double gap = size.Width - innerRect.Width;
                transform.TranslateX = innerRect.Left - gap / 2;

                //y축 좌표가 화면의 중앙에 오도록 설정
                gap = size.Height - innerRect.Height;
                transform.TranslateY = innerRect.Top - gap / 2;
            }
        }

        #endregion
    }
}