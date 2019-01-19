//using ExifLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ChameleonLib.Resources;

namespace ChameleonLib.Helper
{
    public struct JpegOrientation
    {
        public ushort Angle { get; set; }

        public bool IsFlip { get; set; }

        public WriteableBitmapExtensions.FlipMode FlipMode { get; set; }
    }

    public static class JpegHelper
    {
        //원본 스트림이 아니면 안되고(WriteableBitmap등 다른 객체를 거치면 정보를 뽑아내지 못함), using을 사용하면 Disposable이라 스트림이 닫힌다. 
        //그러한 이유로 사용 보류
        /*
        public static JpegOrientation GetOrientation(string name, Stream stream)
        {
            JpegOrientation jo = new JpegOrientation();

            if (name.Contains(".jpg") || name.Contains(".jpeg"))
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        using (ExifReader reader = new ExifReader(stream))
                        {
                            ushort orientation = 0;
                            reader.GetTagValue(ExifTags.Orientation, out orientation);
                            switch (orientation)
                            {
                                case 1:
                                    jo.Angle = 0;
                                    jo.IsFlip = false;
                                    break;
                                case 2:
                                    jo.Angle = 0;
                                    jo.IsFlip = true;
                                    jo.FlipMode = WriteableBitmapExtensions.FlipMode.Vertical;
                                    break;
                                case 3:
                                    jo.Angle = 180;
                                    jo.IsFlip = false;
                                    break;
                                case 4:
                                    jo.Angle = 180;
                                    jo.IsFlip = true;
                                    jo.FlipMode = WriteableBitmapExtensions.FlipMode.Vertical;
                                    break;
                                case 5:
                                    jo.Angle = 90;
                                    jo.IsFlip = true;
                                    jo.FlipMode = WriteableBitmapExtensions.FlipMode.Horizontal;
                                    break;
                                case 6:
                                    jo.Angle = 90;
                                    jo.IsFlip = false;
                                    break;
                                case 7:
                                    jo.Angle = 270;
                                    jo.IsFlip = true;
                                    jo.FlipMode = WriteableBitmapExtensions.FlipMode.Horizontal;
                                    break;
                                case 8:
                                    jo.Angle = 270;
                                    jo.IsFlip = false;
                                    break;
                            }
                        }
                    }
                }
                catch (ExifLibException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            return jo;
        }
        */
        //PhotoChooserTask를 사용하는 락스크린 싱글이미지 추가에서 호출됨
        //락스크린 이미지는 자동생성으므로 중심을 기준으로 잘라내기 사용함.
        public static void Save(string name, Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                WriteableBitmap bitmap = BitmapFactory.New(0, 0).FromStream(ms);
                JpegHelper.Save(name, bitmap, ms, true);
            }
        }

        //LockscreenSelectionPage에서 다운로드후 개별건을 저장할 때 및 이미지 상세보기(PictuerPage)에서 단건 저장할 때 호출
        //락스크린 이미지는 자동생성으므로 중심을 기준으로 잘라내기 사용함.
        public static void Save(string name, WriteableBitmap bitmap) 
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.SaveJpeg(ms, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
                JpegHelper.Save(name, bitmap, ms, true);
            }
        }

        public static void Save(string name, WriteableBitmap bitmap, Stream stream, bool isLockscreenCenterCrop)
        {
            bool isWarnning = bitmap.PixelWidth >= bitmap.PixelHeight;
            //축소 가능한 이미지라면 축소
            Resize(bitmap, stream, ResolutionHelper.CurrentResolution, false);
            
            //이미지 파일 저장
            FileHelper.SaveImage(name, stream);

            if (!isWarnning)
            {
                //락스크린용 이미지로 축소
                Resize(bitmap, stream, LockscreenHelper.Size, isLockscreenCenterCrop);
                //락스크린 파일 저장
                FileHelper.SaveImage(name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_READY_POSTFIX), stream);
            }
            
            //썸네일 만들기
            if (bitmap.PixelWidth > LockscreenHelper.ThumnailSize.Width || bitmap.PixelHeight > LockscreenHelper.ThumnailSize.Height)
            {
                Resize(bitmap, stream, LockscreenHelper.ThumnailSize, true);
                //썸네일 저장
                FileHelper.SaveImage(name.Replace(Constants.LOCKSCREEN_IMAGE_POSTFIX, Constants.LOCKSCREEN_IMAGE_THUMNAIL_POSTFIX), stream);
            }
        }
        
        public static double GetMinRatio(WriteableBitmap bitmap, Size rSize)
        {
            double dx = (double)bitmap.PixelWidth / rSize.Width;
            double dy = (double)bitmap.PixelHeight / rSize.Height;
            double dr = Math.Min(dx, dy);

            return dr;
        }

        public static WriteableBitmap Resize(Stream stream, Size rSize, bool isCenterCrop)
        {
            WriteableBitmap bitmap = BitmapFactory.New(0, 0).FromStream(stream);
            double dr = JpegHelper.GetMinRatio(bitmap, rSize);

            //if (dr > 1) <= 주석 처리하면 축소 뿐만이 아니라 확대까지 된다.
            //{
                bitmap = bitmap.Resize((int)(bitmap.PixelWidth / dr), (int)(bitmap.PixelHeight / dr), WriteableBitmapExtensions.Interpolation.Bilinear);
                //가로/세로를 중심점에 맞추어 잘라내기
                if (isCenterCrop)
                {
                    bitmap = bitmap.Crop(new Rect((bitmap.PixelWidth - rSize.Width) / 2, (bitmap.PixelHeight - rSize.Height) / 2, rSize.Width, rSize.Height));
                }
            //}
            return bitmap;
        }

        public static bool ResizeNoZoomIn(WriteableBitmap bitmap, Stream stream, Size rSize, bool isCenterCrop)
        {
            bool isResized = false;
            double dr = JpegHelper.GetMinRatio(bitmap, rSize);

            if (dr > 1)
            {
                //가로/세로를 중심점에 맞추어 잘라내기
                if (isCenterCrop)
                {
                    //비트맵도 사이즈 변경
                    bitmap = bitmap.Resize((int)(bitmap.PixelWidth / dr), (int)(bitmap.PixelHeight / dr), WriteableBitmapExtensions.Interpolation.Bilinear);
                    bitmap = bitmap.Crop(new Rect((bitmap.PixelWidth - rSize.Width) / 2, (bitmap.PixelHeight - rSize.Height) / 2, rSize.Width, rSize.Height));
                    stream.Seek(0, SeekOrigin.Begin);
                    bitmap.SaveJpeg(stream, (int)rSize.Width, (int)rSize.Height, 0, 100);
                }
                else
                {
                    //스트림에만 저장
                    stream.Seek(0, SeekOrigin.Begin);
                    bitmap.SaveJpeg(stream, (int)(bitmap.PixelWidth / dr), (int)(bitmap.PixelHeight / dr), 0, 100);
                }
                isResized = true;
            }

            return isResized;
        }


        public static bool Resize(WriteableBitmap bitmap, Stream stream, Size rSize, bool isCenterCrop)
        {
            bool isResized = false;
            double dr = JpegHelper.GetMinRatio(bitmap, rSize);

            //if (dr > 1)
            //{
                //가로/세로를 중심점에 맞추어 잘라내기
                if (isCenterCrop)
                {
                    //비트맵도 사이즈 변경
                    bitmap = bitmap.Resize((int)(bitmap.PixelWidth / dr), (int)(bitmap.PixelHeight / dr), WriteableBitmapExtensions.Interpolation.Bilinear);
                    bitmap = bitmap.Crop(new Rect((bitmap.PixelWidth - rSize.Width) / 2, (bitmap.PixelHeight - rSize.Height) / 2, rSize.Width, rSize.Height));
                    stream.Seek(0, SeekOrigin.Begin);
                    bitmap.SaveJpeg(stream, (int)rSize.Width, (int)rSize.Height, 0, 100);
                }
                else
                {
                    //스트림에만 저장
                    stream.Seek(0, SeekOrigin.Begin);
                    bitmap.SaveJpeg(stream, (int)(bitmap.PixelWidth / dr), (int)(bitmap.PixelHeight / dr), 0, 100);
                }
                isResized = true;
            //}

            return isResized;
        }


    }
}
