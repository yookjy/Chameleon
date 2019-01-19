using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ChameleonLib.Resources;
using ChameleonLib.Helper;
using Microsoft.Phone.Tasks;
using Chameleon;
using ChameleonLib.Model;

namespace ChameleonLib.View
{
    public partial class AppListPage : PhoneApplicationPage
    {
        private AppListModel _AppListModel;

        public AppListPage()
        {
            _AppListModel = new AppListModel();

            InitializeComponent();
            //제조사앱 타이틀 설정
            PIMfApps.Header = DeviceHelper.Manufacturer;

            if (!_AppListModel.IsMfDataLoaded)
            {
                _AppListModel.LoadMfData();
                ManufacturerApps.ItemsSource = _AppListModel.MfItems;
            }

            //데이터가 없으면 안내문구 설정
            if (_AppListModel.MfItems.Count == 0)
            {
                PVApps.Items.Remove(PIMfApps);
            }
        }

        private void ManufacturerApps_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if ((sender as LongListSelector).SelectedItem != null)
            {
                MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
                marketplaceDetailTask.ContentIdentifier = ((sender as LongListSelector).SelectedItem as ChameleonLib.Model.App).AppId;
                marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
                marketplaceDetailTask.Show();
            }
        }

        private void Pivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item == PIMsApps)
            {
                if (!_AppListModel.IsMsDataLoaded)
                {
                    _AppListModel.LoadMsData();
                    MicrosoftApps.ItemsSource = _AppListModel.MsItems;
                }
            }
            else if (e.Item == PIVsApps)
            {
                if (!_AppListModel.IsVsDataLoaded)
                {
                    _AppListModel.LoadVsData();
                    VelostepApps.ItemsSource = _AppListModel.VsItems;
                }
            }
        }
    }
}