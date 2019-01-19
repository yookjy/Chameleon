using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ChameleonLib.Helper;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Resources;
using ChameleonLib.Converter;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Model;
using System.Threading;
using ChameleonLib.Api.Calendar;
using System.Collections.Generic;
using Microsoft.Phone.UserData;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace Chameleon.View
{
    enum DragDirection
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

    public partial class PictureEditPage : PhoneApplicationPage
    {
        const int SCALE_MAX = 10;

        private Point currentScale;

        private IApplicationBar backupAppBar;

        private Size imageSize;

        private Rect innerRect;

        private DragDirection protectDirection;
        
        private bool canZoomOut;

        public PictureEditPage()
        {
            InitializeComponent();

            currentScale = new Point(0, 0);
            canZoomOut = true;
            
            imageSize = new Size();
            imageSize.Width = (double)ResolutionHelper.CurrentResolution.Width / App.Current.Host.Content.ScaleFactor * 100;
            imageSize.Height = (double)ResolutionHelper.CurrentResolution.Height / App.Current.Host.Content.ScaleFactor * 100;

            WriteableBitmap wb = new WriteableBitmap((int)imageSize.Width, (int)imageSize.Height);
            ImgFrame.Source = wb;

            ImgFrame.SizeChanged += OnSizeChangedImgFrame;
            ImgDetail.LayoutUpdated += OnLayoutUpdatedImgDetail;
            
            //페이지의 ApplicationBar를 ApplicationBar의 새 인스턴스로 설정합니다.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.8;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = false;

            ApplicationBarIconButton appBarIconBtnCrop = new ApplicationBarIconButton();
            appBarIconBtnCrop.IconUri = PathHelper.GetPath("appbar.crop.png");
            appBarIconBtnCrop.Text = AppResources.Crop;
            appBarIconBtnCrop.Click += appBarIconBtnCrop_Click;

            ApplicationBarIconButton appBarIconBtnTransform = new ApplicationBarIconButton();
            appBarIconBtnTransform.IconUri = PathHelper.GetPath("appbar.transform.rotate.clockwise.png");
            appBarIconBtnTransform.Text = AppResources.Transform;
            appBarIconBtnTransform.Click += appBarIconBtnTransform_Click;

            ApplicationBarIconButton appBarIconBtnSave = new ApplicationBarIconButton();
            appBarIconBtnSave.IconUri = PathHelper.GetPath("appbar.save.png");
            appBarIconBtnSave.Text = AppResources.Save;
            appBarIconBtnSave.Click += appBarIconBtnSave_Click;

            ApplicationBarIconButton appBarIconBtnPreview = new ApplicationBarIconButton();
            appBarIconBtnPreview.IconUri = PathHelper.GetPath("appbar.windowsphone.png");
            appBarIconBtnPreview.Text = AppResources.Preview;
            appBarIconBtnPreview.Click += appBarIconBtnPreview_Click;

            ApplicationBar.Buttons.Add(appBarIconBtnCrop);
            ApplicationBar.Buttons.Add(appBarIconBtnTransform);
            ApplicationBar.Buttons.Add(appBarIconBtnSave);
            ApplicationBar.Buttons.Add(appBarIconBtnPreview);

            backupAppBar = new ApplicationBar();
            backupAppBar.Mode = ApplicationBarMode.Default;
            backupAppBar.Opacity = 0.9;
            backupAppBar.IsVisible = true;
            backupAppBar.IsMenuEnabled = false;

            ApplicationBarIconButton appBarIconBtnRotateLeft = new ApplicationBarIconButton();
            appBarIconBtnRotateLeft.IconUri = PathHelper.GetPath("appbar.transform.rotate.left.png");
            appBarIconBtnRotateLeft.Text = AppResources.RotateToLeft;
            appBarIconBtnRotateLeft.Click += appBarIconBtnRotateLeft_Click;

            ApplicationBarIconButton appBarIconBtnRotateRight = new ApplicationBarIconButton();
            appBarIconBtnRotateRight.IconUri = PathHelper.GetPath("appbar.transform.rotate.right.png");
            appBarIconBtnRotateRight.Text = AppResources.RotateToRight;
            appBarIconBtnRotateRight.Click += appBarIconBtnRotateRight_Click;

            ApplicationBarIconButton appBarIconBtnFlipHorizontal = new ApplicationBarIconButton();
            appBarIconBtnFlipHorizontal.IconUri = PathHelper.GetPath("appbar.transform.flip.horizontal.png");
            appBarIconBtnFlipHorizontal.Text = AppResources.FlipToHorizontal;
            appBarIconBtnFlipHorizontal.Click += appBarIconBtnFlipHorizontal_Click;

            ApplicationBarIconButton appBarIconBtnFlipVertical = new ApplicationBarIconButton();
            appBarIconBtnFlipVertical.IconUri = PathHelper.GetPath("appbar.transform.flip.vertical.png");
            appBarIconBtnFlipVertical.Text = AppResources.FlipToVertical;
            appBarIconBtnFlipVertical.Click += appBarIconBtnFlipVertical_Click;

            backupAppBar.Buttons.Add(appBarIconBtnRotateLeft);
            backupAppBar.Buttons.Add(appBarIconBtnRotateRight);
            backupAppBar.Buttons.Add(appBarIconBtnFlipHorizontal);
            backupAppBar.Buttons.Add(appBarIconBtnFlipVertical);

            BrdFrame.ManipulationStarted += BrdFrame_ManipulationStarted;
            BrdFrame.ManipulationDelta += BrdFrame_ManipulationDelta;

           
        }

        #region Image event handler

        private void OnSizeChangedImgFrame(object sender, SizeChangedEventArgs e)
        {
            WriteableBitmap wb = ImgFrame.Source as WriteableBitmap;
            imageSize = e.NewSize;
            Color color = Color.FromArgb((byte)(255 * 0.7), 0, 0, 0);
            double innerWidth = imageSize.Width * Constants.FRAME_RATIO;
            double innerHeight = imageSize.Height * Constants.FRAME_RATIO;
            innerRect = new Rect(imageSize.Width * (1 - Constants.FRAME_RATIO) / 2, imageSize.Height * (1 - Constants.FRAME_RATIO) / 6, innerWidth, innerHeight);

            for (int i = 0; i < imageSize.Width; i++)
            {
                for (int j = 0; j < imageSize.Height; j++)
                {
                    if (!innerRect.Contains(new Point(i, j)) || innerRect.Right == i || innerRect.Bottom == j)
                    {
                        wb.SetPixel(i, j, color);
                    }
                }
            }
            BrdFrame.Margin = new Thickness(innerRect.Left, innerRect.Top, 0, 0);
            BrdFrame.Width = innerRect.Width;
            BrdFrame.Height = innerRect.Height;
            BrdFrame.BorderBrush = new SolidColorBrush(Colors.White);
            BrdFrame.BorderThickness = new Thickness(1);
            BrdFrame.Background = new SolidColorBrush(Colors.Transparent);
            LoadImage();
        }

        private void OnLayoutUpdatedImgDetail(object sender, EventArgs e)
        {
            Size size = ImgDetail.RenderSize;

            if (size.Width != 0 && size.Height != 0)
            {
                double scale = Math.Max(innerRect.Width / size.Width, innerRect.Height / size.Height);
                transform.ScaleX = scale;
                transform.ScaleY = scale;
                currentScale.X = scale;
                currentScale.Y = scale;

                //x축 좌표가 화면의 중앙에 오도록 설정
                double gap = size.Width - innerRect.Width;
                transform.TranslateX = innerRect.Left - gap / 2;

                //y축 좌표가 화면의 중앙에 오도록 설정
                gap = size.Height - innerRect.Height;
                transform.TranslateY = innerRect.Top - gap / 2;

                ImgDetail.LayoutUpdated -= OnLayoutUpdatedImgDetail;
            }
        }

        #endregion

        #region Appbar button event
        private void appBarIconBtnFlipHorizontal_Click(object sender, EventArgs e)
        {
            WriteableBitmap wb = ImgDetail.Source as WriteableBitmap;
            ImgDetail.Source = wb.Flip(WriteableBitmapExtensions.FlipMode.Vertical);
        }

        private void appBarIconBtnFlipVertical_Click(object sender, EventArgs e)
        {
            WriteableBitmap wb = ImgDetail.Source as WriteableBitmap;
            ImgDetail.Source = wb.Flip(WriteableBitmapExtensions.FlipMode.Horizontal);
        }

        private void appBarIconBtnRotateRight_Click(object sender, EventArgs e)
        {
            WriteableBitmap wb = ImgDetail.Source as WriteableBitmap;
            ImgDetail.Source = wb.Rotate(90);
        }

        private void appBarIconBtnRotateLeft_Click(object sender, EventArgs e)
        {
            WriteableBitmap wb = ImgDetail.Source as WriteableBitmap;
            ImgDetail.Source = wb.Rotate(270);
        }

        void appBarIconBtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    //이미지 로드
                    WriteableBitmap wb = ImgDetail.Source as WriteableBitmap;
                    wb.SaveJpeg(ms, wb.PixelWidth, wb.PixelHeight, 0, 100);
                    //마지막 항목 사진중심으로 잘라내기는 사용하지 않음
                    //JpegHelper.Save(NavigationContext.QueryString["imgName"], wb, ms, false);
                    string name = NavigationContext.QueryString["imgName"];
                    //축소 가능한 이미지라면 축소
                    JpegHelper.Resize(wb, ms, ResolutionHelper.CurrentResolution, false);
                    //이미지 파일 저장
                    FileHelper.SaveImage(name, ms);
                    //썸네일 만들기 (이미지의 중심 비율등이 변경될 수 있으므로 재생성해서 덮어쓰기)
                    JpegHelper.Resize(wb, ms, LockscreenHelper.ThumnailSize, true);
                    //썸네일 저장
                    FileHelper.SaveImage(name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_THUMNAIL_POSTFIX), ms);
                    //현재 프레임으로 자르기
                    WriteableBitmap cropImage = GetCropImage();
                    //락스크린용 이미지로 축소 또는 확대
                    JpegHelper.Resize(cropImage, ms, LockscreenHelper.Size, false);
                    //락스크린 파일 저장
                    FileHelper.SaveImage(name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_READY_POSTFIX), ms);
                }
                MessageBox.Show(AppResources.MsgSuccessSaveImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(AppResources.MsgFailSaveImage, ex.Message));
            }
        }

        void appBarIconBtnCrop_Click(object sender, EventArgs e)
        {
            ImgDetail.Source = GetCropImage();
                        
            //트랜스폼 초기화
            transform.TranslateX = 0;
            transform.TranslateY = 0;

            transform.ScaleX = 1;
            transform.ScaleY = 1;

            //다시 로드 할 때 중앙 정렬을 위해 이벤트 추가
            ImgDetail.LayoutUpdated += OnLayoutUpdatedImgDetail;
        }

        private WriteableBitmap GetCropImage()
        {
            WriteableBitmap wb = ImgDetail.Source as WriteableBitmap;
            WriteableBitmap nwb = BitmapFactory.New((int)ResolutionHelper.CurrentResolution.Width, (int)ResolutionHelper.CurrentResolution.Height);
            double wr = wb.PixelWidth / ResolutionHelper.CurrentResolution.Width;

            GeneralTransform gtf = ImgFrame.TransformToVisual(ImgDetail);
            Rect rt = gtf.TransformBounds(
                new Rect(innerRect.X,
                    innerRect.Y,
                    innerRect.Width,
                    innerRect.Height));

            //원본에서 축소된 비율 (화면 로딩 초기 렌더링된 값)
            double cr = wb.PixelWidth / ImgDetail.ActualWidth;
            double tx = wb.PixelWidth / innerRect.Width;

            Rect dstRect = new Rect(0, 0, nwb.PixelWidth, nwb.PixelHeight);
            Rect srcRect = new Rect(rt.X * cr, rt.Y * cr, rt.Width * cr, rt.Height * cr);
            //복사된 마이너스 영역을 배경색으로 덧칠
            nwb.Blit(dstRect, wb, srcRect, WriteableBitmapExtensions.BlendMode.Additive);
            //왼쪽 영역이 모자라면 검정색 배경으로 칠함
            if (0 < (int)((-rt.X * tx / wr) * transform.ScaleX) && 0 < nwb.PixelHeight)
            {
                nwb.FillRectangle(0, 0, (int)((-rt.X * tx / wr) * transform.ScaleX), nwb.PixelHeight, Colors.Black);
            }
            //오른쪽 영역이 모자라면 검정색 배경으로 칠함
            double right = (ImgDetail.ActualWidth - rt.X) * nwb.PixelWidth / innerRect.Width * transform.ScaleX;
            if ((int)(right) < nwb.PixelWidth && 0 < nwb.PixelHeight)
            {
                nwb.FillRectangle((int)(right), 0, nwb.PixelWidth, nwb.PixelHeight, Colors.Black);
            }
            //상단 영역이 모자라면 검정색 배경으로 칠함
            if (0 < (int)((-rt.Y * tx / wr) * transform.ScaleY))
            {
                nwb.FillRectangle(0, 0, nwb.PixelWidth, (int)((-rt.Y * tx / wr) * transform.ScaleY), Colors.Black);
            }
            //하단 영역이 모자라면 검정색 배경으로 칠함
            double bottom = (ImgDetail.ActualHeight - rt.Y) * nwb.PixelHeight / innerRect.Height * transform.ScaleY;
            if ((int)(bottom) < nwb.PixelHeight && 0 < nwb.PixelWidth)
            {
                nwb.FillRectangle(0, (int)(bottom), nwb.PixelWidth, nwb.PixelHeight, Colors.Black);
            }

            return nwb;
        }
        
        private async void appBarIconBtnPreview_Click(object sender, EventArgs e)
        {
            await Task.Delay(10);
            ApplicationBar.IsVisible = false;
            LoadingProgressBar.Visibility = System.Windows.Visibility.Visible;
            LoadingText.Text = AppResources.MsgCreatingPreview;

            LockscreenData data = new LockscreenData(false)
            {
                BackgroundBitmap = GetCropImage(),
                DayList = VsCalendar.GetCalendarOfMonth(DateTime.Now, DateTime.Now, true, true),
                LiveWeather = SettingHelper.Get(Constants.WEATHER_LIVE_RESULT) as LiveWeather,
                Forecasts = SettingHelper.Get(Constants.WEATHER_FORECAST_RESULT) as Forecasts
            };
            
            if ((bool)SettingHelper.Get(Constants.CALENDAR_SHOW_APPOINTMENT))
            {
                Appointments appointments = new Appointments();
                appointments.SearchCompleted += (s, se) =>
                {
                    VsCalendar.MergeCalendar(data.DayList, se.Results);
                    LockscreenHelper.RenderLayoutToBitmap(data);
                    DisplayPreview(data.BackgroundBitmap);
                };
                appointments.SearchAsync(data.DayList[7].DateTime, data.DayList[data.DayList.Count - 1].DateTime, null);
            }
            else
            {
                LockscreenHelper.RenderLayoutToBitmap(data);
                DisplayPreview(data.BackgroundBitmap);
            }
        }

        private DoubleAnimationUsingKeyFrames GetSlideAnimation(bool isShow, double height)
        {
            EasingDoubleKeyFrame sKeyFrame = new EasingDoubleKeyFrame();
            SplineDoubleKeyFrame eKeyFrame = new SplineDoubleKeyFrame();
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();

            eKeyFrame.KeySpline = new KeySpline();
            eKeyFrame.KeySpline.ControlPoint1 = new Point(0.25, 1);
            eKeyFrame.KeySpline.ControlPoint2 = new Point(0.75, 0.5);

            sKeyFrame.KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0));
            sKeyFrame.Value = isShow ? height : 0;
            eKeyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500));
            eKeyFrame.Value = isShow ? 0 : height;

            animation.KeyFrames.Add(sKeyFrame);
            animation.KeyFrames.Add(eKeyFrame);

            Storyboard.SetTarget(animation, PreviewPanel);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));

            return animation;
        }
        
        private void DisplayPreview(WriteableBitmap bgBitmap)
        {
            //시계 표시
            DateTime now = DateTime.Now;
            string dt = now.ToShortTimeString();

            PreviewTime.Text = now.ToString("h:mm");
            PreviewDay.Text = now.ToString("dddd");
            PreviewDate.Text = now.ToString("m").Replace(" ", "");

            //ImageBrush brush = PreviewPanel.Background as ImageBrush;
            //brush.ImageSource = bgBitmap;
            //PreviewPanel.Background = brush;
            PreviewImage.Source = bgBitmap;

            SystemTray.IsVisible = false;
            ApplicationBar.IsVisible = false;
            PreviewPanel.Visibility = System.Windows.Visibility.Visible;

            Storyboard sb = new Storyboard();
            sb.Children.Clear();
            sb.Children.Add(GetSlideAnimation(true, -PreviewPanel.ActualHeight));
            sb.Begin();
        }

        private void StopPreview()
        {
            //SystemTray.IsVisible = true;
            //ApplicationBar.IsVisible = true;
            LoadingText.Text = string.Empty;
            Storyboard sb = new Storyboard();

            sb.Completed += new EventHandler((object obj, EventArgs ev) =>
            {
                PreviewPanel.Visibility = System.Windows.Visibility.Collapsed;
                LoadingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                SystemTray.IsVisible = true;
                ApplicationBar.IsVisible = true;
            });
            sb.Children.Clear();
            sb.Children.Add(GetSlideAnimation(false, -PreviewPanel.ActualHeight));
            sb.Begin();
        }

        void appBarIconBtnTransform_Click(object sender, EventArgs e)
        {
            SwapPageMode();
        }

        private void SwapPageMode()
        {
            IApplicationBar tmpAppbar = this.ApplicationBar;
            ApplicationBar = backupAppBar;
            backupAppBar = tmpAppbar;
        }

        #endregion

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (PreviewPanel.Visibility == System.Windows.Visibility.Visible)
            {
                StopPreview();
                e.Cancel = true;
            }
            else
            {
                ApplicationBarIconButton btn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                if (btn.Text == AppResources.Crop)
                {
                    //ImgDetail.Source = null;
                    base.OnBackKeyPress(e);
                }
                else
                {
                    SwapPageMode();
                    e.Cancel = true;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        
        #region Gesture event handler
        void BrdFrame_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
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
                //Pinch
                double distanceRatio = e.PinchManipulation.CumulativeScale;

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

        void BrdFrame_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            currentScale.X = transform.ScaleX;
            currentScale.Y = transform.ScaleY;
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

        #region Image loading
        private void LoadImage()
        {
            WriteableBitmap wb = GetCurrentImage();
            ImgDetail.Source = wb;
        }

        private WriteableBitmap GetCurrentImage()
        {
            string imgName = NavigationContext.QueryString["imgName"];
            WriteableBitmap wb = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream sourceStream = isoStore.OpenFile(imgName, FileMode.Open, FileAccess.Read))
                {
                    wb = BitmapFactory.New(0, 0).FromStream(sourceStream);
                }
            }
            return wb;
        }
        #endregion

        private void PreviewPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
           // StopPreview();
        }

        private void PreviewPanel_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            Point deltaPoint = e.TotalManipulation.Translation;

            if (Math.Abs(deltaPoint.X) < Math.Abs(deltaPoint.Y))
            {
                if (e.FinalVelocities.LinearVelocity.Y < 0)
                {
                    StopPreview();
                }
            }
        }
    }
}