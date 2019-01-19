using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace Chameleon.View
{
    public partial class SearchCityPage : PhoneApplicationPage
    {
        private WeatherBug weatherBug;

        public SearchCityPage()
        {
            InitializeComponent();

            weatherBug = new WeatherBug();
            weatherBug.FindLocationCompleted += weatherBug_FindLocationCompleted;
            weatherBug.RequestFailed += weatherBug_RequestFailed;
        }

        private void BtnSearch_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (App.CheckNetworkStatus())
            {
                if (TxtSearch.Text.Trim() == string.Empty)
                {
                    MessageBox.Show(AppResources.MsgEnterCity);
                }
                else
                {
                    SearchingProgressBar.Visibility = System.Windows.Visibility.Visible;
                    weatherBug.FindLocation(TxtSearch.Text.Trim());
                }
            }
        }

        void weatherBug_RequestFailed(object sender, object result)
        {
            SearchingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            if (result.Equals(HttpStatusCode.NoContent))
            {
                MessageBox.Show(AppResources.MsgNoResultCity
                    + "\n" + AppResources.MsgEnteryAsEng);
            }
            else
            {
                MessageBox.Show(AppResources.MsgFailWeatherService);
            }
        }

        void weatherBug_FindLocationCompleted(object sender, object result)
        {
            List<Location> locationList = result as List<Location>;
            LLSLocation.ItemsSource = locationList;

            if (locationList.Count > 0)
            {
                TxtSearchCity.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                TxtSearchCity.Visibility = System.Windows.Visibility.Visible;
                MessageBox.Show(AppResources.MsgNoResultCity
                   + "\n" + AppResources.MsgEnteryAsEng);
            }
            SearchingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void LLSLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            Location location = (sender as LongListSelector).SelectedItem as Location;
            
            WeatherCity city = new WeatherCity();
            city.CityName = location.CityName;
            //city.IsGpsLocation = false;
            city.isZipCode = location.IsUsa;
            city.Code = location.IsUsa ? location.ZipCode : location.CityCode;
            city.IsGpsLocation = true;
            city.Latitude = location.Lat;
            city.Longitude = location.Lon;
            city.StateName = location.CityName != location.StateName ? location.StateName : string.Empty;
            city.CountryName = location.CountryName;

            PhoneApplicationService.Current.State[Constants.WEATHER_MAIN_CITY] = city;
            NavigationService.GoBack();
        }

    }
}