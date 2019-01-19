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
using ChameleonLib.Model;
using System.Collections;
using System.Collections.ObjectModel;

namespace Chameleon.View
{
    public partial class DomainFilterPage : PhoneApplicationPage
    {
        public DomainFilterPage()
        {
            InitializeComponent();

            AppTitle.Text = string.Format("{0} - {1}", AppResources.ApplicationTitle, AppResources.DomainFilter);

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.9;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = false;

            ApplicationBarIconButton selectAppBarIconBtn = new ApplicationBarIconButton();
            selectAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.list.check.png");
            selectAppBarIconBtn.Text = AppResources.AppbarMenuSelect;
            selectAppBarIconBtn.IsEnabled = false;
            selectAppBarIconBtn.Click += selectAppBarIconBtn_Click;

            ApplicationBarIconButton delAppBarIconBtn = new ApplicationBarIconButton();
            delAppBarIconBtn.IconUri = PathHelper.GetPath("appbar.delete.png");
            delAppBarIconBtn.Text = AppResources.AppbarMenuRemove;
            delAppBarIconBtn.Click += delAppBarIconBtn_Click;
            delAppBarIconBtn.IsEnabled = false;

            ApplicationBar.Buttons.Add(selectAppBarIconBtn);
            ApplicationBar.Buttons.Add(delAppBarIconBtn);

            //블랙리스트 로드
            LoadBlackList();
        }

        private void selectAppBarIconBtn_Click(object sender, EventArgs e)
        {
            BlackListSelector.EnforceIsSelectionEnabled = true;
        }

        private void LoadBlackList()
        {
            BlackList blackList = new BlackList();
            BlackListSelector.ItemsSource = blackList.Items;
        }

        private void delAppBarIconBtn_Click(object sender, EventArgs e)
        {
            IList selectedBlackList = BlackListSelector.SelectedItems as IList;
            ObservableCollection<BlackDomain> blackList = BlackListSelector.ItemsSource as ObservableCollection<BlackDomain>;

            BlackDomain domain = null;
            while (selectedBlackList.Count > 0)
            {
                domain = selectedBlackList[0] as BlackDomain;
                blackList.Remove(domain);
            }

            if (blackList.Count == 0)
            {
                foreach(ApplicationBarIconButton button in ApplicationBar.Buttons)
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void OnIsSelectionEnabledChangedBlackListSelector(object sender, DependencyPropertyChangedEventArgs e)
        {
            ApplicationBarIconButton button = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            button.IsEnabled = !(bool)e.NewValue;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (BlackListSelector.ItemsSource.Count > 0)
            {
                ApplicationBarIconButton button = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                button.IsEnabled = true;
            }
        }

        private void OnSelectionChangedBlackListSelector(object sender, SelectionChangedEventArgs e)
        {
            ApplicationBarIconButton button = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            button.IsEnabled = BlackListSelector.SelectedItems.Count > 0;
        }
    }
}