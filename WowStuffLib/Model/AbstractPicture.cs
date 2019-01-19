using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Model
{
    public class AbstractPicture : INotifyPropertyChanged
    {
        public Guid Guid { get; set; }

        public SourceOrigin SourceOrigin { get; set; }

        public string AlbumName { get; set; }

        public string Name { get; set; }

        public string ThumnailName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string progressStatus;

        private double progressRate;

        public string ProgressStatus
        {
            get
            {
                return progressStatus;
            }
            set
            {
                if (progressStatus != value)
                {
                    progressStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double ProgressRate
        {
            get
            {
                return progressRate;
            }
            set
            {
                if (progressRate != value)
                {
                    progressRate = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
