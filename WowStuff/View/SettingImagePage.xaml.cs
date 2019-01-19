using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using ChameleonLib.Api.Open.Bing;
using ChameleonLib.Helper;
using ChameleonLib.Resources;
using ChameleonLib.Model;

namespace Chameleon.View
{
    public partial class SettingImagePage : PhoneApplicationPage
    {
        public SettingImagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SettingHelper.Save();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                InitializeBingSetting();
            }
        }

        #region Bing Settings
        private void InitializeBingSetting()
        {
            SetPickerSource(LanguageMarketPicker, BingMarkets.Markets, SettingHelper.GetString(Constants.BING_LANGUAGE_MARKET));

            List<PickerItem> aspect = new List<PickerItem>();
            aspect.Add(new PickerItem() { Key = "Tall", Name = AppResources.Tall });
            aspect.Add(new PickerItem() { Key = "Wide", Name = AppResources.Wide });
            SetPickerSource(BingSearchAspectPicker, aspect, SettingHelper.GetString(Constants.BING_SEARCH_ASPECT));

            List<PickerItem> options = new List<PickerItem>();
            options.Add(new PickerItem() { Key = "None", Name = AppResources.None });
            options.Add(new PickerItem() { Key = "DisableLocationDetection", Name = AppResources.DisableLocationDetection });
            options.Add(new PickerItem() { Key = "EnableHighlighting", Name = AppResources.EnableHighlighting });
            SetPickerSource(BingSearchOptionsPicker, options, SettingHelper.GetString(Constants.BING_SEARCH_OPTIONS));

            List<PickerItem> size = new List<PickerItem>();
            size.Add(new PickerItem() { Key = "Large", Name = AppResources.Large });
            size.Add(new PickerItem() { Key = "Medium", Name = AppResources.Medium });
            size.Add(new PickerItem() { Key = "Small", Name = AppResources.Small });
            size.Add(new PickerItem() { Key = "Custom", Name = AppResources.Custom });
            string value = SettingHelper.GetString(Constants.BING_SEARCH_SIZE);
            string width = SettingHelper.GetString(Constants.BING_SEARCH_SIZE_WIDTH);
            string height = SettingHelper.GetString(Constants.BING_SEARCH_SIZE_HEIGHT);

            if (string.IsNullOrEmpty(width))
            {
                width = (int)ResolutionHelper.CurrentResolution.Width + "";
                SettingHelper.Set(Constants.BING_SEARCH_SIZE_WIDTH, width, false);
            }

            if (string.IsNullOrEmpty(height))
            {
                height = (int)ResolutionHelper.CurrentResolution.Height + "";
                SettingHelper.Set(Constants.BING_SEARCH_SIZE_HEIGHT, height, false);
            }

            BingSearchSizeWidth.Text = width;
            BingSearchSizeHeight.Text = height;

            SetPickerSource(BingSearchSizePicker, size, value);
            if (value == "Custom")
            {
                BingSearchCustomSize.Visibility = System.Windows.Visibility.Visible;
            }

            List<PickerItem> color = new List<PickerItem>();
            color.Add(new PickerItem() { Key = "Color", Name = AppResources.Color });
            color.Add(new PickerItem() { Key = "Monochrome", Name = AppResources.Monochrome });
            SetPickerSource(BingSearchColorPicker, color, SettingHelper.GetString(Constants.BING_SEARCH_COLOR));

            List<PickerItem> style = new List<PickerItem>();
            style.Add(new PickerItem() { Key = "Photo", Name = AppResources.Photo });
            style.Add(new PickerItem() { Key = "Graphics", Name = AppResources.Graphics });
            SetPickerSource(BingSearchStylePicker, style, SettingHelper.GetString(Constants.BING_SEARCH_STYLE));

            List<PickerItem> face = new List<PickerItem>();
            face.Add(new PickerItem() { Key = "Face", Name = AppResources.Face });
            face.Add(new PickerItem() { Key = "Portrait", Name = AppResources.Portrait });
            face.Add(new PickerItem() { Key = "Other", Name = AppResources.Other });
            SetPickerSource(BingSearchFacePicker, face, SettingHelper.GetString(Constants.BING_SEARCH_FACE));

            List<PickerItem> count = new List<PickerItem>();
            count.Add(new PickerItem() { Key = "20", Name = "20" });
            count.Add(new PickerItem() { Key = "40", Name = "40" });
            count.Add(new PickerItem() { Key = "60", Name = "60" });
            //count.Add(new PickerItem() { Key = "80", Name = "80" });
            //count.Add(new PickerItem() { Key = "100", Name = "100" });
            SetPickerSource(BingSearchQueryCountPicker, count, SettingHelper.GetString(Constants.BING_SEARCH_COUNT));

            List<PickerItem> adult = new List<PickerItem>();
            adult.Add(new PickerItem() { Key = "Strict", Name = AppResources.Strict });
            adult.Add(new PickerItem() { Key = "Moderate", Name = AppResources.Moderate });
            adult.Add(new PickerItem() { Key = "Off", Name = AppResources.Off });
            SetPickerSource(BingSearchAdultPicker, adult, SettingHelper.GetString(Constants.BING_SEARCH_ADULT));

            LanguageMarketPicker.SelectionChanged += OnBingSearchPickerSelectionChanged;
            BingSearchOptionsPicker.SelectionChanged += OnBingSearchPickerSelectionChanged;
            BingSearchSizePicker.SelectionChanged += OnBingSearchPickerSelectionChanged;
            BingSearchAspectPicker.SelectionChanged += OnBingSearchPickerSelectionChanged;
            BingSearchColorPicker.SelectionChanged += OnBingSearchPickerSelectionChanged;
            BingSearchStylePicker.SelectionChanged += OnBingSearchPickerSelectionChanged;
            BingSearchFacePicker.SelectionChanged += OnBingSearchPickerSelectionChanged;
            BingSearchAdultPicker.SelectionChanged += OnBingSearchPickerSelectionChanged;
            BingSearchSizeWidth.TextChanged += BingSearchSizeWidth_TextChanged;
            BingSearchSizeHeight.TextChanged += BingSearchSizeHeight_TextChanged;
            BingSearchSizeWidth.MouseLeave += BingSearchSizeWidth_MouseLeave;
            BingSearchSizeHeight.MouseLeave += BingSearchSizeHeight_MouseLeave;

            DomainFilter.Text = string.Format("{0} ({1})", AppResources.DomainFilter, AppResources.BlackList);
        }

        void BingSearchSizeHeight_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(BingSearchSizeHeight.Text.Trim()) ||
                Int32.Parse(BingSearchSizeHeight.Text.Trim()) < Int32.Parse(Constants.BING_SEARCH_SIZE_MIN_HEIGHT))
            {
                BingSearchSizeHeight.Text = Constants.BING_SEARCH_SIZE_MIN_HEIGHT;
            }
        }

        void BingSearchSizeWidth_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(BingSearchSizeWidth.Text.Trim()) ||
                Int32.Parse(BingSearchSizeWidth.Text.Trim()) < Int32.Parse(Constants.BING_SEARCH_SIZE_MIN_WIDTH))
            {
                BingSearchSizeWidth.Text = Constants.BING_SEARCH_SIZE_MIN_WIDTH;
            }
        }

        void BingSearchSizeHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingHelper.Set(Constants.BING_SEARCH_SIZE_HEIGHT, BingSearchSizeHeight.Text, false);
        }

        void BingSearchSizeWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            SettingHelper.Set(Constants.BING_SEARCH_SIZE_WIDTH, BingSearchSizeWidth.Text, false);
        }

        private string GetSenderKey(object sender)
        {
            ListPicker picker = sender as ListPicker;
            string key = string.Empty;
            switch (picker.Name)
            {
                case "LanguageMarketPicker": key = Constants.BING_LANGUAGE_MARKET; break;
                case "BingSearchOptionsPicker": key = Constants.BING_SEARCH_OPTIONS; break;
                case "BingSearchAspectPicker": key = Constants.BING_SEARCH_ASPECT; break;
                case "BingSearchSizePicker": key = Constants.BING_SEARCH_SIZE; break;
                case "BingSearchColorPicker": key = Constants.BING_SEARCH_COLOR; break;
                case "BingSearchStylePicker": key = Constants.BING_SEARCH_STYLE; break;
                case "BingSearchFacePicker": key = Constants.BING_SEARCH_FACE; break;
                case "BingSearchAdultPicker": key = Constants.BING_SEARCH_ADULT; break;
            }
            return key;
        }

        private void OnBingSearchPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string key = GetSenderKey(sender);
            if (!string.IsNullOrEmpty(key) && e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                string value = (string)(e.AddedItems[0] as PickerItem).Key;
                SettingHelper.Set(key, value, false);

                if (key == Constants.BING_SEARCH_SIZE && value == "Custom"
                    && BingSearchCustomSize.Visibility == System.Windows.Visibility.Collapsed)
                {
                    BingSearchCustomSize.Visibility = System.Windows.Visibility.Visible;
                }
                else if ((key != Constants.BING_SEARCH_SIZE || (key == Constants.BING_SEARCH_SIZE && value != "Custom"))
                    && BingSearchCustomSize.Visibility == System.Windows.Visibility.Visible)
                {
                    BingSearchCustomSize.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void SetPickerSource(ListPicker picker, List<PickerItem> itemList, string selectKey)
        {
            PickerItem selectItem = null;
            foreach (PickerItem item in itemList)
            {
                if ((string)item.Key == selectKey)
                {
                    selectItem = item;
                    break;
                }
            }
            picker.ItemsSource = itemList;
            picker.SelectedItem = selectItem;
        }

        private void OnTapDomainFilter(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/DomainFilterPage.xaml", UriKind.Relative));
        }

        #endregion
    }
}