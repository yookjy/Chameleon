using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Windows.Interactivity;
using System.ComponentModel;

namespace WowLib.UI
{
    public partial class ColorPickerPage : PhoneApplicationPage
    {
        static string[] colorNames = 
        { 
            "Yellow","BananaYellow","LaserLemon","Jasmine","Green","Emerald", 
            "GreenYellow","Lime","Chartreuse","LimeGreen","SpringGreen","LightGreen", 
            "MediumSeaGreen","MediumSpringGreen","Olive","SeaGreen","Red","OrangeRed", 
            "DarkOrange","Orange","ImperialRed","Maroon","Brown","Chocolate", 
            "Coral","Crimson","DarkSalmon","DeepPink","Firebrick","HotPink", 
            "IndianRed","LightCoral","LightPink","LightSalmon","Magenta","MediumVioletRed", 
            "Orchid","PaleVioletRed","Salmon","SandyBrown","Navy","Indigo", 
            "MidnightBlue","Blue","Purple","BlueViolet","CornflowerBlue","Cyan", 
            "DarkCyan","DarkSlateBlue","DeepSkyBlue","DodgerBlue","LightBlue","LightSeaGreen", 
            "LightSkyBlue","LightSteelBlue","Mauve","MediumSlateBlue","RoyalBlue","SlateBlue", 
            "SlateGray","SteelBlue","Teal","Turquoise","DarkGrey","LightGray" 
        };

        static uint[] uintColors = 
        {  
            0xFFFFFF00,0xFFFFE135,0xFFFFFF66,0xFFF8DE7E,0xFF008000,0xFF008A00, 
            0xFFADFF2F,0xFF00FF00,0xFF7FFF00,0xFF32CD32,0xFF00FF7F,0xFF90EE90, 
            0xFF3CB371,0xFF00FA9A,0xFF808000,0xFF2E8B57,0xFFFF0000,0xFFFF4500, 
            0xFFFF8C00,0xFFFFA500,0xFFED2939,0xFF800000,0xFFA52A2A,0xFFD2691E, 
            0xFFFF7F50,0xFFDC143C,0xFFE9967A,0xFFFF1493,0xFFB22222,0xFFFF69B4, 
            0xFFCD5C5C,0xFFF08080,0xFFFFB6C1,0xFFFFA07A,0xFFFF00FF,0xFFC71585, 
            0xFFDA70D6,0xFFDB7093,0xFFFA8072,0xFFF4A460,0xFF000080,0xFF4B0082, 
            0xFF191970,0xFF0000FF,0xFF800080,0xFF8A2BE2,0xFF6495ED,0xFF00FFFF, 
            0xFF008B8B,0xFF483D8B,0xFF00BFFF,0xFF1E90FF,0xFFADD8E6,0xFF20B2AA, 
            0xFF87CEFA,0xFFB0C4DE,0xFF76608A,0xFF7B68EE,0xFF4169E1,0xFF6A5ACD, 
            0xFF708090,0xFF4682B4,0xFF008080,0xFF40E0D0,0xFFA9A9A9,0xFFD3D3D3 
        };

        private Color ConvertColor(uint uintCol)
        {
            byte A = (byte)((uintCol & 0xFF000000) >> 24);
            byte R = (byte)((uintCol & 0x00FF0000) >> 16);
            byte G = (byte)((uintCol & 0x0000FF00) >> 8);
            byte B = (byte)((uintCol & 0x000000FF) >> 0);
            return Color.FromArgb(A, R, G, B); ;
        }

        public string Text { get; set; }

        public Color Color { get; set; }


        public ColorPickerPage()
        {
            InitializeComponent();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SolidColorBrush brush = Application.Current.Resources["PhoneChromeBrush"] as SolidColorBrush;
            SystemTray.BackgroundColor = brush.Color;
            //헤더 설정
            ApplicationName.Text = NavigationContext.QueryString["header"];
            //컬러 추가
            //ObservableCollection<ColorItem> item = new ObservableCollection<ColorItem>();
            List<ColorItem> item = new List<ColorItem>();
            for (int i = 0; i < 10; i++)
            {
                item.Add(new ColorItem() { Text = colorNames[i], Color = ConvertColor(uintColors[i]) });
            };
            listBox.ItemsSource = item; //Fill ItemSource with all colors
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            listBox.SelectionMode = SelectionMode.Multiple;
            listBox.SelectAll();
        }

        private void lstColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1) 
            {
                //(Application.Current as App).CurrentColorItem = ((ColorItem)e.AddedItems[0]); 
                this.NavigationService.GoBack(); 
            }
            else if (e.AddedItems.Count > 1)
            {
                for (int i = 0; i < e.AddedItems.Count; i++)
                {
                    Storyboard sb = new Storyboard();
                    DoubleAnimationUsingKeyFrames timeline = new DoubleAnimationUsingKeyFrames();
                    timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0)), Value = 0 });
                    timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100)), Value = 45 });
                    sb.Children.Add(timeline);

                    ListBoxItem item = (sender as ListBox).ItemContainerGenerator.ContainerFromItem(e.AddedItems[i]) as ListBoxItem;
                    Rectangle rect = FindFirstElementInVisualTree<Rectangle>(item);

                    Storyboard.SetTarget(timeline, rect.Projection);
                    Storyboard.SetTargetProperty(timeline, new PropertyPath("RotationX"));

                    sb.Begin();
                }
            }
        }

        private T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var result = FindFirstElementInVisualTree<T>(child);
                    if (result != null)
                        return result;

                }
            }
            return null;
        }

        private void item_Loaded(object sender, RoutedEventArgs e)
        {
            //Storyboard sb = new Storyboard();
            //DoubleAnimationUsingKeyFrames timeline = new DoubleAnimationUsingKeyFrames();
            //timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0)), Value = -45 });
            //timeline.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)), Value = 0 });
            //sb.Children.Add(timeline);
            //Storyboard.SetTarget(timeline, ((sender as StackPanel).Children[0] as Rectangle).Projection);
            //Storyboard.SetTargetProperty(timeline, new PropertyPath("RotationX"));
            //sb.Begin();
        }
    }
 


}