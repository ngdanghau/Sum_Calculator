using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sum_Calculator_RPC_Client
{
    class Msg
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
