using System;
using System.Net;
using System.Net.Sockets;
using Serilog.Core;

namespace server_sip_alg.Services
{
    public class ListenerUDPService: IListenerService
    {
        private readonly Logger _log;
        private readonly RequestControllerService _requestControllerService;

        public ListenerUDPService(Logger log, RequestControllerService requestControllerService)
        {
            _log = log;
            _requestControllerService = requestControllerService;
        }

        public void StartServer(object arg)
        {
            UdpClient server = (UdpClient)arg;
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Constants.PORT_NUMBER); ;
            byte[] buffer;

            try
            {
                _log.Information("UDPServer thread started");

                while (true)
                {
                    buffer = server.Receive(ref remoteEP);

                    if (buffer != null
                        && buffer.Length != 19 )
                    {
                        var resposeData = _requestControllerService
                                            .CreateResponseData(buffer, buffer.Length,
                                                                remoteEP.Address.ToString(),  remoteEP.Port.ToString(), 
                                                                server_sip_alg.Constants.UDP_TRANSPORT);
                        if (resposeData.RequestHeader != null)
                        {
                            server.Send(resposeData.RequestHeader, resposeData.RequestHeader.Length, remoteEP);

                            if (resposeData.RequestBody != null)
                            {
                                server.Send(resposeData.RequestBody, resposeData.RequestBody.Length, remoteEP);
                            }
                        }
                    }                   
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10004) // unexpected
                    _log.Error("UDPServerProc exception: " + ex);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "UDPServerProc exception ");
            }

            _log.Information("UDP server thread finished");
        }

    }
}
