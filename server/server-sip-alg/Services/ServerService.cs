using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Serilog.Core;

namespace server_sip_alg.Services
{
    public class ServerService
    {
        public  void InitServer(ref TcpListener tcpServer,
                                Logger log,                                
                                RequestControllerService requestControllerService)
        {
            Thread tcpThread = null;
            IListenerService tcpService = new ListenerTCPService(log, requestControllerService);

            try
            {
                tcpServer = new TcpListener(IPAddress.Any, Constants.PORT_NUMBER);

                tcpThread = new Thread(new ParameterizedThreadStart(tcpService.StartServer))
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

                if (tcpThread != null)
                    tcpThread.Abort();
            }
        }

        public  void InitServer(ref UdpClient udpServer,
                                    Logger log,
                                    RequestControllerService requestControllerService)
        {
            Thread udpThread = null;
            IListenerService udpService = new ListenerUDPService(log, requestControllerService);
            try
            {
                udpServer = new UdpClient(Constants.PORT_NUMBER);

                //Starting the UDP Listener thread.
                udpThread = new Thread(new ParameterizedThreadStart(udpService.StartServer))
                {
                    IsBackground = true,
                    Name = "UDP server thread"
                };
                udpThread.Start(udpServer);

                log.Information($"Starting UDP servers on port { Constants.PORT_NUMBER }... \n");
            }
            catch (Exception exp)
            {
                log.Error(exp, "An UDP Exception has occurred!");

                if (udpThread != null)
                    udpThread.Abort();
            }
        }
    }
}
