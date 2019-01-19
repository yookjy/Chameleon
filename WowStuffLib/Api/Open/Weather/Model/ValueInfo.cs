using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public class ValueInfo
    {
        public ValueInfo Parse(XElement element)
        {
            if (element.Attribute("offset") != null)
            {
                Offset = element.Attribute("offset").Value;
            }
            if (element.Attribute("number") != null)
            {
                Number = element.Attribute("number").Value;
            }
            if (element.Attribute("text") != null)
            {
                Text = element.Attribute("text").Value;
            }
            if (element.Attribute("abbrv") != null)
            {
                Abbrv = element.Attribute("abbrv").Value;
            }
            return this;
        }

        public string Offset { get; set; }

        public string Number { get; set; }

        public string Text { get; set; }

        public string Abbrv { get; set; }
    }
}
