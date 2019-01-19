using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace ChameleonLib.Api.Open.Weather.Model
{
    public enum WeatherIconType
    {
        Simple01,
        Normal
    }

    public class WeatherIconMap : Dictionary<string, string>
    {
        private static WeatherIconMap _Instance;

        public WeatherIconType WeatherIconType { get; set; }
        
        private WeatherIconMap()
        {
        }

        public static WeatherIconMap Instance
        {
            get
            {
                lock (typeof(WeatherIconMap))
                {
                    if (_Instance == null)
                    {
                        _Instance = new WeatherIconMap();
                        //날씨 기본 아이콘 로딩...
                        if (!SettingHelper.ContainsKey(Constants.WEATHER_ICON_TYPE))
                        {
                            SettingHelper.Set(Constants.WEATHER_ICON_TYPE, WeatherIconType.Simple01, true);
                        }
                        WeatherIconMap.Instance.Load((WeatherIconType)SettingHelper.Get(Constants.WEATHER_ICON_TYPE));
                    }
                    return _Instance;
                }
            }
        }

        public void Load(WeatherIconType type)
        {
            if (Instance.Count > 0)
            {
                Instance.Clear();
            }

            WeatherIconType = type;

            switch (type)
            {
                case WeatherIconType.Normal:
                    WeatherIconNormalMap();
                    break;
                case WeatherIconType.Simple01:
                    WeatherIconSimple01Map();
                    break;
            }
        }

        private void WeatherIconNormalMap()
        {    
            this.Add("cond023", "cond023");
            this.Add("cond024", "cond024");
            this.Add("cond033", "cond033");
            this.Add("cond034", "cond034");
            this.Add("cond052", "cond052");
            this.Add("cond053", "cond053");
            this.Add("cond062", "cond062");
            this.Add("cond064", "cond064");
            this.Add("cond065", "cond065");
            this.Add("cond066", "cond066");
            this.Add("cond067", "cond067");
            this.Add("cond068", "cond068");
            this.Add("cond069", "cond069");
            this.Add("cond071", "cond071");
            this.Add("cond072", "cond072");
            this.Add("cond073", "cond073");
            this.Add("cond074", "cond074");
            this.Add("cond075", "cond075");
            this.Add("cond076", "cond076");
            this.Add("cond077", "cond077");

            this.Add("cond000", "cond000");
            this.Add("cond007", "cond000");
            this.Add("cond001", "cond001");
            this.Add("cond013", "cond001");
            this.Add("cond002", "cond002");
            this.Add("cond016", "cond002");
            this.Add("cond003", "cond003");
            this.Add("cond026", "cond003");
            this.Add("cond004", "cond004");

            this.Add("cond005", "cond005");
            this.Add("cond020", "cond005");
            this.Add("cond046", "cond005");
            this.Add("cond048", "cond005");
            this.Add("cond087", "cond005");
            this.Add("cond088", "cond005");
            this.Add("cond089", "cond005");
            this.Add("cond120", "cond005");
            this.Add("cond121", "cond005");
            this.Add("cond122", "cond005");
            this.Add("cond141", "cond005");
            this.Add("cond152", "cond005");
            this.Add("cond014", "cond005");
            this.Add("cond015", "cond005");
            this.Add("cond081", "cond005");
            this.Add("cond082", "cond005");
            this.Add("cond083", "cond005");
            this.Add("cond139", "cond005");

            this.Add("cond006", "cond006");
            this.Add("cond022", "cond006");
            this.Add("cond093", "cond006");
            this.Add("cond094", "cond006");
            this.Add("cond095", "cond006");
            this.Add("cond143", "cond006");
            this.Add("cond018", "cond006");
            this.Add("cond030", "cond006");
            this.Add("cond105", "cond006");
            this.Add("cond106", "cond006");
            this.Add("cond107", "cond006");
            this.Add("cond147", "cond006");
            
            this.Add("cond008", "cond008");
            this.Add("cond012", "cond008");
            this.Add("cond011", "cond008");
            this.Add("cond029", "cond008");
            this.Add("cond078", "cond008");
            this.Add("cond079", "cond008");
            this.Add("cond080", "cond008");
            this.Add("cond102", "cond008");
            this.Add("cond103", "cond008");
            this.Add("cond104", "cond008");
            this.Add("cond138", "cond008");
            this.Add("cond146", "cond008");

            this.Add("cond009", "cond009");
            this.Add("cond019", "cond009");
            this.Add("cond084", "cond009");
            this.Add("cond085", "cond009");
            this.Add("cond086", "cond009");
            this.Add("cond140", "cond009");

            this.Add("cond010", "cond010");
            this.Add("cond037", "cond010");
            this.Add("cond999", "cond010");

            this.Add("cond017", "cond017");
            this.Add("cond031", "cond017");

            this.Add("cond025", "cond025");
            this.Add("cond036", "cond025");
            this.Add("cond021", "cond025");
            this.Add("cond028", "cond025");
            this.Add("cond090", "cond025");
            this.Add("cond091", "cond025");
            this.Add("cond092", "cond025");
            this.Add("cond099", "cond025");
            this.Add("cond100", "cond025");
            this.Add("cond101", "cond025");
            this.Add("cond142", "cond025");
            this.Add("cond145", "cond025");

            this.Add("cond032", "cond032");
            this.Add("cond027", "cond032");
            this.Add("cond096", "cond032");
            this.Add("cond097", "cond032");
            this.Add("cond098", "cond032");
            this.Add("cond144", "cond032");

            this.Add("cond035", "cond035");
            this.Add("cond070", "cond035");

            this.Add("cond039", "cond040");
            this.Add("cond040", "cond040");
            this.Add("cond111", "cond040");
            this.Add("cond112", "cond040");
            this.Add("cond113", "cond040");
            this.Add("cond149", "cond040");

            this.Add("cond038", "cond041");
            this.Add("cond041", "cond041");
            this.Add("cond108", "cond041");
            this.Add("cond109", "cond041");
            this.Add("cond110", "cond041");
            this.Add("cond148", "cond041");

            this.Add("cond043", "cond054");
            this.Add("cond044", "cond054");
            this.Add("cond054", "cond054");
            this.Add("cond055", "cond054");
            this.Add("cond117", "cond054");
            this.Add("cond118", "cond054");
            this.Add("cond119", "cond054");
            this.Add("cond126", "cond054");
            this.Add("cond127", "cond054");
            this.Add("cond128", "cond054");
            this.Add("cond151", "cond054");
            this.Add("cond154", "cond054");
            this.Add("cond160", "cond054");
            this.Add("cond176", "cond054");

            this.Add("cond042", "cond045");
            this.Add("cond045", "cond045");
            this.Add("cond114", "cond045");
            this.Add("cond115", "cond045");
            this.Add("cond116", "cond045");
            this.Add("cond150", "cond045");

            this.Add("cond047", "cond047");
            this.Add("cond049", "cond047");
            this.Add("cond123", "cond047");
            this.Add("cond124", "cond047");
            this.Add("cond125", "cond047");
            this.Add("cond153", "cond047");

            this.Add("cond050", "cond050");
            this.Add("cond158", "cond050");

            this.Add("cond051", "cond051");
            this.Add("cond159", "cond051");

            this.Add("cond056", "cond056");
            this.Add("cond057", "cond056");
            this.Add("cond161", "cond056");
            this.Add("cond129", "cond056");
            this.Add("cond169", "cond056");
            this.Add("cond130", "cond056");
            this.Add("cond172", "cond056");
            this.Add("cond131", "cond056");
            this.Add("cond175", "cond056");
            this.Add("cond155", "cond056");
            this.Add("cond164", "cond056");

            this.Add("cond058", "cond058");
            this.Add("cond059", "cond058");
            this.Add("cond060", "cond058");
            this.Add("cond061", "cond058");
            this.Add("cond063", "cond058");
            this.Add("cond132", "cond058");
            this.Add("cond133", "cond058");
            this.Add("cond134", "cond058");
            this.Add("cond135", "cond058");
            this.Add("cond136", "cond058");
            this.Add("cond137", "cond058");
            this.Add("cond156", "cond058");
            this.Add("cond157", "cond058");
            this.Add("cond162", "cond058");
            this.Add("cond163", "cond058");
            this.Add("cond165", "cond058");
            this.Add("cond166", "cond058");
            this.Add("cond167", "cond058");
            this.Add("cond168", "cond058");
            this.Add("cond170", "cond058");
            this.Add("cond171", "cond058");
            this.Add("cond173", "cond058");
            this.Add("cond174", "cond058");
        }

        private void WeatherIconSimple01Map()
        {
            this.Add("cond023", "cond023");
            this.Add("cond024", "cond004");
            this.Add("cond033", "cond033");
            this.Add("cond034", "cond034");
            this.Add("cond052", "cond052");
            this.Add("cond053", "cond053");
            this.Add("cond062", "cond062");

            this.Add("cond064", "cond064");
            this.Add("cond065", "cond065");
            this.Add("cond069", "cond069");
            this.Add("cond074", "cond069");
            this.Add("cond075", "cond075");

            this.Add("cond066", "cond004");
            this.Add("cond067", "cond004");
            this.Add("cond068", "cond004");
            
            this.Add("cond071", "cond002");
            this.Add("cond072", "cond002");
            this.Add("cond073", "cond002");

            this.Add("cond076", "cond076");
            this.Add("cond077", "cond077");

            this.Add("cond000", "cond000");
            this.Add("cond007", "cond000");
            this.Add("cond001", "cond001");
            this.Add("cond013", "cond001");
            this.Add("cond002", "cond002");
            this.Add("cond016", "cond002");
            this.Add("cond003", "cond004");
            this.Add("cond026", "cond004");
            this.Add("cond004", "cond004");

            this.Add("cond005", "cond005");
            this.Add("cond020", "cond005");
            this.Add("cond046", "cond005");
            this.Add("cond048", "cond005");
            this.Add("cond087", "cond005");
            this.Add("cond088", "cond005");
            this.Add("cond089", "cond005");
            this.Add("cond120", "cond005");
            this.Add("cond121", "cond005");
            this.Add("cond122", "cond005");
            this.Add("cond141", "cond005");
            this.Add("cond152", "cond005");
            this.Add("cond014", "cond005");
            this.Add("cond015", "cond005");
            this.Add("cond081", "cond005");
            this.Add("cond082", "cond005");
            this.Add("cond083", "cond005");
            this.Add("cond139", "cond005");

            this.Add("cond006", "cond006");
            this.Add("cond022", "cond006");
            this.Add("cond093", "cond006");
            this.Add("cond094", "cond006");
            this.Add("cond095", "cond006");
            this.Add("cond143", "cond006");
            this.Add("cond018", "cond006");
            this.Add("cond030", "cond006");
            this.Add("cond105", "cond006");
            this.Add("cond106", "cond006");
            this.Add("cond107", "cond006");
            this.Add("cond147", "cond006");

            this.Add("cond008", "cond008");
            this.Add("cond012", "cond008");
            this.Add("cond011", "cond008");
            this.Add("cond029", "cond008");
            this.Add("cond078", "cond008");
            this.Add("cond079", "cond008");
            this.Add("cond080", "cond008");
            this.Add("cond102", "cond008");
            this.Add("cond103", "cond008");
            this.Add("cond104", "cond008");
            this.Add("cond138", "cond008");
            this.Add("cond146", "cond008");

            this.Add("cond009", "cond009");
            this.Add("cond019", "cond009");
            this.Add("cond084", "cond009");
            this.Add("cond085", "cond009");
            this.Add("cond086", "cond009");
            this.Add("cond140", "cond009");

            this.Add("cond010", "cond010");
            this.Add("cond037", "cond010");
            this.Add("cond999", "cond010");

            this.Add("cond017", "cond017");
            this.Add("cond031", "cond017");

            this.Add("cond025", "cond025");
            this.Add("cond036", "cond025");
            this.Add("cond021", "cond025");
            this.Add("cond028", "cond025");
            this.Add("cond090", "cond025");
            this.Add("cond091", "cond025");
            this.Add("cond092", "cond025");
            this.Add("cond099", "cond025");
            this.Add("cond100", "cond025");
            this.Add("cond101", "cond025");
            this.Add("cond142", "cond025");
            this.Add("cond145", "cond025");

            this.Add("cond032", "cond008");
            this.Add("cond027", "cond008");
            this.Add("cond096", "cond008");
            this.Add("cond097", "cond008");
            this.Add("cond098", "cond008");
            this.Add("cond144", "cond008");

            this.Add("cond035", "cond017");
            this.Add("cond070", "cond017");

            this.Add("cond039", "cond040");
            this.Add("cond040", "cond040");
            this.Add("cond111", "cond040");
            this.Add("cond112", "cond040");
            this.Add("cond113", "cond040");
            this.Add("cond149", "cond040");

            this.Add("cond038", "cond041");
            this.Add("cond041", "cond041");
            this.Add("cond108", "cond041");
            this.Add("cond109", "cond041");
            this.Add("cond110", "cond041");
            this.Add("cond148", "cond041");

            this.Add("cond043", "cond008");
            this.Add("cond044", "cond008");
            this.Add("cond054", "cond008");
            this.Add("cond055", "cond008");
            this.Add("cond117", "cond008");
            this.Add("cond118", "cond008");
            this.Add("cond119", "cond008");
            this.Add("cond126", "cond008");
            this.Add("cond127", "cond008");
            this.Add("cond128", "cond008");
            this.Add("cond151", "cond008");
            this.Add("cond154", "cond008");
            this.Add("cond160", "cond008");
            this.Add("cond176", "cond008");

            this.Add("cond042", "cond045");
            this.Add("cond045", "cond045");
            this.Add("cond114", "cond045");
            this.Add("cond115", "cond045");
            this.Add("cond116", "cond045");
            this.Add("cond150", "cond045");

            this.Add("cond047", "cond025");
            this.Add("cond049", "cond025");
            this.Add("cond123", "cond025");
            this.Add("cond124", "cond025");
            this.Add("cond125", "cond025");
            this.Add("cond153", "cond025");

            this.Add("cond050", "cond050");
            this.Add("cond158", "cond050");

            this.Add("cond051", "cond051");
            this.Add("cond159", "cond051");

            this.Add("cond056", "cond056");
            this.Add("cond057", "cond056");
            this.Add("cond161", "cond056");
            this.Add("cond129", "cond056");
            this.Add("cond169", "cond056");
            this.Add("cond130", "cond056");
            this.Add("cond172", "cond056");
            this.Add("cond131", "cond056");
            this.Add("cond175", "cond056");
            this.Add("cond155", "cond056");
            this.Add("cond164", "cond056");

            this.Add("cond058", "cond058");
            this.Add("cond059", "cond058");
            this.Add("cond060", "cond058");
            this.Add("cond061", "cond058");
            this.Add("cond063", "cond058");
            this.Add("cond132", "cond058");
            this.Add("cond133", "cond058");
            this.Add("cond134", "cond058");
            this.Add("cond135", "cond058");
            this.Add("cond136", "cond058");
            this.Add("cond137", "cond058");
            this.Add("cond156", "cond058");
            this.Add("cond157", "cond058");
            this.Add("cond162", "cond058");
            this.Add("cond163", "cond058");
            this.Add("cond165", "cond058");
            this.Add("cond166", "cond058");
            this.Add("cond167", "cond058");
            this.Add("cond168", "cond058");
            this.Add("cond170", "cond058");
            this.Add("cond171", "cond058");
            this.Add("cond173", "cond058");
            this.Add("cond174", "cond058");
        }

        public string GetImageName(string key)
        {
            if (this.ContainsKey(key))
            {
                return this[key];
            }
            return null;
        }
    }
}
