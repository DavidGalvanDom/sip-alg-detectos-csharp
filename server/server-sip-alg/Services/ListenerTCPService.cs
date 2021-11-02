using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using Serilog.Core;

using server_sip_alg.Model;

namespace server_sip_alg.Services
{
    public class ListenerTCPService: IListenerService
    {
        private readonly Logger _log;       
        private readonly RequestControllerService _requestControllerService;

        public ListenerTCPService(Logger log, 
                                  RequestControllerService requestControllerService)
        {
            _log = log;           
            _requestControllerService = requestControllerService;
        }

        public void StartServer(object arg)
        {
            try
            {
                _log.Information("TCP server thread started");

                TcpListener server = (TcpListener)arg;
                server.Start();

                while (true)
                {
                    try
                    {
                        TcpClient client = server.AcceptTcpClient();
                      
                        var arrEndPoint = client.Client.RemoteEndPoint.ToString().Split(':');
                        string ipRemote = arrEndPoint[0];
                        string portRemote = arrEndPoint[1];

                        byte[] buffer = new byte[Constants.SIZE_READ_BUFFER];
                        int dataLength;

                        using (var stream = client.GetStream())
                        {
                            while ((dataLength = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                var resposeData = _requestControllerService
                                                    .CreateResponseData(buffer, dataLength, 
                                                                        ipRemote, portRemote, 
                                                                        server_sip_alg.Constants.TCP_TRANSPORT);

                                if(resposeData.RequestHeader != null)
                                {
                                    stream.Write(resposeData.RequestHeader, 0, resposeData.RequestHeader.Length);
                                    stream.Flush();

                                    if(resposeData.RequestBody != null)
                                    {
                                        stream.Write(resposeData.RequestBody, 0, resposeData.RequestBody.Length);                               
                                        stream.Flush();
                                    }
                                }
                            }
                        }

                        client.Close();
                    }
                    catch (IOException ioEx)
                    {
                        _log.Error(ioEx, "TCP IOException TCP Server thread");
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode != Constants.ERROR_NUM_UNEXPECTED) // unexpected
                            _log.Error(ex, "TCPServerProc exception");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "TCPServerProc exception");
            }

            _log.Information("TCP server thread finished");
        }


       
    }
}
