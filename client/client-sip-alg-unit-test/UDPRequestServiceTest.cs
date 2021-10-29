using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Serilog.Core;

using client_sip_alg.Service;

namespace client_sip_alg_unit_test
{
    [TestClass]
    public class UDPRequestServiceTest
    {
        private readonly Logger _log;
        private readonly ClientRequestService _clientService;

        public UDPRequestServiceTest()
        {
            _log = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
            _clientService = new ClientRequestService(_log);
        }


        [TestMethod]
        public void Send_Request_To_Server_Validate_Status_SIP()
        {
            // Arrange
            var tcpReqService = new UDPRequestService(_log, _clientService);

            // Act 
            var result = tcpReqService.CreateRequest();

            // Assert should by true in my case
            Assert.IsTrue(result.Contains("Process finish"));
        }
    }
}
