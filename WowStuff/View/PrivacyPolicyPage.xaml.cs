using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ChameleonLib.View
{
    public partial class PrivacyPolicyPage : PhoneApplicationPage
    {
        public PrivacyPolicyPage()
        {
            InitializeComponent();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (PrivacyPolicy.CanGoBack)
            {
                PrivacyPolicy.GoBack();
                e.Cancel = true;
            }
        }
    }
}