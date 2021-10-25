using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_sip_alg
{
    public static class General
    {
        /// <summary>
        /// load balancer 170.65.129.182  new 170.65.129.66  athomesipalgcert.ttec.com
        /// -- server ip 10.48.247.190 USZYC-WBCERT01
        /// </summary>

        public const int PORT_NUMBER = 5060;
       // public const string SERVER = "127.0.0.1"; //"10.48.247.190";//"127.0.0.1"; 
        public const string SERVER = "170.65.129.66"; // TTEC F5 "10.48.247.190";//"127.0.0.1"; 
       //   public const string SERVER = "199.180.223.109"; //External provider as a test 

        public const int NUM_MAX_CONNECTE = 150;
    }
}
