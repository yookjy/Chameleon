using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class WeatherUnit
    {
        public WeatherUnit Parse(IEnumerable<XElement> elements, XNamespace nameSpace, string elemName)
        {
            Value = new ValueUnits().Parse(elements.Descendants(nameSpace + elemName).Single());
            if (elements.Descendants(nameSpace + (elemName + "-high")).Count() > 0)
            {
                High = new ValueUnits().Parse(elements.Descendants(nameSpace + (elemName + "-high")).Single());
            }
            if (elements.Descendants(nameSpace + (elemName + "-low")).Count() > 0)
            {
                Low = new ValueUnits().Parse(elements.Descendants(nameSpace + (elemName + "-low")).Single());
            }
            if (elements.Descendants(nameSpace + (elemName + "-rate")).Count() > 0)
            {
                Rate = new ValueUnits().Parse(elements.Descendants(nameSpace + (elemName + "-rate")).Single());
            }
            return this;
        }

        public ValueUnits Value;

        public ValueUnits High;

        public ValueUnits Low;

        public ValueUnits Rate;
    }
}
