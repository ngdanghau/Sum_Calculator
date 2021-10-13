using System;
using System.Net;

namespace Server_Midleware
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