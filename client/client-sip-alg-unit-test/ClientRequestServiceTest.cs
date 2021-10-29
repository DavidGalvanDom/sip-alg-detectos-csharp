using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Serilog.Core;

using client_sip_alg.Service;

namespace client_sip_alg_unit_test
{
    [TestClass]
    public class ClientRequestServiceTest
    {
        private readonly Logger _log;
        public ClientRequestServiceTest()
        {
            _log = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
        }

        [TestMethod]
        public void Call_Methos_Empty_Parameters_Default_Values()
        {
            // Arrange
            var clientService = new ClientRequestService(_log);

            // Act 
            var result = clientService.CreateStringRequest(null, null);

            // Assert
            Assert.IsTrue(result.Contains(client_sip_alg.Constants.TCP_TRANSPORT));
            Assert.IsTrue( result.Contains("170.0.0.1") );
        }


        [TestMethod]
        public void Call_Methos_Empty_Parameters_Mandatory()
        {
            // Arrange
            var clientService = new ClientRequestService(_log);

            // Act 
            var result = clientService.GetMirrorRequest(null);

            // Assert
            Assert.AreEqual(result, string.Empty);            
        }

        [TestMethod]
        public void Compare_Data_False_Empty_Data()
        {
            // Arrange
            var clientService = new ClientRequestService(_log);

            // Act 
            var result = clientService.CompareRequestAndMirror(null,null, client_sip_alg.Constants.TCP_TRANSPORT);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
