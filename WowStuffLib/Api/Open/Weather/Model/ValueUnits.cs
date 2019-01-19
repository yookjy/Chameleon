using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class ValueUnits
    {
        public ValueUnits Parse(XElement element)
        {
            Value = element.Value;
            if (element.Attribute("units") != null)
            {
                Units = element.Attribute("units").Value;
            }
            return this;
        }

        public ValueUnits() { }

        public string Value { get; set; }

        public string Units { get; set; }
    }
}
