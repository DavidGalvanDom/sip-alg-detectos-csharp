using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Core;
using System.Text;

using server_sip_alg;
using server_sip_alg.Services;

namespace server_sip_alg_unit_test
{
    /// <summary>
    /// Summary description for ListenerUDPServiceTest
    /// </summary>
    [TestClass]
    public class RequestControllerServiceTest
    {
        private readonly Logger _log;
        private readonly ResponseStringService _reponseService;

        public RequestControllerServiceTest()
        {
            _log = new ConfigLog().CreateLogerConfig();
            _reponseService = new ResponseStringService();
        }


        [TestMethod]
        public void Return_Not_Null_UDP_Response_CreateResponseData()
        {
            // Arrange
            var requestControllerService = new RequestControllerService(_log, _reponseService);
            byte[] buffer = Encoding.ASCII.GetBytes("INVITE sip:sip-alg-detector-" + server_sip_alg.Constants.PRODUCT_NAME + "@127.0.0.1:5060 SIP/2.0\r\nVia: SIP/2.0/UDP 10.156.17.132:5061;rport;branch=z9hG4bKuxdqzenf\r\nMax-Forwards: 5\r\nTo: <sip:sip-alg-detector-test@127.0.0.1:5060>\r\nFrom: \"SIP ALG Detector\" <sip:sip-alg-detector@killing-alg-routers.war>;tag=uxdqzenf\r\nCall-ID: uxdqzenfc2@10.156.17.132\r\nCSeq: 518 INVITE\r\nContact: <sip:0123@10.156.17.132:5061;transport=UDP>\r\nAllow: INVITE\r\nContent-Type: application/sdp\r\nContent-Length: 252\r\n\r\nv=5\r\no=uxdqzenf 56146132 5614613 IN IP4 10.156.17.132\r\ns=-\r\nc=IN IP4 10.156.17.132\r\nt=0 0\r\nm=audio 5624 RTP/AVP 8 0 3 101\r\na=rtpmap:8 PCMA/8000\r\na=rtpmap:0 PCMU/8000\r\na=rtpmap:3 GSM/8000\r\na=rtpmap:101 telephone-event/8000\r\na=fmtp:101 0-15\r\na=ptime:20\r\n");
            string ipRemote = "170.0.0.1";
            string portRemote = "5060";
            string transport = server_sip_alg.Constants.UDP_TRANSPORT;

            // Act 
            var result = requestControllerService.CreateResponseData(buffer, buffer.Length,ipRemote, portRemote, transport);

            // Assert
            Assert.IsNotNull(result.RequestHeader);
            Assert.IsNotNull(result.RequestBody);

        }

        [TestMethod]
        public void Return_Not_Null_TCP_Response_CreateResponseData()
        {
            // Arrange
            var requestControllerService = new RequestControllerService(_log, _reponseService);
            byte[] buffer = Encoding.ASCII.GetBytes("INVITE sip:sip-alg-detector-" + server_sip_alg.Constants.PRODUCT_NAME + "@127.0.0.1:5060 SIP/2.0\r\nVia: SIP/2.0/TCP 10.156.17.132:5061;rport;branch=z9hG4bKdwf64hru\r\nMax-Forwards: 5\r\nTo: <sip:sip-alg-detector-test@127.0.0.1:5060>\r\nFrom: \"SIP ALG Detector\" <sip:sip-alg-detector@killing-alg-routers.war>;tag=dwf64hru\r\nCall-ID: dwf64hruf5@10.156.17.132\r\nCSeq: 77 INVITE\r\nContact: <sip:0123@10.156.17.132:5061;transport=TCP>\r\nAllow: INVITE\r\nContent-Type: application/sdp\r\nContent-Length: 252\r\n\r\nv=1\r\no=dwf64hru 15287245 1528724 IN IP4 10.156.17.132\r\ns=-\r\nc=IN IP4 10.156.17.132\r\nt=0 0\r\nm=audio 2628 RTP/AVP 8 0 3 101\r\na=rtpmap:8 PCMA/8000\r\na=rtpmap:0 PCMU/8000\r\na=rtpmap:3 GSM/8000\r\na=rtpmap:101 telephone-event/8000\r\na=fmtp:101 0-15\r\na=ptime:20\r\n");
            string ipRemote = "170.0.0.1";
            string portRemote = "5060";
            string transport = server_sip_alg.Constants.TCP_TRANSPORT;

            // Act 
            var result = requestControllerService.CreateResponseData(buffer, buffer.Length, ipRemote, portRemote, transport);

            // Assert
            Assert.IsNotNull(result.RequestHeader);
            Assert.IsNotNull(result.RequestBody);

        }

        [TestMethod]
        public void Return_Error_TCP_Response_CreateResponseData()
        {
            // Arrange
            var requestControllerService = new RequestControllerService(_log, _reponseService);
            byte[] buffer = Encoding.ASCII.GetBytes("INVITE sip:sip-alg-detector-" + server_sip_alg.Constants.PRODUCT_NAME + "error@127.0.0.1:5060 SIP/2.0\r\nVia: SIP/2.0/TCP 10.156.17.132:5061;rport;branch=z9hG4bKdwf64hru\r\nMax-Forwards: 5\r\nTo: <sip:sip-alg-detector-test@127.0.0.1:5060>\r\nFrom: \"SIP ALG Detector\" <sip:sip-alg-detector@killing-alg-routers.war>;tag=dwf64hru\r\nCall-ID: dwf64hruf5@10.156.17.132\r\nCSeq: 77 INVITE\r\nContact: <sip:0123@10.156.17.132:5061;transport=TCP>\r\nAllow: INVITE\r\nContent-Type: application/sdp\r\nContent-Length: 252\r\n\r\nv=1\r\no=dwf64hru 15287245 1528724 IN IP4 10.156.17.132\r\ns=-\r\nc=IN IP4 10.156.17.132\r\nt=0 0\r\nm=audio 2628 RTP/AVP 8 0 3 101\r\na=rtpmap:8 PCMA/8000\r\na=rtpmap:0 PCMU/8000\r\na=rtpmap:3 GSM/8000\r\na=rtpmap:101 telephone-event/8000\r\na=fmtp:101 0-15\r\na=ptime:20\r\n");
            string ipRemote = "170.0.0.1";
            string portRemote = "5060";
            string transport = server_sip_alg.Constants.TCP_TRANSPORT;

            // Act 
            var result = requestControllerService.CreateResponseData(buffer, buffer.Length, ipRemote, portRemote, transport);

            // Assert            
            Assert.AreEqual(Encoding.ASCII.GetString(result.RequestBody,0,result.RequestBody.Length), "Error Request");

        }

    }
}
