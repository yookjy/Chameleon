using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class Station
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Distance { get; set; }

        public string Unit { get; set; }

        public string Country { get; set; }

        public string CityCode { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
    }
}
