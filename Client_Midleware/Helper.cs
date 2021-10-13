using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client_Midleware
{
    public class Helper
    {
        public static int ValidatePort(string number)
        {
            number = number.Trim();
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

        public static IPAddress ValidateAddress(string address)
        {
            address = address.Trim();
            IPAddress ip = null;
            if (address.Length < 1)
            {
                throw new Exception("Address is required");
            }
            else
            {
                try
                {
                    ip = Dns.Resolve(address).AddressList[0];//trả về đối tượng ip từ giá trị string ip.
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
