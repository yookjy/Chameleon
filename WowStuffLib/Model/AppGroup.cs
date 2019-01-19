using Microsoft.Phone.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChameleonLib.Resources;

namespace ChameleonLib.Model
{
    public class AppGroup : List<ChameleonLib.Model.App>
    {
        public string Name { get; private set; }

        private string imgFolder;

        public AppGroup(string imgFolder)
        {
            this.imgFolder = imgFolder;
        }

        public AppGroup(string imgFolder, string categoryName)
        {
            this.imgFolder = imgFolder;
            Name = AppResources.ResourceManager.GetString(categoryName);
        }

        public void Add(string appKey, string imgKey, string appId)
        {
            this.Add(new App(this.imgFolder, appKey, imgKey, appId));
        }
    }
}

