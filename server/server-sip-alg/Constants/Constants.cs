using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server_sip_alg
{
    public static class Constants
    {
        public const int PORT_NUMBER = 5060;
        public const int NUM_MAX_CONNECTE = 150;

        //Request string
        public const int SIZE_MIN_PACKAGES_REQUEST = 640;
        public const int SIZE_FIRST_LINE_REQUEST = 33;

        public const int SIZE_READ_BUFFER = 2048;
        public const int SIZE_APLICE_BODY = 60;

        //Error
        public const int ERROR_NUM_UNEXPECTED = 10004;
    }
}
