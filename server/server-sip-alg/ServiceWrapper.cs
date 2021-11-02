using System;
using System.Net.Sockets;
using Serilog.Core;

using server_sip_alg.Services;

namespace server_sip_alg
{
    public class ServiceWrapper
    {
        private readonly Logger _log;
        private readonly ResponseStringService _reponseService;
        private readonly RequestControllerService _requestControllerService;
        private readonly ServerService _server;

        private TcpListener _tcpServer = null;
        private UdpClient _udpServer = null;

        public ServiceWrapper ()
        {
            _log = new ConfigLog().CreateLogerConfig();
            _reponseService = new ResponseStringService();
            _requestControllerService = new RequestControllerService(_log, _reponseService);
            _server = new ServerService();
        }

        public void Stop()
        {
            if (_log != null)
                _log.Information(" Stop Server server-sip-alg-windows-service");

            if (_udpServer != null)
                _udpServer.Close();

            if (_tcpServer != null)
                _tcpServer.Stop();
        }

        public void Start()
        {            
            try
            {
                _log.Information(" Start Server server-sip-alg-windows-service");
                _server.InitServer(ref _udpServer, _log, _requestControllerService);
                _server.InitServer(ref _tcpServer, _log, _requestControllerService);                
            }
            catch (Exception ex)
            {               
                _log.Error($"Start exception: { ex.Message}");
            }            
        }   
    }
}
