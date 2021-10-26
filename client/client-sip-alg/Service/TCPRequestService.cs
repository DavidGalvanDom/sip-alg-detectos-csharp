using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Serilog.Core;

namespace client_sip_alg.Service
{
    public class TCPRequestService
    {
        private readonly Logger _log;
        private TcpClient _tcpClient;
        private NetworkStream _tcpStream;


        public TCPRequestService (Logger log)
        {
            _log = log;
        }

        public void CreateRequest ()
        {             
            try
            {
                string request = SendRequest();
                string responseData = ReadResponse();

                _tcpStream.Close();
                _tcpClient.Close();

                if (responseData.Length > 0)
                    throw new ApplicationException(responseData.ToString());
                

                string mirrorRequest = GetMirrorRequest(responseData.ToString());
                bool tcpAlgTest = false;
                if (mirrorRequest.Trim() != string.Empty)
                {
                    tcpAlgTest = CompareRequestAndMirror(request, mirrorRequest);
                }
                else
                {
                    _log.Error("ERROR:      Server no data response");                    
                    return;
                }
                
                _log.Information($"INFO: SIP TCP ALG test result: {tcpAlgTest}");
                
                if (tcpAlgTest)
                    _log.Information("INFO: It seems that your router is performing ALG for SIP TCP");
                else
                    _log.Information("INFO: It seems that your router is not performing ALG for SIP TCP");
                
            }
            catch (ArgumentNullException anex)
            {
                Console.WriteLine($"ERROR: CreateTCPRequest ArgumentNullException : {anex}");
            }
            catch (SocketException soex)
            {
                Console.WriteLine($"ERROR: CreateTCPRequest SocketException : {soex}");
            }
            catch (Exception exp)
            {
                _log.Error($" {exp.Message}");
            }
            finally
            {
                if (_tcpStream != null)
                    _tcpStream.Dispose();

                if (_tcpClient != null)
                    _tcpClient.Dispose();
            }
        }

        private string ReadResponse()
        {
            int offset = 0;
            Int32 bytes;
           
            byte[] dataResponse = new Byte[Constants.NUM_PACAGE_RESPONSE_SIZE_BYTES];
            StringBuilder responseData = new StringBuilder();

            _tcpStream.ReadTimeout = 10000;
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

        private string SendRequest ()
        {
            byte[] buffer;

            string request = CreateStringRequest(IPAddress.Loopback.ToString(), "tcp");

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

        private string CreateStringRequest(string ipLocal, string transport)
        {
            //Debug action 
            string headerName = Constants.SERVER == "199.180.223.109" ? "daemon" : "ttec";

            string body = $"v={General.RandomString(1, "0123456789")}\r\n" +
                   $"o={General.RandomString(8)} {General.RandomString(8, "0123456789")} {General.RandomString(7, "0123456789")} IN IP4 {ipLocal}\r\n" +
                   $"s=-\r\n" +
                   $"c=IN IP4 {ipLocal}\r\n" +
                   $"t=0 0\r\n" +
                   $"m=audio {General.RandomString(4, "123456789")} RTP/AVP 8 0 3 101\r\n" +
                   $"a=rtpmap:8 PCMA/8000\r\n" +
                   $"a=rtpmap:0 PCMU/8000\r\n" +
                   $"a=rtpmap:3 GSM/8000\r\n" +
                   $"a=rtpmap:101 telephone-event/8000\r\n" +
                   $"a=fmtp:101 0-15\r\n" +
                   $"a=ptime:20\r\n";

            body = Regex.Replace(body, @"\s|\t", "");

            string headers = $"INVITE sip:sip-alg-detector-{headerName}@{Constants.SERVER}:{Constants.PORT_NUMBER_SERVER} SIP/2.0\r\n" +
                    $"Via: SIP/2.0/{transport.ToUpper()} {ipLocal}:{Constants.PORT_NUMBER_LOCAL};rport;branch={General.GenerateBranch()}\r\n" +
                    $"Max-Forwards: 5\r\n" +
                    $"To: <sip:sip-alg-detector-ttec@{Constants.SERVER}:{Constants.PORT_NUMBER_SERVER}>\r\n" +
                    $"From: \"SIP ALG Detector\" <sip:sip-alg-detector-ttec@killing-alg-routers.war>;tag={General.GenerateTag()}\r\n" +
                    $"Call-ID: {General.GenerateCallid()}@{ipLocal}\r\n" +
                    $"CSeq: {General.GenerateCseq()} INVITE\r\n" +
                    $"Contact: <sip:0123@{ipLocal}:{Constants.PORT_NUMBER_LOCAL};transport={transport}>\r\n" +
                    $"Allow: INVITE\r\n" +
                    $"Content-Type: application/sdp\r\n" +
                    $"Content-Length: {body.Length}\r\n";

            headers = Regex.Replace(headers, @"/^[\s\t]*/", "");

            return headers + "\r\n" + body;
        }

        private string GetMirrorRequest(string mirrorRequest)
        {
            string[] stringNewLine = new string[] { "\r\n" };
            string origenMirrorRequest;
            var arrMirrorRequest = mirrorRequest.Split(stringNewLine, StringSplitOptions.None);

            // 100: Request first line and headers
            string firstLine = arrMirrorRequest[1];
            if (!firstLine.Contains("Server: SipAlgDetectorDaemon"))
            {
                throw new ApplicationException("The server is not a SIP-ALG-Detector daemon");
            }

            var newArr = arrMirrorRequest[13].Split('\n');
            var mirrorHead = newArr.Take(newArr.Length - 1);

            string mirrorRequestFirstLineHeaders = General.Base64Decode(string.Join("", mirrorHead));
            string mirrorRequestBody = General.Base64Decode(arrMirrorRequest[arrMirrorRequest.Length - 1]);

            origenMirrorRequest = mirrorRequestFirstLineHeaders + mirrorRequestBody;

            return origenMirrorRequest;
        }


        private  bool CompareRequestAndMirror(string request, string mirrorRequest)
        {
            // Some stuff to make Diff working.
            string[] stringNewLine = new string[] { "\r\n" };
            List<string> lstDiff = new List<string>();
            var arrRequest = request.Split(stringNewLine, StringSplitOptions.None);

            var arrMirrorRequest = mirrorRequest.Split(stringNewLine, StringSplitOptions.None);

            for (int count = 0; count < arrRequest.Length; count++)
            {
                if (arrRequest[count] != arrMirrorRequest[count])
                    lstDiff.Add(arrRequest[count] + " -- " + arrMirrorRequest[count]);
            }

            if (lstDiff.Count > 0)
            {
                _log.Information("There are differences between sent request and received mirrored request:");
              
                foreach (var diff in lstDiff) _log.Information(diff);               
                return true;
            }

            _log.Information("No differences between sent request and received mirrored request");
            return false;

        }
    }
}
