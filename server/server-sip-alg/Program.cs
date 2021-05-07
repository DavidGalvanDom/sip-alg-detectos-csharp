using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace server_sip_alg
{
    class Program
    {
        public static int Main(String[] args)
        {        
            TcpListener tcpServer = null;
            UdpClient udpServer = null;

            try
            {
                InitUDPServer(ref udpServer);
                InitTCPServer(ref tcpServer);

                Console.WriteLine("Press <ENTER> to stop the servers.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main exception: " + ex);
            }
            finally
            {
                if (udpServer != null)
                    udpServer.Close();

                if (tcpServer != null)
                    tcpServer.Stop();
            }

            return 0;
        }


        public static void InitTCPServer(ref TcpListener tcpServer)
        {
            Thread _TCPThread = null;
            
            try
            {
                tcpServer = new TcpListener(IPAddress.Any, General.PORT_NUMBER);

                var tcpThread = new Thread(new ParameterizedThreadStart(ListenerCommon.TCPServer))
                {
                    IsBackground = true,
                    Name = "TCP server thread"
                };
                tcpThread.Start(tcpServer);

            }
            catch (Exception e)
            {
                Console.WriteLine("An TCP Exception has occurred!" + e.ToString());

                if (_TCPThread != null)
                    _TCPThread.Abort();
            }

        }

        public static void InitUDPServer (ref UdpClient udpServer)
        {
            Thread _UDPThread = null;
        
            try
            {
                udpServer = new UdpClient(General.PORT_NUMBER);

                //Starting the UDP Listener thread.
                _UDPThread = new Thread(new ParameterizedThreadStart(ListenerCommon.UDPServer))
                {
                    IsBackground = true,
                    Name = "UDP server thread"
                };
                _UDPThread.Start(udpServer);

                Console.WriteLine($"Starting UDP servers on port { General.PORT_NUMBER }... \n");
            }
            catch (Exception e)
            {
                Console.WriteLine("An UDP Exception has occurred!" + e.ToString());

                if (_UDPThread != null)
                    _UDPThread.Abort();
            }
        }




        
    }
}
