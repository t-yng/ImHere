using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImHere
{
    public partial class MemberEntryFrom : Form
    {

        private string USER_CONF_FILE = Application.LocalUserAppDataPath+"\\usersetting.config";
        private string HOST_URL = "http://localhost:3000";

        SocketIOClient.Client websocket;

        public MemberEntryFrom(SocketIOClient.Client websocket)
        {
            InitializeComponent();
            this.websocket = websocket;
        }

        /**
         * 「OK」ボタンが押された時の処理
         */
        private void ok_button_Click(object sender, EventArgs e)
        {
            if (validate_Input())    // 入力チェック
            {
                string name = this.textBox1.Text;
                string lab = this.comboBox1.Text;

                Console.WriteLine("name=" + name + ", lab=" + lab);

                /* サーバにメンバーの新規登録を通知(HTTPリクエストに変更するべき) */
                websocket.Emit("c2s_message_register_member", new{name=name, lab=lab, state=1});

                /* TODO : サーバに登録したユーザ情報をローカルで永続保存する */
                UserSetting setting = new UserSetting(name, getMacAddress());
                saveUserSetting(setting);

                /* MACアドレスを一緒に送信することで、サーバからメンバー新規登録メッセージ受信時に
                 * 自分が送信した情報であることを識別すると良い? */

                this.Close();
            }
        }

        /**
         * サーバに登録したユーザ情報をファイルに保存
         */
        private void saveUserSetting(UserSetting setting)
        {
            /* ユーザ情報をファイルに保存 */
            string filename = @USER_CONF_FILE+"\\usersettings.config";
//            UserSetting setting = new UserSetting("柳", "00-1B-DC-05-C2-37");

            BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            bf.Serialize(fs, setting);

            fs.Close();
            
        }


        /*アドレスを取得 */
        private string getMacAddress()
        {
            string macAddress = "";

            /* アダプタリストを取得 */
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in adapters)
            {
                /* ネットワーク接続状態がUPのアダプタのみ表示 */
                if (adapter.OperationalStatus == OperationalStatus.Up)
                {
                   /* MACアドレスの取得 */
                    PhysicalAddress physical = adapter.GetPhysicalAddress();
//                    Console.WriteLine("Status : " + adapter.OperationalStatus);
//                    Console.WriteLine("Name : " + adapter.Name);
//                    Console.WriteLine("Interface type : " + adapter.NetworkInterfaceType);
 //                   Console.WriteLine("MACアドレス=" + physical);

                    macAddress = physical.ToString();

                    break;
                }
            }

            return macAddress;
        }

        /* 「キャンセル」ボタンが押された時の処理 */
        private void cancell_button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /* フォームの入力値が正しいか確認 */
        private bool validate_Input()
        {
            if (textBox1.Text == string.Empty)
            {
                MessageBox.Show("名前が入力さていません", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("研究室を選択して下さい", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}
