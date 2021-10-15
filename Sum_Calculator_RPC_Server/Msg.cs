using System;
using System.Net;

namespace Sum_Calculator_RPC_Server
{
    public class Msg
    {
       
        public static string Error(string msg)
        {
            return string.Format("ERROR: {0}", msg);
        }

        public static string System(string msg)
        {
            return string.Format("SYSTEM: {0}", msg);
        }
    }

}