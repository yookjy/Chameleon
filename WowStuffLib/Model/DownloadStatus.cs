using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Model
{
    public enum DownloadStatus
    {
        Pending,
        DownloadFailed,
        SaveFailed,
        Downloading,
        Downloaded,
        Saving,
        Completed,
    }
}
