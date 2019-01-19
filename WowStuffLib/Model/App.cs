using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace ChameleonLib.Model
{
    public class App
    {
        public string Name { get; private set; }

        public Uri ImageUri { get; private set; }

        public string AppId { get; private set; }

        public App(string imgFolder, string appKey, string imgKey, string appId)
        {
            string appName = "AppKey";
            string[] names = appKey.Split('.');

            if (imgFolder != null || imgFolder.Length > 0)
            {
                appName += imgFolder.Substring(0, 1).ToUpper();
                appName += imgFolder.Substring(1);
            }

            foreach (string val in names)
            {
                appName += val.Substring(0, 1).ToUpper();
                appName += val.Substring(1);
            }
            
            Name = AppResources.ResourceManager.GetString(appName);

            ImageUri = new Uri(string.Format("/ChameleonLib;component/Images/sysapp/{0}/{1}.png", imgFolder, imgKey == null || imgKey == string.Empty ? appKey : imgKey), UriKind.Relative);
            AppId = appId;
        }
    }
}
