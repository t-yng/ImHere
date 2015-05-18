using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImHere
{
    public partial class MemberEntryFrom : Form
    {

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

                /* サーバにメンバーの新規登録を通知 */
                websocket.Emit("c2s_message_register_member", new{name=name, lab=lab, state=1});

                /* TODO : サーバに登録したユーザ情報をローカルで永続保存する */
                
                

                /* MACアドレスを一緒に送信することで、サーバからメンバー新規登録メッセージ受信時に
                 * 自分が送信した情報であることを識別すると良い? */

                this.Close();
            }
        }


        /**
         * ユーザ情報をローカルで永続保存する
         */
        private void saveUserSettingConf(UserSetting setting)
        {
            
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
