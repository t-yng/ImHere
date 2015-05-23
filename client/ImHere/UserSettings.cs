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
        public string macAddress { get; set; }

        public UserSetting(string name, string macAddress)
        {
            this.name = name;
            this.macAddress = macAddress;
        }
    }
}
