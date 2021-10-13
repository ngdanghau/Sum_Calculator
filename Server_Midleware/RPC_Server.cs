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
    public class RPC_Server
    {
        private Task send = null;

        

        ShowMessage writeStatus = null;

        public int SumTotal = 0;
        private long id = 0; 

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


        private void BeginWrite(ConcurrentDictionary<long, Client> clients,string msg, long id = -1)
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

        public void Send(string msg, Client obj)
        {
            msg = Utils.SystemMsg(msg); 
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


        public void Disconnect(ConcurrentDictionary<long, Client> clients, Thread disconnect, long id = -1)
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
                    writeStatus(Utils.ErrorMsg(ex.Message));
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
                        int value = Validation(obj.data.ToString());
                        if (value == -1)
                        {
                            writeStatus(Utils.SystemMsg("Please only enter a number"));
                        }
                        else if (value == -2)
                        {
                            writeStatus(Utils.SystemMsg("Please enter a number between 1 and 10"));
                        }
                        else
                        {
                            SumTotal += value;
                            string msg = string.Format("{0} send: {1} ====> Sum: {2}", obj.username, obj.data, SumTotal);
                            writeStatus(msg);
                        }
                        obj.data.Clear();
                        obj.handle.Set(); //cho luồng khác chạy
                    }
                }
                catch (Exception ex)
                {
                    obj.data.Clear();
                    writeStatus(Utils.ErrorMsg(ex.Message));
                    obj.handle.Set();
                }
            }
            else
            {
                obj.client.Close();
                obj.handle.Set();
            }
        }


        public string openConnection(ConcurrentDictionary<long, Client> clients,Client obj)
        {
            clients.TryAdd(obj.id, obj);

            string msg = string.Format("{0} has connected", obj.username);
            
            return msg; 
        }
        public string closeConnection(ConcurrentDictionary<long, Client> clients, Client obj)
        {

            obj.client.Close();
            clients.TryRemove(obj.id, out Client tmp);
            return tmp.username;
        }




        // Hàm xử lý kết nối và gửi nhận packet
        public void Connection(ConcurrentDictionary<long, Client> clients,Client obj)
        {
            //mở kết nôi: 
            string msg = openConnection(clients, obj);
            writeStatus(Utils.SystemMsg(msg));


            // gửi lại cho client kết quả kết nối
            try
            {
                Send(msg, obj);
            }
            catch (Exception ex)
            {
                writeStatus(ex.Message);
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
                    writeStatus(Utils.ErrorMsg(ex.Message));
                }
            }

            //đóng kết nối: 


            msg = closeConnection(clients, obj); 
            writeStatus(Utils.SystemMsg(msg));

            try
            {
               Send(msg, obj);
            }
            catch (Exception ex)
            {
                writeStatus(ex.Message);
            }
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
        public void inheritMethod(ShowMessage writeLog)
        {
            if (writeStatus == null)
            {
                writeStatus += writeLog;
            }
             
        }
        //public void destroyMethod(ShowMessage writeLog)
        //{
        //    writeStatus -= writeLog; 
        //}
        public void resetTotal()
        {
            this.SumTotal = 0; 
        }
    }
}
