using System;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Core;
using System.Text;

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
            string headerName = Constants.SERVER == "199.180.223.109" ? "daemon" : Constants.PRODUCT_NAME;

            StringBuilder body = new StringBuilder($"v={General.RandomString(1, "0123456789")}{Environment.NewLine}");
            body.Append($"o={General.RandomString(8)} {General.RandomString(8, "0123456789")} {General.RandomString(7, "0123456789")} IN IP4 {ipLocal}{Environment.NewLine}");
            body.Append($"s=-{Environment.NewLine}");
            body.Append($"c=IN IP4 {ipLocal}{Environment.NewLine}");
            body.Append($"t=0 0{Environment.NewLine}");
            body.Append($"m=audio {General.RandomString(4, "123456789")} RTP/AVP 8 0 3 101{Environment.NewLine}");
            body.Append($"a=rtpmap:8 PCMA/8000{Environment.NewLine}");
            body.Append($"a=rtpmap:0 PCMU/8000{Environment.NewLine}");
            body.Append($"a=rtpmap:3 GSM/8000{Environment.NewLine}");
            body.Append($"a=rtpmap:101 telephone-event/8000{Environment.NewLine}");
            body.Append($"a=fmtp:101 0-15{Environment.NewLine}");
            body.Append($"a=ptime:20{Environment.NewLine}");
            
            string bodyGen = Regex.Replace(body.ToString(), @"/^[\s\t]*/", "");
           
            StringBuilder headers = new StringBuilder($"INVITE sip:sip-alg-detector-{headerName}@{Constants.SERVER}:{Constants.PORT_NUMBER_SERVER} SIP/2.0{Environment.NewLine}");
            headers.Append($"Via: SIP/2.0/{transport.ToUpper()} {ipLocal}:{Constants.PORT_NUMBER_LOCAL};rport;branch={General.GenerateBranch()}{Environment.NewLine}");
            headers.Append($"Max-Forwards: 5{Environment.NewLine}");
            headers.Append($"To: <sip:sip-alg-detector-{headerName}@{Constants.SERVER}:{Constants.PORT_NUMBER_SERVER}>{Environment.NewLine}");
            headers.Append($"From: \"SIP ALG Detector\" <sip:sip-alg-detector@killing-alg-routers.war>;tag={General.GenerateTag()}{Environment.NewLine}");
            headers.Append($"Call-ID: {General.GenerateCallid()}@{ipLocal}{Environment.NewLine}");
            headers.Append($"CSeq: {General.GenerateCseq()} INVITE{Environment.NewLine}");
            headers.Append($"Contact: <sip:0123@{ipLocal}:{Constants.PORT_NUMBER_LOCAL};transport={transport}>{Environment.NewLine}");
            headers.Append($"Allow: INVITE{Environment.NewLine}");
            headers.Append($"Content-Type: application/sdp{Environment.NewLine}");
            headers.Append($"Content-Length: {bodyGen.Length}{Environment.NewLine}");

            string headersGen = Regex.Replace(headers.ToString(), @"/^[\s\t]*/", "");

            return headersGen + Environment.NewLine + bodyGen;
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
