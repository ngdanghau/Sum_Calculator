using Client_Midleware;
using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Sum_Calculator_RPC_Client
{
    public partial class Form1 : Form
    {
        // đối tượng Remote Procedure Call - RPC
        private RPC rpc;
        // biến trạng thái
        private bool connected = false;

        // luồng
        private Thread thread = null;

        public Form1()
        {
            InitializeComponent();
            txtIP.Text = "127.0.0.1";
            txtPort.Text = "8080";
        }

        // set trạng thái Enable của các nút trên ứng dụng
        private void SetStateButton(bool status)
        {
            connectBtn.Invoke((MethodInvoker)delegate
            {
                if (status)
                {
                    txtIP.Enabled = false;
                    txtPort.Enabled = false;

                    connectBtn.Enabled = false;
                    disconnectBtn.Enabled = true;
                    WriteLog(Msg.System("You are now connected"));
                }
                else
                {
                    txtIP.Enabled = true;
                    txtPort.Enabled = true;

                    connectBtn.Enabled = true;
                    disconnectBtn.Enabled = false;
                    WriteLog(Msg.System("You are now disconnected"));
                }
            });
        }

        private void Connected(bool status)
        {
            connected = status;
            SetStateButton(status);
        }

        // đọc 
        private void ReadEvent(string message)
        {
            Console.WriteLine(message);
        }

        private void ErrorEvent(string message)
        {
            WriteLog(Msg.System(message));
        }

        private void Connection(IPAddress ip, int port)
        {
            try
            {
                // tạo đối tượng RPC
                rpc = new RPC();
                rpc.CreateClient(ip, port); //kết nối đến server thông qua port và ip tương ứng.
                // set trạng thái connect
                Connected(true); //thay đổi trạng thái button.
                // tạo một connection và bind 2 callback là ReadEvent và ErrorEvent vào
                // mỗi khi có một gói tin gửi đến client => ReadEvent sẽ được gọi
                // mỗi khi có lỗi Exception xảy ra, Errorvent sẽ được gọi
                rpc.StartConnection(ReadEvent, ErrorEvent);
                //set trạng thái connect
                Connected(false);
            }
            catch(Exception ex)
            {
                ErrorEvent(ex.Message);
            }
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (thread == null || !thread.IsAlive)
            {
                string address = txtIP.Text.Trim();
                string number = txtPort.Text.Trim();
                IPAddress ip = null;
                int port = -1;
                try
                {
                    ip = Helper.ValidateAddress(address);
                    port = Helper.ValidatePort(number);
                    // chạy luồng Connection . Tại sao dùng luồng ?? vì để tránh block UI.
                    thread = new Thread(() => Connection(ip, port))
                    {
                        IsBackground = true
                    };
                    thread.Start();
                }
                catch(Exception ex)
                {
                    WriteLog(Msg.System(ex.Message)); 
                }
            }
        }



        // kiểm tra đầu vào input trước khi gửi
        private int Validation(String input)
        {
            int i;
            if (int.TryParse(input, out i) == false)
            {
                WriteLog(Msg.System("Please only enter a number"));
                return -1;
            }
            else if (i < 1 || i > 10)
            {
                WriteLog(Msg.System("Please enter a number between 1 and 10"));
                return -1;
            }
            else
                return i;
        }


        // sự kiện click send 
        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (!rpc.client.Connected)
            {
                WriteLog(Msg.System("You are now disconnected"));
                return;
            }

            if (sendTextBox.Text.Length > 0)
            {
                if (Validation(sendTextBox.Text) != -1)
                {
                    string msg = sendTextBox.Text;
                    sendTextBox.Clear();
                    WriteLog(string.Format("{0} (You): {1}", rpc.username, msg));
                    if (connected)
                    {
                        rpc.Send(msg);
                    }
                }
            }
            else
            {
                WriteLog(Msg.System("A number is needed"));
                return;
            }
        }

        private void disconnectBtn_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                rpc.CloseConnection();
            }
        }


        // sự kiện clear log
        private void clearBtn_Click(object sender, EventArgs e)
        {
            WriteLog();
        }

        // ghi log vào textbox
        private void WriteLog(string msg = "") // xóa log Text nếu msg rỗng hoặc ko có
        {
            logTextBox.Invoke((MethodInvoker)delegate
            {
                if (msg.Length > 0)
                {
                    logTextBox.AppendText(string.Format("[ {0} ] {1}{2}", DateTime.Now.ToString("HH:mm"), msg, Environment.NewLine));
                }
                else
                {
                    logTextBox.Clear();
                }
            });
        }


        // sự kiện khi thoát Form 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connected)
            {
                rpc.CloseConnection();
            }
        }
    }
}
