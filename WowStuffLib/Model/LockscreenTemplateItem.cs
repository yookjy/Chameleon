using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChameleonLib.Resources;

namespace ChameleonLib.Model
{
    public enum LiveItems
    {
        Calendar,
        Weather,
        Battery,
        NoForecast
    }

    public class LockscreenItemInfo
    {
        public LiveItems LockscreenItem { get; set; }

        public byte Column { get; set; }

        public byte Row { get; set; }

        public byte ColSpan { get; set; }

        public byte RowSpan { get; set; }
    }
    
    public class LockscreenTemplateItem
    {
        public LockscreenItemInfo[] LockscreenItemInfos { get; set; }

        public string Text 
        {
            get
            {
                if (LockscreenItemInfos == null || LockscreenItemInfos.Length == 0) return AppResources.None;

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < LockscreenItemInfos.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(AppResources.ResourceManager.GetString(LockscreenItemInfos[i].LockscreenItem.ToString()));
                }
                return builder.ToString();
            }
        }
        public byte Id { get; set; }

    }
}
