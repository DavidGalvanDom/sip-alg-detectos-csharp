using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server_sip_alg.Services
{
    public class ListenerUDPService
    {
        public  void UDPServer(object arg)
        {
            Console.WriteLine("UDPServer thread started");

            try
            {
                UdpClient server = (UdpClient)arg;
                IPEndPoint remoteEP;
                byte[] buffer;

                while (true)
                {
                    remoteEP = null;
                    buffer = server.Receive(ref remoteEP);

                    if (buffer != null && buffer.Length > 0)
                    {
                        Console.WriteLine("UDP: " + Encoding.ASCII.GetString(buffer));
                    }
                    string response = "udp responde";

                    var bufferResp = Encoding.ASCII.GetBytes(response);

                    server.Send(bufferResp, bufferResp.Length, remoteEP);
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10004) // unexpected
                    Console.WriteLine("UDPServerProc exception: " + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UDPServerProc exception: " + ex);
            }

            Console.WriteLine("UDP server thread finished");
        }

    }
}
