using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChameleonLib.Model;

namespace ChameleonLib.Api.Open.Today
{
    
    public delegate void TodayImageEventHandler(object sender, TodayImageResult result);

    public class TodayImageResult
    {
        public ChameleonAlbum Album { get; set; }
        public int Index { get; set; }
    }
}
