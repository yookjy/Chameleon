using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChameleonLib.Helper;
using ChameleonLib.Resources;

namespace ChameleonLib.Model
{
    public class BlackList
    {
        public ObservableCollection<BlackDomain> Items { get; set; }

        public BlackList()
        {
            this.Items = SettingHelper.Get(Constants.DOMAIN_FILTER) as ObservableCollection<BlackDomain>;

            if (this.Items == null)
            {
                this.Items = new ObservableCollection<BlackDomain>();
            }
        }

        /*
        public void LoadData()
        {
            ) as ObservableCollection<BlackDomain>;

            if (this.Items == null)
            {
                this.Items.Add(new BlackDomain()
                {
                    AddedDateTime = DateTime.Now,
                    path = "http://www.naver1.com",
                    SearchKeyword = "검색어ㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇㅇ1"
                });
                this.Items.Add(new BlackDomain()
                {
                    AddedDateTime = DateTime.Now,
                    path = "http://www.naver2.com/djksjkdlsjd/sdjksjdks/dksjdksjkdjks/",
                    SearchKeyword = "검색어2"
                });
                this.Items.Add(new BlackDomain()
                {
                    AddedDateTime = DateTime.Now,
                    path = "http://www.naver3.com",
                    SearchKeyword = "검색어2ㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷㄷ"
                });
            }
        }
* */
        public void SaveData()
        {
            SettingHelper.Set(Constants.DOMAIN_FILTER, this.Items, false);
        }

        public void Add(BlackDomain blackDomain)
        {
            this.Items.Add(blackDomain);
        }
    }
}
