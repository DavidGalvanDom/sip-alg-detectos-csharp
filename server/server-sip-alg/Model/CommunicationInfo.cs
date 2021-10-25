using System;
using System.Collections.Generic;

namespace server_sip_alg.Model
{
    public class CommunicationInfo
    {
        public string RequestHeaders { get; set; }

        public string RequestBody { get; set; }

        public string IpSender { get; set; }

        public string PortSender { get; set; }

        public string RequestFirstLine { get; set; }

    }
}
