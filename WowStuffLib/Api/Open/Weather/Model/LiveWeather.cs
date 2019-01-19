using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class LiveWeather
    {
        //public DateTime UpdatedDateTime { get; set; }

        //public string WebUrl { get; set; }

        //public string InputLocationUrl { get; set; }

        public WeatherDateTime ObDate { get; set; }

        //public string RequestedStationId { get; set; }

        public Station Station { get; set; }

        //public string SiteUrl { get; set; }

        //public WeatherUnit AuxTemp { get; set; }

        public string CurrentCondition { get; set; }
        
        public string CurrentConditionIcon { get; set; }

        //public ValueUnits DewPoint { get; set; }

        //public ValueUnits Elevation { get; set; }

        public ValueUnits FeelsLike { get; set; }

        public string FeelsLikeLabel { get; set; }

        //public WeatherDateTime GustTime { get; set; }

        //public string GustDirection { get; set; }

        //public string GustDirectionDegrees { get; set; }

        //public ValueUnits GustSpeed { get; set; }

        public WeatherUnit Humidity { get; set; }

        //public WeatherUnit IndoorTemp { get; set; }

        //public WeatherUnit Light { get; set; }

        //public string MoonPhase { get; set; }
        
        //public string MoonPhaseImg { get; set; }

        //public WeatherUnit Pressure { get; set; }

        //public ValueUnits RainMonth { get; set; }

        //public ValueUnits RainRate { get; set; }

        //public ValueUnits RainRateMax { get; set; }

        //public ValueUnits RainToday { get; set; }

        //public ValueUnits RainYear { get; set; }

        public WeatherUnit Temp { get; set; }

        //public WeatherDateTime Sunrise { get; set; }

        //public WeatherDateTime Sunset { get; set; }

        //public ValueUnits WetBulb { get; set; }

        public ValueUnits WindSpeed { get; set; }
        
        //public ValueUnits WindSpeedAvg { get; set; }

        public string WindDirection { get; set; }

        //public string WindDirectionDegrees { get; set; }

        //public string WindDirectionAvg { get; set; }
    }
}
