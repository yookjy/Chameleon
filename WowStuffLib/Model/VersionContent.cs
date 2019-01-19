using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ChameleonLib.Model
{
    public enum VersionContentType
    {
        NEW,
        FIX,
        MOD,
        IMP,
        DEL
    }

    public class VersionContent
    {
        public VersionContent(VersionContentType type, string content)
        {
            Color color = Colors.Transparent;
            switch (type)
            {
                case VersionContentType.NEW:
                    color = ColorItem.GetColorByName("Cyan");
                    break;
                case VersionContentType.FIX:
                    color = ColorItem.GetColorByName("Green");
                    break;
                case VersionContentType.MOD:
                    color = ColorItem.GetColorByName("Teal");
                    break;
                case VersionContentType.IMP:
                    color = ColorItem.GetColorByName("Amber");
                    break;
                case VersionContentType.DEL:
                    color = ColorItem.GetColorByName("Red");
                    break;
            }

            Type = type;
            Content = content.Replace("\\n", "\n");
            Background = new SolidColorBrush(color);
            
        }

        public SolidColorBrush Background { get; set; }

        public VersionContentType Type { get; set; }

        public string Content { get; set; }
    }
}
