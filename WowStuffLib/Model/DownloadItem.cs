using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChameleonLib.Model
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private double _DownloadRate;

        private string _DownloadStatus;

        private string _DownloadNetwork;

        private Visibility _Visibility;

        public Guid Guid { get; set; }

        public string AlumName { get; set; }

        public string FileName { get; set; }

        public bool IsActiveDomainFilter { get; set; }

        public BlackListMode BlackListMode { get; set; }

        public SourceOrigin SourceOrigin { get; set; }

        public Visibility Visibility
        {
            get
            {
                return _Visibility;
            }
            set
            {
                if (_Visibility != value)
                {
                    _Visibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public object ThumbnailPath { get; set; }

        public object DownloadPath { get; set; }

        public double DownloadRate
        {
            get
            {
                return _DownloadRate;
            }
            set
            {
                if (_DownloadRate != value)
                {
                    _DownloadRate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DownloadStatus DownloadStatusCode { get; set; }

        public string DownloadStatus
        {
            get
            {
                return _DownloadStatus;
            }
            set
            {
                if (_DownloadStatus != value)
                {
                    _DownloadStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string DownloadNetwork
        {
            get
            {
                return _DownloadNetwork;
            }
            set
            {
                if (_DownloadNetwork != value)
                {
                    _DownloadNetwork = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
