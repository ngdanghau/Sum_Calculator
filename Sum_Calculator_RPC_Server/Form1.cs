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
using Server_Midleware; 
namespace Sum_Calculator_RPC_Server
{
    public partial class Form1 : Form
    {
        private bool active = false;
        private Thread listener = null;
        private int SumTotal = 0;
        
        private ConcurrentDictionary<long, Client> clients = new ConcurrentDictionary<long, Client>();
        
        private RPC_Server rpc = new RPC_Server(); 


        public Form1()
        {
            InitializeComponent();
            txtPort.Text = "8080";
            txtIP.Text = "127.0.0.1";

        }
        

        private void Active(bool status)
        {
            active = status;
            SetStateButton(status);
        }

        // kiểm tra số và parse trả về
        public int Validation(String input)
        {
            int i;
            if (int.TryParse(input, out i) == false)
            {

                return -1;
            }
            else if (i < 1 || i > 10)
            {

                return -2;
            }
            else
                return i;
        }


        private void ReadEvent(string msg, Client obj)
        {
            int value = Validation(msg);
            if (value == -1)
            {
                WriteLog(Msg.System("Please only enter a number"));
            }
            else if (value == -2)
            {
                WriteLog(Msg.System("Please enter a number between 1 and 10"));
            }
            else
            {
                SumTotal += value;
                string message = string.Format("{0} send: {1} ====> Sum: {2}", obj.username, obj.data, SumTotal);
                WriteLog(message);
            }
        }


        private void ErrorEvent(string msg)
        {
            WriteLog(Msg.Error(msg));
        }

        private void WriteStatusEvent(string msg)
        {
            WriteLog(Msg.System(msg));
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
                            Client obj = rpc.createNewClient(listener); 
                           
                            Thread th = new Thread(() => rpc.Connection(clients, obj))
                            {
                                IsBackground = true
                            };
                            th.Start();
                            
                        }
                        catch (Exception ex)
                        {
                            WriteLog(Msg.Error(ex.Message));
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
                WriteLog(Msg.Error(ex.Message));
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
                
                try
                {
                    rpc.inheritMethod(WriteStatusEvent, ReadEvent, ErrorEvent); 
                    IPAddress ip = Helper.getIp(address);
                    int port = Helper.ValidatePort(number);

                    listener = new Thread(() => Listener(ip, port))
                    {
                        IsBackground = true
                    };
                    listener.Start();
                }
                catch(Exception ex)
                {
                    WriteLog(Msg.System(ex.Message));
                }
            }
        }



      

        private void stopBtn_Click(object sender, EventArgs e)
        {
            rpc.Disconnect(clients);
            Active(false);
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            WriteLog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            active = false;
            rpc.Disconnect(clients);

        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            SumTotal = 0;
            WriteLog(Msg.System("Reset Sum = 0"));
        }

        // set trạng thái Enable các nút
        private void SetStateButton(bool status)
        {
            startBtn.Invoke((MethodInvoker)delegate //để chỉnh trạng thái button từ một luồng khác.
            {

                if (status)
                {
                    txtIP.Enabled = false;
                    txtPort.Enabled = false;
                    startBtn.Enabled = false;
                    stopBtn.Enabled = true;
                    WriteLog(Msg.System("Server has started"));
                }
                else
                {
                    txtIP.Enabled = true;
                    txtPort.Enabled = true;
                    startBtn.Enabled = true;
                    stopBtn.Enabled = false;
                    WriteLog(Msg.System("Server has stopped"));
                }
            });
        }

        /// <summary> ghi log vào textbox</summary>
        /// <param name="msg">Nội dung cần hiện thị ra textbox, bỏ trống thì là xóa hết textbox</param>
        /// <returns> void </returns>
        private void WriteLog(string msg = "")
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
}
