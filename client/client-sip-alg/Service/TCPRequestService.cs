using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Serilog.Core;

namespace client_sip_alg.Service
{
    public class TCPRequestService: IRequestService
    {
        private readonly Logger _log;
        private readonly ClientRequestService _requestService;
        private TcpClient _tcpClient;
        private NetworkStream _tcpStream;

        public TCPRequestService (Logger log, ClientRequestService requestService)
        {
            _log = log;
            _requestService = requestService;            
        }

        public string CreateRequest ()
        {
            bool isTCPSipAlgEnabled = false;
            string processStatus = "Process finish, SIP ALG is ";

            try
            {                
                string packageRequest = SendRequest();             
                string responseData = ReadResponse();
                               
                _tcpStream.Close();
                _tcpClient.Close();

                if (responseData.Length < Constants.NUM_MINIMAL_PACKAGE_SIZE)
                {
                    processStatus = $"{Constants.TCP_TRANSPORT} Access denied: {responseData}";
                    return processStatus;
                }

                string mirrorRequest = _requestService.GetMirrorRequest(responseData.ToString());

                if (mirrorRequest.Trim() != string.Empty)
                {
                    isTCPSipAlgEnabled = _requestService.CompareRequestAndMirror(packageRequest, mirrorRequest, Constants.TCP_TRANSPORT);
                }
                else
                {
                    processStatus = $"{Constants.TCP_TRANSPORT} Server no data response {Environment.NewLine}";
                    _log.Error(processStatus);
                    return processStatus;                    
                }
                
                if (isTCPSipAlgEnabled)
                    _log.Warning($"{Constants.TCP_TRANSPORT} It seems that your router is performing ALG for SIP {Constants.TCP_TRANSPORT}");
                else
                    _log.Information($"{Constants.TCP_TRANSPORT} It seems that your router is not performing ALG for SIP {Constants.TCP_TRANSPORT}");
                
            }
            catch (ArgumentNullException anex)
            {
                processStatus = $"{Constants.TCP_TRANSPORT} CreateRequest ArgumentNullException ERROR ";
                _log.Error(anex, processStatus);
            }
            catch (SocketException soex)
            {
                processStatus = $"{Constants.TCP_TRANSPORT} CreateRequest SocketException ERROR";
                _log.Error(soex, processStatus);
            }
            catch (Exception exp)
            {
                processStatus = exp.Message;
                _log.Error(exp.Message + Environment.NewLine);
            }
            finally
            {
                if (_tcpStream != null)
                    _tcpStream.Dispose();

                if (_tcpClient != null)
                    _tcpClient.Dispose();
            }
                        
            return processStatus + (isTCPSipAlgEnabled ? " enabled": " NOT enabled");
        }

        private void ConfigClient()
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(Constants.SERVER,
                               Constants.PORT_NUMBER_SERVER);

            _tcpStream = _tcpClient.GetStream();
            _tcpStream.ReadTimeout = Constants.NUM_TIMEOUT_READ;
            _tcpStream.WriteTimeout = Constants.NUM_TIMEOUT_WRITE;
        }

        private string SendRequest ()
        {
            byte[] buffer;

            string request = _requestService.CreateStringRequest(General.GetLocalIp(), Constants.TCP_TRANSPORT);

            ConfigClient();

            buffer = Encoding.ASCII.GetBytes(request);            
            _tcpStream.Write(buffer, 0, buffer.Length);

            return request;
        }

        private string ReadResponse()
        {
            int totalReceivedBytes = 0;
            Int32 bytes;

            byte[] dataResponse = new Byte[Constants.NUM_PACAGE_RESPONSE_SIZE_BYTES];
            StringBuilder responseData = new StringBuilder();

            Thread.Sleep(100);

            bytes = _tcpStream.Read(dataResponse, 0, dataResponse.Length);
            totalReceivedBytes += bytes;

            while (bytes > 0)
            {
                responseData.Append(System.Text.Encoding.ASCII.GetString(dataResponse, 0, bytes));

                if (_tcpStream.DataAvailable &&
                    totalReceivedBytes < Constants.NUM_MINIMAL_PACKAGE_SIZE) 
                {
                    bytes = _tcpStream.Read(dataResponse, 0, dataResponse.Length);
                    Thread.Sleep(100);
                }
                else
                {
                    return responseData.ToString();                    
                }

                totalReceivedBytes += bytes;
            }

            return responseData.ToString();
        }
    }
}
