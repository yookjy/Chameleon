using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Phone.System.UserProfile;
using ChameleonLib.Api.Open.Weather;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Converter;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace ChameleonLib.Helper
{
    public class AsyncLockScreenResult : IAsyncResult
    {
        public AsyncLockScreenResult(object asyncState, System.Threading.WaitHandle asyncWaitHandle, bool completedSynchronously, bool isCompleted)
        {
            AsyncState = asyncState;
            AsyncWaitHandle = asyncWaitHandle;
            CompletedSynchronously = completedSynchronously;
            IsCompleted = isCompleted;
        }

        public object AsyncState
        {
            get;
            private set;
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get;
            private set;
        }

        public bool CompletedSynchronously
        {
            get;
            private set;
        }

        public bool IsCompleted
        {
            get;
            private set;
        }
    }

    public class LockscreenHelper
    {
        //private static double compensateRatio = DeviceHelper.IsAfterGddr3 ? 1 : 0.9;
        
        private static Size size;
        
        public static Size Size
        {
            get
            {
                if (size.Width == 0)
                {
                    //size = new Size(480 * compensateRatio, 800 * compensateRatio);
                    size = new Size(480, 800);
                }
                return size;
            }
        }
        //WVGA이면 288x288의 크기이고 Left, Top정렬에 Margin = {24, 55, 168, 44} 이내에서
        //WXGA이면 460x460의 크기이고 Left, Tpp정렬에 Margin = {39, 88, 269, 73} 이내에서
        //HD720p이면 432x432의 크기이고 Left, Tpp정렬에 Margin = {36, 84, 252, 146} 이내에서
        public static Size DisplayAreaSize
        {
            get
            {
                Size size = new Size();
                size.Height = (int)(ResolutionHelper.ScaleFactor * 288);
                size.Width = (int)(ResolutionHelper.CurrentResolution.Width - DisplayAreaMargin.Left * 2);
                return size;
            }
        }

        public static Thickness DisplayAreaMargin
        {
            get
            {
                Thickness margin = new Thickness();
                margin.Left = Math.Ceiling(ResolutionHelper.ScaleFactor * 24);
                margin.Top = Math.Ceiling(ResolutionHelper.ScaleFactor * 55);
                margin.Right = Math.Ceiling(ResolutionHelper.ScaleFactor * 168);
                margin.Bottom = Math.Ceiling(ResolutionHelper.ScaleFactor * 44);
                return margin;
            }
        }

        public static Size GetDisplayAreaSize(Size sourceSize)
        {
            Size size = DisplayAreaSize;

            size.Width *= sourceSize.Width / ResolutionHelper.CurrentResolution.Width;
            size.Height *= sourceSize.Height / ResolutionHelper.CurrentResolution.Height;

            return size;
        }

        public static Thickness GetDisplayAreaMargin(Size sourceSize)
        {
            Thickness margin = DisplayAreaMargin;
            margin.Left *= sourceSize.Width / ResolutionHelper.CurrentResolution.Width;
            margin.Top *= sourceSize.Height / ResolutionHelper.CurrentResolution.Height;
            margin.Right *= sourceSize.Width / ResolutionHelper.CurrentResolution.Width;
            margin.Bottom *= sourceSize.Height / ResolutionHelper.CurrentResolution.Height;

            return margin;
        }

        public static double GetFontSizeRatio(bool isWVGA)
        {
            //return (isWVGA ? 1 : ResolutionHelper.ScaleFactor) / 1.6 * compensateRatio;
            return (isWVGA ? 1 : ResolutionHelper.ScaleFactor) / 1.6;
        }

        public static Size ThumnailSize
        {
            get
            {
                Size res = ResolutionHelper.CurrentResolution;
                double ratio = res.Height / res.Width;
                int width = 100;
                return new Size(width, (int)(width * ratio));
            }
        }

        public static async Task<LockScreenRequestResult> SetLockscreenProvider()
        {
            return await Task.Run<LockScreenRequestResult>(async () => 
            {
                LockScreenRequestResult result = LockScreenRequestResult.Denied;
                try
                {
                    var isProvider = LockScreenManager.IsProvidedByCurrentApplication;
                    if (!isProvider)
                    {
                        // If you're not the provider, this call will prompt the user for permission.
                        // Calling RequestAccessAsync from a background agent is not allowed.
                        result = await LockScreenManager.RequestAccessAsync();
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
                return result;
            });
        }

        public static async void SetLockscreen(string filePathOfTheImage, bool isAppResource, AsyncCallback asyncCallback)
        {
            AsyncLockScreenResult result = null;   
            try
            {
                var isProvider = LockScreenManager.IsProvidedByCurrentApplication;
                if (!isProvider)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    var op = await LockScreenManager.RequestAccessAsync();
                    
                    // Only do further work if the access was granted.
                    isProvider = (op == LockScreenRequestResult.Granted);
                }

                if (isProvider)
                {
                    // At this stage, the app is the active lock screen background provider.

                    // The following code example shows the new URI schema.
                    // ms-appdata points to the root of the local app data folder.
                    // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package.
                    var schema = isAppResource ? Constants.PREFIX_APP_RESOURCE_FOLDER : Constants.PREFIX_APP_DATA_FOLDER;
                    var uri = new Uri(schema + filePathOfTheImage, UriKind.Absolute);

                    // Set the lock screen background image.
                    LockScreen.SetImageUri(uri);

                    // Get the URI of the lock screen background image.
                    var currentImage = LockScreen.GetImageUri();
                    System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage.ToString());
                    result = new AsyncLockScreenResult(true, null, true, true);
                }
                else
                {
                    //"You said no, so I can't update your background."
                    System.Diagnostics.Debug.WriteLine("아니오가 선택되어 잠금화면으로 설정할 수 없음."); //"You said no, so I can't update your background."
                    result = new AsyncLockScreenResult(false, null, true, true);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                result = new AsyncLockScreenResult(ex.Message, null, true, true);
            }
            
            asyncCallback.Invoke(result);
        }

        public static Canvas GetWeatherCanvas(double width, double height, LockscreenData data)
        {
            Canvas weatherCanvas = new Canvas();
            weatherCanvas.Width = width;
            weatherCanvas.Height = height;
            Thickness margin = new Thickness(10 * data.FontRatio);

            if (data.LiveWeather != null)
            {
                WeatherIconType type = WeatherIconMap.Instance.WeatherIconType;
                string path = string.Format(WeatherBug.ICON_LOCAL_PATH.Substring(1), type.ToString().ToLower(), "125x105", WeatherImageIconConverter.GetIamgetName(data.LiveWeather.CurrentConditionIcon));
                //string path = string.Format(WeatherBug.ICON_LOCAL_PATH.Substring(1), "125x105", WeatherImageIconConverter.GetIamgetName(data.LiveWeather.CurrentConditionIcon));
                WriteableBitmap weatherWb = BitmapFactory.New(0, 0).FromContent(path);

                //날씨 이미지
                Image ImgLiveWeather = new Image()
                {
                    Margin = new Thickness(margin.Left, 0, 0, 0),
                    Source = weatherWb,
                    Height = 140 * data.FontRatio,
                    Width = weatherCanvas.Width / 2
                };

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
                TextBlock TxtLocation = new TextBlock()
                {
                    Text = tmp,
                    FontSize = data.FontSizeLarge,
                    Foreground = data.ForegroundBrush,
                    Width = ImgLiveWeather.Width * 2.1,
                    FontWeight = data.FontWeight
                };

                if (TxtLocation.ActualWidth > weatherCanvas.Width)
                {
                    TxtLocation.Text = data.LiveWeather.Station.City;
                }

                Canvas tempImageCanvas = new Canvas()
                {
                    Width = ImgLiveWeather.Width,
                    Height = ImgLiveWeather.Height
                };

                Canvas tempTextCanvas = new Canvas()
                {
                    Width = tempImageCanvas.Width,
                    Height = tempImageCanvas.Height
                };

                Canvas etcWeaterCanvas = new Canvas()
                {
                    Width = weatherCanvas.Width,
                    Height = 32 * data.FontRatio
                };

                Canvas.SetLeft(TxtLocation, margin.Left);
                Canvas.SetTop(TxtLocation, margin.Top);
                Canvas.SetLeft(tempTextCanvas, margin.Left);
                Canvas.SetTop(tempTextCanvas, margin.Top + TxtLocation.ActualHeight);
                Canvas.SetLeft(tempImageCanvas, tempTextCanvas.Width - margin.Left);
                Canvas.SetTop(tempImageCanvas, margin.Top + TxtLocation.ActualHeight);
                Canvas.SetLeft(etcWeaterCanvas, margin.Left);
                Canvas.SetTop(etcWeaterCanvas, margin.Top + TxtLocation.ActualHeight + tempImageCanvas.Height);

                weatherCanvas.Children.Add(TxtLocation);
                weatherCanvas.Children.Add(tempTextCanvas);
                weatherCanvas.Children.Add(tempImageCanvas);
                weatherCanvas.Children.Add(etcWeaterCanvas);

                //기온
                string[] temp = data.LiveWeather.Temp.Value.Value.Split('.');
                TextBlock TxtLiveWeatherTemp = new TextBlock()
                {
                    Text = temp[0],
                    FontSize = data.FontSizeExtraExtraLarge * 1.2,
                    Foreground = data.ForegroundBrush,
                    FontWeight = data.FontWeight
                };
                TextBlock TxtLiveWeatherTempFloat = new TextBlock()
                {
                    Text = (temp.Length > 1) ? string.Format(".{0}", temp[1]) : ".0",
                    FontSize = data.FontSizeMedium * 1.25,
                    Foreground = data.ForegroundBrush,
                    FontWeight = data.FontWeight
                };
                TextBlock TxtLiveWeatherTempUnits = new TextBlock()
                {
                    Text = (WeatherUnitsConverter.ConvertOnlyUnit(data.LiveWeather.Temp.Value) as ValueUnits).Units,
                    FontSize = data.FontSizeMedium * 1.15,
                    Foreground = data.ForegroundBrush,
                    FontWeight = data.FontWeight
                };

                double tempCanvasTopMargin = (ImgLiveWeather.Height - TxtLiveWeatherTemp.ActualHeight) / 2;
                double tempCanvasLeftMargin = (tempTextCanvas.Width - TxtLiveWeatherTemp.ActualWidth) / 2 - TxtLiveWeatherTempUnits.ActualWidth;
                Canvas.SetLeft(TxtLiveWeatherTemp, tempCanvasLeftMargin);
                Canvas.SetTop(TxtLiveWeatherTemp, tempCanvasTopMargin);
                Canvas.SetLeft(TxtLiveWeatherTempUnits, tempCanvasLeftMargin + TxtLiveWeatherTemp.ActualWidth);
                Canvas.SetTop(TxtLiveWeatherTempUnits, tempCanvasTopMargin + (TxtLiveWeatherTempUnits.ActualHeight / 2) - 5);
                Canvas.SetLeft(TxtLiveWeatherTempFloat, tempCanvasLeftMargin + TxtLiveWeatherTemp.ActualWidth + TxtLiveWeatherTempFloat.ActualWidth / 9);
                Canvas.SetTop(TxtLiveWeatherTempFloat, tempCanvasTopMargin + (TxtLiveWeatherTempUnits.ActualHeight / 2) + TxtLiveWeatherTempUnits.ActualHeight + 5);
                tempTextCanvas.Children.Add(TxtLiveWeatherTemp);
                tempTextCanvas.Children.Add(TxtLiveWeatherTempFloat);
                tempTextCanvas.Children.Add(TxtLiveWeatherTempUnits);

                Canvas.SetLeft(ImgLiveWeather, 0);
                Canvas.SetTop(ImgLiveWeather, 0);
                tempImageCanvas.Children.Add(ImgLiveWeather);

                Image imageWater = new Image()
                {
                    Width = 20 * data.FontRatio,
                    Height = 20 * data.FontRatio,
                    Source = BitmapFactory.New(0, 0).FromContent("Images/lockscreen/water.png")
                };

                Image imageWind = new Image() {
                    Width = 32 * data.FontRatio,
                    Height = 32 * data.FontRatio,
                    Source = BitmapFactory.New(0, 0).FromContent("Images/lockscreen/wind.png")
                };
                //습도
                TextBlock TxtLiveWeatherHumidity = new TextBlock()
                {
                    Text = data.LiveWeather.Humidity.Value.Value + data.LiveWeather.Humidity.Value.Units,
                    FontSize = data.FontSizeMedium * 1.1,
                    Foreground = data.ForegroundBrush,
                    FontWeight = data.FontWeight
                };
                //바람
                TextBlock TxtLiveWeatherWind = new TextBlock()
                {
                    Text = data.LiveWeather.WindSpeed.Value + data.LiveWeather.WindSpeed.Units,
                    FontSize = data.FontSizeMedium * 1.1,
                    Foreground = data.ForegroundBrush,
                    FontWeight = data.FontWeight
                };

                Canvas.SetLeft(imageWater, tempCanvasLeftMargin);
                Canvas.SetTop(imageWater, (imageWind.Height - imageWater.Height) / 2);

                Canvas.SetLeft(TxtLiveWeatherHumidity, tempCanvasLeftMargin + imageWater.Width + margin.Left / 2);
                Canvas.SetTop(TxtLiveWeatherHumidity, (imageWind.Height - TxtLiveWeatherHumidity.ActualHeight) / 2);

                Canvas.SetLeft(imageWind, tempCanvasLeftMargin + imageWater.Width + TxtLiveWeatherHumidity.ActualWidth + 20 + margin.Left);

                Canvas.SetLeft(TxtLiveWeatherWind, tempCanvasLeftMargin + imageWater.Width + TxtLiveWeatherHumidity.ActualWidth + 20 + imageWind.Width + margin.Left * 3 / 2);
                Canvas.SetTop(TxtLiveWeatherWind, (imageWind.Height - TxtLiveWeatherWind.ActualHeight) / 2);

                etcWeaterCanvas.Children.Add(imageWater);
                etcWeaterCanvas.Children.Add(TxtLiveWeatherHumidity);
                etcWeaterCanvas.Children.Add(imageWind);
                etcWeaterCanvas.Children.Add(TxtLiveWeatherWind);

                //주간일보 
                if (data.Forecasts != null)
                {
                    Canvas forecastCanvas = new Canvas();
                    DayNameConverter dayNameConverter = new DayNameConverter();
                    WeatherRangeConverter tempConverter = new WeatherRangeConverter();

                    for (int i = 0; i < 3; i++)
                    {
                        Forecast forecast = data.Forecasts.Items[i];

                        Canvas dayCanvas = new Canvas();
                        dayCanvas.Width = etcWeaterCanvas.Width / 3;

                        TextBlock dayName = new TextBlock()
                        {
                            Text = dayNameConverter.Convert(forecast.AltTitle, null, null, System.Globalization.CultureInfo.CurrentCulture) as string,
                            FontSize = data.FontSizeMedium * 0.85,
                            Foreground = data.ForegroundBrush,
                            FontWeight = data.FontWeight
                        };
                        dayCanvas.Children.Add(dayName);
                        Canvas.SetLeft(dayName, (dayCanvas.Width - dayName.ActualWidth) / 2);
                        Canvas.SetTop(dayName, margin.Top * 2);

                        WeatherIconType iconType = WeatherIconMap.Instance.WeatherIconType;
                        string forecastPath = string.Format(WeatherBug.ICON_LOCAL_PATH.Substring(1), iconType.ToString().ToLower(), "80x67", WeatherImageIconConverter.GetIamgetName(forecast.ImageIcon));
                        WriteableBitmap forecastWeatherWb = BitmapFactory.New(0, 0).FromContent(forecastPath);

                        Image forecastImage = new Image()
                        {
                            Source = forecastWeatherWb,
                            Width = dayCanvas.Width * 0.7,
                            Height = dayCanvas.Width * 0.7 * 0.84 //0.84는 아이콘 이미지의 원본 비율임
                        };
                        dayCanvas.Children.Add(forecastImage);
                        Canvas.SetLeft(forecastImage, (dayCanvas.Width - forecastImage.Width) / 2);
                        Canvas.SetTop(forecastImage, dayName.ActualHeight + margin.Top + margin.Top / 2);

                        TextBlock tempRange = new TextBlock()
                        {
                            Text = tempConverter.Convert(forecast.LowHigh, null, null, System.Globalization.CultureInfo.CurrentCulture) as string,
                            FontSize = data.FontSizeMedium * 0.8,
                            Foreground = data.ForegroundBrush,
                            FontWeight = data.FontWeight
                        };
                        dayCanvas.Children.Add(tempRange);
                        Canvas.SetLeft(tempRange, (dayCanvas.Width - tempRange.ActualWidth) / 2);
                        Canvas.SetTop(tempRange, dayName.ActualHeight + forecastImage.Height + margin.Top + margin.Top / 2);

                        forecastCanvas.Children.Add(dayCanvas);
                        Canvas.SetLeft(dayCanvas, i * dayCanvas.Width);
                    }
                    
                    weatherCanvas.Children.Add(forecastCanvas);
                    Canvas.SetTop(forecastCanvas, TxtLocation.ActualHeight + tempTextCanvas.Height + etcWeaterCanvas.Height);
                    Canvas.SetLeft(forecastCanvas, 0);
                }
            }
            else
            {
                TextBlock TxtNoWeather = new TextBlock()
                {
                    Width = weatherCanvas.Width * 0.85,
                    FontSize = data.FontSizeMedium,
                    Foreground = data.ForegroundBrush,
                    TextWrapping = TextWrapping.Wrap,
                    Text = AppResources.LockscreenNoWeatherData
                };

                weatherCanvas.Children.Add(TxtNoWeather);
                Canvas.SetTop(TxtNoWeather, (weatherCanvas.Height - TxtNoWeather.ActualHeight) / 2);
                Canvas.SetLeft(TxtNoWeather, (weatherCanvas.Width - TxtNoWeather.ActualWidth) / 2);
            }
            return weatherCanvas;
        }

        public static Canvas GetBatteryCanvas(double width, double height, LockscreenData data)
        {
            Canvas batteryCanvas = new Canvas();
            batteryCanvas.Width = width;
            batteryCanvas.Height = height;

            Thickness margin = new Thickness(10 * data.FontRatio);
            
            Image batteryImage = new Image()
            {
                Width = 92 * data.FontRatio,
                Height = 92 * data.FontRatio,
                Source = BitmapFactory.New(0, 0).FromContent(
                    BatteryHelper.BateryLevel <= 5 ? "Images/lockscreen/battery.empty.png" : "Images/lockscreen/battery.full.png")
            };
            batteryCanvas.Children.Add(batteryImage);

            //if (!BatteryHelper.IsCharging && BatteryHelper.BateryLevel > 5)
            if (BatteryHelper.BateryLevel > 5)
            {
                int level = 78 * BatteryHelper.BateryLevel / 100;
                (batteryImage.Source as WriteableBitmap).FillRectangle(17, 64 - 15, 17 + level, 64 + 15, Colors.White);  //128x128 이미지 일때
            }

            Canvas batteryStatusCanvas = new Canvas();
            TextBlock batteryRemaining = new TextBlock()
            {
                Text = BatteryHelper.BateryLevel.ToString(),
                Foreground = data.ForegroundBrush,
                FontSize = data.FontSizeLarge,
                FontWeight = data.FontWeight
            };
            batteryStatusCanvas.Children.Add(batteryRemaining);

            Canvas.SetLeft(batteryRemaining, margin.Left);
            Canvas.SetTop(batteryRemaining, 0);

            TextBlock batteryRemainingDesc = new TextBlock() {
                Text = string.Format("% {0}", AppResources.BatteryRemaining),
                Foreground = data.ForegroundBrush,
                FontSize = data.FontSizeMedium,
                FontWeight = data.FontWeight
            };
            batteryStatusCanvas.Children.Add(batteryRemainingDesc);

            Canvas.SetLeft(batteryRemainingDesc, batteryRemaining.ActualWidth + margin.Left);
            Canvas.SetTop(batteryRemainingDesc, data.FontSizeLarge - data.FontSizeMedium);

            TextBlock timeRemaining = new TextBlock();

            if (BatteryHelper.IsCharging)
            {
            //    timeRemaining.Text = AppResources.BatteryCharging;
            }
            else
            {
                StringBuilder tr = new StringBuilder();
                if (BatteryHelper.BatteryTime.Days > 0)
                {
                    tr.AppendFormat(AppResources.DayShortener, BatteryHelper.BatteryTime.Days);
                    tr.Append(" ");
                }
                tr.AppendFormat(AppResources.HourShortener, BatteryHelper.BatteryTime.Hours);
                tr.Append(" ");
                tr.Append(AppResources.BatteryRemaining);
                timeRemaining.Text = tr.ToString();
            }
            timeRemaining.Foreground = data.ForegroundBrush;
            timeRemaining.FontSize = data.FontSizeMedium;
            timeRemaining.FontWeight = data.FontWeight;

            Canvas.SetLeft(timeRemaining, margin.Left);
            Canvas.SetTop(timeRemaining, batteryRemaining.ActualHeight);

            batteryStatusCanvas.Children.Add(timeRemaining);
            batteryCanvas.Children.Add(batteryStatusCanvas);

            double leftMargin = (batteryCanvas.Width - (batteryImage.Width + (Math.Max(batteryRemaining.ActualWidth + batteryRemainingDesc.ActualWidth, timeRemaining.ActualWidth)))) / 2 - margin.Left;

            Canvas.SetLeft(batteryImage, leftMargin);
            Canvas.SetTop(batteryImage, (batteryCanvas.Height - batteryImage.ActualHeight) / 2);    
                

            Canvas.SetTop(batteryStatusCanvas, (batteryCanvas.Height - (batteryRemaining.ActualHeight + timeRemaining.ActualHeight)) / 2);
            Canvas.SetLeft(batteryStatusCanvas, leftMargin + batteryImage.Width + margin.Left);

            return batteryCanvas;
        }

        public static Canvas GetCalendarCanvas(double width, double height, LockscreenData data)
        {
            Canvas calendarCanvas = new Canvas();
            calendarCanvas.Width = width;
            calendarCanvas.Height = height;
            SolidColorBrush phoneInactiveBrush = (SolidColorBrush)Application.Current.Resources["PhoneInactiveBrush"];
            SolidColorBrush phoneSemitransparentBrush = (SolidColorBrush)Application.Current.Resources["PhoneSemitransparentBrush"];

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
                    Width = calendarCanvas.Width / 7 ,
                    Height = calendarCanvas.Height / 7 ,
                };

                TextBlock dayTxt = new TextBlock()
                {
                    Text = day.DayName,
                    FontSize = day.FontSize * data.FontRatio,
                    Foreground = day.ForegroundBrush.Color == phoneInactiveBrush.Color ? phoneSemitransparentBrush : day.ForegroundBrush,
                    FontWeight = data.FontWeight
                };

                if (day.DateTime.ToLongDateString() == DateTime.Today.ToLongDateString())
                {
                    Ellipse ellipse = new Ellipse()
                    {
                        Width = dayTxt.ActualHeight * 1.08,
                        Height = dayTxt.ActualHeight * 1.08,
                        Fill = day.BackgroundBrush,
                    };

                    dayPanel.Children.Add(ellipse);
                    Canvas.SetLeft(ellipse, (dayPanel.Width - ellipse.Width) / 2);
                    Canvas.SetTop(ellipse, (dayPanel.Height - ellipse.Width) / 2 - 0.5);
                }

                if (data.DayList[i].AppointmentList != null && data.DayList[i].AppointmentList.Count > 0)
                {
                    TextBlock appCnt = new TextBlock()
                    {
                        FontSize = day.FontSize * data.FontRatio * 0.5,
                        Text = data.DayList[i].AppointmentList.Count.ToString(),
                        Foreground = day.DateTime.ToLongDateString() == DateTime.Now.ToLongDateString() ? day.BackgroundBrush : day.ForegroundBrush,
                        FontWeight = day.ForegroundBrush.Color == phoneInactiveBrush.Color ? FontWeights.Normal : FontWeights.Bold
                    };
                    dayPanel.Children.Add(appCnt);
                    Canvas.SetLeft(appCnt, (dayPanel.Width - appCnt.ActualWidth - 5));
                    Canvas.SetTop(appCnt, -appCnt.ActualHeight / 3);
                }

                dayPanel.Margin = new Thickness((colNum++) * dayPanel.Width, rowNum * dayPanel.Height, 0, 0);

                dayPanel.Children.Add(dayTxt);
                Canvas.SetLeft(dayTxt, (dayPanel.Width - dayTxt.ActualWidth) / 2);
                Canvas.SetTop(dayTxt, (dayPanel.Height - dayTxt.ActualHeight) / 2);
                calendarCanvas.Children.Add(dayPanel);
            }
            return calendarCanvas;
        }

        public static void RenderLayoutToBitmap(LockscreenData data)
        {
            Size areaSize;
            if (data.IsWVGA)
            {
                //함성할 영역
                areaSize = LockscreenHelper.GetDisplayAreaSize(Size);
            }
            else
            {
                //함성할 영역
                areaSize = LockscreenHelper.DisplayAreaSize;
            }
            data.Canvas.Width = areaSize.Width;
            data.Canvas.Height = areaSize.Width / 2;
            
            double width = 0;
            double height = 0;
            LiveItems item;
            Thickness innerMargin;
            LockscreenItemInfo[] lockinfo = data.Items;

            for (int i = 0; i < lockinfo.Length; i++)
            {
                item = lockinfo[i].LockscreenItem;
                innerMargin = new Thickness(0, 0, 5, 0);
                Canvas panel = null;

                if (lockinfo[i].Column == 1)
                {
                    innerMargin.Left = 5;
                    innerMargin.Right = 0;
                }

                if (lockinfo[i].RowSpan < 3)
                {
                    if (lockinfo[i].Row == 0)
                    {
                        innerMargin.Bottom = 5;
                    }
                    else
                    {
                        innerMargin.Top = 5;
                    }
                }

                width = data.Canvas.Width / 2 - (lockinfo[i].Column == 0 ? innerMargin.Right : innerMargin.Left);
                height = data.Canvas.Height / 3 * lockinfo[i].RowSpan - (lockinfo[i].RowSpan < 3 ? (lockinfo[i].Row == 0 ? innerMargin.Bottom : innerMargin.Top) : 0);
                                
                switch (item)
                {
                    case LiveItems.Calendar:
                        panel = LockscreenHelper.GetCalendarCanvas(width, height, data);
                        break;
                    case LiveItems.Battery:
                        panel = LockscreenHelper.GetBatteryCanvas(width, height, data);
                        break;
                    case LiveItems.Weather:
                    case LiveItems.NoForecast:
                        if (lockinfo.Any(x => x.LockscreenItem == LiveItems.NoForecast))
                        {
                            //날씨나 배터리... (RowSpan이 3이 아닌경우이면서, items.Length 가 3보다 작은 경우
                            data.Forecasts = null;
                        }
                        panel = LockscreenHelper.GetWeatherCanvas(width, height, data);
                        break;
                }

                if ((bool)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION))
                {
                    Canvas backPanel = new Canvas()
                    {
                        Background = data.BackgroundBrush,
                        Opacity = data.BackgroundOpacity,
                        Width = panel.Width,
                        Height = panel.Height
                    };
                    panel.Children.Insert(0, backPanel);
                    //panel.Background = data.BackgroundBrush;
                    //panel.Opacity = data.BackgroundOpacity;
                }

                if (lockinfo[i].Column == 1)
                {
                    Canvas.SetLeft(panel, width + innerMargin.Left * 2);
                }

                if (lockinfo[i].Row > 0)
                {
                    //총 아이템이 두개 또는 하나이면서 날씨인 경우
                    if (((lockinfo.Length <= 2 && lockinfo[i].RowSpan == 2)
                        || !(bool)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION))
                        && (lockinfo[i].LockscreenItem == LiveItems.Weather || lockinfo[i].LockscreenItem == LiveItems.NoForecast))
                    {
                        //날씨만 또는 날씨와 달력의 경우 날씨의 상단 여백을 -20
                        //날씨가 배터리 밑에 있고, 배경분리를 사용하지 않는 경우 날씨의 상단여백을 -20 
                        Canvas.SetTop(panel, data.Canvas.Height - height - 20);
                    }
                    else
                    {
                        Canvas.SetTop(panel, data.Canvas.Height - height);
                    }
                }
                else if (lockinfo[i].RowSpan == 2 && (lockinfo[i].LockscreenItem == LiveItems.Weather || lockinfo[i].LockscreenItem == LiveItems.NoForecast)
                    && !(bool)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION))
                {
                    //날씨가 배터리 위에 있는 경우 상단 여백을 10 추가
                    Canvas.SetTop(panel, 10);
                }

                if (lockinfo[i].ColSpan == 2)
                {
                    data.Canvas.Width = panel.Width;
                }

                data.Canvas.Children.Add(panel);
                
                //날씨만을 표시하는 경우
                if (lockinfo.Length == 1 && lockinfo[i].LockscreenItem == LiveItems.NoForecast)
                {
                    Canvas weatherCanvas = (data.Canvas.Children[0] as Canvas);
                    data.Canvas.Height = data.Canvas.Height / 3 * 2 
                        + (double)weatherCanvas.GetValue(Canvas.TopProperty)
                        + (double)weatherCanvas.Children[0].GetValue(Canvas.TopProperty);
                }
            }

            Thickness areaMargin;
            if (data.IsWVGA)
            {
                areaMargin = LockscreenHelper.GetDisplayAreaMargin(new Size(480, 800));
            }
            else
            {
                areaMargin = LockscreenHelper.DisplayAreaMargin;
            }

            if (data.BackgroundPanel != null) 
            {
                data.BackgroundPanel.Width = data.Canvas.Width;
                data.BackgroundPanel.Height = data.Canvas.Height;
            }

            Size size = new Size(data.Canvas.Width, data.Canvas.Height);
            WriteableBitmap canvasWb = new WriteableBitmap((int)data.Canvas.Width, (int)data.Canvas.Height);
            canvasWb.Render(data.Canvas, null);
            canvasWb.Invalidate();
            
            if (data.BackgroundBitmap != null)
            {
                data.BackgroundBitmap.Blit(new Rect(new Point(areaMargin.Left, areaMargin.Top), size), canvasWb, new Rect(new Point(), size));
            }
        }

        public static int CurrentListCount 
        {
            get
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    return isoStore.GetFileNames("*" + Constants.LOCKSCREEN_IMAGE_POSTFIX).Count();
                }
            }
        }
    }
}
