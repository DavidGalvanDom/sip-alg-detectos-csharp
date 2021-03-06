
namespace client_sip_alg
{
    public static class Constants
    {
        public const string PRODUCT_NAME = "test";

        public const int PORT_NUMBER_SERVER = 5060; //Port of the server when the daemon runs. Default "5060".
        public const int PORT_NUMBER_LOCAL = 5061; //Local port from which UDP request will be sent. Default "5061".

        public const string SERVER = "127.0.0.1";       
        // public const string SERVER = "199.180.223.109"; //Public server test 

        public const int NUM_MAX_CONNECTE = 150;

        public const int NUM_MINIMAL_PACKAGE_SIZE = 1950;
        public const int NUM_PACAGE_RESPONSE_SIZE_BYTES = 300;

        //Time out connection milliseconds
        public const int NUM_TIMEOUT_READ = 8000;
        public const int NUM_TIMEOUT_WRITE = 8000;

        //Transport
        public const string UDP_TRANSPORT = "UDP";
        public const string TCP_TRANSPORT = "TCP";
    }
}
