using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChameleonLib.Helper
{
    public static class PathHelper
    {
        public static string GetHighlightFullPath(string path)
        {
            string fullPath = "/Images/{0}/{1}";
            fullPath = string.Format(fullPath, (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible ? "light" : "dark", path);
            return fullPath;
        }

        public static string GetFullPath(string path)
        {
            string fullPath = "/Images/{0}/{1}";
            fullPath = string.Format(fullPath, (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible ? "dark" : "light", path);
            return fullPath;
        }

        public static Uri GetPath(string path)
        {
            string fullPath = "/Images/{0}/{1}";
            fullPath = string.Format(fullPath, (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible ? "dark" : "light", path);
            return new Uri(fullPath, UriKind.Relative);
        }
        
        public static Uri GetThemeImagePath(string path, Boolean isThemeReverse)
        {
            string dark = "dark";
            string light = "light";
            bool isDarkTemem = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible;

            string fullPath = "/Images/{0}/{1}";
            fullPath = string.Format(fullPath, isDarkTemem ? (isThemeReverse ? light : dark) : (isThemeReverse ? dark : light), path);
            return new Uri(fullPath, UriKind.Relative);
        }

    }
}
