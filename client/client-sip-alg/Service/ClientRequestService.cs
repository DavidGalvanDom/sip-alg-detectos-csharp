using System;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Core;

using client_sip_alg.Extension;

namespace client_sip_alg.Service
{
    public class ClientRequestService
    {
        private readonly Logger _log;
       
        public ClientRequestService(Logger log)
        {
            _log = log;          
        }

        public string CreateStringRequest(string ipLocal, string transport)
        {
            //To test the public serve  
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

        public string GetMirrorRequest(string mirrorRequest)
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

        public bool CompareRequestAndMirror(string request, string mirrorRequest, string transport)
        {
            var lstDiff = request.StringCompareRequest(mirrorRequest);

            if (lstDiff.Count > 0)
            {
                _log.Error($"{transport} there are differences between sent request and received mirrored request:");

                foreach (var diff in lstDiff) _log.Warning(diff);
                return true;
            }

            _log.Information($"{transport} No differences between sent request and received mirrored request");
            return false;

        }
    }
}
