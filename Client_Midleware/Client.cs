using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client_Midleware
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