using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using server_sip_alg.Services;
using server_sip_alg.Model;

namespace server_sip_alg_unit_test
{
    /// <summary>
    /// Summary description for ResponseStringServiceTest
    /// </summary>
    [TestClass]
    public class ResponseStringServiceTest
    {
        public ResponseStringServiceTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }
             
        [TestMethod]
        public void Null_Parameter_CreateMirrorBody()
        {
            // Arrange
            var respService = new ResponseStringService();
            CommunicationInfo communiInfo = null;

            // Act 
            var result = respService.CreateMirrorBody(communiInfo);

            // Assert
            Assert.AreEqual(null, result);
        }


        [TestMethod]
        public void Response_With_Header_Text_Plain_CreateMirrorHeader()
        {
            // Arrange
            var respService = new ResponseStringService();
            CommunicationInfo communiInfo = new CommunicationInfo()
            {
                IpSender = "127.0.0.1",
                PortSender = "53341",
                RequestFirstLine = "INVITE sip:sip-alg-detector-test@127.0.0.1:5060 SIP/2.0",
                RequestHeaders = "INVITE sip:sip-alg-detector-test@127.0.0.1:5060 SIP/2.0\r\nVia: SIP/2.0/TCP 192.168.1.10:5060;rport;branch=z9hG4bK5pxqhvcv\r\nMax-Forwards: 5\r\nTo: <sip:sip-alg-detector-test@127.0.0.1:5060>\r\nFrom: \"SIP ALG Detector\" <sip:sip-alg-detector@killing-alg-routers.war>;tag=5pxqhvcv\r\nCall-ID: 5pxqhvcv71@192.168.1.10\r\nCSeq: 900 INVITE\r\nContact: <sip:0123@192.168.1.10:5060;transport=TCP>\r\nAllow: INVITE\r\nContent-Type: application/sdp\r\nContent-Length: 250",
                RequestBody = "v=8\r\no=5pxqhvcv 84642515 8464251 IN IP4 192.168.1.10\r\ns=-\r\nc=IN IP4 192.168.1.10\r\nt=0 0\r\nm=audio 8464 RTP/AVP 8 0 3 101\r\na=rtpmap:8 PCMA/8000\r\na=rtpmap:0 PCMU/8000\r\na=rtpmap:3 GSM/8000\r\na=rtpmap:101 telephone-event/8000\r\na=fmtp:101 0-15\r\na=ptime:20\r\n"
            };

            // Act 
            var result = respService.CreateMirrorHeader(communiInfo);
            string strResult = Encoding.ASCII.GetString(result, 0, result.Length);
            // Assert

            Assert.IsTrue(strResult.Contains("text/plain"));

        }


        [TestMethod]
        public void Response_With_Body_Text_Plain_CreateMirrorBody()
        {
            // Arrange
            var respService = new ResponseStringService();
            CommunicationInfo communiInfo = new CommunicationInfo()
            {
                IpSender = "127.0.0.1",
                PortSender = "53341",
                RequestFirstLine = "INVITE sip:sip-alg-detector-test@127.0.0.1:5060 SIP/2.0",
                RequestHeaders = "INVITE sip:sip-alg-detector-test@127.0.0.1:5060 SIP/2.0\r\nVia: SIP/2.0/TCP 192.168.1.10:5060;rport;branch=z9hG4bK5pxqhvcv\r\nMax-Forwards: 5\r\nTo: <sip:sip-alg-detector-test@127.0.0.1:5060>\r\nFrom: \"SIP ALG Detector\" <sip:sip-alg-detector@killing-alg-routers.war>;tag=5pxqhvcv\r\nCall-ID: 5pxqhvcv71@192.168.1.10\r\nCSeq: 900 INVITE\r\nContact: <sip:0123@192.168.1.10:5060;transport=TCP>\r\nAllow: INVITE\r\nContent-Type: application/sdp\r\nContent-Length: 250",
                RequestBody  = "v=8\r\no=5pxqhvcv 84642515 8464251 IN IP4 192.168.1.10\r\ns=-\r\nc=IN IP4 192.168.1.10\r\nt=0 0\r\nm=audio 8464 RTP/AVP 8 0 3 101\r\na=rtpmap:8 PCMA/8000\r\na=rtpmap:0 PCMU/8000\r\na=rtpmap:3 GSM/8000\r\na=rtpmap:101 telephone-event/8000\r\na=fmtp:101 0-15\r\na=ptime:20\r\n"
            };

            // Act 
            var result = respService.CreateMirrorBody(communiInfo);
            string strResult = Encoding.ASCII.GetString(result, 0, result.Length);
            // Assert

            Assert.IsTrue(strResult.Contains("text/plain"));

        }

    }
}
