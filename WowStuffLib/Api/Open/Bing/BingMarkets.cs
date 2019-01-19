using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChameleonLib.Model;
using ChameleonLib.Resources;

namespace ChameleonLib.Api.Open.Bing
{
    public class BingMarkets
    {
        private static List<PickerItem> markets;

        public static List<PickerItem> Markets 
        {
            get
            {
                if (markets == null)
                {
                    markets = new List<PickerItem>();
                    //markets.Add(new CultureInfo("ar-XA"));
                    markets.Add(new PickerItem() { Key = "ar-XA", Name = AppResources.LanguageRegion_ar_XA });
                    markets.Add(new PickerItem() { Key = "bg-BG", Name = new CultureInfo("bg-BG").DisplayName });
                    markets.Add(new PickerItem() { Key = "cs-CZ", Name = new CultureInfo("cs-CZ").DisplayName });
                    markets.Add(new PickerItem() { Key = "da-DK", Name = new CultureInfo("da-DK").DisplayName });
                    markets.Add(new PickerItem() { Key = "de-AT", Name = new CultureInfo("de-AT").DisplayName });
                    markets.Add(new PickerItem() { Key = "de-CH", Name = new CultureInfo("de-CH").DisplayName });
                    markets.Add(new PickerItem() { Key = "de-DE", Name = new CultureInfo("de-DE").DisplayName });
                    markets.Add(new PickerItem() { Key = "el-GR", Name = new CultureInfo("el-GR").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-AU", Name = new CultureInfo("en-AU").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-CA", Name = new CultureInfo("en-CA").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-GB", Name = new CultureInfo("en-GB").DisplayName });
                    //markets.Add(new PickerItem() { Key = "en-ID", Name = new CultureInfo("en-ID").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-ID", Name = AppResources.LanguageRegion_en_ID });
                    markets.Add(new PickerItem() { Key = "en-IE", Name = new CultureInfo("en-IE").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-IN", Name = new CultureInfo("en-IN").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-MY", Name = new CultureInfo("en-MY").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-NZ", Name = new CultureInfo("en-NZ").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-PH", Name = new CultureInfo("en-PH").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-SG", Name = new CultureInfo("en-SG").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-US", Name = new CultureInfo("en-US").DisplayName });
                    //markets.Add(new PickerItem() { Key = "en-XA", Name = new CultureInfo("en-XA").DisplayName });
                    markets.Add(new PickerItem() { Key = "en-XA", Name = AppResources.LanguageRegion_en_XA });
                    markets.Add(new PickerItem() { Key = "en-ZA", Name = new CultureInfo("en-ZA").DisplayName });
                    markets.Add(new PickerItem() { Key = "es-AR", Name = new CultureInfo("es-AR").DisplayName });
                    markets.Add(new PickerItem() { Key = "es-CL", Name = new CultureInfo("es-CL").DisplayName });
                    markets.Add(new PickerItem() { Key = "es-ES", Name = new CultureInfo("es-ES").DisplayName });
                    markets.Add(new PickerItem() { Key = "es-MX", Name = new CultureInfo("es-MX").DisplayName });
                    markets.Add(new PickerItem() { Key = "es-US", Name = new CultureInfo("es-US").DisplayName });
                    //markets.Add(new PickerItem() { Key = "es-XL", Name = new CultureInfo("es-XL").DisplayName });
                    markets.Add(new PickerItem() { Key = "ex-XL", Name = AppResources.LanguageRegion_es_XL });
                    markets.Add(new PickerItem() { Key = "et-EE", Name = new CultureInfo("et-EE").DisplayName });
                    markets.Add(new PickerItem() { Key = "fi-FI", Name = new CultureInfo("fi-FI").DisplayName });
                    markets.Add(new PickerItem() { Key = "fr-BE", Name = new CultureInfo("fr-BE").DisplayName });
                    markets.Add(new PickerItem() { Key = "fr-CA", Name = new CultureInfo("fr-CA").DisplayName });
                    markets.Add(new PickerItem() { Key = "fr-CH", Name = new CultureInfo("fr-CH").DisplayName });
                    markets.Add(new PickerItem() { Key = "fr-FR", Name = new CultureInfo("fr-FR").DisplayName });
                    markets.Add(new PickerItem() { Key = "he-IL", Name = new CultureInfo("he-IL").DisplayName });
                    markets.Add(new PickerItem() { Key = "hr-HR", Name = new CultureInfo("hr-HR").DisplayName });
                    markets.Add(new PickerItem() { Key = "hu-HU", Name = new CultureInfo("hu-HU").DisplayName });
                    markets.Add(new PickerItem() { Key = "it-IT", Name = new CultureInfo("it-IT").DisplayName });
                    markets.Add(new PickerItem() { Key = "ja-JP", Name = new CultureInfo("ja-JP").DisplayName });
                    markets.Add(new PickerItem() { Key = "ko-KR", Name = new CultureInfo("ko-KR").DisplayName });
                    markets.Add(new PickerItem() { Key = "lt-LT", Name = new CultureInfo("lt-LT").DisplayName });
                    markets.Add(new PickerItem() { Key = "lv-LV", Name = new CultureInfo("lv-LV").DisplayName });
                    markets.Add(new PickerItem() { Key = "nb-NO", Name = new CultureInfo("nb-NO").DisplayName });
                    markets.Add(new PickerItem() { Key = "nl-BE", Name = new CultureInfo("nl-BE").DisplayName });
                    markets.Add(new PickerItem() { Key = "nl-NL", Name = new CultureInfo("nl-NL").DisplayName });
                    markets.Add(new PickerItem() { Key = "pl-PL", Name = new CultureInfo("pl-PL").DisplayName });
                    markets.Add(new PickerItem() { Key = "pt-BR", Name = new CultureInfo("pt-BR").DisplayName });
                    markets.Add(new PickerItem() { Key = "pt-PT", Name = new CultureInfo("pt-PT").DisplayName });
                    markets.Add(new PickerItem() { Key = "ro-RO", Name = new CultureInfo("ro-RO").DisplayName });
                    markets.Add(new PickerItem() { Key = "ru-RU", Name = new CultureInfo("ru-RU").DisplayName });
                    markets.Add(new PickerItem() { Key = "sk-SK", Name = new CultureInfo("sk-SK").DisplayName });
                    //markets.Add(new PickerItem() { Key = "sl-SL", Name = new CultureInfo("sl-SL").DisplayName });
                    markets.Add(new PickerItem() { Key = "sl-SL", Name = AppResources.LanguageRegion_sl_SL });
                    markets.Add(new PickerItem() { Key = "sv-SE", Name = new CultureInfo("sv-SE").DisplayName });
                    markets.Add(new PickerItem() { Key = "th-TH", Name = new CultureInfo("th-TH").DisplayName });
                    markets.Add(new PickerItem() { Key = "tr-TR", Name = new CultureInfo("tr-TR").DisplayName });
                    markets.Add(new PickerItem() { Key = "uk-UA", Name = new CultureInfo("uk-UA").DisplayName });
                    markets.Add(new PickerItem() { Key = "zh-CN", Name = new CultureInfo("zh-CN").DisplayName });
                    markets.Add(new PickerItem() { Key = "zh-HK", Name = new CultureInfo("zh-HK").DisplayName });
                    markets.Add(new PickerItem() { Key = "zh-TW", Name = new CultureInfo("zh-TW").DisplayName });
                }

                return markets;
            }
        }
        
    }
}
