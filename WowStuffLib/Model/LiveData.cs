using ChameleonLib.Api.Open.Weather.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChameleonLib.Model
{
    public class LiveData
    {
        public List<ChameleonLib.Api.Calendar.Model.Day> DayList { get; set; }

        public LiveWeather LiveWeather { get; set; }

        public Forecasts Forecasts { get; set; }

        public FontWeight FontWeight { get; protected set; }
    }
}
