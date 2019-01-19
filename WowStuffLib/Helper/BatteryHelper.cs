using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Devices.Power;

namespace ChameleonLib.Helper
{
    public class BatteryHelper
    {
        public static int BateryLevel
        {
            get
            {
                return Battery.GetDefault().RemainingChargePercent;
            }
        }

        public static bool IsCharging
        {
            get
            {
                return DeviceStatus.PowerSource == PowerSource.External;
            }
        }

        public static TimeSpan BatteryTime
        {
            get 
            {
                return Battery.GetDefault().RemainingDischargeTime;
            }
        }
    }
}
