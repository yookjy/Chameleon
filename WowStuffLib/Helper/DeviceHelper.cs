using ChameleonLib.Resources;
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChameleonLib.Helper
{
    public static class DeviceHelper
    {
        public static string ManufacturerLowerCase
        {
            get
            {
                return Manufacturer.ToLower();
            }
        }

        public static string Manufacturer
        {
            get
            {
                //return "HTC";
                //return "Samsung";
                return DeviceStatus.DeviceManufacturer + string.Empty;
            }
        }

        public static int OsMajorVersion
        {
            get
            {
                return Environment.OSVersion.Version.Major;
            }
        }

        public static int OsMinorVersion
        {
            get
            {
                return Environment.OSVersion.Version.Minor;
            }
        }

        private static Version TargetedVersion = new Version(8, 0, 10492);
        public static bool IsAfterGddr3
        {
            get { return Environment.OSVersion.Version >= TargetedVersion; }
        }
    }
}
