using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class Location
    {
        public string CityName { get; set; }

        public string CityOriginalName { get; set; }

        public string StateName { get; set; }

        public string CityStateName { get; set; }

        public string CountryName { get; set; }

        public string CityCode { get; set; }

        public string ZipCode { get; set; }

        public bool IsUsa { get; set; }

        public string Zone { get; set; }

        public string Lat { get; set; }

        public string Lon { get; set; }
    }
}
