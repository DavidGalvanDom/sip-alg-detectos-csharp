using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Serilog.Core;

namespace client_sip_alg.Service
{
    public class UDPRequestService: IRequestService
    {
        private readonly Logger _log;
        private readonly ClientRequestService _requestService;

        private UdpClient _udpClient;
        private IPEndPoint _endPoint;

        public UDPRequestService(Logger log, ClientRequestService requestService)
        {
            _log = log;
            _requestService = requestService;

            ConfigClient();
        }

        private void ConfigClient ()
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(Constants.SERVER),
                                       Constants.PORT_NUMBER_SERVER);

            _udpClient = new UdpClient(Constants.PORT_NUMBER_LOCAL);
            _udpClient.Client.SendTimeout = Constants.NUM_TIMEOUT_WRITE;
            _udpClient.Client.ReceiveTimeout = Constants.NUM_TIMEOUT_READ;
            
        }

        public string CreateRequest()
        {
            bool isUDPSipAlgEnabled = false;
            string processStatus = "Process finish, SIP ALG is ";

            StringBuilder responseData = new StringBuilder();

            try
            {
                var packageRequest = SendRequest();               
                var arrResponseHeader = _udpClient.Receive(ref _endPoint);
                responseData.Append(System.Text.Encoding.ASCII.GetString(arrResponseHeader, 0, arrResponseHeader.Length));

                Thread.Sleep(100);

                // 1117
                if (_udpClient.Available > 0)
                {
                    var arrResponseBody = _udpClient.Receive(ref _endPoint);
                    //769                   
                    responseData.Append(System.Text.Encoding.ASCII.GetString(arrResponseBody, 0, arrResponseBody.Length));
                }

                if (responseData.Length < Constants.NUM_MINIMAL_PACKAGE_SIZE)
                {
                    processStatus = $"{Constants.UDP_TRANSPORT} Access denied: {responseData}";
                    return processStatus;                    
                }

                string mirrorRequest = _requestService.GetMirrorRequest(responseData.ToString());
                
                if (mirrorRequest.Trim() != string.Empty)
                {
                    isUDPSipAlgEnabled = _requestService.CompareRequestAndMirror(packageRequest, mirrorRequest, Constants.UDP_TRANSPORT);
                }
                else
                {
                    processStatus = $"{Constants.UDP_TRANSPORT} Server no data response {Environment.NewLine}";
                    _log.Error(processStatus);
                    return processStatus;
                }

                if (isUDPSipAlgEnabled)
                    _log.Warning($"{Constants.UDP_TRANSPORT} It seems that your router is performing ALG for SIP {Constants.UDP_TRANSPORT}");
                else
                    _log.Information($"{Constants.UDP_TRANSPORT} It seems that your router is not performing ALG for SIP {Constants.UDP_TRANSPORT}");
            }
            catch(Exception exp)
            {
                processStatus = $"{Constants.UDP_TRANSPORT} CreateRequest {Environment.NewLine}";
                _log.Error(exp, processStatus);
            }
            finally
            {
                _udpClient.Close();

                if (_udpClient != null)
                    _udpClient.Dispose();
            }

            return processStatus + (isUDPSipAlgEnabled  ? " is enabled" : " is NOT enabled");
        }

        private string SendRequest()
        {   
            byte[] buffer;
                        
            string request = _requestService.CreateStringRequest(General.GetLocalIp(), Constants.UDP_TRANSPORT);
            buffer = Encoding.ASCII.GetBytes(request);

            _udpClient.Send(buffer, buffer.Length, _endPoint);

            return request;
        }
    }
}
