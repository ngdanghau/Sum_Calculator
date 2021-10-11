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

    public delegate void ErrorEventDelegate(string msg);

    public class RPC
    {
        private Task send = null;


        public string username;
        public TcpClient client;
        private NetworkStream stream;
        private byte[] buffer;
        private StringBuilder data;
        private EventWaitHandle handle;


        ReadEventDelegate ReadAction = null;
        ErrorEventDelegate ErrorAction = null;

        private void BeginWrite(string msg)
        {
            // chuyển msg thành mảng bytes
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (client.Connected)
            {
                try
                {
                    // gửi bytes lên server, kết quả trả về callback Write
                    stream.Write(buffer, 0, buffer.Length);
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
            if (client.Connected)
            {
                try
                {
                    // kết thúc việc đọc
                    bytes = stream.EndRead(result);
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
                data.AppendFormat("{0}", Encoding.UTF8.GetString(buffer, 0, bytes));
                try
                {
                    // nếu vẫn tồn tại gói tin thì đọc tiếp lại lần nữa
                    if (stream.DataAvailable)
                    {
                        stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(Read), null);
                    }
                    else
                    {
                        // nếu không thì gọi Callback tên là ReadAction 
                        ReadAction(data.ToString());
                        data.Clear();
                        handle.Set();
                    }
                }
                catch (Exception ex)
                {
                    // nếu lỗi thì gọi Callback ErrorAction
                    data.Clear();
                    handle.Set();
                    ErrorAction(ex.Message);
                }
            }
            else
            {
                client.Close();
                handle.Set();
            }
        }

        public void CreateClient(IPAddress ip, int port, string username = "Client")
        {
            this.username = username;
            this.client = new TcpClient();
            this.client.Connect(ip, port);
            this.stream = client.GetStream();
            this.buffer = new byte[client.ReceiveBufferSize];
            this.data = new StringBuilder();
            this.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        public void CloseConnection()
        {
            client.Close();
        }

        public void StartConnection(ReadEventDelegate ReadEvent, ErrorEventDelegate ErrorEvent)
        {
            ReadAction += ReadEvent;
            ErrorAction += ErrorEvent;
            while (client.Connected)
            {
                try
                {
                    // đọc gói tin từ server, nếu có gói tin thì hàm callback tên là Read sẽ được gọi
                    stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(Read), null);
                    handle.WaitOne();
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
