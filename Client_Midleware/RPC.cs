using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client_Midleware
{
    public delegate void ReadEventDelegate(string msg);

    public delegate void DisconnectEventDelegate();

    public delegate void ErrorEventDelegate(string msg);

    public class RPC
    {
        private Task send = null;
        public Client obj;
        ReadEventDelegate ReadAction = null;
        ErrorEventDelegate ErrorAction = null;

        private void BeginWrite(string msg)
        {
            // chuyển msg thành mảng bytes
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected)
            {
                try
                {
                    // gửi bytes lên server, kết quả trả về callback Write
                    obj.stream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    ErrorAction(ex.Message);
                }
            }
        }

        // Hàm để send gói tin tới sever
        public void Send(string msg)
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

        // hàm đọc gói tin từ server
        private void Read(IAsyncResult result)
        {
            int bytes = 0;
            if (obj.client.Connected)
            {
                try
                {
                    // kết thúc việc đọc
                    bytes = obj.stream.EndRead(result);
                }
                catch (Exception ex)
                {
                    ErrorAction(ex.Message);
                }
            }

            // kiểm tra lượng gói tin có tồn tại ko
            if (bytes > 0)
            {
                // chuyển sang thành string
                obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                try
                {
                    // nếu vẫn tồn tại gói tin thì đọc tiếp lại lần nữa
                    if (obj.stream.DataAvailable)
                    {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), null);
                    }
                    else
                    {
                        // nếu không thì gọi Callback tên là ReadAction 
                        ReadAction(obj.data.ToString());
                        obj.data.Clear();
                        obj.handle.Set();
                    }
                }
                catch (Exception ex)
                {
                    // nếu lỗi thì gọi Callback ErrorAction
                    obj.data.Clear();
                    obj.handle.Set();
                    ErrorAction(ex.Message);
                }
            }
            else
            {
                obj.client.Close();
                obj.handle.Set();
            }
        }

        public Client CreateClient(IPAddress ip, int port, string username = "Client")
        {
            obj = new Client();
            obj.username = username;
            obj.client = new TcpClient();
            obj.client.Connect(ip, port);
            obj.stream = obj.client.GetStream();
            obj.buffer = new byte[obj.client.ReceiveBufferSize];
            obj.data = new StringBuilder();
            obj.handle = new EventWaitHandle(false, EventResetMode.AutoReset);

            return obj;
        }

        public void CloseConnection()
        {
            obj.client.Close();
        }

        public void StartConnection(ReadEventDelegate ReadEvent, ErrorEventDelegate ErrorEvent)
        {
            ReadAction += ReadEvent;
            ErrorAction += ErrorEvent;
            while (obj.client.Connected)
            {
                try
                {
                    // đọc gói tin từ server, nếu có gói tin thì hàm callback tên là Read sẽ được gọi
                    obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), null);
                    obj.handle.WaitOne();
                }
                catch (Exception ex)
                {
                    ErrorAction(ex.Message);
                }
            }
            this.CloseConnection();
        }
    }
}
