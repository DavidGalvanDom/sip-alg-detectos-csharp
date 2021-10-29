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
                    string result = "";
                    string action = "";
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.X:
                            run = false;
                            break;

                        case ConsoleKey.U:
                            IRequestService udpService = new UDPRequestService(log, requestService);
                            action = "UDP";
                            log.Information($"{action} running createRequest ...");
                            result = udpService.CreateRequest();
                            break;

                        case ConsoleKey.T:
                            IRequestService tcpService = new TCPRequestService(log, requestService);
                            action = "TCP";
                            log.Information($"{action} running createRequest ...");
                            result = tcpService.CreateRequest();                            
                            break;
                    }

                    log.Warning($"{result}");
                   
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
