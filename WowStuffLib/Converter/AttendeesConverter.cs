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
    public class AttendeesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                IEnumerable<Attendee> attendees = value as IEnumerable<Attendee>;

                if (attendees == null || attendees.Count() < 1)
                {
                    return string.Empty;
                }

                return string.Format(AppResources.AttenddesAccPerson, attendees.Count());
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
