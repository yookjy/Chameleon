using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Model
{
    public class WebPicture : AbstractPicture
    {
        public string Path { get; set; }

        public string FileName { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public long FileSize { get; set; }

        public string ContentType { get; set;}

        public WebPicture Thumbnail { get; set; }

        //public string ThumbnailPath { get; set; }

        //public string ThumbnailFileName { get; set; }

       // public DownloadStatus DownloadStatus { get; set; }

        //public string DownloadFailMessage { get; set; }

        //public WebExceptionStatus DownloadFailStatus { get; set; }

        public WebPicture() { }
    }
}
