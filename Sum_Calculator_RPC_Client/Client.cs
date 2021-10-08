using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sum_Calculator_RPC_Client
{
    public class Client
    {
        public string username;
        public TcpClient client;
        public NetworkStream stream;
        public byte[] buffer;
        public StringBuilder data;
        public EventWaitHandle handle;
    }
}
