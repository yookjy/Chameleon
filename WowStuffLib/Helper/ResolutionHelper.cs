using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChameleonLib.Helper
{
    public static class ResolutionHelper
    {
        public static Size Resolution
        {
            get;
            private set;
        }

        public static double ScaleFactor
        {
            get
            {
                return (double)Application.Current.Host.Content.ScaleFactor / 100;
            }
        }

        private static bool IsWvga
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 100;
            }
        }

        private static bool IsWxga
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 160;
            }
        }

        private static bool IsHD
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 150;
            }
        }

        public static Size CurrentResolution
        {
            get
            {
                if (Resolution.Width == 0 || Resolution.Height == 0)
                {
                    if (IsWvga)
                    {
                        Resolution = new Size(480, 800);
                    }
                    else if (IsHD)
                    {
                        Resolution = new Size(720, 1280);
                    }
                    else
                    {
                        Resolution = new Size(768, 1280);
                    }
                }
                return Resolution;
            }
        }
    }
}
