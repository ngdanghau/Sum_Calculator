using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server_Midleware
{
    public class Client 
    {
        public long id;
        public string username;
        public TcpClient client;
        public NetworkStream stream;
        public byte[] buffer;
        public StringBuilder data;
        public EventWaitHandle handle;
    }

}