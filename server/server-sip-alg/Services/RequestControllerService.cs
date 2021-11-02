using Serilog.Core;
using System;
using System.Text;

using server_sip_alg.Model;

namespace server_sip_alg.Services
{
    public class RequestControllerService
    {
        private readonly Logger _log;
        private readonly ResponseStringService _reponseService;

        public RequestControllerService (Logger log, ResponseStringService reponseService)
        {
            _log = log;
            _reponseService = reponseService;
        }

        public Response CreateResponseData(byte[] buffer, int dataLength, 
                                           string ipRemote, string portRemote, 
                                           string transport)
        {
            Response result = new Response();
            StringBuilder packageRequest = new StringBuilder();
                        
            try
            {
                _log.Information($"{transport} Request read - Stream count:{dataLength}");

                packageRequest.Append(Encoding.ASCII.GetString(buffer, 0, dataLength));

                if (packageRequest.Length > Constants.SIZE_MIN_PACKAGES_REQUEST)
                {
                    int indexHeader = packageRequest.ToString().IndexOf("\r\n\r\n");

                    CommunicationInfo commInfo = new CommunicationInfo
                    {
                        IpSender = ipRemote,
                        PortSender = portRemote,
                        RequestFirstLine = packageRequest.ToString().Substring(0, packageRequest.ToString().IndexOf("\r\n")),
                        RequestHeaders = packageRequest.ToString().Substring(0, indexHeader),
                        RequestBody = packageRequest.ToString().Substring(indexHeader + 4, packageRequest.Length - 4 - indexHeader)
                    };

                    if (commInfo.RequestFirstLine.Substring(0, Constants.SIZE_FIRST_LINE_REQUEST) == "INVITE sip:sip-alg-detector-" + Constants.PRODUCT_NAME + "@")
                    {
                        result.RequestHeader = _reponseService.CreateMirrorHeader(commInfo);
                        result.RequestBody = _reponseService.CreateMirrorBody(commInfo);
                        _log.Information($"{transport} Finish response.");
                    }
                    else
                    {
                        result.RequestHeader = _reponseService.GenerateErrorResponse(commInfo);
                        result.RequestBody = Encoding.ASCII.GetBytes("Error Request");
                        _log.Error($"{transport} Finish with error response.");
                    }

                    packageRequest.Clear();
                }
            }
            catch (Exception exp)
            {
                _log.Error(exp, $"{transport} Package: {packageRequest}");
            }
            finally
            {
                packageRequest.Clear();
            }

            return result;
        }

    }
}
