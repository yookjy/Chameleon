using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ChameleonLib.Helper
{
    public class WwwHelper
    {
        public static bool ContainsOtherLetter(string value)
        {
            char[] chArr = value.ToCharArray();
            for (int i = 0; i < chArr.Length; i++)
            {
                char ch = chArr[i];
                if (!(0x30 <= ch && ch <= 0x39) && !((0x61 <= ch && ch <= 0x7A) || (0x41 <= ch && ch <= 0x5A)))
                    return true;
            }
            return false;
        }
    }
}
