using System;
using System.Net;

namespace Sum_Calculator_RPC_Server
{
    public class Utils
    {
        public static string getDateNow(string format = "dd-MM-yyyy HH:mm:ss")
        {
            return "[" + DateTime.Now.ToString(format) + "]\t";
        }

        public static bool checkIsIP(string input)
        {
            IPAddress address;
            if (IPAddress.TryParse(input, out address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        return true;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        public static string ErrorMsg(string msg)
        {
            return string.Format("ERROR: {0}", msg);
        }

        public static string SystemMsg(string msg)
        {
            return string.Format("SYSTEM: {0}", msg);
        }
    }

}