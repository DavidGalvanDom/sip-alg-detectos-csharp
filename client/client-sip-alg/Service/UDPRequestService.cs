using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace client_sip_alg.Service
{
    public class UDPRequestService
    {
        public readonly Logger _log;

        public UDPRequestService(Logger log)
        {
            _log = log;
        }

        public  void CreateRequest()
        {
            byte[] buffer;

            UdpClient udpClient = null;

            try
            {
                IPEndPoint ep1 = new IPEndPoint(IPAddress.Parse(Constants.SERVER), Constants.PORT_NUMBER_SERVER);
                udpClient = new UdpClient();
                //udpClient.Connect(General.SERVER, General.PORT_NUMBER);

                buffer = Encoding.ASCII.GetBytes(DateTime.Now.ToString("HH:mm:ss.fff"));
                udpClient.Send(buffer, buffer.Length, ep1);
                var dataResponse = udpClient.Receive(ref ep1);

                Console.WriteLine(System.Text.Encoding.ASCII.GetString(dataResponse, 0, dataResponse.Length));
            }
            finally
            {
                if (udpClient != null)
                    udpClient.Close();
            }

        }
    }
}
