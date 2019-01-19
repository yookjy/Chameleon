using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using ChameleonLib.Helper;
using ChameleonLib.Model;
using ChameleonLib.Resources;
using ChameleonLib.Api.Open.Weather.Model;
using ChameleonLib.Storages;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace Chameleon.View
{
    public partial class SettingPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string PageTitle { get; set; }

        private ScheduleSettings scheduleSettings;

        private Dictionary<string, object> orgSettings;

        private List<ColorItem> items;

        private void loadOrgColorPickerValue()
        {
            orgSettings = new Dictionary<string, object>();
            foreach(var keyVal in IsolatedStorageSettings.ApplicationSettings)
            {
                orgSettings[keyVal.Key] = keyVal.Value;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
            SettingHelper.Save();

            var list = from keyVal in IsolatedStorageSettings.ApplicationSettings
                       //where keyVal.Value != orgSettings[keyVal.Key]
                       where !keyVal.Value.Equals(orgSettings[keyVal.Key])
                       select keyVal;

            PhoneApplicationService.Current.State[Constants.CHANGED_SETTINGS] = list.Count() > 0;
            //보호색이 변경 되었는가?
            PhoneApplicationService.Current.State[Constants.CHAMELEON_USE_PROTECTIVE_COLOR] = list.Any(keyVal =>
            {
                return (keyVal.Key == Constants.CHAMELEON_USE_PROTECTIVE_COLOR ||
                    keyVal.Key == Constants.CHAMELEON_SKIN_BACKGROUND_COLOR);
            });

            string[] keys = {
                              Constants.LIVETILE_RANDOM_BACKGROUND_COLOR,   //라이브타일 랜덤 설정이 변경 되었는가?
                              Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR, //달력 백그라운드 컬러가 변경 되었는가?
                              Constants.LIVETILE_WEATHER_BACKGROUND_COLOR,  //날씨 백그라운드 컬러가 변경 되었는가?
                              Constants.LIVETILE_BATTERY_BACKGROUND_COLOR,  //배터리 백그라운드 컬러가 변경 되었는가?
                              Constants.LIVETILE_FONT_WEIGHT,   //날씨및 달력 타일의 폰트 굵기가 변경되었는가?
                              Constants.LIVETILE_WEATHER_FONT_SIZE,   //날씨 타일 폰트 크기가 변경되었는가?
                              Constants.LIVETILE_BATTERY_FULL_INDICATION,   //배터리 완충 표시가 변경 되었는가?
                              Constants.WEATHER_UNIT_TYPE,                  //날씨 설정이 바뀌었는가?
                              Constants.WEATHER_ICON_TYPE,                  //날씨 아이콘이 바뀌었는가?
                              Constants.CALENDAR_FIRST_DAY_OF_WEEK,         //달력 설정이 바뀌었는가?
                              Constants.CALENDAR_SHOW_APPOINTMENT,          //달력 일정 표시 설정이 바뀌었는가?
                              };

            foreach (string key in keys)
            {
                PhoneApplicationService.Current.State[key] = list.Any(keyVal =>
                {
                    return (keyVal.Key == key);
                });
            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                string piName = NavigationContext.QueryString["piName"];
               
                switch (piName)
                {
                    case "PILivetile":
                        PivotSetting.SelectedItem = PILivetileSetting;
                        break;
                    case "PILockscreen":
                        PivotSetting.SelectedItem = PILockscreenSetting;
                        break;
                    default:
                        PivotSetting.SelectedItem = PIExtraSetting;
                        break;
                }

                InitializeSetting();
            }
        }

        private void InitializeSetting()
        {
            items = new List<ColorItem>();

            items.Add(new ColorItem() { Text = AppResources.ColorChrome, Color = ColorItem.ConvertColor(0xFF1F1F1F), Desc = AppResources.ColorChrome });
            items.Add(new ColorItem() { Text = AppResources.ColorLightGray, Color = ColorItem.ConvertColor(0xFFD3D3D3)/*, Desc = AppResources.ColorLightGray*/ });
            items.Add(new ColorItem() { Text = AppResources.AccentColor, Color = (Color)App.Current.Resources["PhoneAccentColor"]/*, Desc = AppResources.AccentColor*/ });

            for (int i = 0; i < ColorItem.UintColors.Length; i++)
            {
                if (!((Color)App.Current.Resources["PhoneAccentColor"]).Equals(ColorItem.ConvertColor(ColorItem.UintColors[i])))
                {
                    items.Add(new ColorItem() { Color = ColorItem.ConvertColor(ColorItem.UintColors[i]) });
                }
            };
        }

        public SettingPage()
        {
            InitializeComponent();
            PageTitle = string.Format("{0} - {1}", AppResources.ApplicationTitle, AppResources.Settings);
            scheduleSettings = MutexedIsoStorageFile.Read<ScheduleSettings>("ScheduleSettings", Constants.MUTEX_DATA);
            //초기 락스크린 컬러 피커값 저장
            loadOrgColorPickerValue();
        }

        #region Lockscreen Settings

        private void InitializeLockscreenSetting()
        {
            LockscreenItemInfo[][] lockscreenItemInfos = new LockscreenItemInfo[][]
            {
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Weather, Column = 0, Row = 0, ColSpan = 1, RowSpan = 3 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Calendar, Column = 1, Row = 0, ColSpan = 1, RowSpan = 3 }
                },
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Calendar, Column = 0, Row = 0, ColSpan = 1, RowSpan = 3 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Weather, Column = 1, Row = 0, ColSpan = 1, RowSpan = 3 },
                },
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.NoForecast, Column = 0, Row = 0, ColSpan = 1, RowSpan = 2 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Battery, Column = 0, Row = 2, ColSpan = 1, RowSpan = 1 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Calendar, Column = 1, Row = 0, ColSpan = 1, RowSpan = 3 }
                },
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Battery, Column = 0, Row = 0, ColSpan = 1, RowSpan = 1 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.NoForecast, Column = 0, Row = 1, ColSpan = 1, RowSpan = 2 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Calendar, Column = 1, Row = 0, ColSpan = 1, RowSpan = 3 }
                },
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Calendar, Column = 0, Row = 0, ColSpan = 1, RowSpan = 3 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.NoForecast, Column = 1, Row = 0, ColSpan = 1, RowSpan = 2 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Battery, Column = 1, Row = 2, ColSpan = 1, RowSpan = 1 }
                },
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Calendar, Column = 0, Row = 0, ColSpan = 1, RowSpan = 3 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Battery, Column = 1, Row = 0, ColSpan = 1, RowSpan = 1 },
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.NoForecast, Column = 1, Row = 1, ColSpan = 1, RowSpan = 2 }
                },
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Weather, Column = 0, Row = 0, ColSpan = 2, RowSpan = 3 },
                },
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.NoForecast, Column = 0, Row = 0, ColSpan = 2, RowSpan = 2 },
                },
                new LockscreenItemInfo[] 
                {
                    new LockscreenItemInfo{ LockscreenItem = LiveItems.Calendar, Column = 0, Row = 0, ColSpan = 2, RowSpan = 3 },
                },
                new LockscreenItemInfo[] { }
            };

            LockscreenTemplateItem templateItem = SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_TEMPLATE) as LockscreenTemplateItem;
            List<LockscreenTemplateItem> templateList = new List<LockscreenTemplateItem>();
            for (byte i = 0; i < lockscreenItemInfos.Length; i++)
            {
                templateList.Add(new LockscreenTemplateItem()
                {
                    Id = i, LockscreenItemInfos = lockscreenItemInfos[i]
                });

                if (i == templateItem.Id)
                {
                    templateItem = templateList[i];
                }
            }
            LockscreenBackgroundTemplatePicker.ItemsSource = templateList;
            LockscreenBackgroundTemplatePicker.SelectedItem = templateItem;
            LockscreenBackgroundTemplatePicker.SelectionChanged += OnSelectionChangedLockscreenBackgroundTemplatePicker;

            LockscreenBackgroundColortPicker.ItemsSource = items;
            LockscreenBackgroundColortPicker.SelectedItem = FindSelectedColorItem(items, Constants.LOCKSCREEN_BACKGROUND_COLOR);
            LockscreenBackgroundColortPicker.SelectionChanged += OnSelectionChangedColortPicker;

            LockscreenBackgroundOpacity.ValueChanged += OnValueChangeLockscreenBackgroundOpacity;
            LockscreenBackgroundOpacity.Value = (int)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_OPACITY);
            LockscreenBackgroundOpacityHeader.Text = string.Format(AppResources.LockscreenItemBackgroundOpacity, (int)LockscreenBackgroundOpacity.Value);

            UseLockscreenBackgroundItemSeparation.IsChecked = (bool)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION);
            UseLockscreenBackgroundItemSeparation.Checked += OnCheckedUseItem;
            UseLockscreenBackgroundItemSeparation.Unchecked += OnUncheckedUseItem;

            //글자 굵기 피커 데이터 설정
            FontWeight[] fontWeights = new FontWeight[] {
                FontWeights.ExtraBlack,
                FontWeights.Black,
                FontWeights.ExtraBold,
                FontWeights.Bold,
                FontWeights.SemiBold,
                FontWeights.Medium,
            };
            List<PickerItem> fontWeightList = new List<PickerItem>();
            Object selectItem = null;

            foreach (FontWeight fw in fontWeights)
            {
                PickerItem item = new PickerItem()
                {
                    Key = fw.ToString(),
                    Name = fw.ToString()
                };
                fontWeightList.Add(item);

                if ((string)item.Key == SettingHelper.GetString(Constants.LOCKSCREEN_FONT_WEIGHT))
                {
                    selectItem = item;
                }
            }
            LockscreenFontWeightPicker.ItemsSource = fontWeightList;
            LockscreenFontWeightPicker.SelectedItem = selectItem == null ? fontWeightList.FirstOrDefault(x => x.Name == FontWeights.Bold.ToString()) : selectItem;
            LockscreenFontWeightPicker.SelectionChanged += LockscreenFontWeightPicker_SelectionChanged;

            List<PickerItem> intervalList = new List<PickerItem>();
            intervalList.Add(new PickerItem() { Key = 60, Name = 1 + AppResources.Hour });
            intervalList.Add(new PickerItem() { Key = 120, Name = 2 + AppResources.Hours });
            intervalList.Add(new PickerItem() { Key = 180, Name = 3 + AppResources.Hours });
            intervalList.Add(new PickerItem() { Key = 360, Name = 6 + AppResources.Hours });
            intervalList.Add(new PickerItem() { Key = 720, Name = 12 + AppResources.Hours });
            intervalList.Add(new PickerItem() { Key = 1440, Name = 1 + AppResources.Day });
            intervalList.Add(new PickerItem() { Key = 2880, Name = 2 + AppResources.Days });
            intervalList.Add(new PickerItem() { Key = 4320, Name = 3 + AppResources.Days });
            intervalList.Add(new PickerItem() { Key = 10080, Name = 1 + AppResources.Week });

            int interval = scheduleSettings.LockscreenUpdateInterval;
            for (int i = 0; i < intervalList.Count; i++)
            {
                if ((int)intervalList[i].Key == interval)
                {
                    interval = i;
                    break;
                }
            }

            UpdateIntervalLockscreenPicker.ItemsSource = intervalList;
            UpdateIntervalLockscreenPicker.SelectedIndex = interval;
            UpdateIntervalLockscreenPicker.SelectionChanged += OnSelectionChangedUpdateInterval;
        }
        
        private void OnSelectionChangedLockscreenBackgroundTemplatePicker(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                LockscreenTemplateItem templateItem = e.AddedItems[0] as LockscreenTemplateItem;
                SettingHelper.Set(Constants.LOCKSCREEN_BACKGROUND_TEMPLATE, templateItem, false);
            }
        }

        private void OnLoadedLsIBTemplate(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            LockscreenTemplateItem item = (grid.Parent as FrameworkElement).DataContext as LockscreenTemplateItem;
            
            //이미지를 폰의 비율에 맞게 변경
            Image bgImg = grid.Children[0] as Image;
            double ratio = bgImg.RenderSize.Width / ResolutionHelper.CurrentResolution.Width;
            bgImg.Height = ratio * ResolutionHelper.CurrentResolution.Height;
            ((grid.Parent as Grid).Children[0] as TextBlock).Width = bgImg.Width;

            if (item.Id < (LockscreenBackgroundTemplatePicker.ItemsSource as List<LockscreenTemplateItem>).Count - 1)
            {
                Thickness margin = LockscreenHelper.GetDisplayAreaMargin(bgImg.RenderSize);
                Size size = LockscreenHelper.GetDisplayAreaSize(bgImg.RenderSize);
                margin.Right = margin.Left;

                Grid panel = grid.Children[1] as Grid;
                panel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                panel.Margin = margin;
                panel.Width = size.Width;
                panel.Height = (panel.Width - panel.Margin.Left) / 2;
                
                //추가할 이미지의 가로 폭
                double imageWidth = panel.Height;
                LockscreenItemInfo[] lsii = item.LockscreenItemInfos;
                bool useSeparation = (bool)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION);

                if (lsii.Length > 1 && !useSeparation)
                {
                    Canvas backPanel = new Canvas()
                    {
                        Background = new SolidColorBrush((SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_COLOR) as ColorItem).Color),
                        Opacity = (double)(int)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_OPACITY) / 100,
                        Width = panel.Width,
                        Height = panel.Height
                    };
                    panel.Children.Add(backPanel);
                }

                for (int i = 0; i < lsii.Length; i++)
                {
                    Thickness innerMargin = new Thickness(5);

                    if (lsii[i].RowSpan == 1)
                    {
                        switch(lsii[i].Row)
                        {
                            case 0: innerMargin.Bottom = 2.5; break;
                            case 1: innerMargin.Bottom = 2.5; break;
                            case 2: innerMargin.Top = 0; break;
                        }
                    }
                    if (lsii[i].RowSpan == 2)
                    {
                        if (lsii[i].Row == 1)
                        {
                            innerMargin.Top = 2.5;
                            innerMargin.Bottom = 5;
                        }
                    }
                                        
                    if (lsii[i].ColSpan == 2)
                    {
                        panel.Width = imageWidth;
                    }
                    //템플렛 이미지를 생성하여 추가                  
                    Grid imgGrid = new Grid()
                    {
                        Margin = innerMargin
                    };

                    Border imgBorder = new Border()
                    {
                        Child = new Image()
                        {
                            Source = new BitmapImage(new Uri(string.Format("/Images/lockscreen/template.{0}.png", lsii[i].LockscreenItem.ToString().ToLower()), UriKind.Relative)),
                            Width = imageWidth - innerMargin.Left - innerMargin.Right
                        }
                    };

                    Grid.SetRowSpan(imgGrid, lsii[i].RowSpan);
                    Grid.SetColumnSpan(imgGrid, lsii[i].ColSpan);
                    Grid.SetRow(imgGrid, lsii[i].Row);
                    Grid.SetColumn(imgGrid, lsii[i].Column);

                    imgGrid.Children.Add(imgBorder);
                    panel.Children.Add(imgGrid);

                    //분리선 사용인 경우 각각의 아이템에 배경을 입힌다.
                    if (lsii.Length == 1 || useSeparation)
                    {
                        if (lsii.Length == 1)
                        {
                            imgGrid.Margin = new Thickness(0);
                        }
                        imgGrid.Children.Insert(0, new Canvas()
                        {
                            Background = new SolidColorBrush((SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_COLOR) as ColorItem).Color),
                            Opacity = (double)(int)SettingHelper.Get(Constants.LOCKSCREEN_BACKGROUND_OPACITY) / 100,
                            Width = imgGrid.Width,
                            Height = imgGrid.Height
                        });
                    }
                }
            }

            DateTime now = DateTime.Now;
            string dt = now.ToShortTimeString();
            (grid.Children[2] as TextBlock).Text = now.ToString("h:mm");
            (grid.Children[3] as TextBlock).Text = now.ToString("dddd");
            (grid.Children[4] as TextBlock).Text = now.ToString("m").Replace(" ", "");
        }

        private void OnValueChangeLockscreenBackgroundOpacity(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            LockscreenBackgroundOpacityHeader.Text = string.Format(AppResources.LockscreenItemBackgroundOpacity, value);
            SettingHelper.Set(Constants.LOCKSCREEN_BACKGROUND_OPACITY, value, false);
        }

        private async void btnGoToLockSettings_Click(object sender, RoutedEventArgs e)
        {
            // Launch URI for the lock screen settings screen.
            var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
        }

        private void LockscreenFontWeightPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                PickerItem item = e.AddedItems[0] as PickerItem;
                if (item != null)
                {
                    SettingHelper.Set(Constants.LOCKSCREEN_FONT_WEIGHT, item.Key, false);
                }
            }
        }

        #endregion

        #region Livetile Settings
        private void InitializeLivetileSetting()
        {
            UseRandomLivetileColor.IsChecked = (bool)SettingHelper.Get(Constants.LIVETILE_RANDOM_BACKGROUND_COLOR);
            UseRandomLivetileColor.Checked += OnCheckedUseItem;
            UseRandomLivetileColor.Unchecked += OnUncheckedUseItem;

            List<ColorItem> itemList = items.ToList<ColorItem>();
            itemList.Insert(0, new ColorItem() { Text = AppResources.ColorTransparent, Color = ColorItem.ConvertColor(0x00FFFFFF), Desc = AppResources.ColorTransparent });

            LivetileCalendarColorPicker.ItemsSource = itemList;
            LivetileCalendarColorPicker.SelectedItem = FindSelectedColorItem(itemList, Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR);
            LivetileCalendarColorPicker.SelectionChanged += OnSelectionChangedColortPicker;

            LivetileWeatherColorPicker.ItemsSource = itemList;
            LivetileWeatherColorPicker.SelectedItem = FindSelectedColorItem(itemList, Constants.LIVETILE_WEATHER_BACKGROUND_COLOR);
            LivetileWeatherColorPicker.SelectionChanged += OnSelectionChangedColortPicker;

            LivetileBatteryColorPicker.ItemsSource = itemList;
            LivetileBatteryColorPicker.SelectedItem = FindSelectedColorItem(itemList, Constants.LIVETILE_BATTERY_BACKGROUND_COLOR);
            LivetileBatteryColorPicker.SelectionChanged += OnSelectionChangedColortPicker;

            //글자 굵기 피커 데이터 설정
            FontWeight[] fontWeights = new FontWeight[] {
                FontWeights.ExtraBlack,
                FontWeights.Black,
                FontWeights.ExtraBold,
                FontWeights.Bold,
                FontWeights.SemiBold,
                FontWeights.Medium
            };
            List<PickerItem> fontWeightList = new List<PickerItem>();
            Object selectItem = null;

            foreach (FontWeight fw in fontWeights)
            {
                PickerItem item = new PickerItem()
                {
                    Key = fw.ToString(),
                    Name = fw.ToString()
                };
                fontWeightList.Add(item);

                if ((string)item.Key == SettingHelper.GetString(Constants.LIVETILE_FONT_WEIGHT))
                {
                    selectItem = item;
                }
            }
            LivetileFontWeightPicker.ItemsSource = fontWeightList;
            LivetileFontWeightPicker.SelectedItem = selectItem == null ? fontWeightList.FirstOrDefault(x => x.Name == FontWeights.SemiBold.ToString()) : selectItem;
            LivetileFontWeightPicker.SelectionChanged += LivetileFontWeightPicker_SelectionChanged;

            List<PickerItem> fontSizePicer = new List<PickerItem>();
            LivetileWeatherFontSizePicker.ItemsSource = fontSizePicer;
            fontSizePicer.Add(new PickerItem() { Key = 1.0, Name = string.Format(AppResources.Percent, 1.0 * 100) });
            fontSizePicer.Add(new PickerItem() { Key = 1.1, Name = string.Format(AppResources.Percent, 1.1 * 100) });
            fontSizePicer.Add(new PickerItem() { Key = 1.2, Name = string.Format(AppResources.Percent, 1.2 * 100) });
            fontSizePicer.Add(new PickerItem() { Key = 1.3, Name = string.Format(AppResources.Percent, 1.3 * 100) });
            fontSizePicer.Add(new PickerItem() { Key = 1.4, Name = string.Format(AppResources.Percent, 1.4 * 100) });
            fontSizePicer.Add(new PickerItem() { Key = 1.5, Name = string.Format(AppResources.Percent, 1.5 * 100) });
            LivetileWeatherFontSizePicker.SelectedItem = fontSizePicer.Find(x => x.Key.Equals((SettingHelper.Get(Constants.LIVETILE_WEATHER_FONT_SIZE) as PickerItem).Key));
            LivetileWeatherFontSizePicker.SelectionChanged += OnSelectionChangeWeatherFontSizePicker;

            List<PickerItem> batteryFullPicker = new List<PickerItem>();
            LivetileBatteryFullPicker.ItemsSource = batteryFullPicker;
            batteryFullPicker.Add(new PickerItem() { Key = 99, Name = AppResources.BatteryFullWPDefault });
            batteryFullPicker.Add(new PickerItem() { Key = 100, Name = AppResources.BatteryFull });
            LivetileBatteryFullPicker.SelectedItem = batteryFullPicker.Find(x => x.Key.Equals((SettingHelper.Get(Constants.LIVETILE_BATTERY_FULL_INDICATION) as PickerItem).Key));
            LivetileBatteryFullPicker.SelectionChanged += OnSelectionChangeBatteryFullPicker;

            List<PickerItem> intervalList = new List<PickerItem>();
            intervalList.Add(new PickerItem() { Key = 30, Name = 30 + AppResources.Minutes });
            intervalList.Add(new PickerItem() { Key = 60, Name = 1 + AppResources.Hour });
            intervalList.Add(new PickerItem() { Key = 120, Name = 2 + AppResources.Hours });
            intervalList.Add(new PickerItem() { Key = 180, Name = 3 + AppResources.Hours });
            intervalList.Add(new PickerItem() { Key = 360, Name = 6 + AppResources.Hours });
            intervalList.Add(new PickerItem() { Key = 720, Name = 12 + AppResources.Hours });
            intervalList.Add(new PickerItem() { Key = 1440, Name = 1 + AppResources.Day });

            int interval = scheduleSettings.LivetileUpdateInterval;
            for (int i = 0; i < intervalList.Count; i++)
            {
                if ((int)intervalList[i].Key == interval)
                {
                    interval = i;
                    break;
                }
            }

            UpdateIntervalLivetilePicker.ItemsSource = intervalList;
            UpdateIntervalLivetilePicker.SelectedIndex = interval;
            UpdateIntervalLivetilePicker.SelectionChanged += OnSelectionChangedUpdateInterval;

            //랜덤 색상을 사용중이면 이하 색상 피커 비활성화
            if (UseRandomLivetileColor.IsChecked == true)
            {
                LivetileWeatherColorPicker.IsEnabled = false;
                LivetileCalendarColorPicker.IsEnabled = false;
                LivetileBatteryColorPicker.IsEnabled = false;
            }

        }

        private void OnSelectionChangeWeatherFontSizePicker(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                SettingHelper.Set(Constants.LIVETILE_WEATHER_FONT_SIZE, e.AddedItems[0] as PickerItem, false);
            }
        }

        private void OnSelectionChangeBatteryFullPicker(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                SettingHelper.Set(Constants.LIVETILE_BATTERY_FULL_INDICATION, e.AddedItems[0] as PickerItem, false);
            }
        }

        private void LivetileFontWeightPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                PickerItem item = e.AddedItems[0] as PickerItem;
                if (item != null)
                {
                    SettingHelper.Set(Constants.LIVETILE_FONT_WEIGHT, item.Key, false);
                }
            }
        }
        #endregion

        #region Chameleon Settings
        private void InitializeChameleonSetting()
        {
            UseProtectiveColor.IsChecked = (bool)SettingHelper.Get(Constants.CHAMELEON_USE_PROTECTIVE_COLOR);
            UseProtectiveColor.Checked += OnCheckedUseItem;
            UseProtectiveColor.Unchecked += OnUncheckedUseItem;
            /*
            UseProtectiveImage.IsChecked = (bool)SettingHelper.Get(Constants.CHAMELEON_USE_PROTECTIVE_IMAGE);
            UseProtectiveImage.Checked += OnCheckedUseItem;
            UseProtectiveImage.Unchecked += OnUncheckedUseItem;
            */
            List<ColorItem> itemList = items.ToList<ColorItem>();
            itemList.RemoveAt(0);
            //itemList.RemoveAt(0);
            SkinColorPicker.ItemsSource = itemList;
            SkinColorPicker.SelectedItem = FindSelectedColorItem(items, Constants.CHAMELEON_SKIN_BACKGROUND_COLOR);
            SkinColorPicker.SelectionChanged += OnSelectionChangedColortPicker;

            //보호색이 사용중이면 비활성화 / 비사용중이면 활성화
            //UseProtectiveImage.IsEnabled = UseProtectiveColor.IsChecked == false;
            //보호색이 사용중이 아니고 / 보호 이미지도 사용중이 아닌 경우라면 스킨 색상 설정 가능
            SkinColorPicker.IsEnabled = UseProtectiveColor.IsChecked == false;// && UseProtectiveImage.IsChecked == false;
        }
        #endregion

        #region Weather Settings
        private void InitializeWeatherSetting()
        {
            DisplayUnit unit = (DisplayUnit)SettingHelper.Get(Constants.WEATHER_UNIT_TYPE);
            List<PickerItem> WeatherUnitSource = new List<PickerItem>();
            WeatherUnitSource.Add(new PickerItem() { Name = AppResources.WeatherUnitCelsius, Key = DisplayUnit.Celsius });
            WeatherUnitSource.Add(new PickerItem() { Name = AppResources.WeatherUnitFahrenheit, Key = DisplayUnit.Fahrenheit });
            WeatherUnitPicker.ItemsSource = WeatherUnitSource;
                        
            foreach (PickerItem item in WeatherUnitSource)
            {
                if ((DisplayUnit)item.Key == unit)
                {
                    WeatherUnitPicker.SelectedItem = item;
                    break;
                }
            }

            WeatherUnitPicker.SelectionChanged += OnWeatherUnitPickerSelectionChanged;

            WeatherIconType iconType = (WeatherIconType)SettingHelper.Get(Constants.WEATHER_ICON_TYPE);
            List<IconPickerItem> WeatherIconSource = new List<IconPickerItem>();
            WeatherIconSource.Add(new IconPickerItem() { WeatherIconType = WeatherIconType.Normal });
            WeatherIconSource.Add(new IconPickerItem() { WeatherIconType = WeatherIconType.Simple01});
            WeatherIconPackPicker.ItemsSource = WeatherIconSource;

            foreach (IconPickerItem item in WeatherIconSource)
            {
                if ((WeatherIconType)item.WeatherIconType == iconType)
                {
                    WeatherIconPackPicker.SelectedItem = item;
                    break;
                }
            }

            WeatherIconPackPicker.SelectionChanged += OnWeatherIconPackPickerSelectionChanged;

            bool isUseLocation = (bool)SettingHelper.Get(Constants.WEATHER_USE_LOCATION_SERVICES);
            WeatherLocation.IsChecked = isUseLocation;
            WeatherLocation.Checked += WeatherLocation_Checked;
            WeatherLocation.Unchecked += WeatherLocation_Unchecked;
        }

        void WeatherLocation_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set(Constants.WEATHER_USE_LOCATION_SERVICES, false, false);
        }

        void WeatherLocation_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Set(Constants.WEATHER_USE_LOCATION_SERVICES, true, false);
        }

        private void OnWeatherUnitPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                DisplayUnit unit = (DisplayUnit)(e.AddedItems[0] as PickerItem).Key;
                SettingHelper.Set(Constants.WEATHER_UNIT_TYPE, unit, false);
            }
        }

        private void OnWeatherIconPackPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                WeatherIconType iconType = (WeatherIconType)(e.AddedItems[0] as IconPickerItem).WeatherIconType;
                SettingHelper.Set(Constants.WEATHER_ICON_TYPE, iconType, false);
            }
        }
        #endregion

        #region Calendar Settings
        private void InitializeCalendarSetting()
        {
            DayOfWeek firstDay = (DayOfWeek)SettingHelper.Get(Constants.CALENDAR_FIRST_DAY_OF_WEEK);
            string[] dayNames = DateTimeFormatInfo.CurrentInfo.DayNames;
            List<PickerItem> CalendarFirstDaySource = new List<PickerItem>();

            for (int i = 0; i < dayNames.Length; i++)
            {
                string dayName = dayNames[i];
                CalendarFirstDaySource.Add(new PickerItem() { Name = dayName, Key = i });
            }
            CalendarFirstDayPicker.ItemsSource = CalendarFirstDaySource;
            CalendarFirstDayPicker.SelectedIndex = (int)firstDay;
            
            CalednarDisplayAppointment.IsChecked = (bool)SettingHelper.Get(Constants.CALENDAR_SHOW_APPOINTMENT);

            CalendarFirstDayPicker.SelectionChanged += OnCalendarFirstDayPickerSelectionChanged;
            CalednarDisplayAppointment.Checked += OnCheckedCalendarDisplayAppointment;
            CalednarDisplayAppointment.Unchecked += OnUncheckedCalendarDisplayAppointment;
        }

        private void OnCalendarFirstDayPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                DayOfWeek firstDay = (DayOfWeek)(e.AddedItems[0] as PickerItem).Key;
                SettingHelper.Set(Constants.CALENDAR_FIRST_DAY_OF_WEEK, firstDay, false);
            }
        }
                
        private void OnCheckedCalendarDisplayAppointment(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)CalednarDisplayAppointment.IsChecked;
            SettingHelper.Set(Constants.CALENDAR_SHOW_APPOINTMENT, isChecked, false);
        }

        private void OnUncheckedCalendarDisplayAppointment(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)CalednarDisplayAppointment.IsChecked;
            SettingHelper.Set(Constants.CALENDAR_SHOW_APPOINTMENT, isChecked, false);
        }
        #endregion

        private void OnSelectionChangedColortPicker(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                string key = Constants.LOCKSCREEN_BACKGROUND_COLOR;
                switch ((sender as ListPicker).Name)
                {
                    case "LivetileCalendarColorPicker":
                        key = Constants.LIVETILE_CALENDAR_BACKGROUND_COLOR;
                        break;
                    case "LivetileWeatherColorPicker":
                        key = Constants.LIVETILE_WEATHER_BACKGROUND_COLOR;
                        break;
                    case "LivetileBatteryColorPicker":
                        key = Constants.LIVETILE_BATTERY_BACKGROUND_COLOR;
                        break;
                    case "SkinColorPicker":
                        key = Constants.CHAMELEON_SKIN_BACKGROUND_COLOR;
                        break;
                }

                ColorItem colorItem = e.AddedItems[0] as ColorItem;
                SettingHelper.Set(key, colorItem, false);
            }
        }

        private void OnCheckedUseItem(object sender, RoutedEventArgs e)
        {
            string key = Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION;
            string name = (sender as ToggleSwitch).Name;

            if (name == UseProtectiveColor.Name)
            {
                key = Constants.CHAMELEON_USE_PROTECTIVE_COLOR;
                //UseProtectiveImage.IsEnabled = false;
                SkinColorPicker.IsEnabled = false;
            }
            //else if (name == UseProtectiveImage.Name)
            //{
            //    SkinColorPicker.IsEnabled = false;
            //    key = Constants.CHAMELEON_USE_PROTECTIVE_IMAGE;
            //}
            else if (name == UseRandomLivetileColor.Name)
            {
                key = Constants.LIVETILE_RANDOM_BACKGROUND_COLOR;
                LivetileWeatherColorPicker.IsEnabled = false;
                LivetileCalendarColorPicker.IsEnabled = false;
                LivetileBatteryColorPicker.IsEnabled = false;
            }

            bool isChecked = (bool)(sender as ToggleSwitch).IsChecked;
            SettingHelper.Set(key, isChecked, false);
        }

        private void OnUncheckedUseItem(object sender, RoutedEventArgs e)
        {
            string key = Constants.LOCKSCREEN_BACKGROUND_USE_SEPARATION;
            string name = (sender as ToggleSwitch).Name;

            if (name == UseProtectiveColor.Name)
            {
                key = Constants.CHAMELEON_USE_PROTECTIVE_COLOR;
                //UseProtectiveImage.IsEnabled = true;
                //if (UseProtectiveImage.IsChecked == false)
                //{
                SkinColorPicker.IsEnabled = true;
                //}
            }
            //else if (name == UseProtectiveImage.Name)
            //{
            //    key = Constants.CHAMELEON_USE_PROTECTIVE_IMAGE;
            //    SkinColorPicker.IsEnabled = true;
            //}
            else if (name == UseRandomLivetileColor.Name)
            {
                key = Constants.LIVETILE_RANDOM_BACKGROUND_COLOR;
                LivetileWeatherColorPicker.IsEnabled = true;
                LivetileCalendarColorPicker.IsEnabled = true;
                LivetileBatteryColorPicker.IsEnabled = true;
            }

            bool isChecked = (bool)(sender as ToggleSwitch).IsChecked;
            SettingHelper.Set(key, isChecked, false);
        }

        private ColorItem FindSelectedColorItem(List<ColorItem> items, string key)
        {
            ColorItem colorItem = SettingHelper.Get(key) as ColorItem;
            //이전과 표시하는 언어가 다를 수 있으므로 새로 적용된 언어를 다시 찾아오는 과정임.
            ColorItem newColorItem = items.Find(x => x.Color == colorItem.Color);
            if (newColorItem != null)
            {
                colorItem = newColorItem;
            }
            return colorItem;
        }


        void OnSelectionChangedUpdateInterval(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                string key = string.Empty;
                PickerItem pickerItem = e.AddedItems[0] as PickerItem;

                switch ((sender as FrameworkElement).Name.Replace("UpdateInterval", "").Replace("Picker", ""))
                {
                    case "Lockscreen":
                        key = Constants.LOCKSCREEN_UPDATE_INTERVAL;
                        scheduleSettings.LockscreenUpdateInterval = (int)pickerItem.Key;
                        break;
                    case "Livetile":
                        key = Constants.LIVETILE_UPDATE_INTERVAL;
                        scheduleSettings.LivetileUpdateInterval = (int)pickerItem.Key;
                        break;
                    default:
                        return;
                }

                MutexedIsoStorageFile.Write<ScheduleSettings>(scheduleSettings, "ScheduleSettings", Constants.MUTEX_DATA);
            }
        }

        private void Pivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
            switch (e.Item.Name)
            {
                case "PILockscreenSetting":
                    if (UpdateIntervalLockscreenPicker.ItemsSource == null)
                    {
                        InitializeLockscreenSetting();
                    }
                    break;
                case "PILivetileSetting":
                    if (UpdateIntervalLivetilePicker.ItemsSource == null)
                    {
                        InitializeLivetileSetting();
                    }
                    break;
                case "PIExtraSetting":
                    if (SkinColorPicker.ItemsSource == null)
                    {
                        InitializeChameleonSetting();
                        InitializeCalendarSetting();
                        InitializeWeatherSetting();
                    }
                    break;
            }
        }

        private void PIExtraSetting_Loaded(object sender, RoutedEventArgs e)
        {
            string piName = NavigationContext.QueryString["piName"];
            Point point;
            if (piName == "PIWeather")
            {
                point = SPExSettingWeather.TransformToVisual(SVExtraSetting).Transform(new Point());
                SVExtraSetting.ScrollToVerticalOffset(point.Y);
            }
            else if (piName == "PICalendar")
            {
                point = SPExSettingCalendar.TransformToVisual(SVExtraSetting).Transform(new Point());
                SVExtraSetting.ScrollToVerticalOffset(point.Y);
            }
        }
    }
}