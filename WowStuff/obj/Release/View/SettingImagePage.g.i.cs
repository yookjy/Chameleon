﻿#pragma checksum "D:\Project\C#\Chameleon\WowStuff\View\SettingImagePage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "A5C6A0AB72C65C21DF37EE390B0301D7"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.34011
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Chameleon.View {
    
    
    public partial class SettingImagePage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.DataTemplate PickerItemTemplate;
        
        internal System.Windows.DataTemplate PickerFullModeItemTemplate;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Microsoft.Phone.Controls.ListPicker LanguageMarketPicker;
        
        internal Microsoft.Phone.Controls.ListPicker BingSearchAspectPicker;
        
        internal Microsoft.Phone.Controls.ListPicker BingSearchOptionsPicker;
        
        internal Microsoft.Phone.Controls.ListPicker BingSearchSizePicker;
        
        internal System.Windows.Controls.Grid BingSearchCustomSize;
        
        internal System.Windows.Controls.TextBox BingSearchSizeWidth;
        
        internal System.Windows.Controls.TextBox BingSearchSizeHeight;
        
        internal Microsoft.Phone.Controls.ListPicker BingSearchColorPicker;
        
        internal Microsoft.Phone.Controls.ListPicker BingSearchStylePicker;
        
        internal Microsoft.Phone.Controls.ListPicker BingSearchFacePicker;
        
        internal Microsoft.Phone.Controls.ListPicker BingSearchQueryCountPicker;
        
        internal Microsoft.Phone.Controls.ListPicker BingSearchAdultPicker;
        
        internal System.Windows.Controls.TextBlock DomainFilter;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/Chameleon;component/View/SettingImagePage.xaml", System.UriKind.Relative));
            this.PickerItemTemplate = ((System.Windows.DataTemplate)(this.FindName("PickerItemTemplate")));
            this.PickerFullModeItemTemplate = ((System.Windows.DataTemplate)(this.FindName("PickerFullModeItemTemplate")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.LanguageMarketPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("LanguageMarketPicker")));
            this.BingSearchAspectPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("BingSearchAspectPicker")));
            this.BingSearchOptionsPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("BingSearchOptionsPicker")));
            this.BingSearchSizePicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("BingSearchSizePicker")));
            this.BingSearchCustomSize = ((System.Windows.Controls.Grid)(this.FindName("BingSearchCustomSize")));
            this.BingSearchSizeWidth = ((System.Windows.Controls.TextBox)(this.FindName("BingSearchSizeWidth")));
            this.BingSearchSizeHeight = ((System.Windows.Controls.TextBox)(this.FindName("BingSearchSizeHeight")));
            this.BingSearchColorPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("BingSearchColorPicker")));
            this.BingSearchStylePicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("BingSearchStylePicker")));
            this.BingSearchFacePicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("BingSearchFacePicker")));
            this.BingSearchQueryCountPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("BingSearchQueryCountPicker")));
            this.BingSearchAdultPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("BingSearchAdultPicker")));
            this.DomainFilter = ((System.Windows.Controls.TextBlock)(this.FindName("DomainFilter")));
        }
    }
}
