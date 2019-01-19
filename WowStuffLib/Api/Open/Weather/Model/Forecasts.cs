using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class Forecasts
    {
        //public string WebUrl { get; set; }

        //public string ForecastType { get; set; }

        //public string ForecastDate { get; set; }

        public Forecast Today { get; set; }

        //public Location Location { get; set; }

        public List<Forecast> Items { get; set; }
    }
}
