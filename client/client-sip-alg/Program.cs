using System;
using Serilog.Core;

using client_sip_alg.Service;

namespace client_sip_alg
{
    class Program
    {
        static void Main(string[] args)
        {                     
            bool run = true;
            Logger log = new ConfigLog().CreateLogerConfig();
            ClientRequestService requestService = new ClientRequestService(log);

            log.Information($"Starting TCP and UDP clients on port {Constants.PORT_NUMBER_LOCAL}...");
            log.Information($"Server {Constants.SERVER}" + Environment.NewLine);

            try
            {            
                while (run)
                {
                    log.Information("Press 'T' for TCP sending, 'U' for UDP sending or 'X' to exit.");
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.X:
                            run = false;
                            break;

                        case ConsoleKey.U:
                            UDPRequestService udpService = new UDPRequestService(log, requestService);
                            log.Information("UDP running createRequest ...");
                            udpService.CreateRequest();
                            break;

                        case ConsoleKey.T:
                            TCPRequestService tcpService = new TCPRequestService(log, requestService);
                            log.Information("TCPrunning createRequest ...");
                            tcpService.CreateRequest();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex,"Main exception: ");
            }

            log.Information("Press <ENTER> to exit.");
            Console.ReadLine();
        }
                
    }
}
