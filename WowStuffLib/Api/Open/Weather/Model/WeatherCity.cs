using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class WeatherCity
    {
        public bool IsGpsLocation { get; set; }

        public string StateName { get; set; }

        public string CountryName { get; set; }

        public string CityName { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Code { get; set; }

        public bool isZipCode { get; set; }
    }
}
