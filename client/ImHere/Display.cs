using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using SocketIOClient;

namespace ImHere
{
    public partial class Display : Form
    {
        private int INROOM = 1;
        private int OUTROOM = 4;

        SocketIOClient.Client websocket;

        public Display()
        {
            InitializeComponent();
            connect();
//            NotifyState2Server(INROOM);
            InitializeTable();

            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(WatchSessionSwitchEvent);
        }

        private void connect()
        {
            websocket = new Client("http://localhost:3000");

            /* サーバとの接続が確定 */
            websocket.On("connect", (fn) =>
            {
                NotifyUpdateState(INROOM);
                Console.WriteLine("connect");
            });

            /* サーバからのプッシュ通知(誰かの状態が更新された)*/
            websocket.On("s2c_update_state", (data) =>
            {
                string json = ""+data.Json.Args[0];
                Console.WriteLine("update state : =" + json);
            });

            /* サーバからのプッシュ通知(新しくメンバーが追加) */
            websocket.On("s2c_message_register_member", (data) =>
            {
                string json = ""+data.Json.Args[0];
                Console.WriteLine("register new member :"+json);
            });

            websocket.Connect();
        }

        /* 状態の変更をサーバに通知 */
        private void NotifyUpdateState(int state)
        {
            // TODO: ユーザ固有の情報を送信
            int user_id=1;
            websocket.Emit("c2s_update_state", new {user_id=user_id, state=state});
        }

        /*
        private void NotifyState2Server(int statement)
        {
            if (SystemInformation.TerminalServerSession == false)
            {
                 HttpClient client = new HttpClient();
                 client.BaseAddress = new Uri("http://localhost:1234/imhere/api/");

                 HttpResponseMessage response = client.GetAsync("change/state?user_id=14&statement="+statement).Result;
                 if (response.IsSuccessStatusCode)
                 {
                    response.Dispose();
                 }
            }
        }
        */

        /* リモート接続しているかの判定 */
        private bool isRemoteSession()
        {
            /* trueならリモート接続中 */
            if (SystemInformation.TerminalServerSession)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /* セッション切り替えイベントを監視 */
        private void WatchSessionSwitchEvent(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            // ユーザーがセッションをロック
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                //I left my desk
                Console.WriteLine("I left my desk");
                NotifyUpdateState(OUTROOM);
            }
            // ユーザーがセッションのロックを解除
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                //I returned to my desk
                Console.WriteLine("I returned to my desk");

                if (isRemoteSession() == false)
                {
                    NotifyUpdateState(INROOM);
                }
            }
            // ユーザーがセッションからログオフ
            else if (e.Reason == SessionSwitchReason.SessionLogoff)
            {                
                Console.WriteLine("I logoff");
                NotifyUpdateState(INROOM);
            }
        }

        /* 在室テーブルの初期化処理 */
        private async void  InitializeTable()
        {
            /* HTTPリクエストにより全ユーザ情報を取得 */
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000/imhere/api/");
            
            HttpResponseMessage response = client.GetAsync("get/state/all").Result;
            if (response.IsSuccessStatusCode)
            {
                System.IO.Stream stream = await response.Content.ReadAsStreamAsync();

                /* JSONをオブジェクトに変換 */
                var serializer = new DataContractJsonSerializer(typeof(List<User>));
                List<User> userList = (List<User>) serializer.ReadObject(stream);

                /* TODO : 各ユーザをテーブルに追加(研究室毎に分ける) */
                foreach (User user in userList)
                {
                    addUserRow(user);
                    Console.WriteLine(user.name);
                }

                response.Dispose();
            }
        }

        /* 在室テーブルにユーザを追加 */
        private void addUserRow(User user)
        {

            DataGridView dataGridView = null;  // ユーザを追加するデータビュー

            if (user.lab.Equals("山本研究室"))
            {
                dataGridView = yLabDataGridView;
            }

            if (user.lab.Equals("高橋研究室"))
            {
                dataGridView = tLabDataGridView;
            }

            if (user.lab.Equals("片山研究室"))
            {
                dataGridView = kLabDataGridView;
            }

            /* TODO : DataGridViewにレコードを追加 */
            if (dataGridView != null)
            {
                int index = dataGridView.Rows.Add();
                dataGridView.Rows[index].Cells[0].Value = user.name;
                dataGridView.Rows[index].Cells[user.state].Value = "●";
            }
            
        }

        private void entry_member_button_Click(object sender, EventArgs e)
        {
            MemberEntryFrom form = new MemberEntryFrom(websocket);
            form.Show();
        }

        private void Display_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }
}
