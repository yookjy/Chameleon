using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Helper;
using ChameleonLib.Model;
using ChameleonLib.Resources;

namespace ChameleonLib.Api.Open.Bing
{
    public class Finder
    {
        //private static ManualResetEvent _clientDone = new ManualResetEvent(false);

        public void FindImageUrlsFor(string searchQuery)
        {
            string market = SettingHelper.GetString(Constants.BING_LANGUAGE_MARKET);
            string options = SettingHelper.GetString(Constants.BING_SEARCH_OPTIONS);
            string adult = SettingHelper.GetString(Constants.BING_SEARCH_ADULT);
            string imageFilter = GetImageFilter();
            //string domainFilter = " -site:nemopan.com -site:gasengi.com -site:onlifezone.com";
            StringBuilder domainFilter = new StringBuilder();
            
            foreach (BlackDomain domain in new BlackList().Items)
            {
                domainFilter.AppendFormat(" -site:{0}", domain.Host);
            }        

            string queryString = searchQuery + domainFilter.ToString();
            int queryCount = 20;
            Int32.TryParse(SettingHelper.GetString(Constants.BING_SEARCH_COUNT), out queryCount);
            options = (options == "None") ? null : options;

            // Create a Bing container. 
            string rootUri = "https://api.datamarket.azure.com/Bing/Search";
            var bingContainer = new BingSearchContainer(new Uri(rootUri));

            // Replace this value with your account key. 
            var accountKey = "oKMIE1hNc6Zoxl7U6B0fwt4plXQlkDSyMAISBbRA2yc";

            bingContainer.UseDefaultCredentials = false;

            // Configure bingContainer to use your credentials. 
            bingContainer.Credentials = new NetworkCredential(accountKey, accountKey);

            // Build the query. 
            //var imageQuery = bingContainer.Image("신민아", "DisableLocationDetection", "en-US", "Off", null, null, "Size:Large+Aspect:Tall+Style:Photo");
            var imageQuery = bingContainer.Image(queryString, options, market, adult, null, null, imageFilter);
            //var imageQuery = bingContainer.Image(searchQuery, null, null, null, null, null, imageFilter);
            imageQuery.AddQueryOption("$top", queryCount);

            //_clientDone.Reset();
            imageQuery.BeginExecute(_onImageQueryComplete, imageQuery);
            //if (!_clientDone.WaitOne(3000))
            //{
            //    FindCompleted(this, new WebAlbum());
            //}
        }

        private string GetImageFilter()
        {
            StringBuilder builder = new StringBuilder();
            string size = SettingHelper.GetString(Constants.BING_SEARCH_SIZE);
            if (size == "Custom")
            {
                builder.AppendFormat("Size:Width:{0}+Size:Height:{1}"
                    , SettingHelper.GetString(Constants.BING_SEARCH_SIZE_WIDTH)
                    , SettingHelper.GetString(Constants.BING_SEARCH_SIZE_HEIGHT));
            }
            else
            {
                builder.AppendFormat("Size:{0}", size);
            }
            builder.AppendFormat("+Aspect:{0}", SettingHelper.GetString(Constants.BING_SEARCH_ASPECT));
            builder.AppendFormat("+Style:{0}", SettingHelper.GetString(Constants.BING_SEARCH_STYLE));
            builder.AppendFormat("+Face:{0}", SettingHelper.GetString(Constants.BING_SEARCH_FACE));
            builder.AppendFormat("+Color:{0}", SettingHelper.GetString(Constants.BING_SEARCH_COLOR));
            
            return builder.ToString();
        }

        // Handle the query callback. 
        private void _onImageQueryComplete(IAsyncResult imageResults)
        {
            //_clientDone.Set();
            // Get the original query from the imageResults.
            DataServiceQuery<Bing.ImageResult> query =
                imageResults.AsyncState as DataServiceQuery<Bing.ImageResult>;

            ChameleonAlbum webAlbum = new ChameleonAlbum();

            try
            {
                foreach (var result in query.EndExecute(imageResults))
                {
                    webAlbum.Add(new WebPicture()
                    {
                        Guid = Guid.NewGuid(),
                        SourceOrigin = SourceOrigin.Search,
                        Name = result.Title,
                        Path = result.MediaUrl,
                        Width = (int)result.Width,
                        Height = (int)result.Height,
                        FileSize = (long)result.FileSize,
                        ContentType = result.ContentType,
                        Thumbnail = new WebPicture()
                        {
                            Path = result.Thumbnail.MediaUrl,
                            Width = (int)result.Thumbnail.Width,
                            Height = (int)result.Thumbnail.Height,
                            FileSize = (long)result.Thumbnail.FileSize,
                            ContentType = result.Thumbnail.ContentType
                        }
                    });
                }
            }
            catch (Exception e)
            {
                if (e.InnerException.Message == "NotFound")
                {
                    //검색된 데이터 없음.
                }
            }
            FindCompleted(this, webAlbum);
        }

        public event FindImageUrlsForEventHandler FindCompleted;
        public delegate void FindImageUrlsForEventHandler(object sender, ChameleonAlbum webAlbum);
    }
}