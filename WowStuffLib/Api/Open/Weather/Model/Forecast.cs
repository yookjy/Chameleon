using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class Forecast 
    {
        //public string Title { get; set; }

        public DateTime DateTime { get; set; }

        public string AltTitle { get; set; }

        //public string ShortPrediction { get; set; }
        
        //public string Image { get; set; }
        
        //public string ImageIsNight { get; set; }

        public string ImageIcon { get; set; }

        //public string Description { get; set; }

        public string Prediction { get; set; }

        public ValueUnits[] LowHigh { get; set; }


        public string AltTitleForNight { get; set; }

        public string ImageIconForNight { get; set; }

        public string PredictionForNight { get; set; }
    }
}
