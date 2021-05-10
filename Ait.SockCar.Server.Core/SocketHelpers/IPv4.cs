using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ait.SockCar.Server.Core.SocketHelpers
{
    public class IPv4
    {
        public static List<string> GetActiveIP4s()
        {
            List<string> activeIps = new List<string>();
            activeIps.Add("127.0.0.1");
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    activeIps.Add(ip.ToString());
                }
            }
            return activeIps;
        }
    }
}
