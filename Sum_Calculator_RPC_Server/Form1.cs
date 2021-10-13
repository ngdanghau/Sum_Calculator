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
        private Thread disconnect = null;
        
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
                           
                            Thread th = new Thread(() => rpc.Connection(clients,obj))
                            {
                                IsBackground = true
                            };
                            th.Start();
                            
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
               
                
                
                try
                {
                    rpc.inheritMethod(WriteLog); 
                    IPAddress ip = Helper.getIp(address);
                    int port = Helper.ValidatePort(number);
                    Helper.ValidateUsername(username); 

                    listener = new Thread(() => Listener(ip, port))
                    {
                        IsBackground = true
                    };
                    listener.Start();
                }
                catch(Exception ex)
                {
                    WriteLog(Utils.SystemMsg(ex.Message));
                }
            }
        }



      

        private void stopBtn_Click(object sender, EventArgs e)
        {
            rpc.Disconnect(clients,disconnect);
            Active(false);
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            WriteLog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            active = false;
            rpc.Disconnect(clients,disconnect);

        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            WriteLog(Utils.SystemMsg("Reset Sum = 0"));
            rpc.resetTotal(); 
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
