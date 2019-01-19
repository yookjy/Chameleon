using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using ChameleonLib.Storages;

namespace ChameleonLib.Helper
{
    public class SettingHelper
    {
        public static void Set(string key, object value, bool isSave)
        {
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static void Save()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static object Get(string key)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                return IsolatedStorageSettings.ApplicationSettings[key];
            }
            return null;
        }

        public static bool Remove(string key)
        {
            return IsolatedStorageSettings.ApplicationSettings.Remove(key);
        }

        public static string GetString(string key)
        {
            return Get(key) as string;
        }

        public static bool ContainsKey(string key)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
        }

        public static void CreateDefaultValues()
        {
            ScheduleSettings scheduleSetting = MutexedIsoStorageFile.Read<ScheduleSettings>("ScheduleSettings", Constants.MUTEX_DATA);

            //락스크린의 템플릿
            SetDefaultSetting(Constants.LOCKSCREEN_BACKGROUND_TEMPLATE, new LockscreenTemplateItem()
            {
                LockscreenItemInfos = new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Weather, Column = 0, Row = 0, ColSpan = 1, RowSpan = 3 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Calendar, Column = 1, Row = 0, ColSpan = 1, RowSpan = 3 }
                }
            });
            
            //락스크린의 뒷배경 분할
            SetDefaultSetting(Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION, false);
            
            //락스크린의 배경 색상
            SetDefaultSetting(Constants.LOCKSCREEN_BACKGROUND_COLOR,
                new ColorItem()
                {
                    Text = AppResources.ColorChrome,
                    Color = ColorItem.ConvertColor(0xFF1F1F1F)
                });
            
            //락스크린의 배경 투명도
            SetDefaultSetting(Constants.LOCKSCREEN_BACKGROUND_OPACITY, Constants.LOCKSCREEN_BACKGROUND_DEFAULT_OPACITY);
            
            //락스크린의 글자 굵기
            SetDefaultSetting(Constants.LOCKSCREEN_FONT_WEIGHT, FontWeights.Bold.ToString());
            
            //락스크린의 업데이트 주기
            if (scheduleSetting.LockscreenUpdateInterval == 0)
            {
                scheduleSetting.LockscreenUpdateInterval = 180;
                MutexedIsoStorageFile.Write<ScheduleSettings>(scheduleSetting, "ScheduleSettings", Constants.MUTEX_DATA);
            }

            //라이브타일 랜덤색상 사용여부
            SetDefaultSetting(Constants.LIVETILE_RANDOM_BACKGROUND_COLOR, true);

            ColorItem accentColorItem = new ColorItem()
            {
                Color = (Color)Application.Current.Resources["PhoneAccentColor"]
            };

            if (string.IsNullOrEmpty(accentColorItem.Text))
            {
                accentColorItem.Text = AppResources.AccentColor;
            }

            //메인 라이브타일의 배경색상
            SetDefaultSetting(Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR, accentColorItem);
            
            //보조 라이브타일의 템플릿...
            SetDefaultSetting(Constants.LIVETILE_WEATHER_BACKGROUND_COLOR, accentColorItem);
                
            //배터리 라이브타일의 템플릿...
            SetDefaultSetting(Constants.LIVETILE_BATTERY_BACKGROUND_COLOR, accentColorItem);

            //날씨 타일 폰트 크기
            SetDefaultSetting(Constants.LIVETILE_WEATHER_FONT_SIZE, new PickerItem() { Key = 1.1, Name = string.Format(AppResources.Percent, 1.1 * 100) });

            //날씨 및 달력의 글자 굵기
            SetDefaultSetting(Constants.LIVETILE_FONT_WEIGHT, FontWeights.SemiBold.ToString());

            //배터리 완충 상태 표시
            SetDefaultSetting(Constants.LIVETILE_BATTERY_FULL_INDICATION, new PickerItem() { Key = 100, Name = AppResources.BatteryFull });

            //라이브타일의 업데이트 주기
            if (scheduleSetting.LivetileUpdateInterval == 0)
            {
                scheduleSetting.LivetileUpdateInterval = 60;
                MutexedIsoStorageFile.Write<ScheduleSettings>(scheduleSetting, "ScheduleSettings", Constants.MUTEX_DATA);
            }

            //보호색 사용여부
            SetDefaultSetting(Constants.CHAMELEON_USE_PROTECTIVE_COLOR, true);

            //보호이미지 사용여부
            SetDefaultSetting(Constants.CHAMELEON_USE_PROTECTIVE_IMAGE, false);

            SetDefaultSetting(Constants.CHAMELEON_SKIN_BACKGROUND_COLOR,
                new ColorItem()
                {
                    Color = ColorItem.GetColorByName("Green")
                });

            //날씨 위치 서비스
            if (!SettingHelper.ContainsKey(Constants.WEATHER_USE_LOCATION_SERVICES))
            {
                SettingHelper.Set(Constants.WEATHER_USE_LOCATION_SERVICES, true, false);
            }

            //날씨 표시 단위
            if (!SettingHelper.ContainsKey(Constants.WEATHER_UNIT_TYPE))
            {
                if (System.Globalization.CultureInfo.CurrentUICulture.Name == "en-US")
                {
                    SettingHelper.Set(Constants.WEATHER_UNIT_TYPE, DisplayUnit.Fahrenheit, false);
                }
                else
                {
                    SettingHelper.Set(Constants.WEATHER_UNIT_TYPE, DisplayUnit.Celsius, false);
                }
            }
            //날씨 기본 아이콘 설정
            if (!SettingHelper.ContainsKey(Constants.WEATHER_ICON_TYPE))
            {
                SettingHelper.Set(Constants.WEATHER_ICON_TYPE, WeatherIconType.Simple01, false);
            }
            
            //달력의 첫요일
            SetDefaultSetting(Constants.CALENDAR_FIRST_DAY_OF_WEEK, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
            //달력 약속 표시
            SetDefaultSetting(Constants.CALENDAR_SHOW_APPOINTMENT, true);
            //시작 페이지 설정
            SetDefaultSetting(Constants.COMMON_FIRST_PAGE_ITEM, 0);
            //기본 빙마켓 설정
            SetDefaultSetting(Constants.BING_LANGUAGE_MARKET, System.Globalization.CultureInfo.CurrentUICulture.Name);
            SetDefaultSetting(Constants.BING_SEARCH_ASPECT, "Tall");
            SetDefaultSetting(Constants.BING_SEARCH_OPTIONS, "None");
            SetDefaultSetting(Constants.BING_SEARCH_SIZE, "Large");
            SetDefaultSetting(Constants.BING_SEARCH_SIZE_WIDTH, "" + (int)ResolutionHelper.CurrentResolution.Width);
            SetDefaultSetting(Constants.BING_SEARCH_SIZE_HEIGHT, "" + (int)ResolutionHelper.CurrentResolution.Height);
            SetDefaultSetting(Constants.BING_SEARCH_COLOR, "Color");
            SetDefaultSetting(Constants.BING_SEARCH_STYLE, "Photo");
            SetDefaultSetting(Constants.BING_SEARCH_FACE, "Other");
            SetDefaultSetting(Constants.BING_SEARCH_COUNT, "40");
            SetDefaultSetting(Constants.BING_SEARCH_ADULT, "Strict");

            //손전등 - 토글버튼 사용 설정
            SetDefaultSetting(Constants.FLASHLIGHT_USE_TOGGLE_SWITCH, true);

            SettingHelper.Save();
        }

        private static void SetDefaultSetting(string key, object value)
        {
            if (!SettingHelper.ContainsKey(key))
            {
                SettingHelper.Set(key, value, false);
            }
        }
    }
}
