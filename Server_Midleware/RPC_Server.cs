using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;


namespace Server_Midleware
{

    public delegate void ShowMessage(string msg);
    public delegate void ReadEventDelegate(string msg, Client client);
    public delegate void ErrorEventDelegate(string msg);
    public class RPC_Server
    {
        private Task send = null;
        private Thread disconnect = null;
        private long id = 0;

        ShowMessage writeStatus = null;
        ReadEventDelegate ReadAction = null;
        ErrorEventDelegate ErrorAction = null;

        
        public void Write(IAsyncResult result)
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
                    throw new Exception(ex.Message);
                }
            }
        }

        // BeginWrite và Send dành cho việc gởi đến hết toàn bộ clients trừ cái thằng id > 0
        private void BeginWrite(ConcurrentDictionary<long, Client> clients, string msg, long id = -1)
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
                        throw new Exception(ex.Message);
                    }
                }
            }
        }

        private void Send(ConcurrentDictionary<long, Client> clients, string msg, long id = -1)
        {
            if (send == null || send.IsCompleted)
            {
                send = Task.Factory.StartNew(() => BeginWrite(clients, msg, id));
            }
            else
            {
                send.ContinueWith(antecendent => BeginWrite(clients, msg, id));
            }
        }




        // dành cho việc gởi đến 1 client
        public void BeginWrite(string msg, Client obj)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected)
            {
                try
                {
                    obj.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), obj); //đưa message cho client.
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        // dành cho việc gởi đến 1 client
        public void Send(string msg, Client obj)
        {
            if (send == null || send.IsCompleted)
            {
                try
                {
                    send = Task.Factory.StartNew(() => BeginWrite(msg, obj));
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                try
                {
                    send.ContinueWith(antecendent => BeginWrite(msg, obj));
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }


        public void Disconnect(ConcurrentDictionary<long, Client> clients)
        {
            if (disconnect == null || !disconnect.IsAlive)
            {
                disconnect = new Thread(() =>
                {
                    foreach (KeyValuePair<long, Client> obj in clients)
                    {
                        obj.Value.client.Close();
                    }
                })
                {
                    IsBackground = true
                };
                disconnect.Start();
            }
        }

        /// <summary> hàm xử lý khi nhận được gói tin</summary>
        /// <param name="result">Kết quả trả về của cái TcpListener</param>
        /// <returns> void </returns
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
                    ErrorAction(ex.Message);
                }
            }
            if (bytes > 0)
            {
                obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                try
                {
                    //chỗ này nên có hàm đọc bên server: 
                    if (obj.stream.DataAvailable)
                    {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                    }
                    else
                    {
                        ReadAction(obj.data.ToString(), obj);
                        obj.data.Clear();
                        obj.handle.Set(); //cho luồng khác chạy
                    }
                }
                catch (Exception ex)
                {
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


       
        public string openConnection(ConcurrentDictionary<long, Client> clients, Client obj)
        {
            clients.TryAdd(obj.id, obj);
            string msg = string.Format("{0} has connected", obj.username);
            writeStatus(msg);
            return msg;
        }

        public string closeConnection(ConcurrentDictionary<long, Client> clients, Client obj)
        {
            obj.client.Close();
            clients.TryRemove(obj.id, out Client tmp);
            string msg = string.Format("{0} has disconnected", obj.username);
            writeStatus(msg);
            return msg;
        }



        // Hàm xử lý kết nối và gửi nhận packet
        public void Connection(ConcurrentDictionary<long, Client> clients, Client obj)
        {
            //mở kết nôi: 
            string msg = openConnection(clients, obj);
            


            // gửi lại cho client kết quả kết nối
            try
            {
                Send(msg, obj);
            }
            catch (Exception ex)
            {
                ErrorAction(ex.Message);
                return;
            }

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
                    ErrorAction(ex.Message);
                }
            }

            //đóng kết nối: 
            msg = closeConnection(clients, obj); 
            

            try
            {
               Send(msg, obj);
            }
            catch (Exception ex)
            {
                ErrorAction(ex.Message);
            }
        }
      
       
        public Client createNewClient(TcpListener listener)
        {
            Client client = new Client();
            client.id = id;
            client.username = "Client " + id;
            client.client = listener.AcceptTcpClient();
            client.stream = client.client.GetStream();
            client.buffer = new byte[client.client.ReceiveBufferSize];
            client.data = new StringBuilder();
            client.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
            id++; 
            return client; 
        }

        public void inheritMethod(ShowMessage writeLog, ReadEventDelegate readEvent, ErrorEventDelegate ErrorEvent)
        {

            if (writeStatus == null)
            {
                writeStatus += writeLog;
            }


            if (ReadAction == null)
            {
                ReadAction += readEvent;
            }
            
            if (ErrorAction == null)
            {
                ErrorAction += ErrorEvent;
            }
             
        }
    }
}
