using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace Server_Midleware
{
   public class Helper
    {

        public static int ValidatePort(string number)
        {
            int port = -1;
            if (number.Length < 1)
            {
                throw new Exception("Port number is required"); 
            }
            else if (!int.TryParse(number, out port))
            {
                throw new Exception("Port number is not valid");
            }
            else if (port < 0 || port > 65535)
            {
                throw new Exception("Port number is out of range"); 
            }
            return port; 
        }

        public static void ValidateUsername(string username)
        {
            if (username.Length < -1)
            {
                throw new Exception("Username is required"); 
            }
        }
        public static IPAddress getIp(string address)
        {
            IPAddress ip = null;
            if (address.Length < 1)
            {

                throw new Exception("Address is required"); 
            }
            else
            {
                try
                {
                    //ip = Dns.Resolve(address).AddressList[0]; //trả về đối tượng ip từ giá trị ip string.
                    ip = Dns.GetHostAddresses(address)[0];

                }
                catch
                {

                    throw new Exception("Address is not valid"); 
                }
            }

            return ip; 
        }
    }
}
