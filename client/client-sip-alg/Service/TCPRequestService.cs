using System;
using System.Net.Sockets;
using System.Text;
using Serilog.Core;

namespace client_sip_alg.Service
{
    public class TCPRequestService
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

        public void CreateRequest ()
        {             
            try
            {
                string packageRequest = SendRequest();
                string responseData = ReadResponse();

                _tcpStream.Close();
                _tcpClient.Close();

                if (responseData.Length < Constants.NUM_MINIMAL_PACKAGE_SIZE)
                {
                    _log.Error($"{Constants.TCP_TRANSPORT} Response size invalid: {responseData}");
                    return;
                }

                string mirrorRequest = _requestService.GetMirrorRequest(responseData.ToString());
                bool tcpAlgTest = false;
                if (mirrorRequest.Trim() != string.Empty)
                {
                    tcpAlgTest = _requestService.CompareRequestAndMirror(packageRequest, mirrorRequest, Constants.TCP_TRANSPORT);
                }
                else
                {
                    _log.Error($"{Constants.TCP_TRANSPORT} Server no data response {Environment.NewLine}");                    
                    return;
                }
                
                if (tcpAlgTest)
                    _log.Warning($"{Constants.TCP_TRANSPORT} It seems that your router is performing ALG for SIP {Constants.TCP_TRANSPORT}");
                else
                    _log.Information($"{Constants.TCP_TRANSPORT} It seems that your router is not performing ALG for SIP {Constants.TCP_TRANSPORT}");
                
            }
            catch (ArgumentNullException anex)
            {
                Console.WriteLine($"{Constants.TCP_TRANSPORT} CreateRequest ArgumentNullException : {anex} {Environment.NewLine}");
            }
            catch (SocketException soex)
            {
                Console.WriteLine($"{Constants.TCP_TRANSPORT} CreateRequest SocketException : {soex} {Environment.NewLine}");
            }
            catch (Exception exp)
            {
                _log.Error(exp.Message + Environment.NewLine);
            }
            finally
            {
                if (_tcpStream != null)
                    _tcpStream.Dispose();

                if (_tcpClient != null)
                    _tcpClient.Dispose();
            }
        }

        private string SendRequest ()
        {
            byte[] buffer;

            string request = _requestService.CreateStringRequest(General.GetLocalIp(), Constants.TCP_TRANSPORT);

            buffer = Encoding.ASCII.GetBytes(request);

            if (_tcpClient == null)
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(Constants.SERVER, Constants.PORT_NUMBER_SERVER);
                _tcpStream = _tcpClient.GetStream();
            }

            _tcpStream.Write(buffer, 0, buffer.Length);

            return request;
        }

        private string ReadResponse()
        {
            int offset = 0;
            Int32 bytes;

            byte[] dataResponse = new Byte[Constants.NUM_PACAGE_RESPONSE_SIZE_BYTES];
            StringBuilder responseData = new StringBuilder();

            _tcpStream.ReadTimeout = Constants.NUM_TIMEOUT_READ;
            _tcpStream.WriteTimeout = Constants.NUM_TIMEOUT_WRITE;

            bytes = _tcpStream.Read(dataResponse, 0, dataResponse.Length);
            offset += bytes;

            while (bytes > 0)
            {
                responseData.Append(System.Text.Encoding.ASCII.GetString(dataResponse, 0, bytes));

                if (bytes < Constants.NUM_PACAGE_RESPONSE_SIZE_BYTES &&
                    offset > Constants.NUM_MINIMAL_PACKAGE_SIZE)
                {
                    return responseData.ToString();
                }
                else
                {
                    bytes = _tcpStream.Read(dataResponse, 0, dataResponse.Length);
                }

                offset += bytes;
            }

            return responseData.ToString();
        }
    }
}
