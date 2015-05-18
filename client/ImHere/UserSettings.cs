using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* DBに登録するユーザ設定 */
namespace ImHere
{
    [Serializable()]
    class UserSetting
    {
        public string name { get; set; }
        public string macAdress { get; set; }

        public UserSetting(string name, string macAdress)
        {
            this.name = name;
            this.macAdress = macAdress;
        }
    }
}
