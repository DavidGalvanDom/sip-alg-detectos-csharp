using Serilog.Core;
using server_sip_alg.Model;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server_sip_alg.Services
{
    public class ListenerUDPService
    {
        private readonly Logger _log;
        private readonly ResponseStringService _reponseService;

        public ListenerUDPService(Logger log, ResponseStringService reponseService)
        {
            _log = log;
            _reponseService = reponseService;
        }

        public  void UDPServer(object arg)
        {
            UdpClient server = (UdpClient)arg;
            IPEndPoint remoteEP;
            byte[] buffer;
            byte[] bodyBuffer;

            try
            {
                _log.Information("UDPServer thread started");

                while (true)
                {
                    remoteEP = null;
                    buffer = server.Receive(ref remoteEP);

                    if (buffer != null && 
                        buffer.Length <= Constants.SIZE_REQUEST_MIN )
                    {
                        _log.Information($"UDP requeste: length - {buffer.Length}  {Encoding.ASCII.GetString(buffer)} " );                        
                    }

                    var headerBuffer = CreateResponseData(buffer,buffer.Length, 
                                                         remoteEP.Address.ToString(), remoteEP.Port.ToString(), 
                                                         out bodyBuffer);
                    if (headerBuffer != null)
                    {
                        server.Send(headerBuffer, headerBuffer.Length, remoteEP);
                        _log.Information("******* send header");

                        if (bodyBuffer != null)
                            server.Send(bodyBuffer, bodyBuffer.Length, remoteEP);

                        _log.Information("******** send buffer");
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

        private byte[] CreateResponseData(byte[] buffer, int dataLength,
                                          string ipServer, string portServer, 
                                          out byte[] bodyBuffer)
        {

            StringBuilder packageRequest = new StringBuilder();
            byte[] headerBuffer = null;
            bodyBuffer = null;

            try
            {
                _log.Information($"UDP Create response - Stream count:{dataLength}");

                packageRequest.Append(Encoding.ASCII.GetString(buffer, 0, dataLength));

                if (packageRequest.Length > Constants.SIZE_MIN_PACKAGES_REQUEST)
                {
                    int indexHeader = packageRequest.ToString().IndexOf("\r\n\r\n");

                    CommunicationInfo commInfo = new CommunicationInfo
                    {
                        IpSender = ipServer,
                        PortSender = portServer,
                        RequestFirstLine = packageRequest.ToString().Substring(0, packageRequest.ToString().IndexOf("\r\n")),
                        RequestHeaders = packageRequest.ToString().Substring(0, indexHeader),
                        RequestBody = packageRequest.ToString().Substring(indexHeader + 4, packageRequest.Length - 4 - indexHeader)
                    };

                    if (commInfo.RequestFirstLine.Substring(0, Constants.SIZE_FIRST_LINE_REQUEST) == "INVITE sip:sip-alg-detector-ttec@")
                    {
                        headerBuffer = _reponseService.CreateMirrorHeader(commInfo);
                        bodyBuffer = _reponseService.CreateMirrorBody(commInfo);
                        _log.Information("UDP Finish response.");
                    }
                    else
                    {
                        headerBuffer = _reponseService.GenerateErrorResponse(commInfo);
                        _log.Error(" UDP Finish with error response.");
                    }

                    packageRequest.Clear();
                }
            }
            catch (Exception exp)
            {
                headerBuffer = null;
                _log.Error(exp, $"UDP Package: {packageRequest}");
            }
            finally
            {
                packageRequest.Clear();
            }

            return headerBuffer;
        }

    }
}
