using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class WeatherDateTime
    {
        public WeatherDateTime Parse(IEnumerable<XElement> elements, XNamespace nameSpace)
        {
            foreach (XElement elem in elements)
            {
                Year = elem.Element(nameSpace + "year").Attribute("number").Value;
                Month = new ValueInfo().Parse(elem.Element(nameSpace + "month"));
                Day = new ValueInfo().Parse(elem.Element(nameSpace + "day"));
                //Hour = elem.Element(nameSpace + "hour").Attribute("number").Value;
                Hour24 = elem.Element(nameSpace + "hour").Attribute("hour-24").Value;
                Minute = elem.Element(nameSpace + "minute").Attribute("number").Value;
                Second = elem.Element(nameSpace + "second").Attribute("number").Value;
                //AmPm = elem.Element(nameSpace + "am-pm").Attribute("abbrv").Value;
                //TimeZone = new ValueInfo().Parse(elem.Element(nameSpace + "time-zone"));
            }
            return this;
        }

        public string Year { get; set; }

        public ValueInfo Month { get; set; }

        public ValueInfo Day { get; set; }

        //public string Hour { get; set; }

        public string Hour24 { get; set; }

        public string Minute { get; set; }

        public string Second { get; set; }

        //public string AmPm { get; set; }

        //public ValueInfo TimeZone { get; set; }
    }
}
