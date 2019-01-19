using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using ChameleonLib.Api.Open.Weather.Model;
using System.Windows;
using ChameleonLib.Helper;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using ICSharpCode.SharpZipLib.GZip;

namespace ChameleonLib.Api.Open.Today
{
    
   
    public class BingToday
    {
        public event TodayImageEventHandler LoadCompleted;
        private const string BING = "http://www.bing.com";
        private const string IMG_URL = "http://www.bing.com/HPImageArchive.aspx?format=xml&idx={0}&n={1}&mkt={2}";

        public void Load(int idx)
        {
            Load(idx, SettingHelper.GetString(Constants.BING_LANGUAGE_MARKET));
        }

        public async void Load(int idx, string regionCode)
        {
            ChameleonAlbum album = new ChameleonAlbum();
            int newIndex = -1;

            if (idx != -1)
            {
                
                Size resolution = ResolutionHelper.CurrentResolution;
                HttpClient httpClient = new HttpClient();
                string url = string.Format(IMG_URL, idx, 7, regionCode);
                var picResponse = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead);

                string sizeFormat = "_{0:0.##}x{1:0.##}.jpg";
                Size thumbSize = new Size(240, 400);
                Size fullSize = resolution;

                if (SettingHelper.GetString(Constants.BING_SEARCH_ASPECT) == "Wide")
                {
                    thumbSize.Width = 400;
                    thumbSize.Height = 240;
                    fullSize.Width = resolution.Height;
                    fullSize.Height = resolution.Width;
                }

                string thumbSizePostfix = string.Format(sizeFormat, thumbSize.Width, thumbSize.Height);
                string fullSizePostfix = string.Format(sizeFormat, fullSize.Width, fullSize.Height);

                if (picResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        if (picResponse.Content.Headers.ContentEncoding.Contains("gzip"))
                        {
                            using (GZipInputStream gzip = new GZipInputStream(await picResponse.Content.ReadAsStreamAsync()))
                            {
                                SetXmlToAlbum(gzip, album, thumbSizePostfix, fullSizePostfix, thumbSize);
                            }
                        }
                        else
                        {
                            using (Stream stream = await picResponse.Content.ReadAsStreamAsync())
                            {
                                SetXmlToAlbum(stream, album, thumbSizePostfix, fullSizePostfix, thumbSize);
                            }
                        }        
                        newIndex = idx + 7;
                    }
                    catch (System.Xml.XmlException xe)
                    {
                        System.Diagnostics.Debug.WriteLine(xe.Message);
                    }
                }

                picResponse.Dispose();
            }

            LoadCompleted(this, new TodayImageResult() { Album = album, Index = newIndex });
        }

        public void SetXmlToAlbum(Stream stream, ChameleonAlbum album,
            string thumbSizePostfix, string fullSizePostfix, Size thumbSize)
        {
            // Load the stream into and XDocument for processing
            XDocument doc = XDocument.Load(stream);

            // Iterate through the image elements
            foreach (XElement image in doc.Descendants("image"))
            {
                string imgUrl = image.Element("url").Value;
                imgUrl = imgUrl.Replace(image.Element("urlBase").Value + "_", "").Replace(".jpg", "");
                string thumbailImg = image.Element("urlBase").Value + thumbSizePostfix;
                string verticalImg = image.Element("urlBase").Value + fullSizePostfix;

                album.Add(new WebPicture()
                {
                    Guid = Guid.NewGuid(),
                    SourceOrigin = SourceOrigin.BingToday,
                    FileName = FileHelper.GetFileName(verticalImg),
                    Name = image.Element("copyright").Value,
                    Path = BING + verticalImg,
                    Width = (int)ResolutionHelper.CurrentResolution.Width,
                    Height = (int)ResolutionHelper.CurrentResolution.Height,
                    ContentType = "image/jpeg",
                    Thumbnail = new WebPicture()
                    {
                        Path = BING + thumbailImg,
                        Width = (int)thumbSize.Width,
                        Height = (int)thumbSize.Height,
                        ContentType = "image/jpeg",
                    }
                });
            }
        }
    }
}
