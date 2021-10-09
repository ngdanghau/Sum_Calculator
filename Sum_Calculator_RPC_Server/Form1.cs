using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sum_Calculator_RPC_Server
{
    public partial class Form1 : Form
    {
        private bool active = false;
        private Thread listener = null;
        private Task send = null;
        private Thread disconnect = null;
        private bool exit = false;
        private long id = 0;
        private ConcurrentDictionary<long, Client> clients = new ConcurrentDictionary<long, Client>();
        private int SumTotal = 0;

        public Form1()
        {
            InitializeComponent();
            txtPort.Text = "8080";
            txtIP.Text = "127.0.0.1";
        }

        // set trạng thái Enable các nút
        private void SetStateButton(bool status)
        {
            startBtn.Invoke((MethodInvoker)delegate
            {

                if (status)
                {
                    txtIP.Enabled = false;
                    txtPort.Enabled = false;
                    startBtn.Enabled = false;
                    stopBtn.Enabled = true;
                    WriteLog(Utils.SystemMsg("Server has started"));
                }
                else
                {
                    txtIP.Enabled = true;
                    txtPort.Enabled = true;
                    startBtn.Enabled = true;
                    stopBtn.Enabled = false;
                    WriteLog(Utils.SystemMsg("Server has stopped"));
                }
            });
        }

        // set trạng thái cho mỗi nút Dissconet Client
        private void SetStateDisconnectButton(bool status)
        {
            disconnectBtn.Invoke((MethodInvoker)delegate
            {
                disconnectBtn.Enabled = status;
            });
        }

        /// <summary> ghi log vào textbox</summary>
        /// <param name="msg">Nội dung cần hiện thị ra textbox, bỏ trống thì là xóa hết textbox</param>
        /// <returns> void </returns>
        private void WriteLog(string msg = "")
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

        private void Active(bool status)
        {
            if (!exit)
            {
                active = status;
                SetStateButton(status);
            }
        }

        // kiểm tra số và parse trả về
        private int Validation(String input)
        {
            int i;
            if (int.TryParse(input, out i) == false)
            {
                this.WriteLog(Utils.SystemMsg("Please only enter a number"));
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

        /// <summary> hàm xử lý khi nhận được gói tin</summary>
        /// <param name="result">Kết quả trả về của cái TcpListener</param>
        /// <returns> void </returns>
        private void Read(IAsyncResult result)
        {
            Client obj = (Client)result.AsyncState;
            int bytes = 0;
            if (obj.client.Connected)
            {
                try
                {
                    bytes = obj.stream.EndRead(result);
                }
                catch (Exception ex)
                {
                    WriteLog(Utils.ErrorMsg(ex.Message));
                }
            }
            if (bytes > 0)
            {
                obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                try
                {
                    if (obj.stream.DataAvailable)
                    {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                    }
                    else
                    {
                        int value = Validation(obj.data.ToString());
                        if (value > -1)
                        {
                            SumTotal += value;
                            string msg = string.Format("{0} send: {1} ====> Sum: {2}", obj.username, obj.data, SumTotal);
                            WriteLog(msg);
                        }
                        obj.data.Clear();
                        obj.handle.Set();
                    }
                }
                catch (Exception ex)
                {
                    obj.data.Clear();
                    WriteLog(Utils.ErrorMsg(ex.Message));
                    obj.handle.Set();
                }
            }
            else
            {
                obj.client.Close();
                obj.handle.Set();
            }
        }


        // Hàm xử lý kết nối và gửi nhận packet
        private void Connection(Client obj)
        {
            // thêm vào danh sách các client
            clients.TryAdd(obj.id, obj);

            string msg = string.Format("{0} has connected", obj.username);
            WriteLog(Utils.SystemMsg(msg));
            SetStateDisconnectButton(true);

            // gửi lại cho client kết quả kết nối
            Send(Utils.SystemMsg(msg), obj);

            // chạy trong khi vẫn kết nối
            while (obj.client.Connected)
            {
                try
                {
                    // bắt đầu nhận packet và nếu có sẽ gửi vào callback tên là Read
                    obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                    obj.handle.WaitOne();
                }
                catch (Exception ex)
                {
                    WriteLog(Utils.ErrorMsg(ex.Message));
                }
            }
            // đóng TcpListener
            obj.client.Close();
            // đồng thời xóa client mới close trong danh sách
            clients.TryRemove(obj.id, out Client tmp);

            // gửi lại trạng thái cho client biết là đã bị disconnect
            msg = string.Format("{0} has disconnected", tmp.username);
            WriteLog(Utils.SystemMsg(msg));
            Send(msg, obj);
        }

        private void Listener(IPAddress ip, int port)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(ip, port);
                listener.Start();

                // kích hoạt trạng thái hệ thống 
                Active(true);
                while (active)
                {
                    if (listener.Pending())
                    {
                        try
                        {
                            // tạo một đối tượng Client
                            Client obj = new Client();
                            obj.id = id;
                            obj.username = "Client " + id;
                            obj.client = listener.AcceptTcpClient();
                            obj.stream = obj.client.GetStream();
                            obj.buffer = new byte[obj.client.ReceiveBufferSize];
                            obj.data = new StringBuilder();
                            obj.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
                            Thread th = new Thread(() => Connection(obj))
                            {
                                IsBackground = true
                            };
                            th.Start();
                            id++;
                        }
                        catch (Exception ex)
                        {
                            WriteLog(Utils.ErrorMsg(ex.Message));
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                // Hủy kích hoạt trạng thái hệ thống 
                Active(false);
            }
            catch (Exception ex)
            {
                WriteLog(Utils.ErrorMsg(ex.Message));
            }
            finally
            {
                if (listener != null)
                {
                    listener.Server.Close();
                }
            }
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (active)
            {
                active = false;
            }
            else if (listener == null || !listener.IsAlive)
            {
                string address = txtIP.Text.Trim();
                string number = txtPort.Text.Trim();
                string username = "Server";
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
                if (username.Length < 1)
                {
                    error = true;
                    WriteLog(Utils.SystemMsg("Username is required"));
                }
                if (!error)
                {
                    listener = new Thread(() => Listener(ip, port))
                    {
                        IsBackground = true
                    };
                    listener.Start();
                }
            }
        }

        private void Write(IAsyncResult result)
        {
            Client obj = (Client)result.AsyncState;
            if (obj.client.Connected)
            {
                try
                {
                    obj.stream.EndWrite(result);
                }
                catch (Exception ex)
                {
                    WriteLog(Utils.ErrorMsg(ex.Message));
                }
            }
        }

        private void BeginWrite(string msg, Client obj)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected)
            {
                try
                {
                    obj.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), obj);
                }
                catch (Exception ex)
                {
                    WriteLog(Utils.ErrorMsg(ex.Message));
                }
            }
        }

        private void BeginWrite(string msg, long id = -1)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            foreach (KeyValuePair<long, Client> obj in clients)
            {
                if (id != obj.Value.id && obj.Value.client.Connected)
                {
                    try
                    {
                        obj.Value.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), obj.Value);
                    }
                    catch (Exception ex)
                    {
                        WriteLog(Utils.ErrorMsg(ex.Message));
                    }
                }
            }
        }

        private void Send(string msg, Client obj)
        {
            if (send == null || send.IsCompleted)
            {
                send = Task.Factory.StartNew(() => BeginWrite(msg, obj));
            }
            else
            {
                send.ContinueWith(antecendent => BeginWrite(msg, obj));
            }
        }

        private void Disconnect(long id = -1)
        {
            if (disconnect == null || !disconnect.IsAlive)
            {
                disconnect = new Thread(() =>
                {
                    if (id >= 0)
                    {
                        clients.TryGetValue(id, out Client obj);
                        obj.client.Close();
                    }
                    else
                    {
                        foreach (KeyValuePair<long, Client> obj in clients)
                        {
                            obj.Value.client.Close();
                        }
                    }
                })
                {
                    IsBackground = true
                };
                disconnect.Start();
            }
            SetStateDisconnectButton(false);
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            WriteLog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            exit = true;
            active = false;
            Disconnect();
        }

        private void disconnectBtn_Click(object sender, EventArgs e)
        {
            Disconnect();
            SetStateDisconnectButton(clients.Count > 0);
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            WriteLog(Utils.SystemMsg("Reset Sum = 0"));
            SumTotal = 0;
        }
    }
}
