using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using Serilog.Core;

using server_sip_alg.Model;

namespace server_sip_alg.Services
{
    public class ListenerTCPService
    {
        private readonly Logger _log;

        public ListenerTCPService(Logger log)
        {
            _log = log;
        }

        public void TCPServer(object arg)
        {
            _log.Information("TCP server thread started");

            try
            {
                TcpListener server = (TcpListener)arg;
                server.Start();

                while (true)
                {
                    try
                    {
                        TcpClient client = server.AcceptTcpClient();
                        var arrEndPoint = client.Client.RemoteEndPoint.ToString().Split(':');
                        byte[] buffer = new byte[Constants.SIZE_READ_BUFFER];
                        byte[] bodyBuffer = null;
                        int dataLength;

                        using (var stream = client.GetStream())
                        {
                            while ((dataLength = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                var headerBuffer = GetRequestData(buffer, dataLength, arrEndPoint, out bodyBuffer);

                                stream.Write(headerBuffer, 0, headerBuffer.Length);
                                stream.Flush();

                                if(bodyBuffer != null)
                                {
                                    stream.Write(bodyBuffer, 0, bodyBuffer.Length);                               
                                    stream.Flush();
                                }
                            }
                        }

                        client.Close();
                    }
                    catch (IOException ioEx)
                    {
                        _log.Error(ioEx, "IOException TCP Server thread");
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode != Constants.ERROR_NUM_UNEXPECTED) // unexpected
                            _log.Error(ex, "CPServerProc exception");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "CPServerProc exception");
            }

            _log.Information("TCP server thread finished");
        }


        private byte[] GetRequestData(byte[] buffer, int dataLength, 
                                      string[] arrEndPoint, out byte[] bodyBuffer)
        {
            HttpEnvelopeService evelopService = new HttpEnvelopeService();
            StringBuilder packageRequest = new StringBuilder();
            byte[] headerBuffer = null;
            bodyBuffer = null;

            try
            {
                _log.Information($"Request read - Stream count:{dataLength}");

                packageRequest.Append(Encoding.ASCII.GetString(buffer, 0, dataLength));

                if (packageRequest.Length > Constants.SIZE_MIN_PACKAGES_REQUEST)
                {
                    int indexHeader = packageRequest.ToString().IndexOf("\r\n\r\n");

                    CommunicationInfo commInfo = new CommunicationInfo
                    {
                        IpSender = arrEndPoint[0],
                        PortSender = arrEndPoint[1],
                        RequestFirstLine = packageRequest.ToString().Substring(0, packageRequest.ToString().IndexOf("\r\n")),
                        RequestHeaders = packageRequest.ToString().Substring(0, indexHeader),
                        RequestBody = packageRequest.ToString().Substring(indexHeader + 4, packageRequest.Length - 4 - indexHeader)
                    };

                    if (commInfo.RequestFirstLine.Substring(0, Constants.SIZE_FIRST_LINE_REQUEST) == "INVITE sip:sip-alg-detector-ttec@")
                    {
                        headerBuffer = evelopService.CreateMirrorHeader(commInfo);                        
                        bodyBuffer = evelopService.CreateMirrorBody(commInfo);       
                        _log.Information($"Finish response.");
                    }
                    else
                    {
                        headerBuffer = evelopService.GenerateErrorResponse(commInfo);
                        _log.Error($"Finish with error response.");
                    }

                    packageRequest.Clear();
                }
            }
            catch (Exception exp)
            {
                _log.Error(exp, $" Package: {packageRequest}");                                
            }
            finally
            {
                packageRequest.Clear();
            }

            return headerBuffer;
        }
    }
}
