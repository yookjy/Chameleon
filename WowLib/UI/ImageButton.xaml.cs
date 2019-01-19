using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WowLib.UI
{
    public partial class ImageButton : UserControl
    {
        private BitmapImage image;
        private BitmapImage pressedImage;

        public ImageButton()
        {
            InitializeComponent();
            this.MouseEnter += ImageButton_MouseEnter;
            this.MouseLeave += ImageButton_MouseLeave;
            this.SizeChanged += ImageButton_SizeChanged;
            //this.LayoutUpdated += ImageButton_LayoutUpdated;
        }

        //void ImageButton_LayoutUpdated(object sender, EventArgs e)
        //{
        //    //상태가 숨김으로 변한 상태라면 마우스 leave 이벤트가 발생하지 않으므로 여기서 처리한다.
        //    if (this._contentLoaded && this.Visibility == System.Windows.Visibility.Collapsed
        //        && ((BitmapImage)BkImage.Source).UriSource.OriginalString != image.UriSource.OriginalString)
        //    {
        //        BkImage.Source = image;
        //    }
        //}

        void ImageButton_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image img = (Image)this.Content;
            img.Width = this.Width;
            img.Height = this.Height;
        }

        public BitmapImage Image
        {
            get
            {
                return image;
            }
            set
            {
                BkImage.Source = value;
                image = value;
            }
        }

        public BitmapImage PressedImage
        {
            get
            {
                return pressedImage;
            }
            set
            {
                pressedImage = value;
            }
        }

        void ImageButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (image != null)
            {
                BkImage.Source = image;
            }
        }

        void ImageButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (pressedImage != null)
            {
                BkImage.Source = pressedImage;
            }
        }

        public void DisplayImage()
        {
            if (image != null)
            {
                BkImage.Source = image;
            }
        }

        public void DisplayPressedImage()
        {
            if (pressedImage != null)
            {
                BkImage.Source = pressedImage;
            }
        }
    }
}
