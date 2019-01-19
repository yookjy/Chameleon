using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ChameleonLib.Helper
{
    public class FileHelper
    {
        public static string GetUniqueFileName(string name)
        {
            return string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), GetFileName(name));
        }

        public static string GetFileName(string name)
        {
            return name.Replace("\\", "_").Replace("/", "_").Replace("*", "").Replace(":", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
        }

        public static void SaveImage(string url, Stream stream)
        {
            if (stream == null || url == null)
            {
                throw new ArgumentException("one of parameters is null");
            }
                
            string fileName = url;
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //long imgSize = isoStore.AvailableFreeSpace;
                using (IsolatedStorageFileStream targetStream = isoStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                {
                    byte[] readBuffer = new byte[4096];
                    int bytesRead = -1;
                    stream.Position = 0;
                    targetStream.Position = 0;

                    while ((bytesRead = stream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        targetStream.Write(readBuffer, 0, bytesRead);
                    }
                }
            }
        }

        public static void LoadImage(string url, Image targetImage)
        {
            if (url == null)
            {
                throw new ArgumentException("one of parameters is null");
            }

            string fileName = GetFileName(url);
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream sourceStream = isoStore.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                {
                    var bi = new BitmapImage();
                    bi.SetSource(sourceStream);
                    targetImage.Source = bi;
                }
            }
        }

        public static void RemoveImage(string url)
        {
            if (url == null)
            {
                throw new ArgumentException("one of parameters is null");
            }

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(url))
                {
                    isoStore.DeleteFile(url);
                }
            }
        }
    }
}