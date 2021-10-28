using System;
using System.Text;

using server_sip_alg.Model;
using server_sip_alg.Utils;
using server_sip_alg.Extension;

namespace server_sip_alg.Services
{
    public class ResponseStringService
    {
        public byte[] CreateMirrorHeader(CommunicationInfo communicaitonInfo)
        {
            byte[] buffer;

            string responseFirstLine = $"SIP/2.0 180 Body contains mirrored request first line and headers{Environment.NewLine}";
            string headerResponsBody = General.Base64Encode(communicaitonInfo.RequestHeaders + "\r\n\r\n");
            headerResponsBody = headerResponsBody.SpliceText(Constants.SIZE_APLICE_BODY);
            string responseHeaders = (string)communicaitonInfo.RequestHeaders.Clone();

            int firstNeLine = responseHeaders.IndexOf("\r\n", 0) + 2;
            responseHeaders = responseHeaders.Substring(firstNeLine, responseHeaders.Length - firstNeLine );

            responseHeaders = "Server: SipAlgDetectorDaemon\r\n" + responseHeaders;
            responseHeaders = responseHeaders.Replace(";rport", $";received={communicaitonInfo.IpSender};rport={communicaitonInfo.PortSender}");
            responseHeaders = responseHeaders.Insert(responseHeaders.IndexOf('>', responseHeaders.IndexOf("To:")) + 1,
                                                     $"; tag={General.RandomString(6, "0123456789")}");
            responseHeaders = responseHeaders.Replace("application/sdp", "text/plain");
            int indexResponseSize = responseHeaders.IndexOf("Content-Length: ") + 16;
            responseHeaders = responseHeaders.Remove(indexResponseSize, (responseHeaders.Length - indexResponseSize));
            responseHeaders = responseHeaders.Insert(indexResponseSize, $"{headerResponsBody.Length}\r\n\r\n");

            buffer = Encoding.ASCII.GetBytes(responseFirstLine + responseHeaders + headerResponsBody + "\n");

            return buffer;
        }

        public byte[] CreateMirrorBody(CommunicationInfo communicaitonInfo)
        {
            byte[] buffer;

            /// Generate a 500 reply containing the mirrored request body.            
            string responseFirstLine = $"SIP/2.0 500 Body contains mirrored request body{Environment.NewLine}";
            string responsBody = General.Base64Encode(communicaitonInfo.RequestBody);
            responsBody = responsBody.SpliceText(Constants.SIZE_APLICE_BODY); 
            string responseHeaders = (string)communicaitonInfo.RequestHeaders.Clone();

            int firstNeLine = responseHeaders.IndexOf("\r\n", 0) + 2;
            responseHeaders = responseHeaders.Substring(firstNeLine, responseHeaders.Length - firstNeLine);

            responseHeaders = "Server: SipAlgDetectorDaemon\r\n" + responseHeaders;
            responseHeaders = responseHeaders.Replace(";rport", $";received={communicaitonInfo.IpSender};rport={communicaitonInfo.PortSender}");
            responseHeaders = responseHeaders.Insert(responseHeaders.IndexOf('>', responseHeaders.IndexOf("To:")) + 1,
                                                   $";tag={General.RandomString(6, "0123456789")}");
            responseHeaders = responseHeaders.Replace("application/sdp", "text/plain");
            int indexResponseSize = responseHeaders.IndexOf("Content-Length: ") + 16;
            responseHeaders = responseHeaders.Remove(indexResponseSize, (responseHeaders.Length - indexResponseSize));
            responseHeaders = responseHeaders.Insert(indexResponseSize, $"{responsBody.Length}\r\n\r\n");

            buffer = Encoding.ASCII.GetBytes(responseFirstLine + responseHeaders + responsBody + "\n");

            return buffer;
        }

        // Generate a 403 error response
        public byte[] GenerateErrorResponse(CommunicationInfo communicaitonInfo)
        {
            byte[] buffer;
            
            // IF an ACK ignore it.
            if (communicaitonInfo.RequestFirstLine.Contains("ACK")) return null;

            string responseFirstLine = "SIP/2.0 403 You seem a real phone, get out\r\n";
            string responseHeaders = (string)communicaitonInfo.RequestHeaders.Clone();

            responseHeaders = "Server: SipAlgDetectorDaemon\r\n" + responseHeaders;
            responseHeaders = responseHeaders.Replace(";rport", $";received={communicaitonInfo.IpSender};rport={communicaitonInfo.PortSender}");
            responseHeaders = responseHeaders.Insert(responseHeaders.IndexOf('>', responseHeaders.IndexOf("To:")) + 1,
                                                     $";tag={General.RandomString(6, "0123456789")}");
            responseHeaders = responseHeaders.Replace("Content-Type: application/sdp", "");
            int indexResponseSize = responseHeaders.IndexOf("Content-Length: ");
            responseHeaders = responseHeaders.Remove(indexResponseSize, (responseHeaders.Length - indexResponseSize));

            buffer = Encoding.ASCII.GetBytes(responseFirstLine + responseHeaders);

            return buffer;
        }
    }
}
