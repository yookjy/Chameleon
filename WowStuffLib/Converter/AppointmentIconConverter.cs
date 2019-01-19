using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Resources;

namespace ChameleonLib.Converter
{
    public class AppointmentIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Account account = value as Account;
                string kind = account.Kind.ToString().ToLower();
                string imageName = kind;

                if (account.Kind == StorageKind.Other && account.Name.ToLower() == "google")
                {
                    imageName = account.Name;
                }

                Uri uri = new Uri(string.Format("/Images/calendar/appointment.{0}.png", imageName), UriKind.Relative);
                return uri;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
