using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sum_Calculator_RPC_Client
{
    public partial class Form1 : Form
    {
        private bool connected = false;
        private Thread thread = null;
        private Client client;
        private Task send = null;
        private bool exit = false;

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
                    WriteLog(Utils.SystemMsg("You are now connected"));
                }
                else
                {
                    txtIP.Enabled = true;
                    txtPort.Enabled = true;

                    connectBtn.Enabled = true;
                    disconnectBtn.Enabled = false;
                    WriteLog(Utils.SystemMsg("You are now disconnected"));
                }
            });
        }

        private void Connected(bool status)
        {
            if (!exit)
            {
                connected = status;
                SetStateButton(status);
            }
        }

        private void Read(IAsyncResult result)
        {
            int bytes = 0;
            if (client.client.Connected)
            {
                try
                {
                    // lấy độ dài packet nhận được từ server
                    bytes = client.stream.EndRead(result);
                }
                catch (Exception ex)
                {
                    WriteLog(Utils.ErrorMsg(ex.Message));
                }
            }

            // kiểm tra độ dài packet
            if (bytes > 0)
            {
                // chuyển buffer thành string và lưu vào client.data
                client.data.AppendFormat("{0}", Encoding.UTF8.GetString(client.buffer, 0, bytes));
                try
                {
                    if (client.stream.DataAvailable)
                    {
                        client.stream.BeginWrite(client.buffer, 0, client.buffer.Length, new AsyncCallback(Write), null);
                    }
                    else
                    {
                        WriteLog(client.data.ToString());
                        client.data.Clear();
                        client.handle.Set();
                    }
                }
                catch (Exception ex)
                {
                    client.data.Clear();
                    WriteLog(Utils.ErrorMsg(ex.Message));
                    client.handle.Set();
                }
            }
            else
            {
                client.client.Close();
                client.handle.Set();
            }
        }

        private void Connection(IPAddress ip, int port)
        {
            try
            {
                // tạo đối tượng Client
                client = new Client();
                client.client = new TcpClient();
                client.client.Connect(ip, port);
                client.username = "Client";
                client.stream = client.client.GetStream();
                client.buffer = new byte[client.client.ReceiveBufferSize];
                client.data = new StringBuilder();
                client.handle = new EventWaitHandle(false, EventResetMode.AutoReset);

                // set trạng thái connect
                Connected(true);

                // while khi còn kết nối
                while (client.client.Connected)
                {
                    try
                    {
                        // nhận packet từ server gửi đến, khi nhận được packet nào đó thì một callback sẽ được gọi tên là Read
                        client.stream.BeginRead(client.buffer, 0, client.buffer.Length, new AsyncCallback(Read), null);
                        client.handle.WaitOne();
                    }
                    catch (Exception ex)
                    {
                        WriteLog(Utils.ErrorMsg(ex.Message));
                    }
                }

                // close TcpClient trong client
                client.client.Close();

                //set trạng thái connect
                Connected(false);
            }
            catch (Exception ex)
            {
                WriteLog(Utils.ErrorMsg(ex.Message));
            }
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (thread == null || !thread.IsAlive)
            {
                string address = txtIP.Text.Trim();
                string number = txtPort.Text.Trim();
                bool error = false;
                IPAddress ip = null;
                if (address.Length < 1)
                {
                    error = true;
                    WriteLog(Utils.SystemMsg("Address is required"));
                }
                else
                {
                    try
                    {
                        ip = Dns.Resolve(address).AddressList[0];
                    }
                    catch
                    {
                        error = true;
                        WriteLog(Utils.SystemMsg("Address is not valid"));
                    }
                }
                int port = -1;
                if (number.Length < 1)
                {
                    error = true;
                    WriteLog(Utils.SystemMsg("Port number is required"));
                }
                else if (!int.TryParse(number, out port))
                {
                    error = true;
                    WriteLog(Utils.SystemMsg("Port number is not valid"));
                }
                else if (port < 0 || port > 65535)
                {
                    error = true;
                    WriteLog(Utils.SystemMsg("Port number is out of range"));
                }

                if (!error)
                {
                    // chạy luồng Connection
                    thread = new Thread(() => Connection(ip, port))
                    {
                        IsBackground = true
                    };
                    thread.Start();
                }
            }
        }



        private void BeginWrite(string msg)
        {
            // chuyển msg thành mảng bytes
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (client.client.Connected)
            {
                try
                {
                    // gửi bytes lên server, kết quả trả về callback Write
                    client.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), null);
                }
                catch (Exception ex)
                {
                    WriteLog(Utils.ErrorMsg(ex.Message));
                }
            }
        }


        // Hàm callback xử lý cho Write
        private void Write(IAsyncResult result)
        {
            if (client.client.Connected)
            {
                try
                {
                    // kết thúc quá trình gửi packet
                    client.stream.EndWrite(result);
                }
                catch (Exception ex)
                {
                    WriteLog(Utils.ErrorMsg(ex.Message));
                }
            }
        }

        // Hàm để send gói tin tới sever
        private void Send(string msg)
        {
            if (send == null || send.IsCompleted)
            {
                send = Task.Factory.StartNew(() => BeginWrite(msg));
            }
            else
            {
                send.ContinueWith(antecendent => BeginWrite(msg));
            }
        }


        // kiểm tra đầu vào input trước khi gửi
        private int Validation(String input)
        {
            int i;
            if (int.TryParse(input, out i) == false)
            {
                WriteLog(Utils.SystemMsg("Please only enter a number"));
                return -1;
            }
            else if (i < 1 || i > 10)
            {
                WriteLog(Utils.SystemMsg("Please enter a number between 1 and 10"));
                return -1;
            }
            else
                return i;
        }


        // sự kiện click send 
        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (sendTextBox.Text.Length > 0)
            {
                if (Validation(sendTextBox.Text) != -1)
                {
                    string msg = sendTextBox.Text;
                    sendTextBox.Clear();
                    WriteLog(string.Format("{0} (You): {1}", client.username, msg));
                    if (connected)
                    {
                        Send(msg);
                    }
                }
            }
            else
            {
                WriteLog(Utils.SystemMsg("A number is needed"));
                return;
            }
        }

        private void disconnectBtn_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                client.client.Close();
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
            if (!exit)
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
        }


        // sự kiện khi thoát Form 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            exit = true;
            if (connected)
            {
                client.client.Close();
            }
        }
    }
}
