using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Converter;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using ImageTools.IO.Png;
using ImageTools;

namespace ChameleonLib.Helper
{
    public class LivetileHelper
    {
        #region Calendar Live Tile
        public static ShellTileData GetCalendarLivetileData(int appointCount)
        {
            ShellTileData tileData = new FlipTileData()
            {
                Title = string.Empty,
                BackTitle = AppResources.ApplicationTitle,
                BackContent = GetCalendarBackTextContent(appointCount),
                SmallBackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.calendar.small.jpg", UriKind.Absolute),
                BackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.calendar.jpg", UriKind.Absolute),
                BackBackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.calendar.back.jpg", UriKind.Absolute),
            };
            return tileData;
        }

        public static string GetCalendarBackTextContent(int appointmentCount)
        {
            string ymd = DateTime.Now.ToLongDateString();
            string y = DateTime.Now.ToString("y").Replace(DateTime.Now.ToString("MMMM"), "");
            string md = DateTime.Now.ToString("M");
            string day = DateTime.Now.ToString("dddd");

            ymd = ymd.Replace(y, y + "\n").Replace(md, md + "\n").Replace(day, day + "\n").Replace(" \n", "\n").Replace("\n ", "\n");
            ymd = ymd.Substring(0, ymd.Length - 1).Replace(",", "");

            if (appointmentCount >= 0)
            {
                return string.Format("{0}\n{1} : {2}{3}", ymd, AppResources.Appointment, appointmentCount, AppResources.AppointmentCount);
            }
            else
            {
                return ymd;
            }
        }


        public static Canvas GetCalendarCanvas(LivetileData data)
        {
            Canvas calendarCanvas = new Canvas();

            calendarCanvas.Width = data.AreaSize.Width;
            calendarCanvas.Height = data.AreaSize.Height;

            int rowNum = -1, colNum = 0; ;

            for (int i = 0; i < data.DayList.Count; i++)
            {
                if (i % 7 == 0)
                {
                    rowNum++;
                    colNum = 0;
                }
                ChameleonLib.Api.Calendar.Model.Day day = data.DayList[i];

                Canvas dayPanel = new Canvas()
                {
                    Width = calendarCanvas.Width / 7 - 1,
                    Height = calendarCanvas.Height / 7 - 1,
                    //Background = day.BackgroundBrush,
                };
                dayPanel.Margin = new Thickness((colNum++) * dayPanel.Width + 1, rowNum * dayPanel.Height, 0, 0);

                TextBlock dayTxt = new TextBlock()
                {
                    Text = day.DayName,
                    FontSize = day.FontSize * (day.DateTime.Year == 1 ? 1.1 : 1.2),
                    Foreground = day.ForegroundBrush,
                    FontWeight = data.FontWeight
                };

                if (day.DateTime.ToLongDateString() == DateTime.Today.ToLongDateString())
                {
                    Ellipse ellipse = new Ellipse()
                    {
                        Width = dayTxt.ActualHeight * 1.08,
                        Height = dayTxt.ActualHeight * 1.08,
                        Fill = day.BackgroundBrush
                    };

                    dayPanel.Children.Add(ellipse);
                    Canvas.SetLeft(ellipse, (dayPanel.Width - ellipse.Width) / 2);
                    Canvas.SetTop(ellipse, (dayPanel.Height - ellipse.Width) / 2 - 0.5);
                }

                if (data.DayList[i].AppointmentList != null && data.DayList[i].AppointmentList.Count > 0)
                {
                    TextBlock appCnt = new TextBlock()
                    {
                        Text = data.DayList[i].AppointmentList.Count.ToString(),
                        FontSize = 13,
                        Foreground = day.DateTime.ToLongDateString() == DateTime.Now.ToLongDateString() ? day.BackgroundBrush : day.ForegroundBrush,
                        FontWeight = FontWeights.ExtraBold
                    };
                    dayPanel.Children.Add(appCnt);
                    Canvas.SetLeft(appCnt, (dayPanel.Width - appCnt.ActualWidth));
                    Canvas.SetTop(appCnt, - appCnt.ActualHeight / 3);
                }

                dayPanel.Children.Add(dayTxt);
                Canvas.SetLeft(dayTxt, (dayPanel.Width - dayTxt.ActualWidth) / 2);
                Canvas.SetTop(dayTxt, (dayPanel.Height - dayTxt.ActualHeight) / 2);
                calendarCanvas.Children.Add(dayPanel);
            }
            return calendarCanvas;
        }
        #endregion

        #region Weather Live Tile
        public static ShellTileData GetWeatherLivetileData()
        {
            ShellTileData tileData = new FlipTileData()
            {
                Title = string.Empty,
                //BackTitle = AppResources.ApplicationTitle,
                SmallBackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.weather.small.jpg", UriKind.Absolute),
                BackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.weather.jpg", UriKind.Absolute),
                BackBackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.weather.back.jpg", UriKind.Absolute),
            };
            return tileData;
        }

        public static Canvas GetWeatherCanvas(LivetileData data)
        {
            SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.White);
            Canvas weatherCanvas = new Canvas();

            weatherCanvas.Width = data.AreaSize.Width;
            weatherCanvas.Height = data.AreaSize.Height;

            if (data.LiveWeather != null)
            {
                double fontRatio = (double)(SettingHelper.Get(Constants.LIVETILE_WEATHER_FONT_SIZE) as PickerItem).Key;
                string tmp = System.Globalization.CultureInfo.CurrentCulture.Name.Split('-')[0];
                if (tmp == "ko" || tmp == "ja" || tmp == "zh")
                {
                    tmp = (string.IsNullOrEmpty(data.LiveWeather.Station.State) ? string.Empty : data.LiveWeather.Station.State + " ") + data.LiveWeather.Station.City;
                }
                else
                {
                    tmp = data.LiveWeather.Station.City + (string.IsNullOrEmpty(data.LiveWeather.Station.State) ? string.Empty : " ," + data.LiveWeather.Station.State);
                }

                //지역
                TextBlock Location = new TextBlock()
                {
                    Text = tmp,
                    FontSize = data.FontSizeLarge * fontRatio,
                    Foreground = foregroundBrush,
                    FontWeight = data.FontWeight 
                };

                if (Location.ActualWidth > weatherCanvas.Width)
                {
                    Location.Text = data.LiveWeather.Station.City;
                }

                //기온
                string[] temp = data.LiveWeather.Temp.Value.Value.Split('.');
                WeatherIconType type = WeatherIconMap.Instance.WeatherIconType;
                string path = string.Format(WeatherBug.ICON_LOCAL_PATH.Substring(1), type.ToString().ToLower(), "205x172", WeatherImageIconConverter.GetIamgetName(data.LiveWeather.CurrentConditionIcon));
                //string path = string.Format("Images/weather/205x172/cond170.png");
                WriteableBitmap weatherWb = BitmapFactory.New(0, 0).FromContent(path);

                //날씨 이미지
                Image MainImage = new Image()
                {
                    Source = weatherWb,
                    Width = weatherCanvas.Width / 2,
                    Height = (double)weatherWb.PixelHeight / weatherWb.PixelWidth * weatherCanvas.Width / 2
                };

                TextBlock MainTemp = new TextBlock()
                {
                    Text = temp[0],
                    FontSize = data.FontSizeExtraExtraLarge * 1.4,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = foregroundBrush
                };

                TextBlock MainTempDesc = new TextBlock()
                {
                    FontSize = data.FontSizeLarge * fontRatio,
                    FontWeight = data.FontWeight,
                    Text = data.LiveWeather.CurrentCondition,
                    //Text = "40% Chance Rain Shower",
                    Foreground = foregroundBrush,
                    Margin = new Thickness(5,0,5,0)
                };

                double tempTopMargin = 0;
                if (MainTempDesc.ActualWidth > weatherCanvas.Width)
                {
                    tempTopMargin = MainTempDesc.ActualHeight / 2;
                    MainTempDesc.TextWrapping = TextWrapping.Wrap;
                    MainTempDesc.Width = weatherCanvas.Width;
                    MainTempDesc.Height = weatherCanvas.Height;
                }

                double totalWidth = MainTemp.ActualWidth + 10 + MainImage.Width;

                Canvas.SetLeft(Location, 20);
                Canvas.SetTop(Location, 20);

                Canvas.SetLeft(MainImage, (weatherCanvas.Width - totalWidth) / 2);
                Canvas.SetTop(MainImage, (weatherCanvas.Height - MainImage.Height) / 2 - tempTopMargin);

                Canvas.SetLeft(MainTemp, (weatherCanvas.Width - totalWidth) / 2 + MainImage.Width);
                Canvas.SetTop(MainTemp, (weatherCanvas.Height - MainImage.Height) / 2 + (MainImage.Height - MainTemp.ActualHeight) / 2 + 5 /*- tempTopMargin*/);

                Canvas.SetLeft(MainTempDesc, (weatherCanvas.Width - MainTempDesc.ActualWidth) / 2);
                Canvas.SetTop(MainTempDesc, (weatherCanvas.Height - MainImage.Height) / 2 + MainImage.Height + 20 - tempTopMargin);

                weatherCanvas.Children.Add(Location);
                weatherCanvas.Children.Add(MainImage);
                weatherCanvas.Children.Add(MainTemp);
                weatherCanvas.Children.Add(MainTempDesc);
            }
            else
            {
                TextBlock TxtNoWeather = new TextBlock()
                {
                    Width = weatherCanvas.Width * 0.85,
                    FontSize = data.FontSizeMedium,
                    Foreground = foregroundBrush,
                    FontWeight = data.FontWeight,
                    TextWrapping = TextWrapping.Wrap,
                    Text = AppResources.LockscreenNoWeatherData,
                };

                weatherCanvas.Children.Add(TxtNoWeather);
                Canvas.SetTop(TxtNoWeather, (weatherCanvas.Height - TxtNoWeather.ActualHeight) / 2);
                Canvas.SetLeft(TxtNoWeather, (weatherCanvas.Width - TxtNoWeather.ActualWidth) / 2);
            }
            
            return weatherCanvas;
        }

        public static Canvas GetWeatherBackCanvas(LivetileData data)
        {
            SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.White);
            Canvas weatherCanvas = new Canvas();

            weatherCanvas.Width = data.AreaSize.Width;
            weatherCanvas.Height = data.AreaSize.Height;

            if (data.Forecasts != null)
            {
                Canvas forecastCanvas = new Canvas();
                DayNameConverter dayNameConverter = new DayNameConverter();
                WeatherRangeConverter tempConverter = new WeatherRangeConverter();
                short row = 0;
                short col = 0;

                if (data.Forecasts.Items.Count == 7)
                {
                    data.Forecasts.Items.RemoveAt(0);
                }

                for (int i = 0; i < data.Forecasts.Items.Count; i++)
                {
                    Forecast forecast = data.Forecasts.Items[i];

                    Canvas dayCanvas = new Canvas();
                    dayCanvas.Width = (weatherCanvas.Width - 40) / 3;
                    dayCanvas.Height = (weatherCanvas.Height - 30) / 2;

                    TextBlock dayName = new TextBlock()
                    {
                        Text = dayNameConverter.Convert(forecast.AltTitle, null, null, System.Globalization.CultureInfo.CurrentCulture) as string,
                        FontSize = data.FontSizeMedium * 1.2,
                        Foreground = foregroundBrush,
                        FontWeight = data.FontWeight
                    };
                    dayCanvas.Children.Add(dayName);
                    Canvas.SetLeft(dayName, (dayCanvas.Width - dayName.ActualWidth) / 2);
                    Canvas.SetTop(dayName, 0);

                    WeatherIconType type = WeatherIconMap.Instance.WeatherIconType;
                    string forecastPath = string.Format(WeatherBug.ICON_LOCAL_PATH.Substring(1), type.ToString().ToLower(), "80x67", WeatherImageIconConverter.GetIamgetName(forecast.ImageIcon));
                    WriteableBitmap forecastWeatherWb = BitmapFactory.New(0, 0).FromContent(forecastPath);

                    Image forecastImage = new Image()
                    {
                        Source = forecastWeatherWb,
                        Width = forecastWeatherWb.PixelWidth,
                        Height = forecastWeatherWb.PixelHeight //0.84는 아이콘 이미지의 원본 비율임
                    };
                    dayCanvas.Children.Add(forecastImage);
                    Canvas.SetLeft(forecastImage, (dayCanvas.Width - forecastImage.Width) / 2);
                    Canvas.SetTop(forecastImage, dayName.ActualHeight + 5);

                    TextBlock tempRange = new TextBlock()
                    {
                        Text = tempConverter.Convert(forecast.LowHigh, null, null, System.Globalization.CultureInfo.CurrentCulture) as string,
                        FontSize = data.FontSizeMedium,
                        Foreground = foregroundBrush,
                        FontWeight = data.FontWeight
                    };
                    dayCanvas.Children.Add(tempRange);
                    Canvas.SetLeft(tempRange, (dayCanvas.Width - tempRange.ActualWidth) / 2);
                    Canvas.SetTop(tempRange, dayName.ActualHeight + 5 + forecastImage.Height + 5);

                    forecastCanvas.Children.Add(dayCanvas);

                    if (i == 3)
                    {
                        row++;
                        col = 0;
                    }

                    Canvas.SetLeft(dayCanvas, col++ * dayCanvas.Width + 20);
                    Canvas.SetTop(dayCanvas, row * dayCanvas.Height + 20);

                }

                weatherCanvas.Children.Add(forecastCanvas);
            }
            else
            {
                TextBlock TxtNoWeather = new TextBlock()
                {
                    Width = weatherCanvas.Width * 0.85,
                    FontSize = data.FontSizeMedium,
                    Foreground = foregroundBrush,
                    FontWeight = data.FontWeight,
                    TextWrapping = TextWrapping.Wrap,
                    Text = AppResources.LockscreenNoWeatherBackData,
                };

                weatherCanvas.Children.Add(TxtNoWeather);
                Canvas.SetTop(TxtNoWeather, (weatherCanvas.Height - TxtNoWeather.ActualHeight) / 2);
                Canvas.SetLeft(TxtNoWeather, (weatherCanvas.Width - TxtNoWeather.ActualWidth) / 2);
            }

            return weatherCanvas;
        }
        #endregion
        
        #region Battery Live Tile
        public static ShellTileData GetBatteryLivetileData()
        {
            ShellTileData tileData = new FlipTileData()
            {
                Title = string.Empty,
                BackTitle = AppResources.ApplicationTitle,
                BackContent = GetBatteryBackTextContent(),
                SmallBackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.battery.small.jpg", UriKind.Absolute),
                BackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.battery.jpg", UriKind.Absolute),
                BackBackgroundImage = new Uri("isostore:/Shared/ShellContent/livetile.battery.back.jpg", UriKind.Absolute),
            };
            return tileData;
        }

        public static string GetBatteryBackTextContent()
        {
            StringBuilder tr = new StringBuilder();
            tr.Append(AppResources.LivetileBatteryRemaining);
            tr.Append("\n");
            if (BatteryHelper.BatteryTime.Days > 0)
            {
                tr.Append(" ");
                tr.AppendFormat(AppResources.DayShortener, BatteryHelper.BatteryTime.Days);
                tr.Append("\n");
            }
            tr.Append(" ");
            tr.AppendFormat(AppResources.HourShortener, BatteryHelper.BatteryTime.Hours);
            tr.Append("\n");;


            if (!BatteryHelper.IsCharging)
            {
                return tr.ToString();
            }
            else
            {
                return string.Format("{0} :\n {1}", AppResources.LivetileBatteryRemaining, AppResources.Unknown);
            }
        }

        public static Canvas GetBatteryCanvas(LivetileData data)
        {
            Canvas batteryCanvas = new Canvas();
            batteryCanvas.Width = data.AreaSize.Width;
            batteryCanvas.Height = data.AreaSize.Height;

            WriteableBitmap batteryWb = BitmapFactory.New(0, 0).FromContent("Images/livetile/battery.png");
            WriteableBitmap flashWb = BitmapFactory.New(0, 0).FromContent("Images/livetile/flash.png");

            Image batteryImage = new Image()
            {
                Source = batteryWb
            };
            
            Image flashImage = new Image()
            {
                Source = flashWb
            };

            TextBlock batteryPercent = new TextBlock()
            {
                FontSize = data.FontSizeExtraExtraLarge * 1.5,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.SemiBold,
                Text = BatteryHelper.BateryLevel.ToString()
            };
            
            if (BatteryHelper.BateryLevel == 100)
            {
                int batteryFullIndicatoion = (Int32)(SettingHelper.Get(Constants.LIVETILE_BATTERY_FULL_INDICATION) as PickerItem).Key;
                if (batteryFullIndicatoion == 100)
                {
                    batteryPercent.Text = AppResources.BatteryFull;
                }
                else
                {
                    batteryPercent.Text = "99";
                }
            }

            System.Windows.Shapes.Rectangle batterySolid = new System.Windows.Shapes.Rectangle()
            {
                Fill = batteryPercent.Foreground,
                Width = batteryWb.PixelWidth * 0.49,
                Height = batteryWb.PixelHeight * 0.6 * BatteryHelper.BateryLevel / 100
            };

            Canvas.SetLeft(flashImage, batteryCanvas.Width - flashWb.PixelWidth - 20);
            Canvas.SetTop(flashImage, 30);

            double totalWidth = batteryWb.PixelWidth + batteryPercent.ActualWidth;
            Canvas.SetLeft(batteryImage, (batteryCanvas.Width - totalWidth) / 2 - 10);
            Canvas.SetTop(batteryImage, (batteryCanvas.Height - batteryWb.PixelHeight) / 2);

            Canvas.SetLeft(batterySolid, (batteryCanvas.Width - totalWidth) / 2 + batteryWb.PixelWidth * 0.1);
            Canvas.SetTop(batterySolid, (batteryCanvas.Height - batteryWb.PixelHeight) / 2 + batteryWb.PixelHeight * 0.265 + (batteryWb.PixelHeight * 0.6) - batterySolid.Height);

            Canvas.SetLeft(batteryPercent, (batteryCanvas.Width - totalWidth) / 2 + batteryWb.PixelWidth + 10);
            Canvas.SetTop(batteryPercent, (batteryCanvas.Height - batteryPercent.ActualHeight) / 2 + 10);

            batteryCanvas.Children.Add(flashImage);
            batteryCanvas.Children.Add(batteryImage);
            batteryCanvas.Children.Add(batterySolid);
            batteryCanvas.Children.Add(batteryPercent);

            return batteryCanvas;
        }
        #endregion

        public static PngEncoder pngEncoder = new PngEncoder();

        public static void CreateLivetileImage(LivetileData data, LiveItems liveItem) 
        {
            Canvas panel = null;
            WriteableBitmap wb = null;

            switch (liveItem)
            {
                case LiveItems.Calendar:
                    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        panel = GetCalendarCanvas(data);
                        panel.Background = data.GetBackgroundBrush(LiveItems.Calendar);
                        
                        System.Diagnostics.Debug.WriteLine("달력 전 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.calendar.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            wb = new WriteableBitmap(panel, null);
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                        }

                        System.Diagnostics.Debug.WriteLine("달력 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        wb = wb.Resize(159, 159, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                        System.Diagnostics.Debug.WriteLine("달력 줄임 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.calendar.small.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                        }
                        System.Diagnostics.Debug.WriteLine("작은 달력 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);

                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.calendar.back.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            panel.Children.Clear();
                            panel.Width = 1;
                            panel.Height = 1;
                            wb = new WriteableBitmap(panel, null);
                            //System.Diagnostics.Debug.WriteLine("###########달력 이미지 저장전 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                            //System.Diagnostics.Debug.WriteLine("###########달력 이미지 저장후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        }
                        System.Diagnostics.Debug.WriteLine("타일 뒷면 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                    }
                    break;
                case LiveItems.Weather:
                    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        SolidColorBrush backBrush = data.GetBackgroundBrush(LiveItems.Weather);
                        panel = GetWeatherCanvas(data);
                        panel.Background = backBrush;

                        System.Diagnostics.Debug.WriteLine("날씨 전 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.weather.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            wb = new WriteableBitmap(panel, null);
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                        }
                        System.Diagnostics.Debug.WriteLine("날씨 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        wb = wb.Resize(159, 159, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                        System.Diagnostics.Debug.WriteLine("날씨 줄임 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.weather.small.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                        }
                        System.Diagnostics.Debug.WriteLine("작은 날씨 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        
                        //날씨 백타일 (주간 일보)
                        panel = GetWeatherBackCanvas(data);
                        panel.Background = backBrush;

                        System.Diagnostics.Debug.WriteLine("주간 날씨 전 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.weather.back.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            wb = new WriteableBitmap(panel, null);
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                        }
                        System.Diagnostics.Debug.WriteLine("주간 날씨 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                    }
                    break;
                case LiveItems.Battery:
                    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        panel = GetBatteryCanvas(data);
                        panel.Background = data.GetBackgroundBrush(LiveItems.Battery);

                        System.Diagnostics.Debug.WriteLine("배터리 전 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.battery.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            wb = new WriteableBitmap(panel, null);
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                        }
                        System.Diagnostics.Debug.WriteLine("배터리 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        wb = wb.Resize(159, 159, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                        System.Diagnostics.Debug.WriteLine("배터리 줄임 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.battery.small.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                        }
                        System.Diagnostics.Debug.WriteLine("작은 배터리 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);


                        using (IsolatedStorageFileStream targetStream = isoStore.OpenFile("Shared/ShellContent/livetile.battery.back.jpg", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            panel.Children.Clear();
                            panel.Width = 1;
                            panel.Height = 1;
                            wb = new WriteableBitmap(panel, null);
                            //wb.SaveJpeg(targetStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            pngEncoder.Encode(wb.ToImage(), targetStream);
                        }
                        System.Diagnostics.Debug.WriteLine("배터리 뒷면 저장 후 =>" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
                    }
                    break;
            }

            wb = null;
            GC.Collect();
        }
    }
}
