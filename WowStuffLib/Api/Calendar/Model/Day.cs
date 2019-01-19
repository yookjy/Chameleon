using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChameleonLib.Api.Calendar.Model
{
    public class Day : INotifyPropertyChanged
    {
        private SolidColorBrush foregroundBrush;

        private string dayName;

        private double fontSize;

        private FontWeight fontWeight;

        private List<Appointment> appointmentList;

        public bool IsAppointment { get; set; }

        public DateTime DateTime { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string DayName
        {
            get
            {
                return dayName;
            }
        }

        public SolidColorBrush ForegroundBrush
        {
            get
            {
                return foregroundBrush;
            }
            set
            {
                if (foregroundBrush != value)
                {
                    foregroundBrush = value;
                    NotifyPropertyChanged(); 
                }
            }
        }

        public SolidColorBrush BackgroundBrush { get; set; }

        public FontWeight FontWeight
        {
            get
            {
                return fontWeight;
            }
            set
            {
                if (fontWeight != value)
                {
                    fontWeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double FontSize 
        { 
            get
            {
                return fontSize;
            }
            set
            {
                if (fontSize != value)
                {
                    fontSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Day() { }

        public Day(string dayName)
        {
            this.dayName = dayName;
        }

        public List<Appointment> AppointmentList 
        {
            get
            {
                return appointmentList;
            }
            set
            {
                if (appointmentList != value)
                {
                    appointmentList = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
