using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Serilog;
using Serilog.Core;

using server_sip_alg.Services;

namespace server_sip_alg
{
    class Program
    {
        public static int Main(String[] args)
        {        
            TcpListener tcpServer = null;
            UdpClient udpServer = null;

            Logger log = new ConfigLog().CreateLogerConfig();

            try
            {
                //InitUDPServer(ref udpServer, ref log);
                InitTCPServer(ref tcpServer, ref log);

                log.Information("Press <ENTER> to stop the servers.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {               
                log.Error($"Main exception: { ex.Message}");
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


        public static void InitTCPServer(ref TcpListener tcpServer, ref Logger log)
        {
            Thread _TCPThread = null;
            ListenerTCPService tcpService = new ListenerTCPService(log);

            try
            {
                tcpServer = new TcpListener(IPAddress.Any,Constants.PORT_NUMBER);
                
                var tcpThread = new Thread(new ParameterizedThreadStart(tcpService.TCPServer))
                {
                    IsBackground = true,
                    Name = "TCP server thread"
                };
                tcpThread.Start(tcpServer);

                log.Information($"Starting TCP servers on port { Constants.PORT_NUMBER }... \n");
            }
            catch (Exception e)
            {
                log.Error($"TCP Exception has occurred! {e.Message}");

                if (_TCPThread != null)
                    _TCPThread.Abort();
            }

        }

        public static void InitUDPServer (ref UdpClient udpServer, ref Logger log)
        {
            Thread _UDPThread = null;
            ListenerUDPService udpService = new ListenerUDPService();
            try
            {
                udpServer = new UdpClient(Constants.PORT_NUMBER);

                //Starting the UDP Listener thread.
                _UDPThread = new Thread(new ParameterizedThreadStart(udpService.UDPServer))
                {
                    IsBackground = true,
                    Name = "UDP server thread"
                };
                _UDPThread.Start(udpServer);

                log.Information($"Starting UDP servers on port { Constants.PORT_NUMBER }... \n");
            }
            catch (Exception exp)
            {
                log.Error(exp , "An UDP Exception has occurred!");

                if (_UDPThread != null)
                    _UDPThread.Abort();
            }
        }
                
    }
}
