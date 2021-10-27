using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_sip_alg
{
    public static class Constants
    {
        
        public const int PORT_NUMBER_SERVER = 5060; //Port of the server when the daemon runs. Default "5060".
        public const int PORT_NUMBER_LOCAL = 5060; //Local port from which UDP request will be sent. Default "5060".

        //public const string SERVER = "127.0.0.1";        
        public const string SERVER = "199.180.223.109"; //Public server test 

        public const int NUM_MAX_CONNECTE = 150;

        public const int NUM_MINIMAL_PACKAGE_SIZE = 1850;
        public const int NUM_PACAGE_RESPONSE_SIZE_BYTES = 250;

        //Time out connection milliseconds
        public const int NUM_TIMEOUT_READ = 8000;
        public const int NUM_TIMEOUT_WRITE = 8000;


        //Transport
        public const string UDP_TRANSPORT = "UDP";
        public const string TCP_TRANSPORT = "TCP";
    }
}
