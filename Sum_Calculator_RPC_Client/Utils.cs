using System;
using System.Net;

namespace Sum_Calculator_RPC_Client
{
    public class Utils
    {
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