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
using Microsoft.Phone.Controls;
using ICSharpCode.SharpZipLib.GZip;

namespace ChameleonLib.Api.Open.Today
{
    public class NasaToday
    {
        public event TodayImageEventHandler LoadCompleted;
        //http://www.nasa.gov/sites/default/files/styles/100x75/public/
        //http://www.nasa.gov/sites/default/files/styles/226x170/public/
        //http://www.nasa.gov/sites/default/files/styles/346x260/public/ 
        //http://www.nasa.gov/sites/default/files/styles/430x323/public/
        //http://www.nasa.gov/sites/default/files/styles/800x600_autoletterbox/public/ 가장크다
        //1920x1080
        //1600x1200
        //1366x768
        //1024x768
        //800x600
        //http://www.nasa.gov/sites/default/files/styles/946xvariable_height/public/
        //
        private const string IMG_URL = "http://www.nasa.gov/sites/default/files/styles/";

        public async void Load()
        {
            ChameleonAlbum album = new ChameleonAlbum();
            using (HttpClient httpClient = new HttpClient())
            {
                using (var picResponse = await httpClient.GetAsync("http://www.nasa.gov/rss/dyn/image_of_the_day.rss", HttpCompletionOption.ResponseContentRead))
                {
                    if (picResponse.IsSuccessStatusCode)
                    {
                        if (picResponse.Content.Headers.ContentEncoding.Contains("gzip"))
                        {
                            using (GZipInputStream gzip = new GZipInputStream(await picResponse.Content.ReadAsStreamAsync()))
                            {
                                SetXmlToAlbum(gzip, album);
                            }
                        }
                        else
                        {
                            using (Stream stream = await picResponse.Content.ReadAsStreamAsync())
                            {
                                SetXmlToAlbum(stream, album);
                            }
                        }
                    }
                }
                
                using (var picResponse = await httpClient.GetAsync("http://www.nasa.gov/rss/dyn/chandra_images.rss", HttpCompletionOption.ResponseContentRead))
                {
                    if (picResponse.IsSuccessStatusCode)
                    {
                        if (picResponse.Content.Headers.ContentEncoding.Contains("gzip"))
                        {
                            using (GZipInputStream gzip = new GZipInputStream(await picResponse.Content.ReadAsStreamAsync()))
                            {
                                SetXmlToAlbum(gzip, album);
                            }
                        }
                        else
                        {
                            using (Stream stream = await picResponse.Content.ReadAsStreamAsync())
                            {
                                SetXmlToAlbum(stream, album);
                            }
                        }
                    }
                }
            }

            if (LoadCompleted != null)
            {
                LoadCompleted(this, new TodayImageResult() { Album = album });
            }
        }

        private void SetXmlToAlbum(Stream stream, ChameleonAlbum album)
        {
            try
            {
                // Load the stream into and XDocument for processing
                XDocument doc = XDocument.Load(stream);

                // Iterate through the image elements
                foreach (XElement image in doc.Descendants("item"))
                {
                    string imgUrl = image.Element("enclosure").Attribute("url").Value;
                    long length = long.Parse(image.Element("enclosure").Attribute("length").Value);
                    string type = image.Element("enclosure").Attribute("type").Value;

                    string thumbailImg = IMG_URL + "226x170/public/";
                    string orgImg = IMG_URL + "946xvariable_height/public/";
                    string fileName = imgUrl.Substring(imgUrl.LastIndexOf("/") + 1);

                    album.Add(new WebPicture()
                    {
                        Guid = Guid.NewGuid(),
                        SourceOrigin = SourceOrigin.NasaToday,
                        FileName = FileHelper.GetFileName(fileName.Substring(0, fileName.LastIndexOf("?"))),
                        Name = image.Element("title").Value,
                        Path = orgImg + fileName,
                        FileSize = length,
                        Width = 946,
                        //Height = (int)resolution.Height,
                        ContentType = type,
                        Thumbnail = new WebPicture()
                        {
                            Path = thumbailImg + fileName,
                            Width = 226,
                            Height = 170,
                            ContentType = type,
                        }
                    });
                }
            }
            catch (System.Xml.XmlException xe)
            {
                System.Diagnostics.Debug.WriteLine(xe.Message);
            }
        }
    }
}
