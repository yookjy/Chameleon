using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ChameleonLib.Api.Calendar.Model;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace ChameleonLib.Api.Calendar
{
    public class VsCalendar
    {
        public static List<Day> GetDayNames(bool isForceWhiteForgroundBrush)
        {
            double normalSize = (double)Application.Current.Resources["PhoneFontSizeNormal"];
            double largeSize = normalSize * 1.3;
            SolidColorBrush foregroundBrush = isForceWhiteForgroundBrush ? new SolidColorBrush(Colors.White) : (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
            DayOfWeek dow = (DayOfWeek)SettingHelper.Get(Constants.CALENDAR_FIRST_DAY_OF_WEEK);
            List<Day> dtList = new List<Day>();

            //요일 인덱스 추가
            string[] dayNames = DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames;
            int startDay = (int)dow;

            for (int i = 0; i < dayNames.Length - startDay; i++)
            {
                dtList.Insert(i, new Day(dayNames[i + startDay])
                {
                    FontSize = normalSize,
                    ForegroundBrush = foregroundBrush
                });
            }

            for (int i = dayNames.Length - startDay; i < dayNames.Length; i++)
            {
                dtList.Insert(i, new Day(dayNames[i - (dayNames.Length - startDay)])
                {
                    FontSize = normalSize,
                    ForegroundBrush = foregroundBrush
                });
            }
            return dtList;
        }

        public static List<Day> GetCalendarOfMonth(DateTime reqDt, DateTime selDt, bool isIncludeDayNames, bool isForceWhiteForgroundBrush)
        {
            DateTime dt;
            DateTime firstDay = reqDt.AddDays((reqDt.Day - 1) * -1);
            DayOfWeek dow = (DayOfWeek)SettingHelper.Get(Constants.CALENDAR_FIRST_DAY_OF_WEEK);
            int dim = DateTimeFormatInfo.CurrentInfo.Calendar.GetDaysInMonth(reqDt.Year, reqDt.Month);
            List<Day> dtList = new List<Day>();

            double normalSize = (double)Application.Current.Resources["PhoneFontSizeNormal"];
            double largeSize = normalSize * 1.3;
            
            SolidColorBrush foregroundBrush = isForceWhiteForgroundBrush ? new SolidColorBrush(Colors.White) : (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
            SolidColorBrush backgroundBrush = (SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"];
            SolidColorBrush transparentBrush = (SolidColorBrush)Application.Current.Resources["TransparentBrush"];
            SolidColorBrush phoneAccentBrush = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
            SolidColorBrush phoneSubtitleBrush = (SolidColorBrush)Application.Current.Resources["PhoneSubtleBrush"];
            SolidColorBrush phoneDisbledBrush = new SolidColorBrush(
                            Color.FromArgb((byte)(foregroundBrush.Color.A * 0.6), (byte)(foregroundBrush.Color.R * 0.6), 
                            (byte)(foregroundBrush.Color.G * 0.6), (byte)(foregroundBrush.Color.B * 0.6)));

            bool isToday = false;
            bool isSelect = false;

            for (int i = 0; i < dim; i++)
            {
                dt = firstDay.AddDays(i);
                
                isToday = (dt.Year == DateTime.Now.Year && dt.Month == DateTime.Now.Month && dt.Day == DateTime.Now.Day);
                isSelect = isSelect = (selDt.Year == 1) ? false : (dt.Year == selDt.Year && dt.Month == selDt.Month && dt.Day == selDt.Day);
                
                dtList.Add(new Day(dt.Day.ToString())
                {
                    DateTime = dt,
                    FontSize = largeSize,
                    FontWeight = FontWeights.Normal,
                    ForegroundBrush = isSelect ? phoneAccentBrush : (isToday ? phoneDisbledBrush : foregroundBrush),
                    BackgroundBrush = isToday ? new SolidColorBrush(Colors.White) : transparentBrush    //흰색 고정 (테마에 무관함)
                });
            }

            //시작 요일 별 offset 설정
            int ds = (int)DateTimeFormatInfo.CurrentInfo.Calendar.GetDayOfWeek(firstDay);
            if (DateTimeFormatInfo.CurrentInfo.Calendar.GetDayOfWeek(firstDay) < dow)
            {
                ds += (7 - (int)dow);
            }
            else if (DateTimeFormatInfo.CurrentInfo.Calendar.GetDayOfWeek(firstDay) > dow)
            {
                ds -= (int)dow;
            }
            else
            {
                ds = 7;
            }

            //첫주에 이전달 날짜가 있으면 추가
            dt = firstDay.AddDays(ds * -1);
            for (int i = 0; i < ds; i++)
            {
                dtList.Insert(i, new Day(dt.Day.ToString())
                {
                    DateTime = dt,
                    FontSize = largeSize,
                    ForegroundBrush = phoneSubtitleBrush
                });
                dt = dt.AddDays(1);
            }

            //마지막 주에 다음달 날짜가 있으면 추가
            dt = dtList[dtList.Count - 1].DateTime;
            int cnt = 42 - dtList.Count;
            for (int i = 0; i < cnt; i++)
            {
                dt = dt.AddDays(1);
                dtList.Add(new Day(dt.Day.ToString())
                {
                    DateTime = dt,
                    FontSize = largeSize,
                    ForegroundBrush = phoneSubtitleBrush
                });
            }

            if (isIncludeDayNames)
            {
                //요일 인덱스 추가
                string[] dayNames = DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames;
                int startDay = (int)dow;

                for (int i = 0; i < dayNames.Length - startDay; i++)
                {
                    dtList.Insert(i, new Day(dayNames[i + startDay])
                    {
                        FontSize = normalSize,
                        ForegroundBrush = foregroundBrush
                    });
                }

                for (int i = dayNames.Length - startDay; i < dayNames.Length; i++)
                {
                    dtList.Insert(i, new Day(dayNames[i])
                    {
                        FontSize = normalSize,
                        ForegroundBrush = foregroundBrush
                    });
                }
            }

            /*
            //일요일의 컬럼 인덱스 및 토요일의 컬럼 인덱스를 구한다.
            int sundayIndex = dtList.FindIndex(x => x.DateTime.DayOfWeek == DayOfWeek.Sunday);
            int saturdayIndex = dtList.FindIndex(x => x.DateTime.DayOfWeek == DayOfWeek.Saturday);

            //루프를 통해 해당 컬럼들의 색상브러시를 변경한다.
            //일요일
            Color sundayColor = Colors.Red;
            sundayColor.R = (byte)((double)sundayColor.R * 0.9);
            sundayColor.G = (byte)((double)255 * 0.2);
            sundayColor.B = (byte)((double)255 * 0.2);

            for (; sundayIndex < dtList.Count; sundayIndex += 7)
            {
                dtList[sundayIndex].ForegroundBrush = new SolidColorBrush(sundayColor);
            }
            //토요일
            Color saturdayColor = Colors.Blue;
            saturdayColor.R = (byte)((double)255 * 0.4);
            saturdayColor.G = (byte)((double)255 * 0.4);
            //saturdayColor.B = (byte)((double)saturdayColor.B * 0.9);
            for (; saturdayIndex < dtList.Count; saturdayIndex += 7)
            {
                dtList[saturdayIndex].ForegroundBrush = new SolidColorBrush(saturdayColor);
            }
             */ 

            return dtList;
        }

        public static List<Day> MergeCalendar(List<Day> dayList, IEnumerable<Appointment> appEnum)
        {
            Dictionary<string, List<Appointment>> appointmentDict = new Dictionary<string, List<Appointment>>();

            foreach (Appointment appointment in appEnum)
            {
                //현재 WindowsLive(Hotmail) 계정에 링크된 생일중 facebook에서 가져온것들은 api에서 일정이 있는것만을 알려줄뿐
                //세부 내역을 알려주지 않는다. 알아내기 위해서는 아래와 같은 과정을 수행한거나 직접 facebook api를 사용해야 하므로 
                //일단 이번 버전에서는 필터링하여 표시 하지 말자.
                //http://developer.nokia.com/Community/Wiki/Birthday_Calendar_for_Windows_Phone
                if (!(string.IsNullOrEmpty(appointment.Subject)
                    && string.IsNullOrEmpty(appointment.Details)
                    && appointment.Account.Kind == StorageKind.WindowsLive
                    && appointment.EndTime.CompareTo(appointment.StartTime.AddDays(1)) == 0))
                {
                    DateTime dt = appointment.StartTime;
                    string dtKey = dt.ToLongDateString();
                    List<Appointment> appointmentList = null;

                    if (appointmentDict.ContainsKey(dtKey))
                    {
                        appointmentList = appointmentDict[dtKey];
                    }
                    else
                    {
                        appointmentList = new List<Appointment>();
                        appointmentDict.Add(dtKey, appointmentList);
                    }
                                
                    appointmentList.Add(appointment);
                }
            }

            foreach (Day day in dayList)
            {
                if (day.DateTime.Year != 1)
                {
                    string dtKey = day.DateTime.ToLongDateString();

                    if (appointmentDict.ContainsKey(dtKey))
                    {
                        day.AppointmentList = appointmentDict[dtKey];                       
                    }
                }
            }

            return dayList;
        }
    }
}
