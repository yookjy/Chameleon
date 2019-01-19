using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ChameleonLib.Api.Open.Weather.Model;

namespace ChameleonLib.Api.Open.Weather
{
    public class WeatherBug
    {
        private string REST_XML_API_KEY = "a68xsjmfrmwkww7jh7whfj6x";
        
        public const string ICON_LOCAL_PATH = "/Images/weather/{0}/{1}/{2}.png";

        private string API_REST_XML_BASE_URL = "http://i.wxbug.net/REST/Direct/";

        private string API_SEARCH_LOCATION = "GetLocationSearch.ashx?ss={0}&c={1}&l={2}&api_key={3}";

        private string API_COMBINING_MULTIPLE_DATA = "GetData.ashx?dt=l&dt=o&ic=1&dt=f&nf=7&ht=t&ht=i&ht=d&units={0}&c={1}&l={2}&api_key={3}";

        public WeatherCity DefaultWeatherCity { get; set; }

        public DisplayUnit DefaultUnitType { get; set; }

        public event RequestFailedHandler RequestFailed;
        public event FindLocationCompletedHandler FindLocationCompleted;
        public event LiveWeatherBeforeHandler LiveWeatherBeforeLoad;
        public event LiveWeatherCompletedHandler LiveWeatherCompletedLoad;

        public delegate void RequestFailedHandler(object sender, object result);
        public delegate void FindLocationCompletedHandler(object sender, List<Location> result);
        public delegate void LiveWeatherBeforeHandler(object sender, object result);
        public delegate void LiveWeatherCompletedHandler(object sender, LiveWeather result, Forecasts forecasts);

        private string country;

        private string language;

        public WeatherBug()
        {
            string[] langs = CultureInfo.CurrentCulture.Name.Split('-');
            //string[] langs = new CultureInfo("ko-KR").Name.Split('-');
            language = langs[0];
            country = langs[1];
        }

        public async void FindLocation(string cityName)
        {
            StringBuilder urlBuilder = new StringBuilder(API_REST_XML_BASE_URL);
            urlBuilder.AppendFormat(API_SEARCH_LOCATION,
                new object[] { HttpUtility.UrlEncode(cityName), country, language, REST_XML_API_KEY });

            using (HttpClient httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(urlBuilder.ToString(), HttpCompletionOption.ResponseContentRead))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        List<Location> locationList = new List<Location>();
                        String jsonString = await response.Content.ReadAsStringAsync();
                        //검색결과가 없을때 아얘 문자열이 공백("")이 돌아올 때가 있음.
                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            var parsedJson = JObject.Parse(jsonString);
                            string tmp = string.Empty;

                            foreach (var item in parsedJson["cityList"])
                            {
                                Location location = new Location();
                                location.CityName = (item["city"] as JValue).Value<string>();
                                //slocation.CityOriginalName = cityName;
                                location.CityCode = (item["cityCode"] as JValue).Value<string>();
                                location.CountryName = (item["country"] as JValue).Value<string>();
                                location.IsUsa = (item["isUs"] as JValue).Value<bool>();
                                location.StateName = (item["state"] as JValue).Value<string>();
                                location.ZipCode = (item["zipCode"] as JValue).Value<string>();
                                //dma
                                //lat
                                location.Lat = (item["lat"] as JValue).Value<string>();
                                location.Lon = (item["lon"] as JValue).Value<string>();
                                //long
                                if (language == "ko" || language == "ja" || language == "zh")
                                {
                                    tmp = (string.IsNullOrEmpty(location.StateName) ? location.CityName :
                                        location.StateName == location.CityName ? location.CityName : location.StateName + " " + location.CityName);
                                }
                                else
                                {
                                    tmp = (string.IsNullOrEmpty(location.StateName) ? location.CityName :
                                        location.StateName == location.CityName ? location.CityName : location.CityName + " ," + location.StateName);
                                }
                                location.CityStateName = tmp;

                                locationList.Add(location);
                            }
                        }

                        if (FindLocationCompleted != null)
                        {
                            FindLocationCompleted(this, locationList);
                        }
                    }
                    else
                    {
                        if (RequestFailed != null)
                        {
                            RequestFailed(this, response.StatusCode);
                        }
                    }
                }
            }
        }
      
        public async void LiveWeather(WeatherCity city, DisplayUnit unitType)
        {
            if (LiveWeatherBeforeLoad != null)
            {
                LiveWeatherBeforeLoad(this, null);
            }

            DefaultWeatherCity = city;
            DefaultUnitType = unitType;

            StringBuilder urlBuilder = new StringBuilder(API_REST_XML_BASE_URL);
            urlBuilder.AppendFormat(API_COMBINING_MULTIPLE_DATA,
                new object[] { unitType == DisplayUnit.Fahrenheit ? "0" : "1", country, language, REST_XML_API_KEY });
            
            if (city.IsGpsLocation)
            {
                urlBuilder.AppendFormat("&{0}={1}&{2}={3}", new object[] { "la", city.Latitude, "lo", city.Longitude });
            }
            else if (city.isZipCode)
            {
                urlBuilder.AppendFormat("&{0}={1}", new object[] { "zip", city.Code});
            }
            else
            {
                urlBuilder.AppendFormat("&{0}={1}", new object[] { "city", city.Code });
            }
            
            using (HttpClient httpClient = new HttpClient())
            {
                LiveWeather liveWeather = null;
                Forecasts forecasts = null;

                var response = await httpClient.GetAsync(urlBuilder.ToString(), HttpCompletionOption.ResponseContentRead);
                {
                    if (response.IsSuccessStatusCode)
                    {
                        String jsonString = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            var parsedJson = JObject.Parse(jsonString);

                            JToken weather = parsedJson["weather"]["ObsData"];
                            if ((bool)(weather["hasData"] as JValue).Value)
                            {
                                if (liveWeather == null)
                                {
                                    liveWeather = new LiveWeather();
                                    liveWeather.Station = new Station();
                                }

                                long ldt = (weather["dateTime"] as JValue).Value<long>();

                                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                dt = dt.AddSeconds(ldt / 1000);

                                liveWeather.ObDate = new WeatherDateTime()
                                {
                                    Year = dt.Year.ToString(),
                                    Month = new ValueInfo() { Number = dt.Month.ToString() },
                                    Day = new ValueInfo() { Number = dt.Day.ToString() },
                                    Hour24 = dt.Hour.ToString(),
                                    Minute = dt.Minute.ToString(),
                                    Second = dt.Second.ToString()
                                };
                                liveWeather.Station.City = (weather["stationName"] as JValue).Value<string>();
                                //liveWeather.Station.Id = (string)(item["stationId"] as JValue).Value;
                                //liveWeather.Station.Name = (string)(item["stationName"] as JValue).Value;
                                liveWeather.CurrentCondition = (string)(weather["desc"] as JValue).Value;
                                liveWeather.CurrentConditionIcon = (string)(weather["icon"] as JValue).Value;
                                liveWeather.FeelsLike = new ValueUnits()
                                {
                                    Value = (weather["feelsLike"] as JValue).Value<string>(),
                                    Units = (weather["temperatureUnits"] as JValue).Value<string>()
                                };
                                liveWeather.FeelsLikeLabel = (weather["feelsLikeLabel"] as JValue).Value<string>();
                                liveWeather.Humidity = new WeatherUnit()
                                {
                                    Value = new ValueUnits()
                                    {
                                        Value = (weather["humidity"] as JValue).Value<string>(),
                                        Units = (weather["humidityUnits"] as JValue).Value<string>()
                                    }
                                };
                                liveWeather.Temp = new WeatherUnit()
                                {
                                    Value = new ValueUnits()
                                    {
                                        Value = (weather["temperature"] as JValue).Value<string>(),
                                        Units = (weather["temperatureUnits"] as JValue).Value<string>()
                                    },
                                    High = new ValueUnits()
                                    {
                                        Value = (weather["temperatureHigh"] as JValue).Value<string>(),
                                        Units = (weather["temperatureUnits"] as JValue).Value<string>()
                                    },
                                    Low = new ValueUnits()
                                    {
                                        Value = (weather["temperatureLow"] as JValue).Value<string>(),
                                        Units = (weather["temperatureUnits"] as JValue).Value<string>()
                                    }
                                };
                                liveWeather.WindSpeed = new ValueUnits()
                                {
                                    Value = (weather["windSpeed"] as JValue).Value<string>(),
                                    Units = (weather["windUnits"] as JValue).Value<string>()
                                };
                                liveWeather.WindDirection = (weather["windDirection"] as JValue).Value<string>();
                            }

                            if (liveWeather != null)
                            {
                                weather = parsedJson["weather"]["LocationData"]["location"];
                                if (weather.Any())
                                {
                                    liveWeather.Station.City = (weather["city"] as JValue).Value<string>();
                                    liveWeather.Station.State = (weather["state"] as JValue).Value<string>();
                                    //liveWeather.Station.ZipCode = (weather["zipCode"] as JValue).Value<string>();
                                    //liveWeather.Station.CityCode = (weather["cityCode"] as JValue).Value<string>();
                                    liveWeather.Station.Country = (weather["country"] as JValue).Value<string>();
                                    //liveWeather.Station.Latitude = Value<string>()(weather["lat"] as JValue).Value<string>();
                                    //liveWeather.Station.Longitude = Value<string>()(weather["lon"] as JValue).Value<string>();
                                    
                                    if (string.IsNullOrEmpty(liveWeather.Station.City))
                                    {
                                        liveWeather.Station.City = city.CityName;
                                    }
                                }
                                else
                                {
                                    liveWeather.Station.City = city.CityName;
                                    liveWeather.Station.State = city.StateName;
                                    liveWeather.Station.Country = city.CountryName;
                                }

                                string title = null;
                                string[] days = DateTimeFormatInfo.CurrentInfo.DayNames;
                                weather = parsedJson["weather"]["ForecastData"]["forecastList"];

                                foreach (var item in weather)
                                {
                                    if (forecasts == null)
                                    {
                                        forecasts = new Forecasts();
                                    }

                                    long ldt = (item["dateTime"] as JValue).Value<long>();
                                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(ldt / 1000);
                                    title = (item["dayTitle"] as JValue).Value<string>();
                                    for (int j = 0; j < days.Length; j++)
                                    {
                                        if (days[j].ToLower() == title.ToLower())
                                        {
                                            if (forecasts.Items == null && days[j] == DateTime.Today.ToString("dddd"))
                                            {
                                                title = ChameleonLib.Resources.AppResources.WeatherToday;
                                            }
                                            else
                                            {
                                                title = DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames[j];
                                            }
                                            break;
                                        }
                                    }

                                    Forecast forecast = new Forecast()
                                    {
                                        DateTime = dt,
                                        AltTitle = title,
                                        ImageIcon = (item["dayIcon"] as JValue).Value<string>(),
                                        LowHigh = new ValueUnits[2] 
                                        {
                                            new ValueUnits() 
                                            {
                                                Value = (item["low"] as JValue).Value<string>(),
                                                Units = liveWeather.Temp.Value.Units
                                            },
                                            new ValueUnits() 
                                            {
                                                Value = (item["high"] as JValue).Value<string>(),
                                                Units = liveWeather.Temp.Value.Units
                                            }
                                        },
                                        Prediction = (item["dayPred"] as JValue).Value<string>(),
                                        AltTitleForNight = (item["nightTitle"] as JValue).Value<string>(),
                                        ImageIconForNight = (item["nightIcon"] as JValue).Value<string>(),
                                        PredictionForNight = (item["nightPred"] as JValue).Value<string>(),
                                    };

                                    if (string.IsNullOrEmpty(forecast.ImageIcon))
                                    {
                                        forecast.ImageIcon = "cond999";
                                    }

                                    if (string.IsNullOrEmpty(forecast.ImageIconForNight))
                                    {
                                        forecast.ImageIconForNight = "cond999";
                                    }

                                    if (forecasts.Items == null)
                                    {
                                        forecasts.Items = new List<Forecast>();
                                    }

                                    forecasts.Items.Add(forecast);
                                }

                                if (forecasts != null && forecasts.Items.Count > 0)
                                {
                                    forecasts.Today = forecasts.Items[0];

                                    if (forecasts.Items.Count == 7)
                                    {
                                        forecasts.Items.RemoveAt(0);
                                    }
                                }
                            }
                        }

                        if (LiveWeatherCompletedLoad != null)
                        {
                            LiveWeatherCompletedLoad(this, liveWeather, forecasts);
                        }
                    }
                    else
                    {
                        if (RequestFailed != null)
                        {
                            RequestFailed(this, response.StatusCode);
                        }
                    }
                } 
            }
        }
               
        public void RefreshLiveWeather(DisplayUnit weatherUnit)
        {
            if (DefaultWeatherCity != null)
            {
                LiveWeather(DefaultWeatherCity, weatherUnit);
            }
        }
    }
}
