using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChameleonLib.Model
{
    public enum BlackListMode
    {
        None,
        Domain
    }

    public class BlackDomain
    {
        public string SearchKeyword { get; set; }

        public BlackListMode BlackListMode { get; set; }

        public string Host { get; set; }

        public DateTime AddedDateTime { get; set; }
    }
}
